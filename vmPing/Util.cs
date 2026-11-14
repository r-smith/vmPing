using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using vmPing.Interop;
using vmPing.Properties;

namespace vmPing
{
    internal static class Util
    {
        public static void ShowError(string message)
        {
            MessageBox.Show(message, Strings.Error_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool IsValidHtmlColor(string htmlColor)
        {
            return Regex.IsMatch(htmlColor, "^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");
        }

        public static string GetSafeFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return string.Empty;
            }

            // Manually check for invalid characters. The .NET Path.GetInvalidFileNameChars()
            // function is missing several invalid filename characters.
            char[] invalidCharacters = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
            return string.Join("_", filename.Split(invalidCharacters));
        }

        // Centers a window over the main window without making it an owned window.
        // The window is kept inside the work area of the main window's monitor.
        // Call before the window is shown.
        public static void CenterOverMainWindow(Window window)
        {
            Window mainWindow = Application.Current.MainWindow;

            // Use the center of the screen when the main window is hidden or minimized.
            if (mainWindow == null || !mainWindow.IsVisible || mainWindow.WindowState == WindowState.Minimized)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                return;
            }

            // A maximized window fills the work area. Left and Top can't be used
            // because they hold the restored position while maximized.
            Rect workArea = NativeMethods.GetWorkArea(mainWindow);
            Rect bounds = mainWindow.WindowState == WindowState.Maximized
                ? workArea
                : new Rect(mainWindow.Left, mainWindow.Top, mainWindow.ActualWidth, mainWindow.ActualHeight);

            double left = bounds.Left + (bounds.Width - window.Width) / 2;
            double top = bounds.Top + (bounds.Height - window.Height) / 2;

            // Move the window back if it would go past an edge of the work area.
            left = Math.Max(workArea.Left, Math.Min(left, workArea.Right - window.Width));
            top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - window.Height));

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = left;
            window.Top = top;
        }

        public static IEnumerable<Visual> GetChildren(this Visual parent, bool recurse = true)
        {
            if (parent == null)
            {
                yield break;
            }

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                // Retrieve child visual at specified index value.
                if (VisualTreeHelper.GetChild(parent, i) is Visual child)
                {
                    yield return child;

                    if (recurse)
                    {
                        foreach (Visual grandChild in child.GetChildren(true))
                        {
                            yield return grandChild;
                        }
                    }
                }
            }
        }
    }
}
