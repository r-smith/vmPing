using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Media;

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

        public enum StartMode
        {
            Blank = 0,
            MultiInput = 1,
            Favorite = 2
        }

        public enum LatencyMode
        {
            Off,
            Auto,
            Fixed
        }

        // Ping & probe options.
        public static int PingInterval { get; set; } = Constants.DefaultInterval;
        public static int PingTimeout { get; set; } = Constants.DefaultTimeout;
        public static int AlertThreshold { get; set; } = 2;
        public static LatencyMode LatencyDetectionMode { get; set; } = LatencyMode.Off;
        public static long HighLatencyMilliseconds { get; set; } = 50;
        public static int HighLatencyAlertTiggerCount { get; set; } = 2;
        public static int TTL { get; set; } = Constants.DefaultTTL;
        public static bool DontFragment { get; set; } = false;
        public static bool UseCustomBuffer { get; set; } = false;
        public static byte[] Buffer { get; set; }
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
        public static bool IsLogOutputEnabled { get; set; } = false;
        public static string LogPath { get; set; }
        public static bool IsLogStatusChangesEnabled { get; set; } = false;
        public static string LogStatusChangesPath { get; set; }
        
        // Startup options.
        public static StartMode InitialStartMode { get; set; } = StartMode.Blank;
        public static int InitialProbeCount { get; set; } = 2;
        public static int InitialColumnCount { get; set; } = 2;
        public static string InitialFavorite { get; set; } = null;

        // Display options.
        public static bool IsAlwaysOnTopEnabled { get; set; } = false;
        public static bool IsMinimizeToTrayEnabled { get; set; } = false;
        public static bool IsExitToTrayEnabled { get; set; } = false;

        // Fonts.
        public static int FontSize_Probe { get; set; } = 11;
        public static int FontSize_Scanner { get; set; } = 16;

        // Probe background colors.
        public static string BackgroundColor_Probe_Inactive { get; set; } = Constants.Color_Probe_Background_Inactive;
        public static string BackgroundColor_Probe_Up { get; set; } = Constants.Color_Probe_Background_Up;
        public static string BackgroundColor_Probe_Down { get; set; } = Constants.Color_Probe_Background_Down;
        public static string BackgroundColor_Probe_Indeterminate { get; set; } = Constants.Color_Probe_Background_Indeterminate;
        public static string BackgroundColor_Probe_Error { get; set; } = Constants.Color_Probe_Background_Error;
        public static string BackgroundColor_Probe_Scanner { get; set; } = Constants.Color_Probe_Background_Scanner;

        // Probe foreground colors.
        public static string ForegroundColor_Probe_Inactive { get; set; } = Constants.Color_Probe_Foreground_Inactive;
        public static string ForegroundColor_Probe_Up { get; set; } = Constants.Color_Probe_Foreground_Up;
        public static string ForegroundColor_Probe_Down { get; set; } = Constants.Color_Probe_Foreground_Down;
        public static string ForegroundColor_Probe_Indeterminate { get; set; } = Constants.Color_Probe_Foreground_Indeterminate;
        public static string ForegroundColor_Probe_Error { get; set; } = Constants.Color_Probe_Foreground_Error;
        public static string ForegroundColor_Probe_Scanner { get; set; } = Constants.Color_Probe_Foreground_Scanner;

        // Probe statistics.
        public static string ForegroundColor_Stats_Inactive { get; set; } = Constants.Color_Statistics_Foreground_Inactive;
        public static string ForegroundColor_Stats_Up { get; set; } = Constants.Color_Statistics_Foreground_Up;
        public static string ForegroundColor_Stats_Down { get; set; } = Constants.Color_Statistics_Foreground_Down;
        public static string ForegroundColor_Stats_Indeterminate { get; set; } = Constants.Color_Statistics_Foreground_Indeterminate;
        public static string ForegroundColor_Stats_Error { get; set; } = Constants.Color_Statistics_Foreground_Error;

        // Alias text.
        public static string ForegroundColor_Alias_Inactive { get; set; } = Constants.Color_Alias_Foreground_Inactive;
        public static string ForegroundColor_Alias_Up { get; set; } = Constants.Color_Alias_Foreground_Up;
        public static string ForegroundColor_Alias_Down { get; set; } = Constants.Color_Alias_Foreground_Down;
        public static string ForegroundColor_Alias_Indeterminate { get; set; } = Constants.Color_Alias_Foreground_Indeterminate;
        public static string ForegroundColor_Alias_Error { get; set; } = Constants.Color_Alias_Foreground_Error;
        public static string ForegroundColor_Alias_Scanner { get; set; } = Constants.Color_Alias_Foreground_Scanner;

        static ApplicationOptions()
        {
            // Set the default ping data.
            Buffer = Encoding.ASCII.GetBytes(Constants.DefaultIcmpData);

            // Set the default ping options.
            PingOptions = new PingOptions(Constants.DefaultTTL, true);
        }

        public static void UpdatePingOptions()
        {
            PingOptions.Ttl = TTL;
            PingOptions.DontFragment = DontFragment;
        }

        public static IEnumerable<Visual> GetChildren(this Visual parent, bool recurse = true)
        {
            if (parent == null)
            {
                yield break;
            }

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                // Retrieve child visual at specified index value.
                if (VisualTreeHelper.GetChild(parent, i) is Visual child)
                {
                    yield return child;

                    if (recurse)
                    {
                        foreach (var grandChild in child.GetChildren(true))
                        {
                            yield return grandChild;
                        }
                    }
                }
            }
        }
    }
}
