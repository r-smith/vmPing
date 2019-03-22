using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using vmPing.Views;

namespace vmPing.Classes
{
    public partial class Probe
    {
        public static void StartStop(Probe probe)
        {
            if (string.IsNullOrEmpty(probe.Hostname)) return;

            if (!probe.IsActive)
            {
                probe.IsActive = true;

                if (probe.Thread != null)
                    probe.Thread.CancelAsync();

                // TODO:
                //if (pingItem.Hostname != null && _Aliases.ContainsKey(pingItem.Hostname))
                //    pingItem.Alias = _Aliases[pingItem.Hostname];

                probe.StatisticsText = string.Empty;
                probe.History = new ObservableCollection<string>();
                probe.AddHistory($"*** Pinging {probe.Hostname}:");

                probe.Thread = new BackgroundWorker();
                probe.ThreadResetEvent = new AutoResetEvent(false);
                if (probe.Hostname.Count(f => f == ':') == 1)
                    probe.Thread.DoWork += new DoWorkEventHandler(Thread_PerformTcpProbe);
                else
                    probe.Thread.DoWork += new DoWorkEventHandler(Thread_PerformIcmpProbe);
                probe.Thread.WorkerSupportsCancellation = true;
                probe.Thread.WorkerReportsProgress = true;
                probe.Thread.ProgressChanged += new ProgressChangedEventHandler(Thread_ProgressChanged);
                probe.Thread.RunWorkerAsync(probe);
            }
            else
            {
                probe.Thread.CancelAsync();
                probe.ThreadResetEvent.WaitOne();
                probe.Status = ProbeStatus.Inactive;
                probe.IsActive = false;

                probe.WriteFinalStatisticsToHistory();
            }

            // TODO:
            //RefreshGlobalStartStop();
        }

        private static void Thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ApplicationOptions.PopupOption == ApplicationOptions.PopupNotificationOption.Always ||
                (ApplicationOptions.PopupOption == ApplicationOptions.PopupNotificationOption.WhenMinimized &&
                Application.Current.MainWindow.WindowState == WindowState.Minimized))
            {
                if (!Application.Current.Windows.OfType<PopupNotificationWindow>().Any())
                {
                    // Mark all existing status changes as read.
                    for (int i = 0; i < StatusChangeLog.Count; ++i)
                        StatusChangeLog[i].HasStatusBeenCleared = true;
                }
                StatusChangeLog.Add(e.UserState as StatusChangeLog);

                if (StatusWindow != null && StatusWindow.IsLoaded)
                {
                    if (StatusWindow.WindowState == WindowState.Minimized)
                        StatusWindow.WindowState = WindowState.Normal;
                    StatusWindow.Focus();
                }
                else if (!Application.Current.Windows.OfType<PopupNotificationWindow>().Any())
                {
                    var wnd = new PopupNotificationWindow(StatusChangeLog);
                    wnd.Show();
                }
            }
            else
            {
                StatusChangeLog.Add(e.UserState as StatusChangeLog);
            }
        }


        private static void Thread_PerformIcmpProbe(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = sender as BackgroundWorker;
            var pingItem = e.Argument as Probe;
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
                    pingItem.Status = ProbeStatus.Error;
                    pingItem.ThreadResetEvent.Set();
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
                            pingItem.ThreadResetEvent.Set();
                            return;
                        }

                        ++pingItem.Statistics.Sent;
                        if (pingItem.Reply.Status == IPStatus.Success)
                        {
                            // Check for status change.
                            if (pingItem.Status == ProbeStatus.Down)
                            {
                                backgroundWorker.ReportProgress(
                                    0,
                                    new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = ProbeStatus.Up });
                                if (ApplicationOptions.IsEmailAlertEnabled)
                                    Util.SendEmail("up", pingItem.Hostname);
                            }

                            pingItem.DownCount = 0;
                            ++pingItem.Statistics.Received;
                            pingItem.Status = ProbeStatus.Up;
                        }
                        else
                        {
                            if (pingItem.Status == ProbeStatus.Up)
                                pingItem.Status = ProbeStatus.Indeterminate;
                            if (pingItem.Status == ProbeStatus.Inactive)
                                pingItem.Status = ProbeStatus.Down;
                            ++pingItem.DownCount;


                            // Check for status change.
                            if (pingItem.Status == ProbeStatus.Indeterminate && pingItem.DownCount >= ApplicationOptions.AlertThreshold)
                            {
                                pingItem.Status = ProbeStatus.Down;
                                backgroundWorker.ReportProgress(
                                    0,
                                    new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = ProbeStatus.Down });
                                if (ApplicationOptions.IsEmailAlertEnabled)
                                    Util.SendEmail("down", pingItem.Hostname);
                            }

                            if (pingItem.Reply.Status == IPStatus.TimedOut ||
                                pingItem.Reply.Status == IPStatus.DestinationHostUnreachable ||
                                pingItem.Reply.Status == IPStatus.DestinationNetworkUnreachable ||
                                pingItem.Reply.Status == IPStatus.DestinationUnreachable
                                )
                                ++pingItem.Statistics.Lost;
                            else
                                ++pingItem.Statistics.Error;
                        }

                        DisplayStatistics(pingItem);
                        DisplayIcmpReply(pingItem);
                        pingItem.ThreadResetEvent.Set();

                        if (pingItem.Reply.Status == IPStatus.TimedOut)
                        {
                            // Ping timed out.  If the ping interval is greater than the timeout,
                            // then sleep for [INTERVAL - TIMEOUT]
                            // Otherwise, sleep for a fixed amount of 1 second
                            if (ApplicationOptions.PingInterval > ApplicationOptions.PingTimeout)
                                System.Threading.Thread.Sleep(ApplicationOptions.PingInterval - ApplicationOptions.PingTimeout);
                            else
                                System.Threading.Thread.Sleep(1000);
                        }
                        else
                            // For any other type of ping response, sleep for the global ping interval amount
                            // before sending another ping.
                            System.Threading.Thread.Sleep(ApplicationOptions.PingInterval);
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
                        if (pingItem.Status == ProbeStatus.Up || pingItem.Status == ProbeStatus.Down || pingItem.Status == ProbeStatus.Indeterminate)
                        {
                            backgroundWorker.ReportProgress(
                                0,
                                new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = ProbeStatus.Error });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                Util.SendEmail("error", pingItem.Hostname);
                        }

                        pingItem.Status = ProbeStatus.Error;
                        pingItem.ThreadResetEvent.Set();
                        pingItem.IsActive = false;
                        return;
                    }
                }
            }

            pingItem.ThreadResetEvent.Set();
        }


        private static void Thread_PerformTcpProbe(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = sender as BackgroundWorker;
            var pingItem = e.Argument as Probe;

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
                pingItem.ThreadResetEvent.Set();
                pingItem.Status = ProbeStatus.Error;
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
                    pingItem.Status = ProbeStatus.Error;
                    pingItem.ThreadResetEvent.Set();
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
                    ++pingItem.Statistics.Sent;
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
                            pingItem.ThreadResetEvent.Set();
                            return;
                        }

                        // Check for status change.
                        if (pingItem.Status == ProbeStatus.Down)
                        {
                            backgroundWorker.ReportProgress(
                                0,
                                new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = ProbeStatus.Up });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                Util.SendEmail("up", pingItem.Hostname);
                        }

                        pingItem.DownCount = 0;
                        ++pingItem.Statistics.Received;
                        pingItem.Status = ProbeStatus.Up;
                        isPortOpen = true;
                    }
                    catch (SocketException ex)
                    {
                        const int WSAHOST_NOT_FOUND = 11001;

                        stopwatch.Stop();

                        if (backgroundWorker.CancellationPending || pingItem.IsActive == false)
                        {
                            pingItem.ThreadResetEvent.Set();
                            return;
                        }

                        if (pingItem.Status == ProbeStatus.Up)
                            pingItem.Status = ProbeStatus.Indeterminate;
                        if (pingItem.Status == ProbeStatus.Inactive)
                            pingItem.Status = ProbeStatus.Down;
                        ++pingItem.DownCount;

                        // Check for status change.
                        if (pingItem.Status == ProbeStatus.Indeterminate && pingItem.DownCount >= ApplicationOptions.AlertThreshold)
                        {
                            pingItem.Status = ProbeStatus.Down;
                            backgroundWorker.ReportProgress(
                                0,
                                new StatusChangeLog { Timestamp = DateTime.Now, Hostname = pingItem.Hostname, Status = ProbeStatus.Down });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                Util.SendEmail("down", pingItem.Hostname);
                        }

                        // If hostname cannot be resolved, report error and stop.
                        if (ex.ErrorCode == WSAHOST_NOT_FOUND)
                        {
                            e.Cancel = true;
                            Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => pingItem.AddHistory("Unable to resolve hostname.")));

                            pingItem.Status = ProbeStatus.Error;
                            pingItem.ThreadResetEvent.Set();
                            pingItem.IsActive = false;
                            return;
                        }

                        ++pingItem.Statistics.Lost;
                        isPortOpen = false;
                        errorCode = ex.ErrorCode;
                    }
                    client.Close();
                }
                DisplayTcpReply(pingItem, isPortOpen, portnumber, errorCode, stopwatch.ElapsedMilliseconds);
                DisplayStatistics(pingItem);
                pingItem.ThreadResetEvent.Set();

                System.Threading.Thread.Sleep(ApplicationOptions.PingInterval);
            }

            pingItem.ThreadResetEvent.Set();
        }


        private static void DisplayIcmpReply(Probe pingItem)
        {
            if (pingItem.Reply == null)
                return;
            if (pingItem.Thread.CancellationPending)
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


        private static void DisplayTcpReply(Probe pingItem, bool isPortOpen, int portnumber, int errorCode, long elapsedTime)
        {
            if (pingItem.Thread.CancellationPending)
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


        private static void DisplayStatistics(Probe pingItem)
        {
            // Update the ping statistics label with the current
            // number of pings sent, received, and lost.
            pingItem.StatisticsText =
                $"Sent: {pingItem.Statistics.Sent} Received: {pingItem.Statistics.Received} Lost: {pingItem.Statistics.Lost}";
        }
    }
}