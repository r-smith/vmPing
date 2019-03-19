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

        // Default probe foreground colors.
        public const string Color_Probe_Foreground_Inactive = "#839496";
        public const string Color_Probe_Foreground_Up = "#002b36";
        public const string Color_Probe_Foreground_Down = "#002b36";
        public const string Color_Probe_Foreground_Indeterminate = "#002b36";
        public const string Color_Probe_Foreground_Error = "#000000";

        // Default statistics foreground colors.
        public const string Color_Statistics_Foreground_Inactive = "#657b83";
        public const string Color_Statistics_Foreground_Up = "#fdf6e3";
        public const string Color_Statistics_Foreground_Down = "#fdf6e3";
        public const string Color_Statistics_Foreground_Indeterminate = "#111";
        public const string Color_Statistics_Foreground_Error = "#ffffff";

        // Default alias / probe tile colors.
        public const string Color_Alias_Foreground_Inactive = "#444444";
        public const string Color_Alias_Foreground_Up = "#ffff00";
        public const string Color_Alias_Foreground_Down = "#ffff00";
        public const string Color_Alias_Foreground_Indeterminate = "#ffffff";
        public const string Color_Alias_Foreground_Error = "#ffff00";

        // Unused
        //public const string TXTOUTPUT_FONT_NAME = "Consolas";
        //public const float TXTOUTPUT_FONT_SIZE = 9.75F;
        //public const string TXTHOST_BACKCOLOR = "White";
        //public const string TXTHOST_FORECOLOR = "Black";
        //public const string TXTHOST_FONT_NAME = "Segoe UI";
        //public const float TXTHOST_FONT_SIZE = 14.25F;
        //public const string LBLSTATS_FONT_NAME = "Segoe UI";
        //public const float LBLSTATS_FONT_SIZE = 9.75F;

        // Default probe options.
        public const string PING_DATA = "https://github.com/R-Smith/vmPing";
        public const int PING_TIMEOUT = 2000;       // In miliseconds.
        public const int PING_TTL = 64;
        public const int PING_INTERVAL = 2000;      // In miliseconds.
    }
}
