using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// TracerouteWindow is a tool for performing a network traceroute to a given host.
    /// </summary>
    public partial class TracerouteWindow : Window
    {
        internal NetworkRoute Route { get; set; } = new NetworkRoute();

        public TracerouteWindow()
        {
            InitializeComponent();

            DataContext = Route;
            TraceData.ItemsSource = Route.networkRoute;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Trace_Click(object sender, RoutedEventArgs e)
        {
            if (!Route.IsActive)
            {
                if (Hostname.Text.Length == 0)
                    return;

                if (Route.BgWorker != null)
                    Route.BgWorker.CancelAsync();

                TraceStatus.Text = "Tracing Route...";
                Route.DestinationHost = Hostname.Text;
                Route.MaxHops = 30;
                Route.PingTimeout = 2000;
                Route.networkRoute.Clear();
                Route.IsActive = true;

                Route.BgWorker = new BackgroundWorker();
                Route.ResetEvent = new AutoResetEvent(false);
                Route.BgWorker.DoWork += new DoWorkEventHandler(BackgroundThread_TraceRoute);
                Route.BgWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundThread_ProgressChanged);
                Route.BgWorker.WorkerSupportsCancellation = true;
                Route.BgWorker.WorkerReportsProgress = true;
                Route.BgWorker.RunWorkerAsync();
            }
            else
            {
                Route.BgWorker.CancelAsync();
                Route.ResetEvent.WaitOne();
                Route.IsActive = false;
                TraceStatus.Text = "Trace Cancelled";
            }
        }

        public void BackgroundThread_TraceRoute(object sender, DoWorkEventArgs e)
        {
            var bgWorker = sender as BackgroundWorker;

            var pingBuffer = Encoding.ASCII.GetBytes(Constants.DefaultIcmpData);
            var pingOptions = new PingOptions(1, true);
            PingReply pingReply;
            var timer = new Stopwatch();
            Route.Timer = timer;

            while (!bgWorker.CancellationPending && Route.IsActive && pingOptions.Ttl <= Route.MaxHops)
            {
                if (IPAddress.TryParse(Route.DestinationHost, out IPAddress ipAddress))
                {
                    Route.DestinationIp = ipAddress;
                }
                else
                {
                    try { Route.DestinationIp = Dns.GetHostEntry(Route.DestinationHost).AddressList[0]; }
                    catch { bgWorker.ReportProgress(-1); break; }
                }

                using (Ping ping = new Ping())
                {
                    try
                    {
                        Route.Timer.Reset();
                        Route.Timer.Start();
                        pingReply = ping.Send(Route.DestinationIp, Route.PingTimeout, pingBuffer, pingOptions);
                        if (pingReply.Status == IPStatus.TimedOut)
                        {
                            Route.Timer.Reset();
                            Route.Timer.Start();
                            pingReply = ping.Send(Route.DestinationIp, Route.PingTimeout, pingBuffer, pingOptions);
                        }

                        if (!bgWorker.CancellationPending)
                            bgWorker.ReportProgress(pingOptions.Ttl, pingReply);

                        if (pingReply.Status == IPStatus.Success)
                            break;

                        Route.ResetEvent.Set();
                        Thread.Sleep(100);
                        ++pingOptions.Ttl;
                    }

                    catch { break; }
                }
            }

            e.Cancel = true;
            Route.ResetEvent.Set();
            Route.IsActive = false;
        }

        private void BackgroundThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Route.Timer.Stop();

            if (e.ProgressPercentage < 0)
            {
                TraceStatus.Text = "Invalid Hostname";
                return;
            }

            var pingReply = e.UserState as PingReply;
            var node = new NetworkRouteNode();

            if (pingReply.Address != null)
                node.HostAddress = pingReply.Address.ToString();
            node.ReplyStatus = pingReply.Status;
            node.HopId = e.ProgressPercentage;
            node.RoundTripTime = Route.Timer.ElapsedMilliseconds;

            if (node.ReplyStatus == IPStatus.TimedOut)
                node.HostAddress = "Timed Out";

            if (node.ReplyStatus == IPStatus.Success)
                TraceStatus.Text = "Trace Complete";

            Route.networkRoute.Add(node);
            TraceData.ScrollIntoView(TraceData.Items[Route.networkRoute.Count - 1]);
        }
    }
}
