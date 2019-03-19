using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
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
        
        public static int PingInterval { get; set; } = Constants.PING_INTERVAL;
        public static int PingTimeout { get; set; } = Constants.PING_TIMEOUT;
        public static int AlertThreshold { get; set; } = 2;
        public static bool AlwaysOnTop { get; set; } = false;
        public static bool IsEmailAlertEnabled { get; set; } = false;
        public static bool IsEmailAuthenticationRequired { get; set; } = false;
        public static string EmailServer { get; set; }
        public static string EmailUser { get; set; }
        public static string EmailPassword { get; set; }
        public static string EmailPort { get; set; } = "25";
        public static string EmailRecipient { get; set; }
        public static string EmailFromAddress { get; set; }
        public static bool IsLogOutputEnabled { get; set; } = false;
        public static string LogPath { get; set; }
        public static PopupNotificationOption PopupOption { get; set; } = PopupNotificationOption.Always;
        public static int TTL { get; set; } = Constants.PING_TTL;
        public static bool DontFragment { get; set; } = true;
        public static bool UseCustomBuffer { get; set; } = false;
        public static byte[] Buffer { get; set; }
        public static PingOptions GetPingOptions { get; }

        // Probe window background colors.
        public static string BackgroundColor_Probe_Inactive { get; set; } = Constants.Color_Probe_Background_Inactive;
        public static string BackgroundColor_Probe_Up { get; set; } = Constants.Color_Probe_Background_Up;
        public static string BackgroundColor_Probe_Down { get; set; } = Constants.Color_Probe_Background_Down;
        public static string BackgroundColor_Probe_Indeterminate { get; set; } = Constants.Color_Probe_Background_Indeterminate;
        public static string BackgroundColor_Probe_Error { get; set; } = Constants.Color_Probe_Background_Error;

        // Probe window foreground colors.
        public static string ForegroundColor_Probe_Inactive { get; set; } = Constants.Color_Probe_Foreground_Inactive;
        public static string ForegroundColor_Probe_Up { get; set; } = Constants.Color_Probe_Foreground_Up;
        public static string ForegroundColor_Probe_Down { get; set; } = Constants.Color_Probe_Foreground_Down;
        public static string ForegroundColor_Probe_Indeterminate { get; set; } = Constants.Color_Probe_Foreground_Indeterminate;
        public static string ForegroundColor_Probe_Error { get; set; } = Constants.Color_Probe_Foreground_Error;

        // Probe statistics foreground colors.
        public static string ForegroundColor_Stats_Inactive { get; set; } = Constants.Color_Statistics_Foreground_Inactive;
        public static string ForegroundColor_Stats_Up { get; set; } = Constants.Color_Statistics_Foreground_Up;
        public static string ForegroundColor_Stats_Down { get; set; } = Constants.Color_Statistics_Foreground_Down;
        public static string ForegroundColor_Stats_Indeterminate { get; set; } = Constants.Color_Statistics_Foreground_Indeterminate;
        public static string ForegroundColor_Stats_Error { get; set; } = Constants.Color_Statistics_Foreground_Error;

        // Alias foreground colors.
        public static string ForegroundColor_Alias_Inactive { get; set; } = Constants.Color_Alias_Foreground_Inactive;
        public static string ForegroundColor_Alias_Up { get; set; } = Constants.Color_Alias_Foreground_Up;
        public static string ForegroundColor_Alias_Down { get; set; } = Constants.Color_Alias_Foreground_Down;
        public static string ForegroundColor_Alias_Indeterminate { get; set; } = Constants.Color_Alias_Foreground_Indeterminate;
        public static string ForegroundColor_Alias_Error { get; set; } = Constants.Color_Alias_Foreground_Error;


        static ApplicationOptions()
        {
            // Set the default ping data.
            Buffer = Encoding.ASCII.GetBytes(Constants.PING_DATA);

            // Set the default ping options.
            GetPingOptions = new PingOptions(Constants.PING_TTL, true);
        }


        public static void UpdatePingOptions()
        {
            GetPingOptions.Ttl = TTL;
            GetPingOptions.DontFragment = DontFragment;
        }


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


        public static IEnumerable<Visual> GetChildren(this Visual parent, bool recurse = true)
        {
            if (parent != null)
            {
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    // Retrieve child visual at specified index value.
                    var child = VisualTreeHelper.GetChild(parent, i) as Visual;

                    if (child != null)
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
}
