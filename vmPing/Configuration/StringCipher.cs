using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace vmPing.Configuration
{
    // Obfuscates the SMTP credentials stored in the configuration file.
    internal static class StringCipher
    {
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            // WARNING: The key is hardcoded and published to GitHub. It's not truly secure.
            // The encryption process still provides local obfuscation.
            using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + Environment.MachineName, Encoding.ASCII.GetBytes(Environment.UserName + "@@vmping-salt@@")))
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.GenerateIV();
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(BitConverter.GetBytes(aes.IV.Length), 0, sizeof(int));
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter writer = new StreamWriter(cryptoStream, Encoding.UTF8))
                    {
                        writer.Write(plainText);
                    }

                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(cipherText);

                // WARNING: The key is hardcoded and published to GitHub. It's not truly secure.
                // The encryption process still provides local obfuscation.
                using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes("https://github.com/R-Smith/vmPing" + Environment.MachineName, Encoding.ASCII.GetBytes(Environment.UserName + "@@vmping-salt@@")))
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = ReadByteArray(memoryStream);
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader reader = new StreamReader(cryptoStream, Encoding.UTF8))
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
    }
}
