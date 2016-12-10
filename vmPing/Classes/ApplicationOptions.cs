using System.Windows;

namespace vmPing.Classes
{
    public class ApplicationOptions
    {
        int pingInterval = Constants.PING_INTERVAL;
        public int PingInterval
        {
            get { return this.pingInterval; }
            set { this.pingInterval = value; }
        }

        int pingTimeout = Constants.PING_TIMEOUT;
        public int PingTimeout
        {
            get { return this.pingTimeout; }
            set { this.pingTimeout = value; }
        }

        public bool EmailAlert { get; set; }
        public bool AlwaysOnTop { get; set; }
        public string EmailServer { get; set; }
        public string EmailRecipient { get; set; }
        public string EmailFromAddress { get; set; }
        public bool LogOutput { get; set; }
        public string LogPath { get; set; }

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
