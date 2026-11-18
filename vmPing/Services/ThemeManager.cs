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
        private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private static ResourceDictionary s_colors;

        public static ThemeMode Mode { get; private set; } = ThemeMode.System;
        public static bool IsDark { get; private set; }

        // Raised after the colors change, for anything the color resources can't update (such as the title bar).
        public static event EventHandler ThemeChanged;

        public static void Initialize(ThemeMode mode)
        {
            // Find the color set merged in App.xaml (the light one). It gets replaced when the theme changes.
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source != null && dictionary.Source.OriginalString.Contains("Themes/Colors."))
                {
                    s_colors = dictionary;
                    break;
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

            ResourceDictionary colors = new ResourceDictionary
            {
                Source = new Uri(isDark ? DarkColorsUri : LightColorsUri, UriKind.Absolute)
            };

            Collection<ResourceDictionary> merged = Application.Current.Resources.MergedDictionaries;
            int index = s_colors != null ? merged.IndexOf(s_colors) : -1;
            if (index >= 0)
            {
                merged[index] = colors;
            }
            else
            {
                merged.Insert(0, colors);
            }

            s_colors = colors;
            IsDark = isDark;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
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
