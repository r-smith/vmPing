using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

namespace vmPing.Classes
{
    class Configuration
    {
        public static bool CheckAndInitializeConfigurationFile()
        {
            var rootPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing");
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
            if (!Directory.Exists(rootPath))
                try
                {
                    Directory.CreateDirectory(rootPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create directory for vmPing configuration file.  " + ex.Message);
                    return false;
                }

            if (!File.Exists(path))
            {
                try
                {
                    // Generate new xml configuration file.
                    var xmlFile = new XmlDocument();
                    var rootNode = xmlFile.CreateElement("vmping");
                    xmlFile.AppendChild(rootNode);

                    rootNode.AppendChild(xmlFile.CreateElement("aliases"));
                    rootNode.AppendChild(xmlFile.CreateElement("configuration"));
                    rootNode.AppendChild(xmlFile.CreateElement("favorites"));

                    xmlFile.Save(path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create vmPing configuration file.  " + ex.Message);
                    return false;
                }
            }

            return true;
        }

        public static void UpgradeConfigurationFile()
        {
            var rootPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing");
            var oldPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            var newPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");

            if (!Directory.Exists(rootPath))
                return;
            if (File.Exists(newPath))
                return;
            if (!File.Exists(oldPath))
                return;
            else
            {
                // Upgrade old configuration file.
                var newXmlFile = new XmlDocument();
                var newRootNode = newXmlFile.CreateElement("vmping");
                newXmlFile.AppendChild(newRootNode);

                var oldXmlFile = new XmlDocument();
                oldXmlFile.Load(oldPath);
                var oldRootNode = oldXmlFile.FirstChild;

                newRootNode.AppendChild(newXmlFile.CreateElement("aliases"));
                newRootNode.AppendChild(newXmlFile.CreateElement("configuration"));
                newRootNode.AppendChild(newXmlFile.ImportNode(oldRootNode, true));

                newXmlFile.Save(newPath);
            }
        }


        public static string GetEscapedXpath(string xpath)
        {
            if (!xpath.Contains("'"))
                return '\'' + xpath + '\'';
            else if (!xpath.Contains("\""))
                return '"' + xpath + '"';
            else
                return "concat('" + xpath.Replace("'", "',\"'\",'") + "')";
        }


        public static void WriteConfigurationOptions()
        {
            if (Configuration.CheckAndInitializeConfigurationFile() == false)
                return;

            try
            {
                var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeRoot = xd.SelectSingleNode("/vmping");

                // Check if configuration node already exists.  If so, delete it.
                XmlNodeList nodeSearch = xd.SelectNodes($"/vmping/configuration");
                foreach (XmlNode node in nodeSearch)
                {
                    nodeRoot.RemoveChild(node);
                }

                // Check if colors node already exists.  If so, delete it.
                nodeSearch = xd.SelectNodes($"/vmping/colors");
                foreach (XmlNode node in nodeSearch)
                {
                    nodeRoot.RemoveChild(node);
                }

                XmlElement configuration = GenerateConfigurationNode(xd);
                XmlElement colors = GenerateColorsNode(xd);

                nodeRoot.AppendChild(configuration);
                nodeRoot.AppendChild(colors);
                xd.Save(path);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private static XmlElement GenerateColorsNode(XmlDocument xd)
        {
            XmlElement colors = xd.CreateElement("colors");

            // Write probe background colors.
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Background.Inactive",
                value: ApplicationOptions.BackgroundColor_Probe_Inactive));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Background.Up",
                value: ApplicationOptions.BackgroundColor_Probe_Up));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Background.Down",
                value: ApplicationOptions.BackgroundColor_Probe_Down));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Background.Indeterminate",
                value: ApplicationOptions.BackgroundColor_Probe_Indeterminate));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Background.Error",
                value: ApplicationOptions.BackgroundColor_Probe_Error));

            // Write probe foreground colors.
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Foreground.Inactive",
                value: ApplicationOptions.ForegroundColor_Probe_Inactive));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Foreground.Up",
                value: ApplicationOptions.ForegroundColor_Probe_Up));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Foreground.Down",
                value: ApplicationOptions.ForegroundColor_Probe_Down));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Foreground.Indeterminate",
                value: ApplicationOptions.ForegroundColor_Probe_Indeterminate));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Probe.Foreground.Error",
                value: ApplicationOptions.ForegroundColor_Probe_Error));

            // Write statistics foreground colors.
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Statistics.Foreground.Inactive",
                value: ApplicationOptions.ForegroundColor_Stats_Inactive));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Statistics.Foreground.Up",
                value: ApplicationOptions.ForegroundColor_Stats_Up));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Statistics.Foreground.Down",
                value: ApplicationOptions.ForegroundColor_Stats_Down));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Statistics.Foreground.Indeterminate",
                value: ApplicationOptions.ForegroundColor_Stats_Indeterminate));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Statistics.Foreground.Error",
                value: ApplicationOptions.ForegroundColor_Stats_Error));

            // Write alias foreground colors.
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Alias.Foreground.Inactive",
                value: ApplicationOptions.ForegroundColor_Alias_Inactive));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Alias.Foreground.Up",
                value: ApplicationOptions.ForegroundColor_Alias_Up));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Alias.Foreground.Down",
                value: ApplicationOptions.ForegroundColor_Alias_Down));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Alias.Foreground.Indeterminate",
                value: ApplicationOptions.ForegroundColor_Alias_Indeterminate));
            colors.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Alias.Foreground.Error",
                value: ApplicationOptions.ForegroundColor_Alias_Error));

            return colors;
        }

        private static XmlElement GenerateConfigurationNode(XmlDocument xd)
        {
            XmlElement configuration = xd.CreateElement("configuration");
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "PingInterval",
                value: ApplicationOptions.PingInterval.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "PingTimeout",
                value: ApplicationOptions.PingTimeout.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "TTL",
                value: ApplicationOptions.TTL.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "DontFragment",
                value: ApplicationOptions.DontFragment.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "UseCustomBuffer",
                value: ApplicationOptions.UseCustomBuffer.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "Buffer",
                value: Encoding.ASCII.GetString(ApplicationOptions.Buffer)));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "AlertThreshold",
                value: ApplicationOptions.AlertThreshold.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "IsEmailAlertEnabled",
                value: ApplicationOptions.IsEmailAlertEnabled.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "EmailServer",
                value: ApplicationOptions.EmailServer != null ? ApplicationOptions.EmailServer.ToString() : string.Empty));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "EmailPort",
                value: ApplicationOptions.EmailPort != null ? ApplicationOptions.EmailPort.ToString() : string.Empty));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "IsEmailAuthenticationRequired",
                value: ApplicationOptions.IsEmailAuthenticationRequired.ToString()));

            if (!string.IsNullOrWhiteSpace(ApplicationOptions.EmailUser))
            {
                configuration.AppendChild(GenerateOptionNode(
                    xmlDocument: xd,
                    name: "EmailUser",
                    value: EncryptStringAES(ApplicationOptions.EmailUser)));
            }
            else
            {
                configuration.AppendChild(GenerateOptionNode(
                    xmlDocument: xd,
                    name: "EmailUser",
                    value: string.Empty));
            }

            if (!string.IsNullOrWhiteSpace(ApplicationOptions.EmailPassword))
            {
                configuration.AppendChild(GenerateOptionNode(
                    xmlDocument: xd,
                    name: "EmailPassword",
                    value: EncryptStringAES(ApplicationOptions.EmailPassword)));
            }
            else
            {
                configuration.AppendChild(GenerateOptionNode(
                    xmlDocument: xd,
                    name: "EmailPassword",
                    value: string.Empty));
            }

            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "EmailRecipient",
                value: ApplicationOptions.EmailRecipient != null ? ApplicationOptions.EmailRecipient.ToString() : string.Empty));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "EmailFromAddress",
                value: ApplicationOptions.EmailFromAddress != null ? ApplicationOptions.EmailFromAddress.ToString() : string.Empty));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "IsLogOutputEnabled",
                value: ApplicationOptions.IsLogOutputEnabled.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "LogPath",
                value: ApplicationOptions.LogPath != null ? ApplicationOptions.LogPath.ToString() : string.Empty));

            return configuration;
        }

        private static XmlElement GenerateOptionNode(XmlDocument xmlDocument, string name, string value)
        {
            XmlElement option = xmlDocument.CreateElement("option");
            option.SetAttribute("name", name);
            option.InnerText = value;

            return option;
        }


        private static string EncryptStringAES(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");

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
                if (aesAlgorithm != null)
                    aesAlgorithm.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return encryptedString;
        }


        public static string DecryptStringAES(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");

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
                if (aesAlgorithm != null)
                    aesAlgorithm.Clear();
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


        public static void LoadConfigurationOptions()
        {


            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
            if (!File.Exists(path))
                return;

            try
            {
                var xd = new XmlDocument();
                xd.Load(path);

                LoadConfigurationNode(xd.SelectNodes("/vmping/configuration/option"));
                LoadColorsNode(xd.SelectNodes("/vmping/colors/option"));
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private static void LoadColorsNode(XmlNodeList nodeList)
        {
            var options = new Dictionary<string, string>();

            foreach (XmlNode node in nodeList)
                options.Add(node.Attributes["name"].Value, node.InnerText);

            string optionValue;
            // Load probe backgorund colors.
            if (options.TryGetValue("Probe.Background.Inactive", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Inactive = optionValue;
            }
            if (options.TryGetValue("Probe.Background.Up", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Up = optionValue;
            }
            if (options.TryGetValue("Probe.Background.Down", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Down = optionValue;
            }
            if (options.TryGetValue("Probe.Background.Indeterminate", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Indeterminate = optionValue;
            }
            if (options.TryGetValue("Probe.Background.Error", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Error = optionValue;
            }

            // Load probe foreground colors.
            if (options.TryGetValue("Probe.Foreground.Inactive", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Inactive = optionValue;
            }
            if (options.TryGetValue("Probe.Foreground.Up", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Up = optionValue;
            }
            if (options.TryGetValue("Probe.Foreground.Down", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Down = optionValue;
            }
            if (options.TryGetValue("Probe.Foreground.Indeterminate", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Indeterminate = optionValue;
            }
            if (options.TryGetValue("Probe.Foreground.Error", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Error = optionValue;
            }

            // Load statistics foreground colors.
            if (options.TryGetValue("Statistics.Foreground.Inactive", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Inactive = optionValue;
            }
            if (options.TryGetValue("Statistics.Foreground.Up", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Up = optionValue;
            }
            if (options.TryGetValue("Statistics.Foreground.Down", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Down = optionValue;
            }
            if (options.TryGetValue("Statistics.Foreground.Indeterminate", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Indeterminate = optionValue;
            }
            if (options.TryGetValue("Statistics.Foreground.Error", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Error = optionValue;
            }

            // Load alias foreground colors.
            if (options.TryGetValue("Alias.Foreground.Inactive", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Inactive = optionValue;
            }
            if (options.TryGetValue("Alias.Foreground.Up", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Up = optionValue;
            }
            if (options.TryGetValue("Alias.Foreground.Down", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Down = optionValue;
            }
            if (options.TryGetValue("Alias.Foreground.Indeterminate", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Indeterminate = optionValue;
            }
            if (options.TryGetValue("Alias.Foreground.Error", out optionValue))
            {
                if (IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Error = optionValue;
            }
        }


        public static bool IsValidHtmlColor(string htmlColor)
        {
            var regex = new Regex("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");

            return regex.IsMatch(htmlColor);
        }


        private static void LoadConfigurationNode(XmlNodeList nodeList)
        {
            var options = new Dictionary<string, string>();

            foreach (XmlNode node in nodeList)
                options.Add(node.Attributes["name"].Value, node.InnerText);

            string optionValue;
            if (options.TryGetValue("PingInterval", out optionValue))
            {
                ApplicationOptions.PingInterval = int.Parse(optionValue);
            }
            if (options.TryGetValue("PingTimeout", out optionValue))
            {
                ApplicationOptions.PingTimeout = int.Parse(optionValue);
            }
            if (options.TryGetValue("TTL", out optionValue))
            {
                ApplicationOptions.TTL = int.Parse(optionValue);
            }
            if (options.TryGetValue("DontFragment", out optionValue))
            {
                ApplicationOptions.DontFragment = bool.Parse(optionValue);
            }
            if (options.TryGetValue("UseCustomBuffer", out optionValue))
            {
                ApplicationOptions.UseCustomBuffer = bool.Parse(optionValue);
            }
            if (options.TryGetValue("Buffer", out optionValue))
            {
                ApplicationOptions.Buffer = Encoding.ASCII.GetBytes(optionValue);
            }
            if (options.TryGetValue("AlertThreshold", out optionValue))
            {
                ApplicationOptions.AlertThreshold = int.Parse(optionValue);
            }
            if (options.TryGetValue("IsEmailAlertEnabled", out optionValue))
            {
                ApplicationOptions.IsEmailAlertEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("IsEmailAuthenticationRequired", out optionValue))
            {
                ApplicationOptions.IsEmailAuthenticationRequired = bool.Parse(optionValue);
            }
            if (options.TryGetValue("EmailServer", out optionValue))
            {
                ApplicationOptions.EmailServer = optionValue;
            }
            if (options.TryGetValue("EmailPort", out optionValue))
            {
                ApplicationOptions.EmailPort = optionValue;
            }
            if (options.TryGetValue("EmailRecipient", out optionValue))
            {
                ApplicationOptions.EmailRecipient = optionValue;
            }
            if (options.TryGetValue("EmailFromAddress", out optionValue))
            {
                ApplicationOptions.EmailFromAddress = optionValue;
            }
            if (options.TryGetValue("IsLogOutputEnabled", out optionValue))
            {
                ApplicationOptions.IsLogOutputEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("LogPath", out optionValue))
            {
                ApplicationOptions.LogPath = optionValue;
            }

            if (options.TryGetValue("EmailUser", out optionValue))
            {
                if (optionValue.Length > 0)
                    ApplicationOptions.EmailUser = DecryptStringAES(optionValue);
            }
            if (options.TryGetValue("EmailPassword", out optionValue))
            {
                if (optionValue.Length > 0)
                    ApplicationOptions.EmailPassword = DecryptStringAES(optionValue);
            }
        }
    }
}
