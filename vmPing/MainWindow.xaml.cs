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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace vmPing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<PingItem> _pingItems = new ObservableCollection<PingItem>();
        ApplicationOptions _applicationOptions = new ApplicationOptions();

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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }


        public MainWindow()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(AlwaysOnTopCommand, AlwaysOnTopExecute));
            CommandBindings.Add(new CommandBinding(ProbeOptionsCommand, ProbeOptionsExecute));
            CommandBindings.Add(new CommandBinding(LogOutputCommand, LogOutputExecute));
            CommandBindings.Add(new CommandBinding(EmailAlertsCommand, EmailAlertsExecute));
            CommandBindings.Add(new CommandBinding(StartStopCommand, StartStopExecute));
            CommandBindings.Add(new CommandBinding(HelpCommand, HelpExecute));
            CommandBindings.Add(new CommandBinding(NewInstanceCommand, NewInstanceExecute));
            CommandBindings.Add(new CommandBinding(TraceRouteCommand, TraceRouteExecute));
            CommandBindings.Add(new CommandBinding(FloodHostCommand, FloodHostExecute));
            CommandBindings.Add(new CommandBinding(AddMonitorCommand, AddMonitorExecute));

            var kgAlwaysOnTop = new KeyGesture(Key.F9);
            var kgProbeOptions = new KeyGesture(Key.F10);
            var kgLogOutput = new KeyGesture(Key.F11);
            var kgEmailAlerts = new KeyGesture(Key.F12);
            var kgStartStop = new KeyGesture(Key.F5);
            var kgHelp = new KeyGesture(Key.F1);
            var kgNewInstance = new KeyGesture(Key.N, ModifierKeys.Control);
            var kgTraceRoute = new KeyGesture(Key.T, ModifierKeys.Control);
            var kgFloodHost = new KeyGesture(Key.F, ModifierKeys.Control);
            var kgAddMonitor = new KeyGesture(Key.A, ModifierKeys.Control);
            InputBindings.Add(new InputBinding(AlwaysOnTopCommand, kgAlwaysOnTop));
            InputBindings.Add(new InputBinding(ProbeOptionsCommand, kgProbeOptions));
            InputBindings.Add(new InputBinding(LogOutputCommand, kgLogOutput));
            InputBindings.Add(new InputBinding(EmailAlertsCommand, kgEmailAlerts));
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

            var commandLineArgs = Environment.GetCommandLineArgs();
            var errorString = "";
            var hostnameList = new List<string>();

            for (int index = 1; index < commandLineArgs.Length; ++index)
            {
                int numValue;

                switch (commandLineArgs[index].ToLower())
                {
                    case "/i":
                    case "-i":
                        ++index;
                        if (commandLineArgs.Length > index + 1 && int.TryParse(commandLineArgs[index + 1], out numValue) && numValue > 0 && numValue <= 86400)
                            _applicationOptions.PingInterval = numValue * 1000;
                        else
                            errorString += $"For switch -i you must specify the number of seconds between 1 and 86400.{Environment.NewLine}";
                        break;
                    case "/w":
                    case "-w":
                        ++index;
                        if (commandLineArgs.Length > index + 1 && int.TryParse(commandLineArgs[index + 1], out numValue) && numValue > 0 && numValue <= 60)
                            _applicationOptions.PingTimeout = numValue * 1000;
                        else
                            errorString += $"For switch -w you must specify the number of seconds between 1 and 60.{Environment.NewLine}";
                        break;
                    default:
                        hostnameList.Add(commandLineArgs[index]);
                        break;
                }
            }
            if (errorString.Length > 0)
                MessageBox.Show(
                    $"{errorString}{Environment.NewLine}{Environment.NewLine}Command Line Usage:{Environment.NewLine}vmPing [-i interval] [-w timeout] [<target_host>...]",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

            if (hostnameList.Count > 0)
            {
                AddHostMonitor(hostnameList.Count);
                for (int i = 0; i < hostnameList.Count; ++i)
                {
                    _pingItems[i].Hostname = hostnameList[i].ToUpper();
                    PingStartStop(_pingItems[i]);
                }
            }
            else
                AddHostMonitor(2);

            sliderColumns.Value = _pingItems.Count;
            icPingItems.ItemsSource = _pingItems;
        }

        private void AlwaysOnTopExecute(object sender, ExecutedRoutedEventArgs e)
        {
            mnuOnTop.IsChecked = !mnuOnTop.IsChecked;
            ToggleAlwaysOnTop();
        }

        private void ProbeOptionsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DisplayProbeOptions();
        }

        private void LogOutputExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DisplayLogOutput();
        }

        private void EmailAlertsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DisplayEmailAlerts();
        }


        private void StartStopExecute(object sender, ExecutedRoutedEventArgs e)
        {
            string toggleStatus = StartStopMenuHeader.Text;

            foreach (var pingX in _pingItems)
            {
                if (toggleStatus == "_Stop All (F5)" && pingX.IsActive)
                    PingStartStop(pingX);
                else if (toggleStatus == "_Start All (F5)" && !pingX.IsActive)
                    PingStartStop(pingX);
            }
        }


        private void HelpExecute(object sender, ExecutedRoutedEventArgs e)
        {
            // Blur all open windows.
            foreach (Window window in Application.Current.Windows)
            {
                var objBlur = new System.Windows.Media.Effects.BlurEffect();
                objBlur.Radius = 4;
                window.Opacity = 0.85;
                window.Effect = objBlur;
            }

            // Display help window.
            var helpWindow = new HelpWindow();
            helpWindow.Owner = this;
            helpWindow.ShowDialog();

            // Remove blur effect from all windows.
            foreach (Window window in Application.Current.Windows)
            {
                window.Effect = null;
                window.Opacity = 1;
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
            var traceWindow = new TraceRouteWindow(_applicationOptions.AlwaysOnTop);
            traceWindow.Show();
        }


        private void FloodHostExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var floodWindow = new FloodHostWindow(_applicationOptions.AlwaysOnTop);
            floodWindow.Show();
        }


        private void AddMonitorExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _pingItems.Add(new PingItem());
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set initial focus first text box.
            if (_pingItems.Count > 0)
            {
                var cp = icPingItems.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
                var tb = (TextBox)cp.ContentTemplate.FindName("tbHostname", cp);

                if (tb != null)
                    tb.Focus();
            }
        }


        public void AddHostMonitor(int numberOfHostMonitors)
        {
            for (; numberOfHostMonitors > 0; --numberOfHostMonitors)
                _pingItems.Add(new PingItem());
        }


        public void DisplayReply(PingItem pingItem)
        {
            if (pingItem.Reply == null)
                return;
            if (pingItem.PingBackgroundWorker.CancellationPending)
                return;

            string pingOutput;

            // Prefix the ping reply output with a timestamp.
            pingOutput = $"[{DateTime.Now.ToLongTimeString()}]  ";

            // Read the status code of the ping response.
            switch (pingItem.Reply.Status)
            {
                case IPStatus.Success:
                    pingOutput += "Reply from ";
                    pingOutput += pingItem.Reply.Address.ToString();
                    if (pingItem.Reply.RoundtripTime < 1)
                        pingOutput += "  [<1ms]";
                    else
                        pingOutput += $"  [{pingItem.Reply.RoundtripTime} ms]";
                    break;
                case IPStatus.DestinationHostUnreachable:
                    pingOutput += "Reply  [Host unreachable]";
                    break;
                case IPStatus.DestinationNetworkUnreachable:
                    pingOutput += "Reply  [Network unreachable]";
                    break;
                case IPStatus.DestinationUnreachable:
                    pingOutput += "Reply  [Unreachable]";
                    break;
                case IPStatus.TimedOut:
                    pingOutput += "Request timed out.";
                    break;
                default:
                    pingOutput = pingOutput + pingItem.Reply.Status.ToString();
                    break;
            }
            // Add response to the output window.
            pingItem.PingOutput += pingOutput + Environment.NewLine;

            // If logging is enabled, write the response to a file.
            if (_applicationOptions.LogOutput && _applicationOptions.LogPath.Length > 0)
            {
                var logPath = $@"{_applicationOptions.LogPath}\{pingItem.Hostname}.txt";
                using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter(@logPath, true))
                {
                    outputFile.WriteLine(pingOutput.Insert(1, DateTime.Now.ToShortDateString() + " "));
                }
            }

            // Set the colors of the output window and ping statistics label.
            if (pingItem.Reply.Status == IPStatus.Success)
            {
                // Ping reply received.  Set colors to signify the host is up.
                pingItem.Brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_UP);
                pingItem.Brush_OutputBackground.Freeze();
                pingItem.Brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ACTIVE);
                pingItem.Brush_OutputForeground.Freeze();
                pingItem.Brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_UP);
                pingItem.Brush_StatsForeground.Freeze();

                // Send email alert if the option is enabled and there has been a change in status.
                if (pingItem.IsHostUp == false && _applicationOptions.EmailAlert)
                    SendEmail("up", pingItem.Hostname);

                // Set status to record that the host is up.
                pingItem.IsHostUp = true;
            }
            else
            {
                // Ping timeout or error.  Set colors to signify the host is down.
                pingItem.Brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_DOWN);
                pingItem.Brush_OutputBackground.Freeze();
                pingItem.Brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ACTIVE);
                pingItem.Brush_OutputForeground.Freeze();
                pingItem.Brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_DOWN);
                pingItem.Brush_StatsForeground.Freeze();

                // Send email alert if the option is enabled and there has been a change in status.
                if (pingItem.IsHostUp == true && _applicationOptions.EmailAlert)
                    SendEmail("down", pingItem.Hostname);

                // Set status to record that the host is down.
                pingItem.IsHostUp = false;
            }
        }

        public void DisplayReplyALT(PingItem pingItem, bool isPortOpen, int portnumber, int errorCode)
        {
            if (pingItem.PingBackgroundWorker.CancellationPending)
                return;

            string pingOutput;

            // Prefix the ping reply output with a timestamp.
            pingOutput = $"[{DateTime.Now.ToLongTimeString()}]  Port {portnumber.ToString()}: ";
            if (isPortOpen)
                pingOutput += "OPEN";
            else
            {
                pingOutput += "CLOSED";
            }

            // Add response to the output window.
            pingItem.PingOutput += pingOutput + Environment.NewLine;

            // If logging is enabled, write the response to a file.
            if (_applicationOptions.LogOutput && _applicationOptions.LogPath.Length > 0)
            {
                var logPath = $@"{_applicationOptions.LogPath}\{pingItem.Hostname}.txt";
                using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter(@logPath, true))
                {
                    outputFile.WriteLine(pingOutput);
                }
            }

            // Set the colors of the output window and ping statistics label.
            if (isPortOpen)
            {
                // Ping reply received.  Set colors to signify the host is up.
                pingItem.Brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_UP);
                pingItem.Brush_OutputBackground.Freeze();
                pingItem.Brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ACTIVE);
                pingItem.Brush_OutputForeground.Freeze();
                pingItem.Brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_UP);
                pingItem.Brush_StatsForeground.Freeze();

                // Send email alert if the option is enabled and there has been a change in status.
                if (pingItem.IsHostUp == false && _applicationOptions.EmailAlert)
                    SendEmail("up", pingItem.Hostname);

                // Set status to record that the host is up.
                pingItem.IsHostUp = true;
            }
            else
            {
                // Ping timeout or error.  Set colors to signify the host is down.
                pingItem.Brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_DOWN);
                pingItem.Brush_OutputBackground.Freeze();
                pingItem.Brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ACTIVE);
                pingItem.Brush_OutputForeground.Freeze();
                pingItem.Brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_DOWN);
                pingItem.Brush_StatsForeground.Freeze();

                // Send email alert if the option is enabled and there has been a change in status.
                if (pingItem.IsHostUp == true && _applicationOptions.EmailAlert)
                    SendEmail("down", pingItem.Hostname);

                // Set status to record that the host is down.
                pingItem.IsHostUp = false;
            }
        }


        public void DisplayStatistics(PingItem pingItem)
        {
            // Update the ping statistics label with the current
            // number of pings sent, received, and lost.
            pingItem.PingStatisticsText =
                $"Sent: {pingItem.Statistics.PingsSent} Received: {pingItem.Statistics.PingsReceived} Lost: {pingItem.Statistics.PingsLost}";
        }


        public void RefreshGlobalStartStop()
        {
            // Check if any pings are in progress and update the start/stop all toggle accordingly.
            bool isActive = false;
            foreach (PingItem pingItem in _pingItems)
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
                StartStopMenuImage.Source = new BitmapImage(new Uri(@"Resources/stopCircle-16.png", UriKind.Relative));
            }
            else
            {
                StartStopMenuHeader.Text = "_Start All (F5)";
                StartStopMenuImage.Source = new BitmapImage(new Uri(@"Resources/play-16.png", UriKind.Relative));
            }
        }


        public void PingStartStop(PingItem pingItem)
        {
            if (pingItem.Hostname == null) return;
            if (pingItem.Hostname.Length == 0) return;

            pingItem.Brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_INACTIVE);
            pingItem.Brush_OutputBackground.Freeze();
            pingItem.Brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_INACTIVE);
            pingItem.Brush_OutputForeground.Freeze();
            pingItem.Brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_INACTIVE);
            pingItem.Brush_StatsForeground.Freeze();

            if (!pingItem.IsActive)
            {
                pingItem.IsActive = true;

                if (pingItem.PingBackgroundWorker != null)
                    pingItem.PingBackgroundWorker.CancelAsync();

                pingItem.PingStatisticsText = string.Empty;
                pingItem.PingOutput = $"*** Pinging {pingItem.Hostname}:{Environment.NewLine}";

                pingItem.PingBackgroundWorker = new BackgroundWorker();
                pingItem.PingResetEvent = new AutoResetEvent(false);
                if (pingItem.Hostname.Count(f => f == ':') == 1)
                    pingItem.PingBackgroundWorker.DoWork += new DoWorkEventHandler(backgroundThread_DoWorkALT);
                else
                    pingItem.PingBackgroundWorker.DoWork += new DoWorkEventHandler(backgroundThread_DoWork);
                pingItem.PingBackgroundWorker.WorkerSupportsCancellation = true;
                pingItem.PingBackgroundWorker.RunWorkerAsync(pingItem);
            }
            else
            {
                pingItem.PingBackgroundWorker.CancelAsync();
                pingItem.PingResetEvent.WaitOne();
                pingItem.IsActive = false;
            }

            RefreshGlobalStartStop();
        }

        public void btnPing_Click(object sender, EventArgs e)
        {
            var pingButton = sender as Button;
            var pingItem = pingButton.DataContext as PingItem;

            PingStartStop(pingItem);
        }


        public void backgroundThread_DoWorkALT(object sender, DoWorkEventArgs e)
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
                // Error.  Set colors to signify there was an error and abort!
                pingItem.PingOutput += "Invalid port number.";
                pingItem.Brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_ERROR);
                pingItem.Brush_OutputBackground.Freeze();
                pingItem.Brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ERROR);
                pingItem.Brush_OutputForeground.Freeze();
                pingItem.Brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_ERROR);
                pingItem.Brush_StatsForeground.Freeze();

                e.Cancel = true;
                pingItem.PingResetEvent.Set();
                pingItem.IsActive = false;
                return;
            }

            pingItem.Statistics = new PingStatistics();
            int errorCode = 0;


            while (!backgroundWorker.CancellationPending && pingItem.IsActive)
            {
                using (TcpClient client = new TcpClient())
                {
                    ++pingItem.Statistics.PingsSent;
                    DisplayStatistics(pingItem);
                    try
                    {
                        client.Connect(hostname, portnumber);
                        ++pingItem.Statistics.PingsReceived;
                        isPortOpen = true;
                    }
                    catch (SocketException ex)
                    {
                        ++pingItem.Statistics.PingsLost;
                        isPortOpen = false;
                        errorCode = ex.ErrorCode;
                    }
                    client.Close();
                }
                DisplayReplyALT(pingItem, isPortOpen, portnumber, errorCode);
                DisplayStatistics(pingItem);
                pingItem.PingResetEvent.Set();
                Thread.Sleep(5000);
            }

            pingItem.PingResetEvent.Set();
        }


        public void backgroundThread_DoWork(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = sender as BackgroundWorker;
            var pingItem = e.Argument as PingItem;

            pingItem.Statistics = new PingStatistics();
            pingItem.Sender = new Ping();
            var buffer = Encoding.ASCII.GetBytes(Constants.PING_DATA);
            var options = new PingOptions(Constants.PING_TTL, true);

            while (!backgroundWorker.CancellationPending && pingItem.IsActive)
            {
                try
                {
                    pingItem.Reply = pingItem.Sender.Send(pingItem.Hostname, _applicationOptions.PingTimeout, buffer, options);
                    ++pingItem.Statistics.PingsSent;
                    if (pingItem.Reply.Status == IPStatus.Success)
                        ++pingItem.Statistics.PingsReceived;
                    else if (pingItem.Reply.Status == IPStatus.TimedOut ||
                        pingItem.Reply.Status == IPStatus.DestinationHostUnreachable ||
                        pingItem.Reply.Status == IPStatus.DestinationNetworkUnreachable ||
                        pingItem.Reply.Status == IPStatus.DestinationUnreachable
                        )
                        ++pingItem.Statistics.PingsLost;
                    else
                        ++pingItem.Statistics.PingsError;

                    DisplayStatistics(pingItem);
                    DisplayReply(pingItem);
                    pingItem.PingResetEvent.Set();

                    if (pingItem.Reply.Status == IPStatus.TimedOut)
                    {
                        // Ping timed out.  If the ping interval is greater than the timeout,
                        // then sleep for [INTERVAL - TIMEOUT]
                        // Otherwise, sleep for a fixed amount of 1 second
                        if (_applicationOptions.PingInterval > _applicationOptions.PingTimeout)
                            Thread.Sleep(_applicationOptions.PingInterval - _applicationOptions.PingTimeout);
                        else
                            Thread.Sleep(1000);
                    }
                    else
                        // For any other type of ping response, sleep for the global ping interval amount
                        // before sending another ping.
                        Thread.Sleep(_applicationOptions.PingInterval);
                }
                catch (PingException ex)
                {
                    if (ex.InnerException is System.Net.Sockets.SocketException)
                        pingItem.PingOutput = pingItem.PingOutput + "Unable to resolve hostname.";
                    else
                        pingItem.PingOutput = pingItem.PingOutput + ex.InnerException.Message;

                    // Ping error.  Set colors to signify there was an error.
                    pingItem.Brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_ERROR);
                    pingItem.Brush_OutputBackground.Freeze();
                    pingItem.Brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ERROR);
                    pingItem.Brush_OutputForeground.Freeze();
                    pingItem.Brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_ERROR);
                    pingItem.Brush_StatsForeground.Freeze();

                    e.Cancel = true;
                    pingItem.PingResetEvent.Set();
                    pingItem.IsActive = false;
                    return;
                }
                catch
                {
                    // Ping error.  Set colors to signify there was an error.
                    pingItem.Brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_ERROR);
                    pingItem.Brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ERROR);
                    pingItem.Brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_ERROR);
                    pingItem.Brush_OutputBackground.Freeze();
                    pingItem.Brush_OutputForeground.Freeze();
                    pingItem.Brush_StatsForeground.Freeze();

                    e.Cancel = true;
                    pingItem.PingResetEvent.Set();
                    pingItem.IsActive = false;
                    return;
                }
            }

            pingItem.PingResetEvent.Set();
        }

        private void txtOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var textBoxAncestor = textBox.Parent;
            var svTextBox = textBoxAncestor as ScrollViewer;
            svTextBox.ScrollToBottom();
        }

        private void sliderColumns_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderColumns.Value > _pingItems.Count)
                sliderColumns.Value = _pingItems.Count;
        }

        private void tbHostname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var pingTB = sender as TextBox;
                var pingItem = pingTB.DataContext as PingItem;
                PingStartStop(pingItem);

                int index = _pingItems.IndexOf(pingItem);
                if (index < _pingItems.Count - 1)
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
            if (_pingItems.Count <= 1)
                return;

            var pingButton = sender as Button;
            var pingItem = pingButton.DataContext as PingItem;
            if (pingItem.PingBackgroundWorker != null)
                pingItem.PingBackgroundWorker.CancelAsync();
            _pingItems.Remove(pingItem);
            if (sliderColumns.Value > _pingItems.Count)
                sliderColumns.Value = _pingItems.Count;
        }


        public void SendEmail(string hostStatus, string hostName)
        {
            var serverAddress = _applicationOptions.EmailServer;
            var mailFromAddress = _applicationOptions.EmailFromAddress;
            var mailFromFriendly = "vmPing";
            var mailToAddress = _applicationOptions.EmailRecipient;
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

        private void mnuOnTop_Click(object sender, RoutedEventArgs e)
        {
            ToggleAlwaysOnTop();
        }

        private void mnuEmailAlerts_Click(object sender, RoutedEventArgs e)
        {
            DisplayEmailAlerts();
        }

        private void mnuLogOutput_Click(object sender, RoutedEventArgs e)
        {
            DisplayLogOutput();
        }

        private void mnuProbeOptions_Click(object sender, RoutedEventArgs e)
        {
            DisplayProbeOptions();
        }

        private void ToggleAlwaysOnTop()
        {
            _applicationOptions.AlwaysOnTop = mnuOnTop.IsChecked;

            foreach (Window window in Application.Current.Windows)
                window.Topmost = _applicationOptions.AlwaysOnTop;
        }

        private void DisplayEmailAlerts()
        {
            if (_applicationOptions.EmailAlert)
            {
                mnuEmailAlerts.IsChecked = false;
                _applicationOptions.EmailAlert = false;
                return;
            }

            // Blur all open windows.
            foreach (Window window in Application.Current.Windows)
            {
                System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
                objBlur.Radius = 4;
                window.Opacity = 0.85;
                window.Effect = objBlur;
            }

            // Display email alerts window
            var emailAlertWindow = new EmailAlertWindow(_applicationOptions);
            emailAlertWindow.Owner = this;

            emailAlertWindow.ShowDialog();
            mnuEmailAlerts.IsChecked = _applicationOptions.EmailAlert;

            // Remove blur from all windows and set topmost property if set in options.
            foreach (Window window in Application.Current.Windows)
            {
                window.Effect = null;
                window.Opacity = 1;
            }
        }

        private void DisplayLogOutput()
        {
            if (_applicationOptions.LogOutput)
            {
                mnuLogOutput.IsChecked = false;
                _applicationOptions.LogOutput = false;
                return;
            }

            // Blur all open windows.
            foreach (Window window in Application.Current.Windows)
            {
                System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
                objBlur.Radius = 4;
                window.Opacity = 0.85;
                window.Effect = objBlur;
            }

            // Display folder browse dialog box.
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select a location for the log files.";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _applicationOptions.LogPath = dialog.SelectedPath;
                _applicationOptions.LogOutput = true;
            }
            else
            {
                _applicationOptions.LogOutput = false;
            }
            mnuLogOutput.IsChecked = _applicationOptions.LogOutput;

            // Remove blur from all windows and set topmost property if set in options.
            foreach (Window window in Application.Current.Windows)
            {
                window.Effect = null;
                window.Opacity = 1;
            }
        }

        private void DisplayProbeOptions()
        {
            // Blur all open windows.
            foreach (Window window in Application.Current.Windows)
            {
                System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
                objBlur.Radius = 4;
                window.Opacity = 0.85;
                window.Effect = objBlur;
            }

            // Display probe options window
            var probeOptionsWindow = new ProbeOptionsWindow(_applicationOptions);
            probeOptionsWindow.Owner = this;

            probeOptionsWindow.ShowDialog();

            // Remove blur from all windows and set topmost property if set in options.
            foreach (Window window in Application.Current.Windows)
            {
                window.Effect = null;
                window.Opacity = 1;
            }
        }
    }




    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value == true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PingStatusButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == false)
                return "Ping";
            else
                return "Stop";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PingStatusButtonImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == false)
                return new BitmapImage(new Uri(@"Resources/play-16.png", UriKind.Relative));
            else
                return new BitmapImage(new Uri(@"Resources/stopCircle-16.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
