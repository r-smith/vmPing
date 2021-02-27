using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using vmPing.Views;

namespace vmPing.Classes
{
    class CommandLine
    {
        public static List<string> ParseArguments()
        {
            var args = Environment.GetCommandLineArgs();
            var errorMessage = string.Empty;
            var hostnames = new List<string>();

            const int MinimumInterval = 1;
            const int MaxInterval = 86400;
            const int MinimumTimeout = 1;
            const int MaxTimeout = 60;

            const string CommandLineUsage = "vmPing [-i interval] [-w timeout] [<target_host>...] [<path_to_list_of_hosts>...]";

            for (var index = 1; index < args.Length; ++index)
            {
                switch (args[index].ToLower())
                {
                    case "/i":
                    case "-i":
                        if (index + 1 < args.Length &&
                            int.TryParse(args[index + 1], out int interval) &&
                            interval >= MinimumInterval && interval <= MaxInterval)
                        {
                            ApplicationOptions.PingInterval = interval * 1000;
                            ++index;
                        }
                        else
                        {
                            errorMessage +=
                                $"For switch -i you must specify the number of seconds between {MinimumInterval} and {MaxInterval}.{Environment.NewLine}";
                            break;
                        }
                        break;
                    case "/w":
                    case "-w":
                        if (args.Length > index + 1 &&
                            int.TryParse(args[index + 1], out int timeout) &&
                            timeout >= MinimumTimeout && timeout <= MaxTimeout)
                        {
                            ApplicationOptions.PingTimeout = timeout * 1000;
                            ++index;
                        }
                        else
                        {
                            errorMessage +=
                                $"For switch -w you must specify the number of seconds between {MinimumTimeout} and {MaxTimeout}.{Environment.NewLine}";
                            break;
                        }
                        break;
                    case "/?":
                    case "-?":
                    case "-h":
                    case "--help":
                        var dialogWindow = new DialogWindow(
                            icon: DialogWindow.DialogIcon.None,
                            title: "Command Line Usage",
                            body: CommandLineUsage,
                            confirmationText: "OK",
                            isCancelButtonVisible: false);
                        dialogWindow.Topmost = true;
                        dialogWindow.ShowDialog();
                        Application.Current.Shutdown();
                        break;
                    default:
                        // If an invalid argument is supplied, check to see if the argument is a valid path name.
                        //   If so, attempt to parse and read hosts from the file.  If not, use the argument as a hostname.
                        if (File.Exists(args[index]))
                            hostnames.AddRange(ReadHostsFromFile(args[index]));
                        else
                            hostnames.Add(args[index]);
                        break;
                }
            }

            // Display error message if any problems were encountered while parsing the arguments.
            if (errorMessage.Length > 0)
            {
                var dialogWindow = DialogWindow.ErrorWindow(
                    $"{errorMessage}{Environment.NewLine}Command line usage:{Environment.NewLine}{CommandLineUsage}");
                dialogWindow.Topmost = true;
                dialogWindow.ShowDialog();
                Application.Current.Shutdown();
            }

            for (int i = 0; i < hostnames.Count; ++i)
            {
                hostnames[i] = hostnames[i].ToUpper();
            }

            return hostnames;
        }


        private static List<string> ReadHostsFromFile(string path)
        {
            try
            {
                
                var linesInFile = new List<string>(File.ReadAllLines(path));
                var hostsInFile = new List<string>();

                foreach (var line in linesInFile)
                {
                    if (line == String.Empty)
                        continue;

                    if (!Char.IsLetterOrDigit(line[0]))
                        continue;
                    
                    hostsInFile.Add(line.Trim());
                }

                return hostsInFile;
            }
            catch
            {
                MessageBox.Show(
                    $"Failed parsing {path}",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return new List<string>();
            }
        }
    }
}
