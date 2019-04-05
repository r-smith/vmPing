using System;
using vmPing.Properties;

namespace vmPing.Classes
{
    public class StatusChangeLog
    {
        public DateTime Timestamp { get; set; }
        public string Hostname { get; set; }
        public ProbeStatus Status { get; set; }
        public bool HasStatusBeenCleared { get; set; }
        public string StatusAsString
        {
            get
            {
                string returnString = string.Empty;
                switch (Status)
                {
                    case ProbeStatus.Down:
                        returnString = Strings.StatusChange_Down;
                        break;
                    case ProbeStatus.Up:
                        returnString = Strings.StatusChange_Up;
                        break;
                    case ProbeStatus.Error:
                        returnString = Strings.StatusChange_Error;
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
                    case ProbeStatus.Error:
                    case ProbeStatus.Down:
                        returnString = "u";
                        break;
                    case ProbeStatus.Up:
                        returnString = "t";
                        break;
                }
                return returnString;
            }
        }
    }
}
