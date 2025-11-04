using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using vmPing.UI;

namespace vmPing.Classes
{
    class CommandLine
    {
        private const int MinInterval = 1;
        private const int MaxInterval = 86400;
        private const int MinTimeout = 1;
        private const int MaxTimeout = 60;
        private const long MaxHostFileSize = 10 * 1024;

        public static List<string> ParseArguments()
        {
            var args = Environment.GetCommandLineArgs();
            var errors = new StringBuilder();
            var hostnames = new List<string>();

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "/i":
                    case "-i":
                        if (i + 1 < args.Length &&
                            int.TryParse(args[i + 1], out int interval) &&
                            interval >= MinInterval && interval <= MaxInterval)
                        {
                            ApplicationOptions.PingInterval = interval * 1000;
                            i++; // Skip over next arg.
                        }
                        else
                        {
                            errors.AppendLine($"-i: Ping interval must be between {MinInterval} and {MaxInterval}.");
                        }
                        break;

                    case "/w":
                    case "-w":
                        if (args.Length > i + 1 &&
                            int.TryParse(args[i + 1], out int timeout) &&
                            timeout >= MinTimeout && timeout <= MaxTimeout)
                        {
                            ApplicationOptions.PingTimeout = timeout * 1000;
                            i++; // Skip over next arg.
                        }
                        else
                        {
                            errors.AppendLine($"-w: Ping timeout must be between {MinTimeout} and {MaxTimeout}.");
                        }
                        break;

                    case "/minimized":
                    case "-minimized":
                        Application.Current.MainWindow.WindowState = WindowState.Minimized;
                        break;

                    case "/?":
                    case "-?":
                    case "-h":
                    case "--help":
                        ShowHelpDialog();
                        Application.Current.Shutdown();
                        break;
                    default:
                        // If an argument isn't one of the above options, check to see if it's a file path.
                        // If so, open and read hosts from the file. If not, use the argument as a hostname.
                        if (File.Exists(args[i]))
                        {
                            hostnames.AddRange(ReadHostsFromFile(args[i]));
                        }
                        else
                        {
                            hostnames.Add(args[i]);
                        }
                        break;
                }
            }

            // Display error message if any problems were encountered while parsing the arguments.
            if (errors.Length > 0)
            {
                ShowErrorDialog(errors.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
                Application.Current.Shutdown();
            }

            return hostnames;
        }

        private static List<string> ReadHostsFromFile(string path)
        {
            try
            {
                // Check file size.
                long length = new FileInfo(path).Length;
                if (length > MaxHostFileSize)
                {
                    ShowErrorDialog($"\"{path}\" is too large. The maximum file size is {MaxHostFileSize / 1024} KB.");
                    Application.Current.Shutdown();
                    return new List<string>();
                }

                // Read, validate, and trim each line from the specified file.
                // Valid lines must not be empty and must begin with a letter, digit, or '[' character (for IPv6).
                var validLines = File.ReadAllLines(path)
                    .Where(line =>
                        !string.IsNullOrWhiteSpace(line) &&
                        (char.IsLetterOrDigit(line[0]) || line[0] == '['))
                    .Select(line => line.Trim())
                    .ToList();

                return validLines;
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Unable to parse \"{path}\": {ex.Message}");
                Application.Current.Shutdown();
                return new List<string>();
            }
        }

        private static void ShowHelpDialog()
        {
            Application.Current.MainWindow.Hide();
            new UsageWindow().ShowDialog();
        }

        private static void ShowErrorDialog(string message)
        {
            Application.Current.MainWindow.Hide();
            var dialog = DialogWindow.ErrorWindow(message);
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowInTaskbar = true;
            dialog.ShowDialog();
        }
    }
}
