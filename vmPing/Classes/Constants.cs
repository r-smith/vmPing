using System.Windows.Input;

namespace vmPing.Classes
{
    class Constants
    {
        // Default probe background colors.
        public const string Color_Probe_Background_Inactive = "#eceafa";
        public const string Color_Probe_Background_Up = "#859900";
        public const string Color_Probe_Background_Down = "#dc322f";
        public const string Color_Probe_Background_Indeterminate = "#dfdf00";
        public const string Color_Probe_Background_Error = "#b58900";
        public const string Color_Probe_Background_Scanner = "#505050";

        // Default probe foreground colors.
        public const string Color_Probe_Foreground_Inactive = "#839496";
        public const string Color_Probe_Foreground_Up = "#002b36";
        public const string Color_Probe_Foreground_Down = "#002b36";
        public const string Color_Probe_Foreground_Indeterminate = "#002b36";
        public const string Color_Probe_Foreground_Error = "#000000";
        public const string Color_Probe_Foreground_Scanner = "#f0f0f0";

        // Default statistics foreground colors.
        public const string Color_Statistics_Foreground_Inactive = "#657b83";
        public const string Color_Statistics_Foreground_Up = "#fdf6e3";
        public const string Color_Statistics_Foreground_Down = "#fdf6e3";
        public const string Color_Statistics_Foreground_Indeterminate = "#111";
        public const string Color_Statistics_Foreground_Error = "#ffffff";

        // Default alias / probe title colors.
        public const string Color_Alias_Foreground_Inactive = "#000000";
        public const string Color_Alias_Foreground_Up = "#ffff00";
        public const string Color_Alias_Foreground_Down = "#ffff00";
        public const string Color_Alias_Foreground_Indeterminate = "#ffffff";
        public const string Color_Alias_Foreground_Error = "#ffff00";
        public const string Color_Alias_Foreground_Scanner = "#ffff00";

        // Default probe options.
        public const string DefaultIcmpData = "https://github.com/R-Smith/vmPing";
        public const int DefaultTimeout = 2000;       // In miliseconds.
        public const int DefaultTTL = 64;
        public const int DefaultInterval = 2000;      // In miliseconds.

        // Default audio alert file paths.
        public const string DefaultAudioDownFilePath = @"%WINDIR%\Media\Windows Notify Email.wav";
        public const string DefaultAudioUpFilePath = @"%WINDIR%\Media\Windows Unlock.wav";

        // Key bindings.
        public const Key StatusHistoryKeyBinding = Key.F12;
        public const Key HelpKeyBinding = Key.F1;

    }
}
