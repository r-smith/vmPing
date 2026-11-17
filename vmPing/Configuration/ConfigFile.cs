using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using vmPing.Properties;
using vmPing.Views;

namespace vmPing.Configuration
{
    internal static class ConfigFile
    {
        public static string FilePath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");

        static ConfigFile()
        {
            // If a configuration file exists in the current directory, use it (portable mode).
            // Otherwise, the default location is used (%LocalAppData%\vmPing).
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "vmPing.xml"))
            {
                FilePath = AppDomain.CurrentDomain.BaseDirectory + "vmPing.xml";
            }
        }

        public static bool Exists()
        {
            return File.Exists(FilePath);
        }

        public static bool IsReady()
        {
            if (Exists())
            {
                return true;
            }

            // No configuration found.
            // Prompt the user to create a new configuration file.
            NewConfigurationWindow newConfigWindow = new NewConfigurationWindow();

            // Find the best owner for the new window.
            // Checks: Owned windows of MainWindow, plus owned windows of any child windows.
            WindowCollection mainOwnedWindows = Application.Current.MainWindow.OwnedWindows;
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

            // Display the new configuration window.
            // FilePath is updated if the user chooses portable mode.
            if (newConfigWindow.ShowDialog() == false)
            {
                // User cancelled.
                return false;
            }

            // Create the directory if it doesn't exist.
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

            // Create a minimal XML configuration file.
            try
            {
                XDocument xd = new XDocument(
                    new XElement("vmping",
                        new XElement("aliases"),
                        new XElement("favorites"),
                        new XElement("configuration"),
                        new XElement("colors")));
                WriteSafely(xd.Save);

                return true;
            }
            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_CreateConfig} {ex.Message}");
                return false;
            }
        }

        // Saves to a temporary file first, then replaces the config file with it.
        // If saving fails, the config file is left as it was.
        public static void WriteSafely(Action<string> save)
        {
            string tempPath = FilePath + ".tmp";
            try
            {
                save(tempPath);
                if (File.Exists(FilePath))
                {
                    File.Replace(tempPath, FilePath, null);
                }
                else
                {
                    File.Move(tempPath, FilePath);
                }
            }
            catch
            {
                File.Delete(tempPath);
                throw;
            }
        }

        public static string GetEscapedXpath(string xpath)
        {
            if (!xpath.Contains("'"))
            {
                return '\'' + xpath + '\'';
            }
            else if (!xpath.Contains("\""))
            {
                return '"' + xpath + '"';
            }
            else
            {
                return "concat('" + xpath.Replace("'", "',\"'\",'") + "')";
            }
        }

        public static void Save()
        {
            if (!IsReady())
            {
                return;
            }

            try
            {
                // Open XML configuration file and get root <vmping> node.
                XDocument xd = XDocument.Load(FilePath);
                XElement root = xd.Element("vmping")
                    ?? throw new XmlException("Invalid configuration file.");

                // Delete old nodes then recreate them with current config.
                root.Descendants("configuration").Remove();
                root.Descendants("colors").Remove();
                root.Add(GenerateConfigurationNode());
                root.Add(GenerateColorsNode());

                WriteSafely(xd.Save);
            }
            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }

        // Updates only the window size and position in the config file.
        public static void SaveWindowPlacement(bool saveSize, bool savePosition)
        {
            if (!Exists())
            {
                return;
            }

            try
            {
                XDocument xd = XDocument.Load(FilePath);
                XElement configuration = xd.Element("vmping")?.Element("configuration")
                    ?? throw new XmlException("Invalid configuration file.");

                if (saveSize)
                {
                    SetOption(configuration, "WindowWidth", ApplicationOptions.WindowWidth);
                    SetOption(configuration, "WindowHeight", ApplicationOptions.WindowHeight);
                    SetOption(configuration, "IsWindowMaximized", ApplicationOptions.IsWindowMaximized);
                }
                if (savePosition)
                {
                    SetOption(configuration, "WindowLeft", ApplicationOptions.WindowLeft);
                    SetOption(configuration, "WindowTop", ApplicationOptions.WindowTop);
                }

                WriteSafely(xd.Save);
            }
            catch
            {
                // Called while closing, so an error message isn't worth showing.
            }
        }

        private static void SetOption(XElement configuration, string name, object value)
        {
            XElement option = configuration.Elements("option")
                .FirstOrDefault(x => (string)x.Attribute("name") == name);
            if (option == null)
            {
                configuration.Add(new XElement("option", new XAttribute("name", name), value));
            }
            else
            {
                option.SetValue(value);
            }
        }

        private static XElement GenerateConfigurationNode()
        {
            // In XML, options are written as:
            // <configuration>
            //   <option name="MyOptionName">myValue</option>
            // </configuration>

            // Local function to create nodes.
            XElement Node(string name, object value) =>
                new XElement("option", new XAttribute("name", name), value ?? string.Empty);

            return new XElement("configuration",
                Node("PingInterval", ApplicationOptions.PingInterval),
                Node("PingTimeout", ApplicationOptions.PingTimeout),
                Node("TTL", ApplicationOptions.TTL),
                Node("DontFragment", ApplicationOptions.DontFragment),
                Node("UseCustomPacketData", ApplicationOptions.UseCustomPacketData),
                Node("PacketSize", ApplicationOptions.PacketSize),
                Node("CustomPacketData", ApplicationOptions.CustomPacketData),
                Node("AlertThreshold", ApplicationOptions.AlertThreshold),
                new XComment(" LatencyDetectionMode: [Off, Dynamic, Fixed] "),
                Node("LatencyDetectionMode", ApplicationOptions.LatencyDetectionMode),
                Node("HighLatencyMilliseconds", ApplicationOptions.HighLatencyMilliseconds),
                Node("HighLatencyAlertTriggerCount", ApplicationOptions.HighLatencyAlertTriggerCount),
                new XComment(" InitialStartMode: [Blank, MultiInput, Favorite] "),
                Node("InitialStartMode", ApplicationOptions.InitialStartMode),
                Node("InitialProbeCount", ApplicationOptions.InitialProbeCount),
                Node("InitialColumnCount", ApplicationOptions.InitialColumnCount),
                Node("InitialFavorite", ApplicationOptions.InitialFavorite),
                Node("IsStartMinimizedEnabled", ApplicationOptions.IsStartMinimizedEnabled),
                new XComment(" WindowSizeOption: [Remember, Fixed] "),
                Node("WindowSizeOption", ApplicationOptions.WindowSizeOption),
                Node("WindowWidth", ApplicationOptions.WindowWidth),
                Node("WindowHeight", ApplicationOptions.WindowHeight),
                Node("IsWindowMaximized", ApplicationOptions.IsWindowMaximized),
                new XComment(" WindowPositionOption: [Default, Remember, Fixed] "),
                Node("WindowPositionOption", ApplicationOptions.WindowPositionOption),
                Node("WindowLeft", ApplicationOptions.WindowLeft),
                Node("WindowTop", ApplicationOptions.WindowTop),
                new XComment(" PopupNotifications: [Always, Never, WhenMinimized] "),
                Node("PopupNotifications", ApplicationOptions.PopupOption),
                Node("IsAutoDismissEnabled", ApplicationOptions.IsAutoDismissEnabled),
                Node("AutoDismissMilliseconds", ApplicationOptions.AutoDismissMilliseconds),
                Node("IsEmailAlertEnabled", ApplicationOptions.IsEmailAlertEnabled),
                Node("EmailServer", ApplicationOptions.EmailServer),
                Node("EmailPort", ApplicationOptions.EmailPort),
                Node("IsEmailSslEnabled", ApplicationOptions.IsEmailSslEnabled),
                Node("IsEmailAuthenticationRequired", ApplicationOptions.IsEmailAuthenticationRequired),
                Node("EmailUser", string.IsNullOrWhiteSpace(ApplicationOptions.EmailUser)
                    ? string.Empty
                    : StringCipher.Encrypt(ApplicationOptions.EmailUser)),
                Node("EmailPassword", string.IsNullOrWhiteSpace(ApplicationOptions.EmailPassword)
                        ? string.Empty
                        : StringCipher.Encrypt(ApplicationOptions.EmailPassword)),
                Node("EmailRecipient", ApplicationOptions.EmailRecipient),
                Node("EmailFromAddress", ApplicationOptions.EmailFromAddress),
                Node("IsAudioUpAlertEnabled", ApplicationOptions.IsAudioUpAlertEnabled),
                Node("AudioUpFilePath", ApplicationOptions.AudioUpFilePath),
                Node("IsAudioDownAlertEnabled", ApplicationOptions.IsAudioDownAlertEnabled),
                Node("AudioDownFilePath", ApplicationOptions.AudioDownFilePath),
                Node("LogFolder", ApplicationOptions.LogFolder),
                Node("IsLogOutputEnabled", ApplicationOptions.IsLogOutputEnabled),
                Node("IsLogStatusChangesEnabled", ApplicationOptions.IsLogStatusChangesEnabled),
                Node("IsAlwaysOnTopEnabled", ApplicationOptions.IsAlwaysOnTopEnabled),
                Node("IsMinimizeToTrayEnabled", ApplicationOptions.IsMinimizeToTrayEnabled),
                Node("IsExitToTrayEnabled", ApplicationOptions.IsExitToTrayEnabled),
                Node("IsAlwaysShowTrayIconEnabled", ApplicationOptions.IsAlwaysShowTrayIconEnabled)
            );
        }

        private static XElement GenerateColorsNode()
        {
            // In XML, options are written as:
            // <colors>
            //   <option name="MyOptionName">myValue</option>
            // </colors>

            // Local function to create nodes.
            XElement Node(string name, object value) =>
                new XElement("option", new XAttribute("name", name), value ?? string.Empty);

            return new XElement("colors",
                new XComment(" Probe background "),
                Node("Probe.Background.Inactive", ApplicationOptions.ProbeBackgroundInactive),
                Node("Probe.Background.Up", ApplicationOptions.ProbeBackgroundUp),
                Node("Probe.Background.Down", ApplicationOptions.ProbeBackgroundDown),
                Node("Probe.Background.Indeterminate", ApplicationOptions.ProbeBackgroundIndeterminate),
                Node("Probe.Background.Error", ApplicationOptions.ProbeBackgroundError),

                new XComment(" Probe foreground "),
                Node("Probe.Foreground.Inactive", ApplicationOptions.ProbeForegroundInactive),
                Node("Probe.Foreground.Up", ApplicationOptions.ProbeForegroundUp),
                Node("Probe.Foreground.Down", ApplicationOptions.ProbeForegroundDown),
                Node("Probe.Foreground.Indeterminate", ApplicationOptions.ProbeForegroundIndeterminate),
                Node("Probe.Foreground.Error", ApplicationOptions.ProbeForegroundError),

                new XComment(" Statistics foreground "),
                Node("Statistics.Foreground.Inactive", ApplicationOptions.StatisticsForegroundInactive),
                Node("Statistics.Foreground.Up", ApplicationOptions.StatisticsForegroundUp),
                Node("Statistics.Foreground.Down", ApplicationOptions.StatisticsForegroundDown),
                Node("Statistics.Foreground.Indeterminate", ApplicationOptions.StatisticsForegroundIndeterminate),
                Node("Statistics.Foreground.Error", ApplicationOptions.StatisticsForegroundError)
            );
        }

        public static void Load()
        {
            if (!Exists())
            {
                return;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(FilePath);

                LoadAppOptions(xd.SelectNodes("/vmping/configuration/option"));
                LoadColors(xd.SelectNodes("/vmping/colors/option"));
                ApplicationOptions.UpdatePingOptions();
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_LoadConfig} {ex.Message}");
            }
        }

        private static void LoadColors(XmlNodeList nodes)
        {
            if (nodes == null)
            {
                return;
            }

            Dictionary<string, string> options = nodes.Cast<XmlNode>()
                .Where(n => n.Attributes?["name"] != null)
                .ToDictionary(n => n.Attributes["name"].Value, n => n.InnerText);

            // Load probe background colors.
            ApplyColor("Probe.Background.Inactive", v => ApplicationOptions.ProbeBackgroundInactive = v, options);
            ApplyColor("Probe.Background.Up", v => ApplicationOptions.ProbeBackgroundUp = v, options);
            ApplyColor("Probe.Background.Down", v => ApplicationOptions.ProbeBackgroundDown = v, options);
            ApplyColor("Probe.Background.Indeterminate", v => ApplicationOptions.ProbeBackgroundIndeterminate = v, options);
            ApplyColor("Probe.Background.Error", v => ApplicationOptions.ProbeBackgroundError = v, options);
            ApplyColor("Probe.Foreground.Inactive", v => ApplicationOptions.ProbeForegroundInactive = v, options);
            ApplyColor("Probe.Foreground.Up", v => ApplicationOptions.ProbeForegroundUp = v, options);
            ApplyColor("Probe.Foreground.Down", v => ApplicationOptions.ProbeForegroundDown = v, options);
            ApplyColor("Probe.Foreground.Indeterminate", v => ApplicationOptions.ProbeForegroundIndeterminate = v, options);
            ApplyColor("Probe.Foreground.Error", v => ApplicationOptions.ProbeForegroundError = v, options);
            ApplyColor("Statistics.Foreground.Inactive", v => ApplicationOptions.StatisticsForegroundInactive = v, options);
            ApplyColor("Statistics.Foreground.Up", v => ApplicationOptions.StatisticsForegroundUp = v, options);
            ApplyColor("Statistics.Foreground.Down", v => ApplicationOptions.StatisticsForegroundDown = v, options);
            ApplyColor("Statistics.Foreground.Indeterminate", v => ApplicationOptions.StatisticsForegroundIndeterminate = v, options);
            ApplyColor("Statistics.Foreground.Error", v => ApplicationOptions.StatisticsForegroundError = v, options);
        }

        private static void ApplyColor(string key, Action<string> setter, IDictionary<string, string> options)
        {
            if (options.TryGetValue(key, out string value) && Util.IsValidHtmlColor(value))
            {
                setter(value);
            }
        }

        private static void LoadAppOptions(XmlNodeList nodes)
        {
            if (nodes == null)
            {
                return;
            }

            Dictionary<string, string> options = nodes.Cast<XmlNode>()
                .Where(n => n.Attributes?["name"] != null)
                .ToDictionary(n => n.Attributes["name"].Value, n => n.InnerText);

            ApplyInt("PingInterval", 1000, 86400000, v => ApplicationOptions.PingInterval = v, options);
            ApplyInt("PingTimeout", 1000, 60000, v => ApplicationOptions.PingTimeout = v, options);
            ApplyInt("TTL", 1, 255, v => ApplicationOptions.TTL = v, options);
            ApplyBool("DontFragment", v => ApplicationOptions.DontFragment = v, options);
            ApplyBool("UseCustomPacketData", v => ApplicationOptions.UseCustomPacketData = v, options);
            ApplyInt("PacketSize", 0, 65500, v => ApplicationOptions.PacketSize = v, options);
            ApplyString("CustomPacketData", v => ApplicationOptions.CustomPacketData = v, options);
            ApplyInt("AlertThreshold", 1, 60, v => ApplicationOptions.AlertThreshold = v, options);
            ApplyEnum<LatencyMode>("LatencyDetectionMode", v => ApplicationOptions.LatencyDetectionMode = v, options);
            ApplyInt("HighLatencyMilliseconds", 1, 60000, v => ApplicationOptions.HighLatencyMilliseconds = v, options);
            ApplyInt("HighLatencyAlertTriggerCount", 1, 60, v => ApplicationOptions.HighLatencyAlertTriggerCount = v, options);
            ApplyEnum<StartMode>("InitialStartMode", v => ApplicationOptions.InitialStartMode = v, options);
            ApplyInt("InitialProbeCount", 1, 20, v => ApplicationOptions.InitialProbeCount = v, options);
            ApplyInt("InitialColumnCount", 1, 10, v => ApplicationOptions.InitialColumnCount = v, options);
            ApplyString("InitialFavorite", v => ApplicationOptions.InitialFavorite = v, options);
            ApplyBool("IsStartMinimizedEnabled", v => ApplicationOptions.IsStartMinimizedEnabled = v, options);
            ApplyEnum<WindowStartupOption>("WindowSizeOption", v => ApplicationOptions.WindowSizeOption = v, options);
            ApplyInt("WindowWidth", 350, 9999, v => ApplicationOptions.WindowWidth = v, options);
            ApplyInt("WindowHeight", 200, 9999, v => ApplicationOptions.WindowHeight = v, options);
            ApplyBool("IsWindowMaximized", v => ApplicationOptions.IsWindowMaximized = v, options);
            ApplyEnum<WindowStartupOption>("WindowPositionOption", v => ApplicationOptions.WindowPositionOption = v, options);
            ApplyInt("WindowLeft", -9999, 9999, v => ApplicationOptions.WindowLeft = v, options);
            ApplyInt("WindowTop", -9999, 9999, v => ApplicationOptions.WindowTop = v, options);
            ApplyEnum<PopupNotificationOption>("PopupNotifications", v => ApplicationOptions.PopupOption = v, options);
            ApplyBool("IsAutoDismissEnabled", v => ApplicationOptions.IsAutoDismissEnabled = v, options);
            ApplyInt("AutoDismissMilliseconds", 1000, 99000, v => ApplicationOptions.AutoDismissMilliseconds = v, options);
            ApplyBool("IsEmailAlertEnabled", v => ApplicationOptions.IsEmailAlertEnabled = v, options);
            ApplyBool("IsEmailAuthenticationRequired", v => ApplicationOptions.IsEmailAuthenticationRequired = v, options);
            ApplyString("EmailServer", v => ApplicationOptions.EmailServer = v, options);
            ApplyString("EmailPort", v => ApplicationOptions.EmailPort = v, options);
            ApplyBool("IsEmailSslEnabled", v => ApplicationOptions.IsEmailSslEnabled = v, options);
            ApplyString("EmailRecipient", v => ApplicationOptions.EmailRecipient = v, options);
            ApplyString("EmailFromAddress", v => ApplicationOptions.EmailFromAddress = v, options);

            // For compatibility with version 1.3.4 and lower.
            ApplyBool("IsAudioAlertEnabled", v => ApplicationOptions.IsAudioDownAlertEnabled = v, options);
            ApplyString("AudioFilePath", v => ApplicationOptions.AudioDownFilePath = v, options);

            ApplyBool("IsAudioUpAlertEnabled", v => ApplicationOptions.IsAudioUpAlertEnabled = v, options);
            ApplyBool("IsAudioDownAlertEnabled", v => ApplicationOptions.IsAudioDownAlertEnabled = v, options);
            ApplyString("AudioUpFilePath", v => ApplicationOptions.AudioUpFilePath = v, options);
            ApplyString("AudioDownFilePath", v => ApplicationOptions.AudioDownFilePath = v, options);
            // For compatibility with version 1.4.0 and lower, which had a separate path for each log.
            // The ping output folder is used if set, else the status log file's folder.
            ApplyString("LogStatusChangesPath", v => ApplicationOptions.LogFolder = GetFolderOrNull(v) ?? ApplicationOptions.LogFolder, options);
            ApplyString("LogPath", v => ApplicationOptions.LogFolder = string.IsNullOrEmpty(v) ? ApplicationOptions.LogFolder : v, options);

            ApplyString("LogFolder", v => ApplicationOptions.LogFolder = v, options);
            ApplyBool("IsLogOutputEnabled", v => ApplicationOptions.IsLogOutputEnabled = v, options);
            ApplyBool("IsLogStatusChangesEnabled", v => ApplicationOptions.IsLogStatusChangesEnabled = v, options);
            ApplyBool("IsAlwaysOnTopEnabled", v => ApplicationOptions.IsAlwaysOnTopEnabled = v, options);
            ApplyBool("IsMinimizeToTrayEnabled", v => ApplicationOptions.IsMinimizeToTrayEnabled = v, options);
            ApplyBool("IsExitToTrayEnabled", v => ApplicationOptions.IsExitToTrayEnabled = v, options);
            ApplyBool("IsAlwaysShowTrayIconEnabled", v => ApplicationOptions.IsAlwaysShowTrayIconEnabled = v, options);

            // Loaded last, since a value that can't be decrypted stops the load with an error.
            if (options.TryGetValue("EmailUser", out string emailUser) && emailUser.Length > 0)
            {
                ApplicationOptions.EmailUser = StringCipher.Decrypt(emailUser);
            }
            if (options.TryGetValue("EmailPassword", out string emailPassword) && emailPassword.Length > 0)
            {
                ApplicationOptions.EmailPassword = StringCipher.Decrypt(emailPassword);
            }
        }

        // The folder of a file path, or null if there isn't one.
        private static string GetFolderOrNull(string path)
        {
            try
            {
                string folder = Path.GetDirectoryName(path);
                return string.IsNullOrEmpty(folder) ? null : folder;
            }
            catch
            {
                return null;
            }
        }

        private static void ApplyString(string key, Action<string> setter, IDictionary<string, string> options)
        {
            if (options.TryGetValue(key, out string value))
            {
                setter(value);
            }
        }

        private static void ApplyBool(string key, Action<bool> setter, IDictionary<string, string> options)
        {
            if (options.TryGetValue(key, out string value) && bool.TryParse(value, out bool result))
            {
                setter(result);
            }
        }

        private static void ApplyInt(string key, int min, int max, Action<int> setter, IDictionary<string, string> options)
        {
            if (options.TryGetValue(key, out string value) && int.TryParse(value, out int result) && result >= min && result <= max)
            {
                setter(result);
            }
        }

        private static void ApplyEnum<T>(string key, Action<T> setter, IDictionary<string, string> options) where T : struct
        {
            if (options.TryGetValue(key, out string value) && Enum.TryParse(value, out T result) && Enum.IsDefined(typeof(T), result))
            {
                setter(result);
            }
        }
    }
}