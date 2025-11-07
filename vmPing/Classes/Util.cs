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
                throw new ArgumentNullException("plainText");
            }

            string encryptedString = null;                       // Encrypted string to return.
            RijndaelManaged aesAlgorithm = null;                 // RijndaelManaged object used to encrypt the data.

            try
            {
                // Generate the key from a shared secret and initilization vector.
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + Environment.MachineName, Encoding.ASCII.GetBytes(Environment.UserName + "@@vmping-salt@@"));

                // Create a RijndaelManaged object.
                aesAlgorithm = new RijndaelManaged();
                aesAlgorithm.Padding = PaddingMode.PKCS7;
                aesAlgorithm.Key = key.GetBytes(aesAlgorithm.KeySize / 8);

                key?.Dispose();

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);

                // Create the streams used for encryption.
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Prepend the IV.
                    memoryStream.Write(BitConverter.GetBytes(aesAlgorithm.IV.Length), 0, sizeof(int));
                    memoryStream.Write(aesAlgorithm.IV, 0, aesAlgorithm.IV.Length);
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            // Write all data to the stream.
                            streamWriter.Write(plainText);
                        }
                    }
                    encryptedString = Convert.ToBase64String(memoryStream.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                aesAlgorithm?.Clear();
                aesAlgorithm.Dispose();
            }

            // Return the encrypted bytes from the memory stream.
            return encryptedString;
        }

        public static string DecryptStringAES(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                throw new ArgumentNullException("cipherText");
            }

            // Declare the RijndaelManaged object used to decrypt the data.
            RijndaelManaged aesAlgorithm = null;

            // Declare the string used to hold the decrypted text.
            string plaintext = null;

            try
            {
                // Generate the key from a shared secret and initilization vector.
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + Environment.MachineName, Encoding.ASCII.GetBytes(Environment.UserName + "@@vmping-salt@@"));

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object with the specified key and IV.
                    aesAlgorithm = new RijndaelManaged();
                    aesAlgorithm.Padding = PaddingMode.PKCS7;
                    aesAlgorithm.Key = key.GetBytes(aesAlgorithm.KeySize / 8);
                    // Get the initialization vector from the encrypted stream.
                    aesAlgorithm.IV = ReadByteArray(memoryStream);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlgorithm.CreateDecryptor(aesAlgorithm.Key, aesAlgorithm.IV);
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))

                            // Read the decrypted bytes from the decrypting stream and place them in a string.
                            plaintext = streamReader.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                aesAlgorithm?.Clear();
            }

            return plaintext;
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
