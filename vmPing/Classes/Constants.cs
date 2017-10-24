namespace vmPing.Classes
{
    class Constants
    {
        // txtOutput constants.
        public const int TXTOUTPUT_WIDTH = 390;
        public const int TXTOUTPUT_HEIGHT = 160;
        public const int TXTOUTPUT_X_OFFSET = 13;
        public const int TXTOUTPUT_Y_OFFSET = 13;
        //public const string TXTOUTPUT_BACKCOLOR_INACTIVE = "#fdf6e3";
        public const string TXTOUTPUT_BACKCOLOR_INACTIVE = "#eceafa";
        public const string TXTOUTPUT_BACKCOLOR_UP = "#859900";
        public const string TXTOUTPUT_BACKCOLOR_DOWN = "#dc322f";
        public const string TXTOUTPUT_BACKCOLOR_ERROR = "#b58900";
        public const string TXTOUTPUT_BACKCOLOR_INDETERMINATE = "#dfdf00";
        public const string TXTOUTPUT_FORECOLOR_ACTIVE = "#002b36";
        public const string TXTOUTPUT_FORECOLOR_INACTIVE = "#839496";
        public const string TXTOUTPUT_FORECOLOR_ERROR = "Black";
        public const string TXTOUTPUT_FONT_NAME = "Consolas";
        public const float TXTOUTPUT_FONT_SIZE = 9.75F;

        // txtHost constants.
        public const int TXTHOST_WIDTH = 303;
        public const int TXTHOST_HEIGHT = 33;
        public const int TXTHOST_X_OFFSET = 12;
        public const int TXTHOST_Y_OFFSET = 173;
        public const string TXTHOST_BACKCOLOR = "White";
        public const string TXTHOST_FORECOLOR = "Black";
        public const string TXTHOST_FONT_NAME = "Segoe UI";
        public const float TXTHOST_FONT_SIZE = 14.25F;

        // lblStats constants.
        public const int LBLSTATS_X_OFFSET = 18;
        public const int LBLSTATS_Y_OFFSET = 151;
        public const string LBLSTATS_BACKCOLOR_INACTIVE = TXTOUTPUT_BACKCOLOR_INACTIVE;
        public const string LBLSTATS_FORECOLOR_INACTIVE = "#657b83";
        public const string LBLSTATS_FORECOLOR_UP = "#fdf6e3";
        public const string LBLSTATS_FORECOLOR_DOWN = LBLSTATS_FORECOLOR_UP;
        public const string LBLSTATS_FORECOLOR_INDETERMINATE = "#111";
        public const string LBLSTATS_FORECOLOR_ERROR = "White";
        public const string LBLSTATS_FONT_NAME = "Segoe UI";
        public const float LBLSTATS_FONT_SIZE = 9.75F;

        // btnPing constants.
        public const int BTNPING_WIDTH = 90;
        public const int BTNPING_HEIGHT = 33;
        public const int BTNPING_X_OFFSET = 314;
        public const int BTNPING_Y_OFFSET = 173;

        // Layout constants.
        public const int LAYOUT_X_OFFSET = TXTOUTPUT_WIDTH + TXTOUTPUT_X_OFFSET;
        public const int LAYOUT_Y_OFFSET = TXTOUTPUT_HEIGHT + 55;
        public const int LAYOUT_COL_WIDTH = TXTOUTPUT_WIDTH + TXTOUTPUT_X_OFFSET;
        public const int LAYOUT_ROW_HEIGHT = LAYOUT_Y_OFFSET;
        public const int LAYOUT_MAX_SETS = 20;

        // Ping constants.
        public const string PING_DATA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        public const int PING_TIMEOUT = 2000;       // In miliseconds.
        public const int PING_TTL = 64;
        public const int PING_INTERVAL = 2000;      // In miliseconds.
    }
}
