using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using vmPing.Classes;
using System.Net;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<PingItem> _PingItems = new ObservableCollection<PingItem>();
        private Dictionary<string, string> _Aliases = new Dictionary<string, string>();

        public static RoutedCommand AlwaysOnTopCommand = new RoutedCommand();
        public static RoutedCommand ProbeOptionsCommand = new RoutedCommand();
        public static RoutedCommand LogOutputCommand = new RoutedCommand();
        public static RoutedCommand EmailAlertsCommand = new RoutedCommand();
        public static RoutedCommand StartStopCommand = new RoutedCommand();
        public static RoutedCommand HelpCommand = new RoutedCommand();
        public static RoutedCommand NewInstanceCommand = new RoutedCommand();
        public static RoutedCommand TraceRouteCommand = new RoutedCommand();
        public static RoutedCommand FloodHostCommand = new RoutedCommand();
        public static RoutedCommand AddMonitorCommand = new RoutedCommand();


        public MainWindow()
        {
            InitializeComponent();
            InitializeAplication();
        }


        private void InitializeAplication()
        {
            InitializeCommandBindings();
            Configuration.UpgradeConfigurationFile();
            LoadFavorites();
            LoadAliases();
            Configuration.LoadConfigurationOptions();
            ParseCommandLineArguments();

            sliderColumns.Value = _PingItems.Count;
            icPingItems.ItemsSource = _PingItems;
        }


        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ProbeOptionsCommand, ProbeOptionsExecute));
            CommandBindings.Add(new CommandBinding(StartStopCommand, StartStopExecute));
            CommandBindings.Add(new CommandBinding(HelpCommand, HelpExecute));
            CommandBindings.Add(new CommandBinding(NewInstanceCommand, NewInstanceExecute));
            CommandBindings.Add(new CommandBinding(TraceRouteCommand, TraceRouteExecute));
            CommandBindings.Add(new CommandBinding(FloodHostCommand, FloodHostExecute));
            CommandBindings.Add(new CommandBinding(AddMonitorCommand, AddMonitorExecute));

            var kgProbeOptions = new KeyGesture(Key.F10);
            var kgStartStop = new KeyGesture(Key.F5);
            var kgHelp = new KeyGesture(Key.F1);
            var kgNewInstance = new KeyGesture(Key.N, ModifierKeys.Control);
            var kgTraceRoute = new KeyGesture(Key.T, ModifierKeys.Control);
            var kgFloodHost = new KeyGesture(Key.F, ModifierKeys.Control);
            var kgAddMonitor = new KeyGesture(Key.A, ModifierKeys.Control);
            InputBindings.Add(new InputBinding(ProbeOptionsCommand, kgProbeOptions));
            InputBindings.Add(new InputBinding(StartStopCommand, kgStartStop));
            InputBindings.Add(new InputBinding(HelpCommand, kgHelp));
            InputBindings.Add(new InputBinding(NewInstanceCommand, kgNewInstance));
            InputBindings.Add(new InputBinding(TraceRouteCommand, kgTraceRoute));
            InputBindings.Add(new InputBinding(FloodHostCommand, kgFloodHost));
            InputBindings.Add(new InputBinding(AddMonitorCommand, kgAddMonitor));

            StartStopMenu.Command = StartStopCommand;
            HelpMenu.Command = HelpCommand;
            NewInstanceMenu.Command = NewInstanceCommand;
            TraceRouteMenu.Command = TraceRouteCommand;
            FloodHostMenu.Command = FloodHostCommand;
            AddMonitorMenu.Command = AddMonitorCommand;
        }


        private void ParseCommandLineArguments()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            var errorString = string.Empty;
            var hostnameList = new List<string>();

            for (var index = 1; index < commandLineArgs.Length; ++index)
            {
                int numValue;

                switch (commandLineArgs[index].ToLower())
                {
                    case "/i":
                    case "-i":
                        if (index + 1 < commandLineArgs.Length &&
                            int.TryParse(commandLineArgs[index + 1], out numValue) &&
                            numValue > 0 && numValue <= 86400)
                        {
                            ApplicationOptions.PingInterval = numValue * 1000;
                            ++index;
                        }
                        else
                        {
                            errorString += $"For switch -i you must specify the number of seconds between 1 and 86400.{Environment.NewLine}";
                            break;
                        }
                        break;
                    case "/w":
                    case "-w":
                        if (commandLineArgs.Length > index + 1 &&
                            int.TryParse(commandLineArgs[index + 1], out numValue) &&
                            numValue > 0 && numValue <= 60)
                        {
                            ApplicationOptions.PingTimeout = numValue * 1000;
                            ++index;
                        }
                        else
                        {
                            errorString += $"For switch -w you must specify the number of seconds between 1 and 60.{Environment.NewLine}";
                            break;
                        }
                        break;
                    case "/?":
                    case "-?":
                    case "--help":
                        MessageBox.Show(
                            $"Command Line Usage:{Environment.NewLine}vmPing [-i interval] [-w timeout] [<target_host>...]",
                            "vmPing Help",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        Application.Current.Shutdown();
                        break;
                    default:
                        hostnameList.Add(commandLineArgs[index]);
                        break;
                }
            }
            if (errorString.Length > 0)
            {
                MessageBox.Show(
                    $"{errorString}{Environment.NewLine}{Environment.NewLine}Command Line Usage:{Environment.NewLine}vmPing [-i interval] [-w timeout] [<target_host>...]",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            if (hostnameList.Count > 0)
            {
                AddHostMonitor(hostnameList.Count);
                for (int i = 0; i < hostnameList.Count; ++i)
                {
                    _PingItems[i].Hostname = hostnameList[i].ToUpper();
                    PingStartStop(_PingItems[i]);
                }
            }
            else
                AddHostMonitor(2);
        }


        public void AddHostMonitor(int numberOfHostMonitors)
        {
            for (; numberOfHostMonitors > 0; --numberOfHostMonitors)
                _PingItems.Add(new PingItem());
        }


        public void btnPing_Click(object sender, EventArgs e)
        {
            PingStartStop((PingItem)((Button)sender).DataContext);
        }


        public void PingStartStop(PingItem pingItem)
        {
            if (string.IsNullOrEmpty(pingItem.Hostname)) return;

            if (!pingItem.IsActive)
            {
                pingItem.IsActive = true;

                if (pingItem.PingBackgroundWorker != null)
                    pingItem.PingBackgroundWorker.CancelAsync();

                if (pingItem.Hostname != null && _Aliases.ContainsKey(pingItem.Hostname))
                    pingItem.Alias = _Aliases[pingItem.Hostname];

                pingItem.PingStatisticsText = string.Empty;
                pingItem.History = new ObservableCollection<string>();
                pingItem.AddHistory($"*** Pinging {pingItem.Hostname}:");

                pingItem.PingBackgroundWorker = new BackgroundWorker();
                pingItem.PingResetEvent = new AutoResetEvent(false);
                if (pingItem.Hostname.Count(f => f == ':') == 1)
                    pingItem.PingBackgroundWorker.DoWork += new DoWorkEventHandler(backgroundThread_PerformTcpProbe);
                else
                    pingItem.PingBackgroundWorker.DoWork += new DoWorkEventHandler(backgroundThread_PerformIcmpProbe);
                pingItem.PingBackgroundWorker.WorkerSupportsCancellation = true;
                pingItem.PingBackgroundWorker.WorkerReportsProgress = true;
                pingItem.PingBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundThread_ProgressChanged);
                pingItem.PingBackgroundWorker.RunWorkerAsync(pingItem);
            }
            else
            {
                pingItem.PingBackgroundWorker.CancelAsync();
                pingItem.PingResetEvent.WaitOne();
                pingItem.Status = PingStatus.Inactive;
                pingItem.IsActive = false;

                pingItem.WriteFinalStatisticsToHistory();
            }

            RefreshGlobalStartStop();
        }


        private void backgroundThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ApplicationOptions.PopupOption == ApplicationOptions.PopupNotificationOption.Always ||
                (ApplicationOptions.PopupOption == ApplicationOptions.PopupNotificationOption.WhenMinimized &&
                this.WindowState == WindowState.Minimized))
            {
                if (!Application.Current.Windows.OfType<PopupNotificationWindow>().Any())
                {
                    // Mark all existing status changes as read.
                    for (int i = 0; i < PingItem.StatusChangeLog.Count; ++i)
                        PingItem.StatusChangeLog[i].HasStatusBeenCleared = true;
                }
                PingItem.StatusChangeLog.Add(e.UserState as StatusChangeLog);

                if (PingItem.StatusWindow != null && PingItem.StatusWindow.IsLoaded)
                {
                    if (PingItem.StatusWindow.WindowState == WindowState.Minimized)
                        PingItem.StatusWindow.WindowState = WindowState.Normal;
                    PingItem.StatusWindow.Focus();
                }
                else if (!Application.Current.Windows.OfType<PopupNotificationWindow>().Any())
                {
                    var wnd = new PopupNotificationWindow(PingItem.StatusChangeLog);
                    wnd.Show();
                }
            }
            else
            {
                PingItem.StatusChangeLog.Add(e.UserState as StatusChangeLog);
            }
        }


        public void RefreshGlobalStartStop()
        {
            // Check if any pings are in progress and update the start/stop all toggle accordingly.
            bool isActive = false;
            foreach (PingItem pingItem in _PingItems)
            {
                if (pingItem.IsActive)
                {
                    isActive = true;
                    break;
                }
            }

            if (isActive)
            {
                StartStopMenuHeader.Text = "_Stop All (F5)";
                StartStopMenuImage.Source = new BitmapImage(new Uri(@"/Resources/stopCircle-16.png", UriKind.Relative));
            }
            else
            {
                StartStopMenuHeader.Text = "_Start All (F5)";
                StartStopMenuImage.Source = new BitmapImage(new Uri(@"/Resources/play-16.png", UriKind.Relative));
            }
        }


        public void backgroundThread_PerformIcmpProbe(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = sender as BackgroundWorker;
            var pingItem = e.Argument as PingItem;
            pingItem.Statistics = new PingStatistics();

            // Check whether a hostname or an IP address was provided.  If hostname, resolve and print IP.
            var hostnameType = Uri.CheckHostName(pingItem.Hostname);
            if (hostnameType != UriHostNameType.IPv4 && hostnameType != UriHostNameType.IPv6)
            {
                try
                {
                    var host = Dns.GetHostEntry(pingItem.Hostname);

                    if (host.AddressList.Length > 0)
                        Application.Current.Dispatcher.BeginInvoke(
                                    new Action(() => pingItem.AddHistory("*** [" + host.AddressList[0].ToString() + "]")));
                }
                catch
                {
                    Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => pingItem.AddHistory("Unable to resolve hostname.")));
                    pingItem.Status = PingStatus.Error;
                    pingItem.PingResetEvent.Set();
                    pingItem.IsActive = false;
                    return;
                }
            }

            using (pingItem.Sender = new Ping())
            {
                while (!backgroundWorker.CancellationPending && pingItem.IsActive)
                {
                    try
                    {
                        pingItem.Reply = pingItem.Sender.Send(
                            hostNameOrAddress: pingItem.Hostname,
                            timeout: ApplicationOptions.PingTimeout,
                            buffer: ApplicationOptions.Buffer,
                            options: ApplicationOptions.GetPingOptions);
                        if (backgroundWorker.CancellationPending || pingItem.IsActive == false)
                        {
                            pingItem.PingResetEvent.Set();
                            return;
                        }

                        ++pingItem.Statistics.PingsSent;
                        if (pingItem.Reply.Status == IPStatus.Success)
                        {
                            // Check for status change.
                            if (pingItem.Status == PingStatus.Down)
                            {
                                backgroundWorker.ReportProgress(
                                    0,
                                    new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = PingStatus.Up });
                                if (ApplicationOptions.IsEmailAlertEnabled)
                                    SendEmail("up", pingItem.Hostname);
                            }

                            pingItem.DownCount = 0;
                            ++pingItem.Statistics.PingsReceived;
                            pingItem.Status = PingStatus.Up;
                        }
                        else
                        {
                            if (pingItem.Status == PingStatus.Up)
                                pingItem.Status = PingStatus.Indeterminate;
                            if (pingItem.Status == PingStatus.Inactive)
                                pingItem.Status = PingStatus.Down;
                            ++pingItem.DownCount;


                            // Check for status change.
                            if (pingItem.Status == PingStatus.Indeterminate && pingItem.DownCount >= ApplicationOptions.AlertThreshold)
                            {
                                pingItem.Status = PingStatus.Down;
                                backgroundWorker.ReportProgress(
                                    0,
                                    new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = PingStatus.Down });
                                if (ApplicationOptions.IsEmailAlertEnabled)
                                    SendEmail("down", pingItem.Hostname);
                            }

                            if (pingItem.Reply.Status == IPStatus.TimedOut ||
                                pingItem.Reply.Status == IPStatus.DestinationHostUnreachable ||
                                pingItem.Reply.Status == IPStatus.DestinationNetworkUnreachable ||
                                pingItem.Reply.Status == IPStatus.DestinationUnreachable
                                )
                                ++pingItem.Statistics.PingsLost;
                            else
                                ++pingItem.Statistics.PingsError;
                        }

                        DisplayStatistics(pingItem);
                        DisplayIcmpReply(pingItem);
                        pingItem.PingResetEvent.Set();

                        if (pingItem.Reply.Status == IPStatus.TimedOut)
                        {
                            // Ping timed out.  If the ping interval is greater than the timeout,
                            // then sleep for [INTERVAL - TIMEOUT]
                            // Otherwise, sleep for a fixed amount of 1 second
                            if (ApplicationOptions.PingInterval > ApplicationOptions.PingTimeout)
                                Thread.Sleep(ApplicationOptions.PingInterval - ApplicationOptions.PingTimeout);
                            else
                                Thread.Sleep(1000);
                        }
                        else
                            // For any other type of ping response, sleep for the global ping interval amount
                            // before sending another ping.
                            Thread.Sleep(ApplicationOptions.PingInterval);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException is SocketException)
                            Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => pingItem.AddHistory("Unable to resolve hostname.")));
                        else
                            Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => pingItem.AddHistory("Error: " + ex.Message)));

                        e.Cancel = true;

                        // Check for status change.
                        if (pingItem.Status == PingStatus.Up || pingItem.Status == PingStatus.Down || pingItem.Status == PingStatus.Indeterminate)
                        {
                            backgroundWorker.ReportProgress(
                                0,
                                new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = PingStatus.Error });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                SendEmail("error", pingItem.Hostname);
                        }

                        pingItem.Status = PingStatus.Error;
                        pingItem.PingResetEvent.Set();
                        pingItem.IsActive = false;
                        return;
                    }
                }
            }

            pingItem.PingResetEvent.Set();
        }


        public void backgroundThread_PerformTcpProbe(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = sender as BackgroundWorker;
            var pingItem = e.Argument as PingItem;

            var hostAndPort = pingItem.Hostname.Split(':');
            string hostname = hostAndPort[0];
            int portnumber;
            bool isPortValid;
            bool isPortOpen = false;
            if (int.TryParse(hostAndPort[1], out portnumber) && portnumber >= 1 && portnumber <= 65535)
                isPortValid = true;
            else
                isPortValid = false;

            if (!isPortValid)
            {
                // Error.
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => pingItem.AddHistory("Invalid port number.")));

                e.Cancel = true;
                pingItem.PingResetEvent.Set();
                pingItem.Status = PingStatus.Error;
                pingItem.IsActive = false;
                return;
            }

            // Check whether a hostname or an IP address was provided.  If hostname, resolve and print IP.
            var hostnameType = Uri.CheckHostName(hostname);
            if (hostnameType != UriHostNameType.IPv4 && hostnameType != UriHostNameType.IPv6)
            {
                try
                {
                    var host = Dns.GetHostEntry(hostname);

                    if (host.AddressList.Length > 0)
                        Application.Current.Dispatcher.BeginInvoke(
                                    new Action(() => pingItem.AddHistory("*** [" + host.AddressList[0].ToString() + "]")));
                }
                catch
                {
                    Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => pingItem.AddHistory("Unable to resolve hostname.")));
                    pingItem.Status = PingStatus.Error;
                    pingItem.PingResetEvent.Set();
                    pingItem.IsActive = false;
                    return;
                }
            }

            pingItem.Statistics = new PingStatistics();
            int errorCode = 0;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (!backgroundWorker.CancellationPending && pingItem.IsActive)
            {
                stopwatch.Restart();

                using (TcpClient client = new TcpClient())
                {
                    ++pingItem.Statistics.PingsSent;
                    DisplayStatistics(pingItem);

                    try
                    {
                        var result = client.BeginConnect(hostname, portnumber, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                        if (!success)
                        {
                            throw new SocketException();
                        }

                        client.EndConnect(result);

                        if (backgroundWorker.CancellationPending || pingItem.IsActive == false)
                        {
                            pingItem.PingResetEvent.Set();
                            return;
                        }

                        // Check for status change.
                        if (pingItem.Status == PingStatus.Down)
                        {
                            backgroundWorker.ReportProgress(
                                0,
                                new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = PingStatus.Up });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                SendEmail("up", pingItem.Hostname);
                        }

                        pingItem.DownCount = 0;
                        ++pingItem.Statistics.PingsReceived;
                        pingItem.Status = PingStatus.Up;
                        isPortOpen = true;
                    }
                    catch (SocketException ex)
                    {
                        const int WSAHOST_NOT_FOUND = 11001;

                        stopwatch.Stop();

                        if (backgroundWorker.CancellationPending || pingItem.IsActive == false)
                        {
                            pingItem.PingResetEvent.Set();
                            return;
                        }

                        if (pingItem.Status == PingStatus.Up)
                            pingItem.Status = PingStatus.Indeterminate;
                        if (pingItem.Status == PingStatus.Inactive)
                            pingItem.Status = PingStatus.Down;
                        ++pingItem.DownCount;

                        // Check for status change.
                        if (pingItem.Status == PingStatus.Indeterminate && pingItem.DownCount >= ApplicationOptions.AlertThreshold)
                        {
                            pingItem.Status = PingStatus.Down;
                            backgroundWorker.ReportProgress(
                                0,
                                new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = PingStatus.Down });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                SendEmail("down", pingItem.Hostname);
                        }

                        // If hostname cannot be resolved, report error and stop.
                        if (ex.ErrorCode == WSAHOST_NOT_FOUND)
                        {
                            e.Cancel = true;
                            Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => pingItem.AddHistory("Unable to resolve hostname.")));

                            pingItem.Status = PingStatus.Error;
                            pingItem.PingResetEvent.Set();
                            pingItem.IsActive = false;
                            return;
                        }

                        ++pingItem.Statistics.PingsLost;
                        isPortOpen = false;
                        errorCode = ex.ErrorCode;
                    }
                    client.Close();
                }
                DisplayTcpReply(pingItem, isPortOpen, portnumber, errorCode, stopwatch.ElapsedMilliseconds);
                DisplayStatistics(pingItem);
                pingItem.PingResetEvent.Set();

                Thread.Sleep(ApplicationOptions.PingInterval);
            }

            pingItem.PingResetEvent.Set();
        }


        public void DisplayIcmpReply(PingItem pingItem)
        {
            if (pingItem.Reply == null)
                return;
            if (pingItem.PingBackgroundWorker.CancellationPending)
                return;

            var pingOutput = new StringBuilder($"[{DateTime.Now.ToLongTimeString()}]  ");

            // Read the status code of the ping response.
            switch (pingItem.Reply.Status)
            {
                case IPStatus.Success:
                    pingOutput.Append("Reply from ");
                    pingOutput.Append(pingItem.Reply.Address.ToString());
                    if (pingItem.Reply.RoundtripTime < 1)
                        pingOutput.Append("  [<1ms]");
                    else
                        pingOutput.Append($"  [{pingItem.Reply.RoundtripTime} ms]");
                    break;
                case IPStatus.DestinationHostUnreachable:
                    pingOutput.Append("Reply  [Host unreachable]");
                    break;
                case IPStatus.DestinationNetworkUnreachable:
                    pingOutput.Append("Reply  [Network unreachable]");
                    break;
                case IPStatus.DestinationUnreachable:
                    pingOutput.Append("Reply  [Unreachable]");
                    break;
                case IPStatus.TimedOut:
                    pingOutput.Append("Request timed out.");
                    break;
                default:
                    pingOutput.Append(pingItem.Reply.Status.ToString());
                    break;
            }
            // Add response to the output window.
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => pingItem.AddHistory(pingOutput.ToString())));

            // If logging is enabled, write the response to a file.
            if (ApplicationOptions.IsLogOutputEnabled && ApplicationOptions.LogPath.Length > 0)
            {
                var logPath = $@"{ApplicationOptions.LogPath}\{pingItem.Hostname}.txt";
                using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter(@logPath, true))
                {
                    outputFile.WriteLine(pingOutput.ToString().Insert(1, DateTime.Now.ToShortDateString() + " "));
                }
            }
        }


        public void DisplayTcpReply(PingItem pingItem, bool isPortOpen, int portnumber, int errorCode, long elapsedTime)
        {
            if (pingItem.PingBackgroundWorker.CancellationPending)
                return;

            // Prefix the ping reply output with a timestamp.
            var pingOutput = new StringBuilder($"[{DateTime.Now.ToLongTimeString()}]  Port {portnumber.ToString()}: ");
            if (isPortOpen)
                pingOutput.Append("OPEN  [" + elapsedTime.ToString() + "ms]");
            else
            {
                pingOutput.Append("CLOSED");
            }

            // Add response to the output window.
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => pingItem.AddHistory(pingOutput.ToString())));

            // If logging is enabled, write the response to a file.
            if (ApplicationOptions.IsLogOutputEnabled && ApplicationOptions.LogPath.Length > 0)
            {
                var index = pingItem.Hostname.IndexOf(':');
                var hostname = (index > 0) ? pingItem.Hostname.Substring(0, index) : pingItem.Hostname;
                var logPath = $@"{ApplicationOptions.LogPath}\{hostname}.txt";
                using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter(@logPath, true))
                {
                    outputFile.WriteLine(pingOutput.ToString().Insert(1, DateTime.Now.ToShortDateString() + " "));
                }
            }
        }


        public void DisplayStatistics(PingItem pingItem)
        {
            // Update the ping statistics label with the current
            // number of pings sent, received, and lost.
            pingItem.PingStatisticsText =
                $"Sent: {pingItem.Statistics.PingsSent} Received: {pingItem.Statistics.PingsReceived} Lost: {pingItem.Statistics.PingsLost}";
        }


        private void sliderColumns_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderColumns.Value > _PingItems.Count)
                sliderColumns.Value = _PingItems.Count;
        }


        private void tbHostname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var pingTB = sender as TextBox;
                var pingItem = pingTB.DataContext as PingItem;
                PingStartStop(pingItem);

                int index = _PingItems.IndexOf(pingItem);
                if (index < _PingItems.Count - 1)
                {
                    var cp = icPingItems.ItemContainerGenerator.ContainerFromIndex(index + 1) as ContentPresenter;
                    var tb = (TextBox)cp.ContentTemplate.FindName("tbHostname", cp);

                    if (tb != null)
                        tb.Focus();
                }
            }
        }


        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (_PingItems.Count <= 1)
                return;

            var pingButton = sender as Button;
            var pingItem = pingButton.DataContext as PingItem;
            if (pingItem.PingBackgroundWorker != null)
                pingItem.PingBackgroundWorker.CancelAsync();
            _PingItems.Remove(pingItem);
            if (sliderColumns.Value > _PingItems.Count)
                sliderColumns.Value = _PingItems.Count;
            RefreshGlobalStartStop();
        }


        public void SendEmail(string hostStatus, string hostName)
        {
            var serverAddress = ApplicationOptions.EmailServer;
            var serverUser = ApplicationOptions.EmailUser;
            var serverPassword = ApplicationOptions.EmailPassword;
            var serverPort = ApplicationOptions.EmailPort;
            var mailFromAddress = ApplicationOptions.EmailFromAddress;
            var mailFromFriendly = "vmPing";
            var mailToAddress = ApplicationOptions.EmailRecipient;
            var mailSubject = $"[vmPing] {hostName} <> Host {hostStatus}";
            var mailBody =
                $"{hostName} is {hostStatus}.{Environment.NewLine}" +
                $"{DateTime.Now.ToLongDateString()}  {DateTime.Now.ToLongTimeString()}";

            var message = new MailMessage();

            try
            {
                var smtpClient = new SmtpClient();
                MailAddress fromAddress;
                if (mailFromFriendly.Length > 0)
                    fromAddress = new MailAddress(mailFromAddress, mailFromFriendly);
                else
                    fromAddress = new MailAddress(mailFromAddress);

                smtpClient.Host = serverAddress;

                if (ApplicationOptions.IsEmailAuthenticationRequired)
                {
                    smtpClient.Credentials = new NetworkCredential(serverUser, serverPassword);
                }

                if (serverPort.Length > 0)
                    smtpClient.Port = Int32.Parse(serverPort);

                message.From = fromAddress;
                message.Subject = mailSubject;
                message.Body = mailBody;

                message.To.Add(mailToAddress);

                //Send the email.
                smtpClient.Send(message);
            }
            catch
            {
                // There was an error sending Email.
            }
            finally
            {
                message.Dispose();
            }
        }

        
        private void ProbeOptionsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DisplayProbeOptions();
        }


        private void StartStopExecute(object sender, ExecutedRoutedEventArgs e)
        {
            string toggleStatus = StartStopMenuHeader.Text;

            foreach (var pingItem in _PingItems)
            {
                if (toggleStatus == "_Stop All (F5)" && pingItem.IsActive)
                    PingStartStop(pingItem);
                else if (toggleStatus == "_Start All (F5)" && !pingItem.IsActive)
                    PingStartStop(pingItem);
            }
        }


        private void HelpExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (HelpWindow.openWindow != null)
                HelpWindow.openWindow.Activate();
            else
            {
                var helpWindow = new HelpWindow();
                helpWindow.Show();
            }
        }


        private void NewInstanceExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName =
                System.Reflection.Assembly.GetExecutingAssembly().Location;
            try
            {
                p.Start();
            }

            catch
            {
                // do nothing.
            }
        }


        private void TraceRouteExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var traceWindow = new TraceRouteWindow(ApplicationOptions.AlwaysOnTop);
            traceWindow.Show();
        }


        private void FloodHostExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var floodWindow = new FloodHostWindow(ApplicationOptions.AlwaysOnTop);
            floodWindow.Show();
        }


        private void AddMonitorExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _PingItems.Add(new PingItem());
        }

        
        private void mnuProbeOptions_Click(object sender, RoutedEventArgs e)
        {
            DisplayProbeOptions();
        }


        private void DisplayProbeOptions()
        {
            if (OptionsWindow.openWindow != null)
                OptionsWindow.openWindow.Activate();
            else
            {
                var optionsWindow = new OptionsWindow();
                optionsWindow.Show();
            }
        }


        private void ClearAllPingItems()
        {
            foreach (var pingItem in _PingItems)
            {
                if (pingItem.PingBackgroundWorker != null)
                    pingItem.PingBackgroundWorker.CancelAsync();
            }
            _PingItems.Clear();
            RefreshGlobalStartStop();
        }

        private void LoadFavorites()
        {
            var favoritesList = Favorite.GetFavoriteTitles();

            // Clear existing favorites menu.
            for (int i = mnuFavorites.Items.Count - 1; i > 2; --i)
                mnuFavorites.Items.RemoveAt(i);

            // Load favorites.
            foreach (var fav in favoritesList)
            {
                var menuItem = new MenuItem();
                menuItem.Header = fav;
                menuItem.Click += (s, r) =>
                {
                    ClearAllPingItems();

                    var selectedFavorite = s as MenuItem;
                    var favorite = Favorite.GetFavoriteContents(selectedFavorite.Header.ToString());
                    if (favorite.Hostnames.Count < 1)
                        AddHostMonitor(1);
                    else
                    {
                        AddHostMonitor(favorite.Hostnames.Count);
                        for (int i = 0; i < favorite.Hostnames.Count; ++i)
                        {
                            _PingItems[i].Hostname = favorite.Hostnames[i].ToUpper();
                            PingStartStop(_PingItems[i]);
                        }
                    }

                    sliderColumns.Value = favorite.ColumnCount;
                };

                mnuFavorites.Items.Add(menuItem);
            }
        }


        private void LoadAliases()
        {
            _Aliases = Alias.GetAliases();
            var aliasList = _Aliases.ToList();
            aliasList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            
            // Clear existing aliases menu.
            for (int i = mnuAliases.Items.Count - 1; i > 1; --i)
                mnuAliases.Items.RemoveAt(i);

            // Load aliases.
            foreach (var alias in aliasList)
            {
                mnuAliases.Items.Add(BuildAliasMenuItem(alias, false));
            }

            foreach (var pingItem in _PingItems)
            {
                if (pingItem.Hostname != null && _Aliases.ContainsKey(pingItem.Hostname))
                    pingItem.Alias = _Aliases[pingItem.Hostname];
                else
                    pingItem.Alias = string.Empty;
            }
        }

        private MenuItem BuildAliasMenuItem(KeyValuePair<string, string> alias, bool isContextMenu)
        {
            var menuItem = new MenuItem();
            menuItem.Header = alias.Value;

            if (isContextMenu)
            {
                menuItem.Click += (s, r) =>
                {
                    var selectedMenuItem = s as MenuItem;
                    var selectedAlias = (PingItem)selectedMenuItem.DataContext;
                    selectedAlias.Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedMenuItem.Header.ToString()).Key;
                    PingStartStop(selectedAlias);
                };
            }
            else
            {
                menuItem.Click += (s, r) =>
                {
                    var selectedAlias = s as MenuItem;

                    var didFindEmptyHost = false;
                    for (int i = 0; i < _PingItems.Count; ++i)
                    {
                        if (string.IsNullOrWhiteSpace(_PingItems[i].Hostname))
                        {
                            _PingItems[i].Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedAlias.Header.ToString()).Key;
                            PingStartStop(_PingItems[i]);
                            didFindEmptyHost = true;
                            break;
                        }
                    }

                    if (!didFindEmptyHost)
                    {
                        AddHostMonitor(1);
                        _PingItems[_PingItems.Count - 1].Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedAlias.Header.ToString()).Key;
                        PingStartStop(_PingItems[_PingItems.Count - 1]);
                    }
                };
            }

            return menuItem;
        }


        private void mnuAddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            // Display add to favorites window.
            var currentHostList = new List<string>();
            var haveAnyHostnamesBeenEntered = false;

            for (int i = 0; i < _PingItems.Count; ++i)
            {
                currentHostList.Add(_PingItems[i].Hostname);
                if (!string.IsNullOrWhiteSpace(_PingItems[i].Hostname))
                    haveAnyHostnamesBeenEntered = true;
            }

            if (!haveAnyHostnamesBeenEntered)
            {
                var dialogWindow = new DialogWindow(
                    DialogWindow.DialogIcon.Warning,
                    "Error",
                    $"You have not entered any hostnames.  Please setup vmPing with the hosts you would like to save as a favorite set.",
                    "OK",
                    false);
                dialogWindow.Owner = this;
                dialogWindow.ShowDialog();
                return;
            }

            var addToFavoritesWindow = new NewFavoriteWindow(currentHostList, (int)sliderColumns.Value);
            addToFavoritesWindow.Owner = this;
            if (addToFavoritesWindow.ShowDialog() == true)
            {
                LoadFavorites();
            }
        }

        private void mnuManageFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (ManageFavoritesWindow.openWindow != null)
                ManageFavoritesWindow.openWindow.Activate();
            else
            {
                var manageFavoritesWindow = new ManageFavoritesWindow();
                manageFavoritesWindow.Owner = this;
                manageFavoritesWindow.ShowDialog();
                LoadFavorites();
            }
        }

        private void mnuManageAliases_Click(object sender, RoutedEventArgs e)
        {
            if (ManageAliasesWindow.openWindow != null)
                ManageAliasesWindow.openWindow.Activate();
            else
            {
                var manageAliasesWindow = new ManageAliasesWindow();
                manageAliasesWindow.Owner = this;
                manageAliasesWindow.ShowDialog();
                LoadAliases();
            }
        }

        private void mnuPopupNotification_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            mnuPopupAlways.IsChecked = false;
            mnuPopupNever.IsChecked = false;
            mnuPopupWhenMinimized.IsChecked = false;

            menuItem.IsChecked = true;

            switch (menuItem.Header.ToString())
            {
                case "Always":
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Always;
                    break;
                case "Never":
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Never;
                    break;
                case "When Minimized":
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.WhenMinimized;
                    break;
            }
        }

        private void ButtonIsolatedView_Click(object sender, RoutedEventArgs e)
        {
            var pingButton = sender as Button;
            var pingItem = pingButton.DataContext as PingItem;
            if (pingItem.IsolatedWindow == null || pingItem.IsolatedWindow.IsLoaded == false)
            {
                var wnd = new IsolatedPingWindow(pingItem);
                wnd.Show();
            }
            else if (pingItem.IsolatedWindow.IsLoaded)
            {
                pingItem.IsolatedWindow.Focus();
            }
        }

        private void ButtonEditAlias_Click(object sender, RoutedEventArgs e)
        {
            var pingButton = sender as Button;
            var pingItem = pingButton.DataContext as PingItem;

            if (string.IsNullOrEmpty(pingItem.Hostname))
                return;

            if (_Aliases.ContainsKey(pingItem.Hostname))
                pingItem.Alias = _Aliases[pingItem.Hostname];
            else
                pingItem.Alias = string.Empty;

            var wnd = new EditAliasWindow(pingItem);
            wnd.Owner = this;

            if (wnd.ShowDialog() == true)
            {
                LoadAliases();
            }
        }

        private void mnuStatusHistory_Click(object sender, RoutedEventArgs e)
        {
            if (PingItem.StatusWindow == null || PingItem.StatusWindow.IsLoaded == false)
            {
                var wnd = new StatusHistoryWindow(PingItem.StatusChangeLog);
                PingItem.StatusWindow = wnd;
                wnd.Show();
            }
            else if (PingItem.StatusWindow.IsLoaded)
            {
                PingItem.StatusWindow.Focus();
            }
        }

        private void tbHostname_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to textbox on newly added monitors.  If the hostname field is blank for any existing monitors, do not change focus.
            for (int i = 0; i < _PingItems.Count - 1; ++i)
            {
                if (string.IsNullOrEmpty(_PingItems[i].Hostname))
                    return;
            }
            var tb = (TextBox)sender;
            tb.Focus();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Set initial focus first text box.
            if (_PingItems.Count > 0)
            {
                var cp = icPingItems.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
                var tb = (TextBox)cp.ContentTemplate.FindName("tbHostname", cp);

                if (tb != null)
                    tb.Focus();
            }
        }
    }
}