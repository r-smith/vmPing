using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using vmPing.Configuration;
using vmPing.Interop;

namespace vmPing.Services
{
    // Switches the app between the light and dark color sets.
    public static class ThemeManager
    {
        private const string LightColorsUri = "pack://application:,,,/Themes/Colors.Light.xaml";
        private const string DarkColorsUri = "pack://application:,,,/Themes/Colors.Dark.xaml";
        private const string IconsUri = "pack://application:,,,/Themes/Icons.xaml";
        private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private static ResourceDictionary s_colors;
        private static ResourceDictionary s_icons;

        public static ThemeMode Mode { get; private set; } = ThemeMode.System;
        public static bool IsDark { get; private set; }

        // Raised after the colors change, for anything the color resources can't update (such as the title bar).
        public static event EventHandler ThemeChanged;

        public static void Initialize(ThemeMode mode)
        {
            // Find the color set (the light one) and the icons merged in App.xaml. Both get replaced when the theme changes.
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source == null)
                {
                    continue;
                }

                if (dictionary.Source.OriginalString.Contains("Themes/Colors."))
                {
                    s_colors = dictionary;
                }
                else if (dictionary.Source.OriginalString.Contains("Themes/Icons.xaml"))
                {
                    s_icons = dictionary;
                }
            }

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            Application.Current.Exit += (sender, e) => SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

            SetMode(mode);
        }

        // Keeps a window's title bar in step with the theme. Call from the window's constructor.
        public static void RegisterWindow(Window window)
        {
            EventHandler themeChanged = (sender, e) => UpdateTitleBar(window);
            ThemeChanged += themeChanged;
            window.Closed += (sender, e) => ThemeChanged -= themeChanged;

            // The title bar is set once the window has a handle, before it's shown.
            window.SourceInitialized += (sender, e) => UpdateTitleBar(window);
        }

        private static void UpdateTitleBar(Window window)
        {
            SolidColorBrush caption = (SolidColorBrush)Application.Current.FindResource("Theme.TitleBar.Background");
            NativeMethods.SetTitleBarTheme(window, IsDark, caption.Color);
        }

        public static void SetMode(ThemeMode mode)
        {
            Mode = mode;
            ApplyColors(mode == ThemeMode.Dark || (mode == ThemeMode.System && IsWindowsInDarkMode()));
        }

        private static void ApplyColors(bool isDark)
        {
            if (s_colors != null && isDark == IsDark)
            {
                return;
            }

            s_colors = ReplaceDictionary(s_colors, isDark ? DarkColorsUri : LightColorsUri);

            // Icons look up their colors once, when they're created, so load a fresh copy that uses the new colors.
            s_icons = ReplaceDictionary(s_icons, IconsUri);

            IsDark = isDark;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        // Loads the dictionary at uri and puts it where the old one was (or first, if there wasn't one).
        private static ResourceDictionary ReplaceDictionary(ResourceDictionary old, string uri)
        {
            ResourceDictionary replacement = new ResourceDictionary
            {
                Source = new Uri(uri, UriKind.Absolute)
            };

            Collection<ResourceDictionary> merged = Application.Current.Resources.MergedDictionaries;
            int index = old != null ? merged.IndexOf(old) : -1;
            if (index >= 0)
            {
                merged[index] = replacement;
            }
            else
            {
                merged.Insert(0, replacement);
            }

            return replacement;
        }

        private static bool IsWindowsInDarkMode()
        {
            // AppsUseLightTheme is 0 when Windows apps are set to dark. It doesn't exist before Windows 10.
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(PersonalizeKey))
                {
                    return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // Windows raises this (General category) when the app mode changes between light and dark.
            if (e.Category != UserPreferenceCategory.General || Mode != ThemeMode.System)
            {
                return;
            }

            Application.Current?.Dispatcher.BeginInvoke(new Action(() => SetMode(ThemeMode.System)));
        }
    }
}
