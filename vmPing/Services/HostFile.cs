using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace vmPing.Services
{
    internal static class HostFile
    {
        private const long MaxSizeInBytes = 10 * 1024;

        // Splits text into host lines: one per line, or several on a line separated by commas.
        // A line with a # (a label or a comment) is kept whole, since the text after # may contain commas.
        // Lines that can't be a host or a comment are ignored, such as notes starting with //.
        public static List<string> Parse(string text, bool keepBlankLines = false)
        {
            return text
                .Split('\n')
                .SelectMany(line => line.Contains("#") ? new[] { line } : line.Split(','))
                .Select(line => line.Trim())
                .Where(line => line.Length == 0 ? keepBlankLines : IsHostOrComment(line))
                .ToList();
        }

        // Reads host lines from a text file.
        public static List<string> Read(string path)
        {
            if (new FileInfo(path).Length > MaxSizeInBytes)
            {
                throw new IOException($"The file is too large. The maximum file size is {MaxSizeInBytes / 1024} KB.");
            }

            return Parse(File.ReadAllText(path));
        }

        // Hosts start with a letter or digit, or with [ or : for IPv6 addresses. Comments start with #.
        private static bool IsHostOrComment(string line)
        {
            char first = line[0];
            return char.IsLetterOrDigit(first) || first == '[' || first == ':' || first == '#';
        }
    }
}
