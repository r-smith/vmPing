using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

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
                        MessageBox.Show(
                            $"Command Line Usage:{Environment.NewLine}vmPing [-i interval] [-w timeout] [<target_host>...] [<path_to_list_of_hosts>...]",
                            "vmPing Help",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
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
                MessageBox.Show(
                    $"{errorMessage}{Environment.NewLine}{Environment.NewLine}Command Line Usage:{Environment.NewLine}vmPing [-i interval] [-w timeout] [<target_host>...] [<path_to_list_of_hosts>...]",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
                return new List<string>(File.ReadAllLines(path));
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
