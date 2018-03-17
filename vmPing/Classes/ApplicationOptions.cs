using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;

namespace vmPing.Classes
{
    public static class ApplicationOptions
    {
        public enum PopupNotificationOption
        {
            Always,
            Never,
            WhenMinimized
        }

        public static int PingInterval { get; set; } = Constants.PING_INTERVAL;
        public static int PingTimeout { get; set; } = Constants.PING_TIMEOUT;
        public static int AlertThreshold { get; set; } = 2;
        public static bool EmailAlert { get; set; } = false;
        public static bool AlwaysOnTop { get; set; } = false;
        public static string EmailServer { get; set; } = "";
        public static string EmailUser { get; set; } = "";
        public static string EmailPassword { get; set; } = "";
        public static string EmailPort { get; set; } = "";
        public static string EmailRecipient { get; set; } = "";
        public static string EmailFromAddress { get; set; } = "";
        public static bool LogOutput { get; set; } = false;
        public static string LogPath { get; set; } = "";
        public static PopupNotificationOption PopupOption { get; set; } = PopupNotificationOption.Always;

        public static void BlurWindows()
        {
            // Add blur effect to all windows.
            foreach (Window window in Application.Current.Windows)
            {
                System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
                objBlur.Radius = 4;
                window.Opacity = 0.85;
                window.Effect = objBlur;
            }
        }

        public static void RemoveBlurWindows()
        {
            // Remove blur effect from all windows.
            foreach (Window window in Application.Current.Windows)
            {
                window.Effect = null;
                window.Opacity = 1;
            }
        }

        public static void CreatApplicationOptions()
        {
            var rootPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing");
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingApplicationOptions.xml");
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);
            if (!File.Exists(path))
            {
                try
                {
                    string[] lines = { "<ApplicationOptions>", "</ApplicationOptions>" };
                    File.WriteAllLines(path, lines);
                    var xd = new XmlDocument();
                    xd.Load(path);
                    XmlNode nodeRoot = xd.SelectSingleNode("/ApplicationOptions");
                    var xmlOptions = new List<XmlElement>();

                    XmlElement alertThreshold = xd.CreateElement("AlertThreshold");
                    alertThreshold.InnerText = ApplicationOptions.AlertThreshold.ToString();
                    nodeRoot.AppendChild(alertThreshold);

                    XmlElement alwaysOnTop = xd.CreateElement("AlwaysOnTop");
                    alwaysOnTop.InnerText = ApplicationOptions.AlwaysOnTop.ToString();
                    nodeRoot.AppendChild(alwaysOnTop);

                    XmlElement emailAlert = xd.CreateElement("EmailAlert");
                    emailAlert.InnerText = ApplicationOptions.EmailAlert.ToString();
                    nodeRoot.AppendChild(emailAlert);

                    XmlElement emailFromAddress = xd.CreateElement("EmailFromAddress");
                    emailFromAddress.InnerText = ApplicationOptions.EmailFromAddress.ToString();
                    nodeRoot.AppendChild(emailFromAddress);

                    XmlElement emailPassword = xd.CreateElement("EmailPassword");
                    emailPassword.InnerText = ApplicationOptions.EmailPassword.ToString();
                    nodeRoot.AppendChild(emailPassword);

                    XmlElement emailPort = xd.CreateElement("EmailPort");
                    emailPort.InnerText = ApplicationOptions.EmailPort.ToString();
                    nodeRoot.AppendChild(emailPort);

                    XmlElement emailRecipient = xd.CreateElement("EmailRecipient");
                    emailRecipient.InnerText = ApplicationOptions.EmailRecipient.ToString();
                    nodeRoot.AppendChild(emailRecipient);

                    XmlElement emailServer = xd.CreateElement("EmailServer");
                    emailServer.InnerText = ApplicationOptions.EmailServer.ToString();
                    nodeRoot.AppendChild(emailServer);

                    XmlElement emailUser = xd.CreateElement("EmailUser");
                    emailUser.InnerText = ApplicationOptions.EmailUser.ToString();
                    nodeRoot.AppendChild(emailUser);

                    XmlElement logOutput = xd.CreateElement("LogOutput");
                    logOutput.InnerText = ApplicationOptions.LogOutput.ToString();
                    nodeRoot.AppendChild(logOutput);

                    XmlElement logPath = xd.CreateElement("LogPath");
                    logPath.InnerText = ApplicationOptions.LogPath.ToString();
                    nodeRoot.AppendChild(logPath);

                    XmlElement pingInterval = xd.CreateElement("PingInterval");
                    pingInterval.InnerText = ApplicationOptions.PingInterval.ToString();
                    nodeRoot.AppendChild(pingInterval);

                    XmlElement pingTimeout = xd.CreateElement("PingTimeout");
                    pingTimeout.InnerText = ApplicationOptions.PingTimeout.ToString();
                    nodeRoot.AppendChild(pingTimeout);

                    XmlElement popupOption = xd.CreateElement("PopupOption");
                    popupOption.InnerText = ApplicationOptions.PopupOption.ToString();
                    nodeRoot.AppendChild(popupOption);

                    xd.Save(path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        public static void LoadApplicationOption()
        {
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingApplicationOptions.xml");
            
            if (!File.Exists(path))
                CreatApplicationOptions();

            var xd = new XmlDocument();
            xd.Load(path);

            XmlNode nodeRoot = xd.SelectSingleNode("/ApplicationOptions");
            var xmlOptions = new List<XmlElement>();

            XmlNode alertThreshold = xd.SelectSingleNode("/ApplicationOptions/AlertThreshold");
            ApplicationOptions.AlertThreshold = int.Parse(alertThreshold.InnerText);

            XmlNode alwaysOnTop = xd.SelectSingleNode("/ApplicationOptions/AlwaysOnTop");
            ApplicationOptions.AlwaysOnTop = bool.Parse(alwaysOnTop.InnerText);

            XmlNode emailAlert = xd.SelectSingleNode("/ApplicationOptions/EmailAlert");
            ApplicationOptions.EmailAlert = bool.Parse(emailAlert.InnerText);

            XmlNode emailFromAddress = xd.SelectSingleNode("/ApplicationOptions/EmailFromAddress");
            ApplicationOptions.EmailFromAddress = emailFromAddress.InnerText;

            XmlNode emailPassword = xd.SelectSingleNode("/ApplicationOptions/EmailPassword");
            ApplicationOptions.EmailPassword = emailPassword.InnerText;

            XmlNode emailPort = xd.SelectSingleNode("/ApplicationOptions/EmailPort");
            ApplicationOptions.EmailPort = emailPort.InnerText;

            XmlNode emailRecipient = xd.SelectSingleNode("/ApplicationOptions/EmailRecipient");
            ApplicationOptions.EmailRecipient = emailRecipient.InnerText;

            XmlNode emailServer = xd.SelectSingleNode("/ApplicationOptions/EmailServer");
            ApplicationOptions.EmailServer = emailServer.InnerText;

            XmlNode emailUser = xd.SelectSingleNode("/ApplicationOptions/EmailUser");
            ApplicationOptions.EmailUser = emailUser.InnerText;

            XmlNode logOutput = xd.SelectSingleNode("/ApplicationOptions/LogOutput");
            ApplicationOptions.LogOutput = bool.Parse(logOutput.InnerText);

            XmlNode logPath = xd.SelectSingleNode("/ApplicationOptions/LogPath");
            ApplicationOptions.LogPath = logPath.InnerText;

            XmlNode pingInterval = xd.SelectSingleNode("/ApplicationOptions/PingInterval");
            ApplicationOptions.PingInterval = int.Parse(pingInterval.InnerText);

            XmlNode pingTimeout = xd.SelectSingleNode("/ApplicationOptions/PingTimeout");
            ApplicationOptions.PingTimeout =  int.Parse(pingTimeout.InnerText);

            XmlNode popupOption = xd.SelectSingleNode("/ApplicationOptions/PopupOption");
            ApplicationOptions.PopupOption = (PopupNotificationOption)Enum.Parse(typeof(PopupNotificationOption), popupOption.InnerText);
        }

        public static void SaveApplicationOption()
        {
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingApplicationOptions.xml");

            var xd = new XmlDocument();
            xd.Load(path);
            XmlNode nodeRoot = xd.SelectSingleNode("/ApplicationOptions");
            var xmlOptions = new List<XmlElement>();

            XmlNode alertThreshold = xd.SelectSingleNode("/ApplicationOptions/AlertThreshold");
            alertThreshold.InnerText = ApplicationOptions.AlertThreshold.ToString();

            XmlNode alwaysOnTop = xd.SelectSingleNode("/ApplicationOptions/AlwaysOnTop");
            alwaysOnTop.InnerText = ApplicationOptions.AlwaysOnTop.ToString();

            XmlNode emailAlert = xd.SelectSingleNode("/ApplicationOptions/EmailAlert");
            emailAlert.InnerText = ApplicationOptions.EmailAlert.ToString();

            XmlNode emailFromAddress = xd.SelectSingleNode("/ApplicationOptions/EmailFromAddress");
            emailFromAddress.InnerText = ApplicationOptions.EmailFromAddress.ToString();

            XmlNode emailPassword = xd.SelectSingleNode("/ApplicationOptions/EmailPassword");
            emailPassword.InnerText = ApplicationOptions.EmailPassword.ToString();

            XmlNode emailPort = xd.SelectSingleNode("/ApplicationOptions/EmailPort");
            emailPort.InnerText = ApplicationOptions.EmailPort.ToString();

            XmlNode emailRecipient = xd.SelectSingleNode("/ApplicationOptions/EmailRecipient");
            emailRecipient.InnerText = ApplicationOptions.EmailRecipient.ToString();

            XmlNode emailServer = xd.SelectSingleNode("/ApplicationOptions/EmailServer");
            emailServer.InnerText = ApplicationOptions.EmailServer.ToString();

            XmlNode emailUser = xd.SelectSingleNode("/ApplicationOptions/EmailUser");
            emailUser.InnerText = ApplicationOptions.EmailUser.ToString();

            XmlNode logOutput = xd.SelectSingleNode("/ApplicationOptions/LogOutput");
            logOutput.InnerText = ApplicationOptions.LogOutput.ToString();

            XmlNode logPath = xd.SelectSingleNode("/ApplicationOptions/LogPath");
            logPath.InnerText = ApplicationOptions.LogPath.ToString();

            XmlNode pingInterval = xd.SelectSingleNode("/ApplicationOptions/PingInterval");
            pingInterval.InnerText = ApplicationOptions.PingInterval.ToString();

            XmlNode pingTimeout = xd.SelectSingleNode("/ApplicationOptions/PingTimeout");
            pingTimeout.InnerText = ApplicationOptions.PingTimeout.ToString();

            XmlNode popupOption = xd.SelectSingleNode("/ApplicationOptions/PopupOption");
            popupOption.InnerText = ApplicationOptions.PopupOption.ToString();
            
            xd.Save(path);
        }
    }
}
