using System;

namespace vmPing.Classes
{
    public class StatusChangeLog
    {
        public DateTime Timestamp { get; set; }
        public string Hostname { get; set; }
        public PingStatus Status { get; set; }
        public string StatusAsString
        {
            get
            {
                string returnString = string.Empty;
                switch (Status)
                {
                    case PingStatus.Down:
                        returnString = "DOWN";
                        break;
                    case PingStatus.Up:
                        returnString = "UP";
                        break;
                }
                return returnString;
            }
        }

        public string StatusAsGlyph
        {
            get
            {
                string returnString = string.Empty;
                switch (Status)
                {
                    case PingStatus.Down:
                        returnString = "u";
                        break;
                    case PingStatus.Up:
                        returnString = "t";
                        break;
                }
                return returnString;
            }
        }
    }
}
