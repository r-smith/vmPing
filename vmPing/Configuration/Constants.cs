using System.Windows.Input;

namespace vmPing.Configuration
{
    class Constants
    {
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

        // Default alias / probe title colors.
        public const string DefaultAliasForegroundInactive = "#000000";
        public const string DefaultAliasForegroundUp = "#ffff00";
        public const string DefaultAliasForegroundDown = "#ffff00";
        public const string DefaultAliasForegroundIndeterminate = "#ffffff";
        public const string DefaultAliasForegroundError = "#ffff00";
        public const string DefaultAliasForegroundScanner = "#ffff00";

        // Default probe options.
        public const string DefaultIcmpData = "https://github.com/R-Smith/vmPing";
        public const int DefaultTimeout = 2000;       // In milliseconds.
        public const int DefaultTTL = 64;
        public const int DefaultInterval = 2000;      // In milliseconds.

        // Default audio alert file paths.
        public const string DefaultAudioDownFilePath = @"%WINDIR%\Media\Windows Notify Email.wav";
        public const string DefaultAudioUpFilePath = @"%WINDIR%\Media\Windows Unlock.wav";

        // Key bindings.
        public const Key StatusHistoryKeyBinding = Key.F12;
        public const Key HelpKeyBinding = Key.F1;
    }
}
