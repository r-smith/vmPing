using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using vmPing.Configuration;
using vmPing.Services;
using vmPing.Views;

namespace vmPing
{
    internal static class CommandLine
    {
        private const int MinInterval = 1;
        private const int MaxInterval = 86400;
        private const int MinTimeout = 1;
        private const int MaxTimeout = 60;

        // Checked before the main window is shown, so it can start minimized without appearing first.
        public static bool HasMinimizedArgument()
        {
            return Environment.GetCommandLineArgs()
                .Skip(1)
                .Any(x => GetOptionName(x) == "minimized");
        }

        public static List<string> ParseArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            StringBuilder errors = new StringBuilder();
            List<string> hostnames = new List<string>();

            for (int i = 1; i < args.Length; i++)
            {
                string option = GetOptionName(args[i]);

                // Not an option, so it's a file of hosts or a hostname.
                if (option == null)
                {
                    if (File.Exists(args[i]))
                    {
                        try
                        {
                            hostnames.AddRange(HostFile.Read(args[i]));
                        }
                        catch (Exception ex)
                        {
                            errors.AppendLine($"Unable to read \"{args[i]}\": {ex.Message}");
                        }
                    }
                    else
                    {
                        hostnames.Add(args[i]);
                    }
                    continue;
                }

                switch (option)
                {
                    case "i":
                        if (TryReadNumber(args, ref i, MinInterval, MaxInterval, out int interval))
                        {
                            ApplicationOptions.PingInterval = interval * 1000;
                        }
                        else
                        {
                            errors.AppendLine($"-i: Ping interval must be between {MinInterval} and {MaxInterval}.");
                        }
                        break;

                    case "w":
                        if (TryReadNumber(args, ref i, MinTimeout, MaxTimeout, out int timeout))
                        {
                            ApplicationOptions.PingTimeout = timeout * 1000;
                        }
                        else
                        {
                            errors.AppendLine($"-w: Ping timeout must be between {MinTimeout} and {MaxTimeout}.");
                        }
                        break;

                    case "minimized":
                        // Handled before the window is shown. See HasMinimizedArgument.
                        break;

                    case "?":
                    case "h":
                    case "help":
                        ShowHelpDialog();
                        Application.Current.Shutdown();
                        return new List<string>();

                    default:
                        errors.AppendLine($"{args[i]}: Unknown option.");
                        break;
                }
            }

            // Display error message if any problems were encountered while parsing the arguments.
            if (errors.Length > 0)
            {
                ShowErrorDialog(errors.ToString().TrimEnd());
                Application.Current.Shutdown();
                return new List<string>();
            }

            return hostnames;
        }

        // Returns the option name without its prefix: "/minimized", "-minimized" and "--minimized" all return "minimized".
        // Returns null if the argument isn't an option. Hostnames and file paths never start with - or /.
        private static string GetOptionName(string arg)
        {
            if (!arg.StartsWith("-") && !arg.StartsWith("/"))
            {
                return null;
            }
            return arg.TrimStart('-', '/').ToLowerInvariant();
        }

        // Reads the number after an option, like the 5 in "-i 5", and moves past it.
        private static bool TryReadNumber(string[] args, ref int i, int min, int max, out int value)
        {
            value = 0;
            if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out value) || value < min || value > max)
            {
                return false;
            }

            i++;
            return true;
        }

        private static void ShowHelpDialog()
        {
            Application.Current.MainWindow.Hide();
            new UsageWindow().ShowDialog();
        }

        private static void ShowErrorDialog(string message)
        {
            Application.Current.MainWindow.Hide();
            DialogWindow dialog = DialogWindow.ErrorWindow(message);
            dialog.Topmost = true;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowInTaskbar = true;
            dialog.ShowDialog();
        }
    }
}
