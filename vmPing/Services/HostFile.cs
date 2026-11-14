using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace vmPing.Services
{
    internal static class HostFile
    {
        private const long MaxSizeInBytes = 10 * 1024;

        // Reads a list of hosts from a text file, one host per line.
        // Lines that don't begin with a letter, digit, or '[' (for IPv6) are skipped.
        public static List<string> Read(string path)
        {
            if (new FileInfo(path).Length > MaxSizeInBytes)
            {
                throw new IOException($"The file is too large. The maximum file size is {MaxSizeInBytes / 1024} KB.");
            }

            return File.ReadLines(path)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0 && (char.IsLetterOrDigit(line[0]) || line[0] == '['))
                .ToList();
        }
    }
}
