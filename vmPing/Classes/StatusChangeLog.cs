using System;
using vmPing.Properties;

namespace vmPing.Classes
{
    public class StatusChangeLog
    {
        public DateTime Timestamp { get; set; }
        public string Hostname { get; set; }
        public string Alias { get; set; }
        public ProbeStatus Status { get; set; }
        public bool HasStatusBeenCleared { get; set; }

        public string AliasIfExistOrHostname
        {
            get
            {
                return (Alias != null && Alias.Length > 0) ? Alias : Hostname;
            }

        }
        public string StatusAsString
        {
            get
            {
                switch (Status)
                {
                    case ProbeStatus.Down:
                        return Strings.StatusChange_Down;
                    case ProbeStatus.Up:
                        return Strings.StatusChange_Up;
                    case ProbeStatus.Error:
                        return Strings.StatusChange_Error;
                    case ProbeStatus.Start:
                        return Strings.StatusChange_Start;
                    case ProbeStatus.Stop:
                        return Strings.StatusChange_Stop;
                    case ProbeStatus.LatencyHigh:
                        return Strings.StatusChange_LatencyHigh;
                    case ProbeStatus.LatencyNormal:
                        return Strings.StatusChange_LatencyNormal;
                    default:
                        return string.Empty;
                }
            }
        }

        public string StatusAsGlyph
        {
            get
            {
                switch (Status)
                {
                    case ProbeStatus.Error:
                    case ProbeStatus.Down:
                        return "u";
                    case ProbeStatus.Up:
                        return "t";
                    default:
                        return "h";
                }
            }
        }
    }
}
