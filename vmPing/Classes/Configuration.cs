using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using vmPing.Properties;
using vmPing.Views;

namespace vmPing.Classes
{
    class Configuration
    {
        public static string FilePath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");

        static Configuration()
        {
            // Constructor: Determine the location of vmPing configuration file.
            // If a configuration file is found in the current directory, update FilePath.
            // Otherwise the default location is used (%LocalAppData%\vmPing).
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "vmPing.xml"))
                FilePath = AppDomain.CurrentDomain.BaseDirectory + "vmPing.xml";
        }

        public static bool Exists()
        {
            return File.Exists(FilePath);
        }

        public static bool IsReady()
        {
            if (!File.Exists(FilePath))
            {
                // Configuration file does not exist. Prompt if and where file should be created.
                var newConfigWindow = new NewConfigurationWindow();

                // Try to determine window that should be owner. Is there a better way?
                // Currently checks owned windows of main window, plus owned windows of any child windows.
                var mainOwnedWindows = Application.Current.MainWindow.OwnedWindows;
                if (mainOwnedWindows.Count > 0)
                {
                    newConfigWindow.Owner = mainOwnedWindows[0].OwnedWindows.Count > 0
                        ? mainOwnedWindows[0].OwnedWindows[0]
                        : mainOwnedWindows[0];
                }
                else
                {
                    newConfigWindow.Owner = Application.Current.MainWindow;
                }

                // Display new config prompt. Configuration.FilePath is updated if user chooses portable mode.
                if (newConfigWindow.ShowDialog() == false)
                {
                    // User decided not to create a configuration file.
                    return false;
                }

                // Create directory if it also doesn't exist.
                if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                    catch (Exception ex)
                    {
                        Util.ShowError($"{Strings.Error_CreateDirectory} {ex.Message}");
                        return false;
                    }
                }

                // Directory should be ready. Create a basic XML configuration file.
                try
                {
                    var xd = new XDocument(
                        new XElement("vmping",
                            new XElement("aliases"),
                            new XElement("favorites"),
                            new XElement("configuration"),
                            new XElement("colors")));
                    xd.Save(FilePath);
                }
                catch (Exception ex)
                {
                    Util.ShowError($"{Strings.Error_CreateConfig} {ex.Message}");
                    return false;
                }
            }

            return true;
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
                // Open XML configuration file and get root <vmping> node.
                var xd = XDocument.Load(FilePath);
                XElement rootNode = xd.Element("vmping");
                
                // Generate new <configuration> and <colors> nodes.
                XElement configuration = GenerateConfigurationNode();
                XElement colors = GenerateColorsNode();

                // Delete old <configuration> and <colors> nodes from XML file (they will be recreated).
                rootNode.Descendants("configuration").Remove();
                rootNode.Descendants("colors").Remove();

                // Add the newly generated <configuration> and <colors> nodes, and then save the XML file.
                rootNode.Add(configuration);
                rootNode.Add(colors);
                xd.Save(FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }

        private static XElement GenerateConfigurationNode()
        {
            // In XML, options are written as:
            // <configuration>
            //   <option name="MyOptionName">myValue</option>
            // </configuration>
            XElement configuration = new XElement("configuration");
            configuration.Add(
                new XElement("option",
                    new XAttribute("name", "PingInterval"),
                    ApplicationOptions.PingInterval),
                new XElement("option",
                    new XAttribute("name", "PingTimeout"),
                    ApplicationOptions.PingTimeout),
                new XElement("option",
                    new XAttribute("name", "TTL"),
                    ApplicationOptions.TTL),
                new XElement("option",
                    new XAttribute("name", "DontFragment"),
                    ApplicationOptions.DontFragment),
                new XElement("option",
                    new XAttribute("name", "UseCustomBuffer"),
                    ApplicationOptions.UseCustomBuffer),
                new XElement("option",
                    new XAttribute("name", "Buffer"),
                    Encoding.ASCII.GetString(ApplicationOptions.Buffer)),
                new XElement("option",
                    new XAttribute("name", "AlertThreshold"),
                    ApplicationOptions.AlertThreshold),
                new XComment(" [InitialStartMode] Blank, MultiInput, Favorite "),
                new XElement("option",
                    new XAttribute("name", "InitialStartMode"),
                    ApplicationOptions.InitialStartMode),
                new XElement("option",
                    new XAttribute("name", "InitialProbeCount"),
                    ApplicationOptions.InitialProbeCount),
                new XElement("option",
                    new XAttribute("name", "InitialColumnCount"),
                    ApplicationOptions.InitialColumnCount),
                new XElement("option",
                    new XAttribute("name", "InitialFavorite"),
                    ApplicationOptions.InitialFavorite ?? string.Empty),
                new XComment(" [PopupNotifications] Always, Never, WhenMinimized "),
                new XElement("option",
                    new XAttribute("name", "PopupNotifications"),
                    ApplicationOptions.PopupOption),
                new XElement("option",
                    new XAttribute("name", "IsAutoDismissEnabled"),
                    ApplicationOptions.IsAutoDismissEnabled),
                new XElement("option",
                    new XAttribute("name", "AutoDismissMilliseconds"),
                    ApplicationOptions.AutoDismissMilliseconds),
                new XElement("option",
                    new XAttribute("name", "IsEmailAlertEnabled"),
                    ApplicationOptions.IsEmailAlertEnabled),
                new XElement("option",
                    new XAttribute("name", "EmailServer"),
                    ApplicationOptions.EmailServer ?? string.Empty),
                new XElement("option",
                    new XAttribute("name", "EmailPort"),
                    ApplicationOptions.EmailPort ?? string.Empty),
                new XElement("option",
                    new XAttribute("name", "IsEmailSslEnabled"),
                    ApplicationOptions.IsEmailSslEnabled),
                new XElement("option",
                    new XAttribute("name", "IsEmailAuthenticationRequired"),
                    ApplicationOptions.IsEmailAuthenticationRequired),
                new XElement("option",
                    new XAttribute("name", "EmailUser"),
                    string.IsNullOrWhiteSpace(ApplicationOptions.EmailUser)
                        ? string.Empty
                        : Util.EncryptStringAES(ApplicationOptions.EmailUser)),
                new XElement("option",
                    new XAttribute("name", "EmailPassword"),
                    string.IsNullOrWhiteSpace(ApplicationOptions.EmailPassword)
                        ? string.Empty
                        : Util.EncryptStringAES(ApplicationOptions.EmailPassword)),
                new XElement("option",
                    new XAttribute("name", "EmailRecipient"),
                    ApplicationOptions.EmailRecipient ?? string.Empty),
                new XElement("option",
                    new XAttribute("name", "EmailFromAddress"),
                    ApplicationOptions.EmailFromAddress ?? string.Empty),
                new XElement("option",
                    new XAttribute("name", "IsAudioUpAlertEnabled"),
                    ApplicationOptions.IsAudioUpAlertEnabled),
                new XElement("option",
                    new XAttribute("name", "AudioUpFilePath"),
                    ApplicationOptions.AudioUpFilePath ?? string.Empty),
                new XElement("option",
                    new XAttribute("name", "IsAudioDownAlertEnabled"),
                    ApplicationOptions.IsAudioDownAlertEnabled),
                new XElement("option",
                    new XAttribute("name", "AudioDownFilePath"),
                    ApplicationOptions.AudioDownFilePath ?? string.Empty),
                new XElement("option",
                    new XAttribute("name", "IsLogOutputEnabled"),
                    ApplicationOptions.IsLogOutputEnabled),
                new XElement("option",
                    new XAttribute("name", "LogPath"),
                    ApplicationOptions.LogPath ?? string.Empty),
                new XElement("option",
                    new XAttribute("name", "IsLogStatusChangesEnabled"),
                    ApplicationOptions.IsLogStatusChangesEnabled),
                new XElement("option",
                    new XAttribute("name", "LogStatusChangesPath"),
                    ApplicationOptions.LogStatusChangesPath ?? string.Empty),
                new XElement("option",
                    new XAttribute("name", "IsAlwaysOnTopEnabled"),
                    ApplicationOptions.IsAlwaysOnTopEnabled),
                new XElement("option",
                    new XAttribute("name", "IsMinimizeToTrayEnabled"),
                    ApplicationOptions.IsMinimizeToTrayEnabled),
                new XElement("option",
                    new XAttribute("name", "IsExitToTrayEnabled"),
                    ApplicationOptions.IsExitToTrayEnabled),
                new XElement("option",
                    new XAttribute("name", "IsChangeTrayIconColorEnabled"),
                    ApplicationOptions.IsChangeTrayIconColorEnabled));

            return configuration;
        }

        private static XElement GenerateColorsNode()
        {
            // In XML, options are written as:
            // <colors>
            //   <option name="MyOptionName">myValue</option>
            // </colors>
            XElement colors = new XElement("colors");
            colors.Add(
                // Probe background colors.
                new XComment(" Probe background "),
                new XElement("option",
                    new XAttribute("name", "Probe.Background.Inactive"),
                    ApplicationOptions.BackgroundColor_Probe_Inactive),
                new XElement("option",
                    new XAttribute("name", "Probe.Background.Up"),
                    ApplicationOptions.BackgroundColor_Probe_Up),
                new XElement("option",
                    new XAttribute("name", "Probe.Background.Down"),
                    ApplicationOptions.BackgroundColor_Probe_Down),
                new XElement("option",
                    new XAttribute("name", "Probe.Background.Indeterminate"),
                    ApplicationOptions.BackgroundColor_Probe_Indeterminate),
                new XElement("option",
                    new XAttribute("name", "Probe.Background.Error"),
                    ApplicationOptions.BackgroundColor_Probe_Error),
                // Probe foreground colors.
                new XComment(" Probe foreground "),
                new XElement("option",
                    new XAttribute("name", "Probe.Foreground.Inactive"),
                    ApplicationOptions.ForegroundColor_Probe_Inactive),
                new XElement("option",
                    new XAttribute("name", "Probe.Foreground.Up"),
                    ApplicationOptions.ForegroundColor_Probe_Up),
                new XElement("option",
                    new XAttribute("name", "Probe.Foreground.Down"),
                    ApplicationOptions.ForegroundColor_Probe_Down),
                new XElement("option",
                    new XAttribute("name", "Probe.Foreground.Indeterminate"),
                    ApplicationOptions.ForegroundColor_Probe_Indeterminate),
                new XElement("option",
                    new XAttribute("name", "Probe.Foreground.Error"),
                    ApplicationOptions.ForegroundColor_Probe_Error),
                // Statisitcs foreground colors.
                new XComment(" Statistics foreground "),
                new XElement("option",
                    new XAttribute("name", "Statistics.Foreground.Inactive"),
                    ApplicationOptions.ForegroundColor_Stats_Inactive),
                new XElement("option",
                    new XAttribute("name", "Statistics.Foreground.Up"),
                    ApplicationOptions.ForegroundColor_Stats_Up),
                new XElement("option",
                    new XAttribute("name", "Statistics.Foreground.Down"),
                    ApplicationOptions.ForegroundColor_Stats_Down),
                new XElement("option",
                    new XAttribute("name", "Statistics.Foreground.Indeterminate"),
                    ApplicationOptions.ForegroundColor_Stats_Indeterminate),
                new XElement("option",
                    new XAttribute("name", "Statistics.Foreground.Error"),
                    ApplicationOptions.ForegroundColor_Stats_Error),
                // Alias foreground colors.
                new XComment(" Alias foreground "),
                new XElement("option",
                    new XAttribute("name", "Alias.Foreground.Inactive"),
                    ApplicationOptions.ForegroundColor_Alias_Inactive),
                new XElement("option",
                    new XAttribute("name", "Alias.Foreground.Up"),
                    ApplicationOptions.ForegroundColor_Alias_Up),
                new XElement("option",
                    new XAttribute("name", "Alias.Foreground.Down"),
                    ApplicationOptions.ForegroundColor_Alias_Down),
                new XElement("option",
                    new XAttribute("name", "Alias.Foreground.Indeterminate"),
                    ApplicationOptions.ForegroundColor_Alias_Indeterminate),
                new XElement("option",
                    new XAttribute("name", "Alias.Foreground.Error"),
                    ApplicationOptions.ForegroundColor_Alias_Error));

            return colors;
        }

        public static void Load()
        {
            if (!Exists())
                return;

            try
            {
                var xd = new XmlDocument();
                xd.Load(FilePath);

                LoadConfigurationNode(xd.SelectNodes("/vmping/configuration/option"));
                LoadColorsNode(xd.SelectNodes("/vmping/colors/option"));
                ApplicationOptions.UpdatePingOptions();
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
            if (options.TryGetValue("InitialStartMode", out optionValue))
            {
                if (optionValue.Equals(ApplicationOptions.StartMode.Favorite.ToString()))
                    ApplicationOptions.InitialStartMode = ApplicationOptions.StartMode.Favorite;
                else if (optionValue.Equals(ApplicationOptions.StartMode.MultiInput.ToString()))
                    ApplicationOptions.InitialStartMode = ApplicationOptions.StartMode.MultiInput;
                else
                    ApplicationOptions.InitialStartMode = ApplicationOptions.StartMode.Blank;
            }
            if (options.TryGetValue("InitialProbeCount", out optionValue))
            {
                ApplicationOptions.InitialProbeCount = int.Parse(optionValue);
            }
            if (options.TryGetValue("InitialColumnCount", out optionValue))
            {
                ApplicationOptions.InitialColumnCount = int.Parse(optionValue);
            }
            if (options.TryGetValue("InitialFavorite", out optionValue))
            {
                ApplicationOptions.InitialFavorite = optionValue;
            }
            if (options.TryGetValue("PopupNotifications", out optionValue))
            {
                if (optionValue.Equals(ApplicationOptions.PopupNotificationOption.Always.ToString()))
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Always;
                else if (optionValue.Equals(ApplicationOptions.PopupNotificationOption.WhenMinimized.ToString()))
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.WhenMinimized;
                else
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Never;
            }
            if (options.TryGetValue("IsAutoDismissEnabled", out optionValue))
            {
                ApplicationOptions.IsAutoDismissEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("AutoDismissMilliseconds", out optionValue))
            {
                ApplicationOptions.AutoDismissMilliseconds = int.Parse(optionValue);
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
            if (options.TryGetValue("IsEmailSslEnabled", out optionValue))
            {
                ApplicationOptions.IsEmailSslEnabled = bool.Parse(optionValue);
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
            if (options.TryGetValue("IsAlwaysOnTopEnabled", out optionValue))
            {
                ApplicationOptions.IsAlwaysOnTopEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("IsMinimizeToTrayEnabled", out optionValue))
            {
                ApplicationOptions.IsMinimizeToTrayEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("IsExitToTrayEnabled", out optionValue))
            {
                ApplicationOptions.IsExitToTrayEnabled = bool.Parse(optionValue);
            }
            if (options.TryGetValue("IsChangeTrayIconColorEnabled", out optionValue))
            {
                ApplicationOptions.IsChangeTrayIconColorEnabled = bool.Parse(optionValue);
            }
        }
    }
}
