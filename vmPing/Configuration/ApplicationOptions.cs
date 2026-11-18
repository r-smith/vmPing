using System.Net.NetworkInformation;
using System.Text;

namespace vmPing.Configuration
{
    public static class ApplicationOptions
    {
        // Ping & probe options.
        public static int PingInterval { get; set; } = Constants.DefaultInterval;
        public static int PingTimeout { get; set; } = Constants.DefaultTimeout;
        public static int AlertThreshold { get; set; } = 2;
        public static LatencyMode LatencyDetectionMode { get; set; } = LatencyMode.Off;
        public static long HighLatencyMilliseconds { get; set; } = 50;
        public static int HighLatencyAlertTriggerCount { get; set; } = 2;
        public static int TTL { get; set; } = Constants.DefaultTTL;
        public static bool DontFragment { get; set; } = false;
        public static bool UseCustomPacketData { get; set; } = false;
        public static int PacketSize { get; set; } = Constants.DefaultPacketSize;
        public static string CustomPacketData { get; set; } = string.Empty;
        public static byte[] PingData { get; private set; }
        public static PingOptions PingOptions { get; }

        // Popup notifications.
        public static PopupNotificationOption PopupOption { get; set; } = PopupNotificationOption.Always;
        public static bool IsAutoDismissEnabled { get; set; } = false;
        public static int AutoDismissMilliseconds { get; set; } = 7000;

        // Email notifications.
        public static bool IsEmailAlertEnabled { get; set; } = false;
        public static bool IsEmailAuthenticationRequired { get; set; } = false;
        public static bool IsEmailSslEnabled { get; set; } = false;
        public static string EmailServer { get; set; }
        public static string EmailUser { get; set; }
        public static string EmailPassword { get; set; }
        public static string EmailPort { get; set; } = "25";
        public static string EmailRecipient { get; set; }
        public static string EmailFromAddress { get; set; }

        // Audio alerts.
        public static bool IsAudioUpAlertEnabled { get; set; } = false;
        public static bool IsAudioDownAlertEnabled { get; set; } = false;
        public static string AudioUpFilePath { get; set; }
        public static string AudioDownFilePath { get; set; }

        // Logging.
        public static string LogFolder { get; set; }
        public static bool IsLogOutputEnabled { get; set; } = false;
        public static bool IsLogStatusChangesEnabled { get; set; } = false;

        // Startup options.
        public static StartMode InitialStartMode { get; set; } = StartMode.Blank;
        public static int InitialProbeCount { get; set; } = 2;
        public static int InitialColumnCount { get; set; } = 2;
        public static string InitialFavorite { get; set; } = null;
        public static bool IsStartMinimizedEnabled { get; set; } = false;
        public static WindowStartupOption WindowSizeOption { get; set; } = WindowStartupOption.Fixed;
        public static int WindowWidth { get; set; } = 800;
        public static int WindowHeight { get; set; } = 400;
        public static bool IsWindowMaximized { get; set; } = false;
        public static WindowStartupOption WindowPositionOption { get; set; } = WindowStartupOption.Default;
        public static int WindowLeft { get; set; } = 100;
        public static int WindowTop { get; set; } = 100;

        // Window options.
        public static bool IsAlwaysOnTopEnabled { get; set; } = false;
        public static bool IsMinimizeToTrayEnabled { get; set; } = false;
        public static bool IsExitToTrayEnabled { get; set; } = false;
        public static bool IsAlwaysShowTrayIconEnabled { get; set; } = false;

        // Fonts.
        public static int ProbeFontSize { get; set; } = 11;
        public static int ScannerFontSize { get; set; } = 16;

        // Theme.
        public static ThemeMode Theme { get; set; } = ThemeMode.System;

        // Probe background colors.
        public static string ProbeBackgroundInactive { get; set; } = Constants.DefaultProbeBackgroundInactive;
        public static string ProbeBackgroundUp { get; set; } = Constants.DefaultProbeBackgroundUp;
        public static string ProbeBackgroundDown { get; set; } = Constants.DefaultProbeBackgroundDown;
        public static string ProbeBackgroundIndeterminate { get; set; } = Constants.DefaultProbeBackgroundIndeterminate;
        public static string ProbeBackgroundError { get; set; } = Constants.DefaultProbeBackgroundError;
        public static string ProbeBackgroundScanner { get; set; } = Constants.DefaultProbeBackgroundScanner;

        // Probe foreground colors.
        public static string ProbeForegroundInactive { get; set; } = Constants.DefaultProbeForegroundInactive;
        public static string ProbeForegroundUp { get; set; } = Constants.DefaultProbeForegroundUp;
        public static string ProbeForegroundDown { get; set; } = Constants.DefaultProbeForegroundDown;
        public static string ProbeForegroundIndeterminate { get; set; } = Constants.DefaultProbeForegroundIndeterminate;
        public static string ProbeForegroundError { get; set; } = Constants.DefaultProbeForegroundError;
        public static string ProbeForegroundScanner { get; set; } = Constants.DefaultProbeForegroundScanner;

        // Probe statistics.
        public static string StatisticsForegroundInactive { get; set; } = Constants.DefaultStatisticsForegroundInactive;
        public static string StatisticsForegroundUp { get; set; } = Constants.DefaultStatisticsForegroundUp;
        public static string StatisticsForegroundDown { get; set; } = Constants.DefaultStatisticsForegroundDown;
        public static string StatisticsForegroundIndeterminate { get; set; } = Constants.DefaultStatisticsForegroundIndeterminate;
        public static string StatisticsForegroundError { get; set; } = Constants.DefaultStatisticsForegroundError;

        static ApplicationOptions()
        {
            // Set the default ping data.
            PingData = CreatePingData(PacketSize);

            // Set the default ping options.
            PingOptions = new PingOptions(TTL, DontFragment);
        }

        public static void UpdatePingOptions()
        {
            PingOptions.Ttl = TTL;
            PingOptions.DontFragment = DontFragment;
            PingData = UseCustomPacketData
                ? Encoding.ASCII.GetBytes(CustomPacketData)
                : CreatePingData(PacketSize);
        }

        // Ping data of the given size, filled the same way as Windows ping.
        public static byte[] CreatePingData(int size)
        {
            byte[] data = new byte[size];
            for (int i = 0; i < size; i++)
            {
                data[i] = (byte)Constants.PingDataPattern[i % Constants.PingDataPattern.Length];
            }
            return data;
        }
    }
}
