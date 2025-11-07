using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using vmPing.Properties;

namespace vmPing.Classes
{
    internal static class Util
    {
        public static async void SendEmail(string status, string hostname, string alias)
        {
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    var affectedShortName = string.IsNullOrWhiteSpace(alias) ? hostname : alias;
                    var affectedLongName = string.IsNullOrWhiteSpace(alias) ? hostname : $"{alias} ({hostname})";

                    message.Subject = $"Notice: {affectedShortName} {Strings.Email_Verb} {status}";
                    message.Body = $"{affectedLongName} {Strings.Email_Verb} {status}.{Environment.NewLine}" +
                        $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
                    message.From = new MailAddress(ApplicationOptions.EmailFromAddress, "vmPing");
                    message.To.Add(ApplicationOptions.EmailRecipient.Replace(";", ","));

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Host = ApplicationOptions.EmailServer;
                        smtp.EnableSsl = ApplicationOptions.IsEmailSslEnabled;
                        if (int.TryParse(ApplicationOptions.EmailPort, out var port) && port > 0)
                        {
                            smtp.Port = port;
                        }
                        if (ApplicationOptions.IsEmailAuthenticationRequired)
                        {
                            smtp.Credentials = new NetworkCredential(
                                ApplicationOptions.EmailUser,
                                ApplicationOptions.EmailPassword);
                        }

                        await smtp.SendMailAsync(message);
                    }
                }
            }
            catch
            {
                // Silently ignore errors.
            }
        }

        public static void SendTestEmail(
            string server,
            string port,
            bool isSslEnabled,
            bool isAuthRequired,
            string username,
            System.Security.SecureString password,
            string fromAddress,
            string recipientAddress)
        {
            using (MailMessage message = new MailMessage())
            {
                message.Subject = "[vmPing] Test Email Notification";
                message.Body = $"This is a test email sent by vmPing on {DateTime.Now:F}";
                message.From = new MailAddress(fromAddress, "vmPing");
                message.To.Add(recipientAddress.Replace(";", ","));

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = server;
                    smtp.EnableSsl = isSslEnabled;
                    if (int.TryParse(port, out var portNumber) && portNumber > 0)
                    {
                        smtp.Port = portNumber;
                    }
                    if (isAuthRequired)
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(username, password);
                    }

                    smtp.Send(message);
                }
            }
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, Strings.Error_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool IsValidHtmlColor(string htmlColor)
        {
            return Regex.IsMatch(htmlColor, "^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");
        }

        public static string EncryptStringAES(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            // WARNING: The key is hardcoded and published to GitHub. It's not truly secure.
            // The encryption process still provides local obfuscation.
            using (var key = new Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + Environment.MachineName, Encoding.ASCII.GetBytes(Environment.UserName + "@@vmping-salt@@")))
            using (var aes = Aes.Create())
            {
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.GenerateIV();
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(BitConverter.GetBytes(aes.IV.Length), 0, sizeof(int));
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8))
                    {
                        writer.Write(plainText);
                    }

                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public static string DecryptStringAES(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            try
            {
                var bytes = Convert.FromBase64String(cipherText);

                // WARNING: The key is hardcoded and published to GitHub. It's not truly secure.
                // The encryption process still provides local obfuscation.
                using (var key = new Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + Environment.MachineName, Encoding.ASCII.GetBytes(Environment.UserName + "@@vmping-salt@@")))
                using (var memoryStream = new MemoryStream(bytes))
                using (var aes = Aes.Create())
                {
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = ReadByteArray(memoryStream);
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cryptoStream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error decrypting value: " + ex.Message);
            }
        }

        private static byte[] ReadByteArray(Stream stream)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (stream.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

        public static string GetSafeFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return string.Empty;
            }

            // Manually check for invalid characters. The .NET Path.GetInvalidFileNameChars()
            // function is missing several invalid filename characters.
            char[] invalidCharacters = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
            return string.Join("_", filename.Split(invalidCharacters));
        }
    }
}
