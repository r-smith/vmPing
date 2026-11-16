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
        public static List<string> Parse(string text, bool keepBlankLines = false)
        {
            IEnumerable<string> lines = text
                .Split('\n')
                .SelectMany(line => line.Contains("#") ? new[] { line } : line.Split(','))
                .Select(line => line.Trim());

            return keepBlankLines
                ? lines.ToList()
                : lines.Where(line => line.Length > 0).ToList();
        }

        // Reads host lines from a text file.
        // Lines that don't begin with a letter, digit, '[' (for IPv6) or '#' are skipped.
        public static List<string> Read(string path)
        {
            if (new FileInfo(path).Length > MaxSizeInBytes)
            {
                throw new IOException($"The file is too large. The maximum file size is {MaxSizeInBytes / 1024} KB.");
            }

            return Parse(File.ReadAllText(path))
                .Where(line => char.IsLetterOrDigit(line[0]) || line[0] == '[' || line[0] == '#')
                .ToList();
        }
    }
}
