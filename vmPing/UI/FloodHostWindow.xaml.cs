using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.UI
{
    public partial class FloodHostWindow : Window
    {
        readonly FloodHostNode _floodHost = new FloodHostNode();

        public FloodHostWindow()
        {
            InitializeComponent();
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;

            DataContext = _floodHost;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void btnFloodHost_Click(object sender, RoutedEventArgs e)
        {
            if (!_floodHost.IsActive && MessageBox.Show(
                $"Run the ICMP test against {txtHostname.Text} for up to 60 seconds? Only test systems you own or are authorized to monitor.",
                "Confirm ICMP test",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            lblInformation.Visibility = Visibility.Collapsed;
            FloodHost(_floodHost);
        }


        public void FloodHost(FloodHostNode node)
        {
            if (!node.IsActive)
            {
                if (txtHostname.Text.Length == 0)
                {
                    return;
                }

                node.BgWorker?.CancelAsync();

                node.DestinationAddress = txtHostname.Text;
                node.PacketsSent = 0;
                node.PacketsReceived = 0;
                node.PacketsLost = 0;
                node.StartTime = DateTime.Now;
                node.IsActive = true;

                node.BgWorker = new BackgroundWorker();
                node.ResetEvent = new AutoResetEvent(false);
                node.BgWorker.DoWork += new DoWorkEventHandler(backgroundThread_FloodHost);
                node.BgWorker.WorkerSupportsCancellation = true;
                node.BgWorker.RunWorkerAsync(node);
            }
            else
            {
                node.ResetEvent.Reset();
                node.BgWorker.CancelAsync();
                node.ResetEvent.WaitOne();
                node.IsActive = false;
            }
        }


        public void backgroundThread_FloodHost(object sender, DoWorkEventArgs e)
        {
            var bgWorker = sender as BackgroundWorker;
            var node = e.Argument as FloodHostNode;

            var pingBuffer = Encoding.ASCII.GetBytes(Constants.DefaultIcmpData);
            var pingOptions = new PingOptions(Constants.DefaultTTL, true);
            var elapsed = Stopwatch.StartNew();

            while (!bgWorker.CancellationPending && node.IsActive && elapsed.Elapsed < TimeSpan.FromSeconds(60))
            {
                using (Ping ping = new Ping())
                {
                    try
                    {
                        ++node.PacketsSent;
                        if (ping.Send(node.DestinationAddress, 100, pingBuffer, pingOptions).Status == IPStatus.Success)
                        {
                            ++node.PacketsReceived;
                        }
                        else
                        {
                            ++node.PacketsLost;
                        }
                    }
                    catch
                    {
                        e.Cancel = true;
                        node.IsActive = false;
                        break;
                    }
                }

                if (!bgWorker.CancellationPending && elapsed.Elapsed < TimeSpan.FromSeconds(60))
                {
                    Thread.Sleep(900);
                }
            }

            node.IsActive = false;
            node.ResetEvent.Set();
        }
    }
}
