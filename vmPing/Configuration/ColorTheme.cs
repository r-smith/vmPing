using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace vmPing.Configuration
{
    // A named set of probe colors for light or dark mode.
    // The built-in themes are created here and never saved. Custom themes are saved in the config file.
    public class ColorTheme : INotifyPropertyChanged
    {
        public const string SystemName = "Use Windows setting";
        public const string LightName = "Light";
        public const string DarkName = "Dark";

        private string _name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        public bool IsDark { get; set; }

        // Null for Use Windows setting, which shows the built-in Light or Dark colors.
        public ProbeColors Colors { get; set; }

        public bool IsBuiltIn { get; private set; }

        // Use Windows setting: the only theme that follows the Windows app mode.
        public bool IsSystem { get; private set; }

        // Shown beside the name in the theme list.
        public string Description => IsBuiltIn ? "Built-in" : (IsDark ? "Dark" : "Light");

        public ColorTheme Clone()
        {
            return new ColorTheme
            {
                Name = Name,
                IsDark = IsDark,
                Colors = Colors?.Clone(),
                IsBuiltIn = IsBuiltIn,
                IsSystem = IsSystem
            };
        }

        public static ColorTheme CreateSystem()
        {
            return new ColorTheme { Name = SystemName, IsBuiltIn = true, IsSystem = true };
        }

        public static ColorTheme CreateLight()
        {
            return new ColorTheme { Name = LightName, Colors = ProbeColors.CreateLightDefaults(), IsBuiltIn = true };
        }

        public static ColorTheme CreateDark()
        {
            return new ColorTheme { Name = DarkName, IsDark = true, Colors = ProbeColors.CreateDarkDefaults(), IsBuiltIn = true };
        }

        public static bool IsBuiltInName(string name)
        {
            name = name?.Trim();
            return string.Equals(name, SystemName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, LightName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, DarkName, StringComparison.OrdinalIgnoreCase);
        }

        // A name based on baseName that no theme uses yet: "Dark copy", then "Dark copy 2", and so on.
        public static string GetUniqueName(string baseName, IEnumerable<ColorTheme> themes)
        {
            string name = baseName;
            for (int i = 2; IsBuiltInName(name) || themes.Any(t => string.Equals(t.Name?.Trim(), name, StringComparison.OrdinalIgnoreCase)); ++i)
            {
                name = $"{baseName} {i}";
            }
            return name;
        }
    }
}
