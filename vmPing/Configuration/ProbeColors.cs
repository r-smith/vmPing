using System;
using System.Linq;

namespace vmPing.Configuration
{
    // One set of probe colors, used by a theme.
    public class ProbeColors
    {
        public string BackgroundInactive { get; set; }
        public string BackgroundUp { get; set; }
        public string BackgroundDown { get; set; }
        public string BackgroundIndeterminate { get; set; }
        public string BackgroundError { get; set; }
        public string BackgroundTool { get; set; }

        public string ForegroundInactive { get; set; }
        public string ForegroundUp { get; set; }
        public string ForegroundDown { get; set; }
        public string ForegroundIndeterminate { get; set; }
        public string ForegroundError { get; set; }
        public string ForegroundTool { get; set; }

        public string StatisticsInactive { get; set; }
        public string StatisticsUp { get; set; }
        public string StatisticsDown { get; set; }
        public string StatisticsIndeterminate { get; set; }
        public string StatisticsError { get; set; }

        public ProbeColors Clone()
        {
            return (ProbeColors)MemberwiseClone();
        }

        // True when every color is the same as in other. Case doesn't matter (#ABCDEF and #abcdef match).
        public bool Matches(ProbeColors other)
        {
            return other != null && Values().SequenceEqual(other.Values(), StringComparer.OrdinalIgnoreCase);
        }

        private string[] Values()
        {
            return new string[]
            {
                BackgroundInactive, BackgroundUp, BackgroundDown, BackgroundIndeterminate, BackgroundError, BackgroundTool,
                ForegroundInactive, ForegroundUp, ForegroundDown, ForegroundIndeterminate, ForegroundError, ForegroundTool,
                StatisticsInactive, StatisticsUp, StatisticsDown, StatisticsIndeterminate, StatisticsError
            };
        }

        public static ProbeColors CreateLightDefaults()
        {
            return new ProbeColors
            {
                BackgroundInactive = "#eceafa",
                BackgroundUp = "#859900",
                BackgroundDown = "#dc322f",
                BackgroundIndeterminate = "#dfdf00",
                BackgroundError = "#b58900",
                BackgroundTool = "#505050",

                ForegroundInactive = "#839496",
                ForegroundUp = "#002b36",
                ForegroundDown = "#002b36",
                ForegroundIndeterminate = "#002b36",
                ForegroundError = "#000000",
                ForegroundTool = "#f0f0f0",

                StatisticsInactive = "#657b83",
                StatisticsUp = "#fdf6e3",
                StatisticsDown = "#fdf6e3",
                StatisticsIndeterminate = "#111",
                StatisticsError = "#ffffff"
            };
        }

        public static ProbeColors CreateDarkDefaults()
        {
            return new ProbeColors
            {
                BackgroundInactive = "#1c2128",
                BackgroundUp = "#2ec267",
                BackgroundDown = "#ff5c5c",
                BackgroundIndeterminate = "#facc15",
                BackgroundError = "#ff5fd7",
                BackgroundTool = "#2d333b",

                ForegroundInactive = "#9da7b3",
                ForegroundUp = "#0a0a0a",
                ForegroundDown = "#0a0a0a",
                ForegroundIndeterminate = "#0a0a0a",
                ForegroundError = "#0a0a0a",
                ForegroundTool = "#e6edf3",

                StatisticsInactive = "#9da7b3",
                StatisticsUp = "#ffffff",
                StatisticsDown = "#ffffff",
                StatisticsIndeterminate = "#ffffff",
                StatisticsError = "#ffffff"
            };
        }
    }
}
