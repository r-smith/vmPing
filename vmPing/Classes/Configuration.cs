using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using vmPing.Properties;

namespace vmPing.Classes
{
    class Configuration
    {
        public static string Path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
        public static string ParentFolder = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing");
        public static string OldPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");

        public static bool Exists()
        {
            return File.Exists(Path);
        }

        public static bool IsReady()
        {
            if (!Directory.Exists(ParentFolder))
            {
                try
                {
                    Directory.CreateDirectory(ParentFolder);
                }
                catch (Exception ex)
                {
                    Util.ShowError($"{Strings.Error_CreateDirectory} {ex.Message}");
                    return false;
                }
            }

            if (!File.Exists(Path))
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

                    xmlFile.Save(Path);
                }
                catch (Exception ex)
                {
                    Util.ShowError($"{Strings.Error_CreateConfig} {ex.Message}");
                    return false;
                }
            }

            return true;
        }


        public static void UpgradeConfigurationFile()
        {
            if (!Directory.Exists(ParentFolder))
                return;
            if (File.Exists(Path))
                return;

            if (File.Exists(OldPath))
            {
                try
                {
                    // Upgrade old configuration file.
                    var newXmlFile = new XmlDocument();
                    var newRootNode = newXmlFile.CreateElement("vmping");
                    newXmlFile.AppendChild(newRootNode);

                    var oldXmlFile = new XmlDocument();
                    oldXmlFile.Load(OldPath);
                    var oldRootNode = oldXmlFile.FirstChild;

                    newRootNode.AppendChild(newXmlFile.CreateElement("aliases"));
                    newRootNode.AppendChild(newXmlFile.CreateElement("configuration"));
                    newRootNode.AppendChild(newXmlFile.ImportNode(oldRootNode, true));

                    newXmlFile.Save(Path);
                }
                catch (Exception ex)
                {
                    Util.ShowError($"{Strings.Error_UpgradeConfig} {ex.Message}");
                }
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
            if (IsReady() == false)
                return;

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.Path);

                // Check if configuration node already exists.  If so, delete it.
                XmlNode nodeRoot = xd.SelectSingleNode("/vmping");
                foreach (XmlNode node in xd.SelectNodes($"/vmping/configuration"))
                {
                    nodeRoot.RemoveChild(node);
                }

                // Check if colors node already exists.  If so, delete it.
                foreach (XmlNode node in xd.SelectNodes($"/vmping/colors"))
                {
                    nodeRoot.RemoveChild(node);
                }

                XmlElement configuration = GenerateConfigurationNode(xd);
                XmlElement colors = GenerateColorsNode(xd);

                nodeRoot.AppendChild(configuration);
                nodeRoot.AppendChild(colors);
                xd.Save(Configuration.Path);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
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
                    value: Util.EncryptStringAES(ApplicationOptions.EmailUser)));
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
                    value: Util.EncryptStringAES(ApplicationOptions.EmailPassword)));
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
                name: "IsAudioUpAlertEnabled",
                value: ApplicationOptions.IsAudioUpAlertEnabled.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "IsAudioDownAlertEnabled",
                value: ApplicationOptions.IsAudioDownAlertEnabled.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "AudioUpFilePath",
                value: ApplicationOptions.AudioUpFilePath != null ? ApplicationOptions.AudioUpFilePath.ToString() : string.Empty));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "AudioDownFilePath",
                value: ApplicationOptions.AudioDownFilePath != null ? ApplicationOptions.AudioDownFilePath.ToString() : string.Empty));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "IsLogOutputEnabled",
                value: ApplicationOptions.IsLogOutputEnabled.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "LogPath",
                value: ApplicationOptions.LogPath != null ? ApplicationOptions.LogPath.ToString() : string.Empty));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "IsLogStatusChangesEnabled",
                value: ApplicationOptions.IsLogStatusChangesEnabled.ToString()));
            configuration.AppendChild(GenerateOptionNode(
                xmlDocument: xd,
                name: "LogStatusChangesPath",
                value: ApplicationOptions.LogStatusChangesPath != null ? ApplicationOptions.LogStatusChangesPath.ToString() : string.Empty));

            return configuration;
        }


        private static XmlElement GenerateOptionNode(XmlDocument xmlDocument, string name, string value)
        {
            XmlElement option = xmlDocument.CreateElement("option");
            option.SetAttribute("name", name);
            option.InnerText = value;

            return option;
        }


        public static void Load()
        {
            if (!Exists())
                return;

            try
            {
                var xd = new XmlDocument();
                xd.Load(Path);

                LoadConfigurationNode(xd.SelectNodes("/vmping/configuration/option"));
                LoadColorsNode(xd.SelectNodes("/vmping/colors/option"));
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_LoadConfig} {ex.Message}");
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
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Inactive = optionValue;
            }
            if (options.TryGetValue("Probe.Background.Up", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Up = optionValue;
            }
            if (options.TryGetValue("Probe.Background.Down", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Down = optionValue;
            }
            if (options.TryGetValue("Probe.Background.Indeterminate", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Indeterminate = optionValue;
            }
            if (options.TryGetValue("Probe.Background.Error", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.BackgroundColor_Probe_Error = optionValue;
            }

            // Load probe foreground colors.
            if (options.TryGetValue("Probe.Foreground.Inactive", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Inactive = optionValue;
            }
            if (options.TryGetValue("Probe.Foreground.Up", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Up = optionValue;
            }
            if (options.TryGetValue("Probe.Foreground.Down", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Down = optionValue;
            }
            if (options.TryGetValue("Probe.Foreground.Indeterminate", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Indeterminate = optionValue;
            }
            if (options.TryGetValue("Probe.Foreground.Error", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Probe_Error = optionValue;
            }

            // Load statistics foreground colors.
            if (options.TryGetValue("Statistics.Foreground.Inactive", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Inactive = optionValue;
            }
            if (options.TryGetValue("Statistics.Foreground.Up", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Up = optionValue;
            }
            if (options.TryGetValue("Statistics.Foreground.Down", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Down = optionValue;
            }
            if (options.TryGetValue("Statistics.Foreground.Indeterminate", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Indeterminate = optionValue;
            }
            if (options.TryGetValue("Statistics.Foreground.Error", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Stats_Error = optionValue;
            }

            // Load alias foreground colors.
            if (options.TryGetValue("Alias.Foreground.Inactive", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Inactive = optionValue;
            }
            if (options.TryGetValue("Alias.Foreground.Up", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Up = optionValue;
            }
            if (options.TryGetValue("Alias.Foreground.Down", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Down = optionValue;
            }
            if (options.TryGetValue("Alias.Foreground.Indeterminate", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Indeterminate = optionValue;
            }
            if (options.TryGetValue("Alias.Foreground.Error", out optionValue))
            {
                if (Util.IsValidHtmlColor(optionValue))
                    ApplicationOptions.ForegroundColor_Alias_Error = optionValue;
            }
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
            if (options.TryGetValue("IsAudioAlertEnabled", out optionValue))
            {
                // For compatibility with version 1.3.4 and lower.
                ApplicationOptions.IsAudioDownAlertEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("IsAudioUpAlertEnabled", out optionValue))
            {
                ApplicationOptions.IsAudioUpAlertEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("IsAudioDownAlertEnabled", out optionValue))
            {
                ApplicationOptions.IsAudioDownAlertEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("AudioFilePath", out optionValue))
            {
                // For compatibility with version 1.3.4 and lower.
                ApplicationOptions.AudioDownFilePath = optionValue;
            }
            if (options.TryGetValue("AudioUpFilePath", out optionValue))
            {
                ApplicationOptions.AudioUpFilePath = optionValue;
            }
            if (options.TryGetValue("AudioDownFilePath", out optionValue))
            {
                ApplicationOptions.AudioDownFilePath = optionValue;
            }
            if (options.TryGetValue("IsLogOutputEnabled", out optionValue))
            {
                ApplicationOptions.IsLogOutputEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("LogPath", out optionValue))
            {
                ApplicationOptions.LogPath = optionValue;
            }
            if (options.TryGetValue("IsLogStatusChangesEnabled", out optionValue))
            {
                ApplicationOptions.IsLogStatusChangesEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("LogStatusChangesPath", out optionValue))
            {
                ApplicationOptions.LogStatusChangesPath = optionValue;
            }

            if (options.TryGetValue("EmailUser", out optionValue))
            {
                if (optionValue.Length > 0)
                    ApplicationOptions.EmailUser = Util.DecryptStringAES(optionValue);
            }
            if (options.TryGetValue("EmailPassword", out optionValue))
            {
                if (optionValue.Length > 0)
                    ApplicationOptions.EmailPassword = Util.DecryptStringAES(optionValue);
            }
        }
    }
}
