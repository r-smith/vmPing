using System.Net.NetworkInformation;
using System.Text;
using System.Windows;

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
    }
}
