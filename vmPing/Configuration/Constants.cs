using System.Windows.Input;

namespace vmPing.Configuration
{
    internal static class Constants
    {
        // Status changes are logged to this file in the log folder.
        public const string StatusLogFileName = "vmPing-status.txt";

        // Default probe background colors.
        public const string DefaultProbeBackgroundInactive = "#eceafa";
        public const string DefaultProbeBackgroundUp = "#859900";
        public const string DefaultProbeBackgroundDown = "#dc322f";
        public const string DefaultProbeBackgroundIndeterminate = "#dfdf00";
        public const string DefaultProbeBackgroundError = "#b58900";
        public const string DefaultProbeBackgroundScanner = "#505050";

        // Default probe foreground colors.
        public const string DefaultProbeForegroundInactive = "#839496";
        public const string DefaultProbeForegroundUp = "#002b36";
        public const string DefaultProbeForegroundDown = "#002b36";
        public const string DefaultProbeForegroundIndeterminate = "#002b36";
        public const string DefaultProbeForegroundError = "#000000";
        public const string DefaultProbeForegroundScanner = "#f0f0f0";

        // Default statistics foreground colors.
        public const string DefaultStatisticsForegroundInactive = "#657b83";
        public const string DefaultStatisticsForegroundUp = "#fdf6e3";
        public const string DefaultStatisticsForegroundDown = "#fdf6e3";
        public const string DefaultStatisticsForegroundIndeterminate = "#111";
        public const string DefaultStatisticsForegroundError = "#ffffff";

        // Default probe options.
        public const int DefaultPacketSize = 32;
        // Ping data repeats these letters, the same as Windows ping.
        public const string PingDataPattern = "abcdefghijklmnopqrstuvw";
        public const int DefaultTimeout = 2000;       // In milliseconds.
        public const int DefaultTTL = 128;
        public const int DefaultInterval = 2000;      // In milliseconds.

        // Default audio alert file paths.
        public const string DefaultAudioDownFilePath = @"%WINDIR%\Media\Windows Notify Email.wav";
        public const string DefaultAudioUpFilePath = @"%WINDIR%\Media\Windows Unlock.wav";

        // Key bindings.
        public const Key StatusHistoryKeyBinding = Key.F12;
        public const Key HelpKeyBinding = Key.F1;
    }
}
