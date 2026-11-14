using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
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
