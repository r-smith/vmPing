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
        public static bool EmailAlert { get; set; } = false;
        public static bool AlwaysOnTop { get; set; } = false;
        public static string EmailServer { get; set; }
        public static string EmailRecipient { get; set; }
        public static string EmailFromAddress { get; set; }
        public static bool LogOutput { get; set; } = false;
        public static string LogPath { get; set; }
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
    }
}
