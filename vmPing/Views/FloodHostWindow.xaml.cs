using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// FloodHostWindow is a tool for generating a high volume of traffic to a specific host.
    /// </summary>
    public partial class FloodHostWindow : Window
    {
        FloodHostNode _floodHost = new FloodHostNode();


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
            lblInformation.Visibility = Visibility.Collapsed;
            FloodHost(_floodHost);
        }


        public void FloodHost(FloodHostNode node)
        {
            if (!node.IsActive)
            {
                if (txtHostname.Text.Length == 0)
                    return;

                if (node.BgWorker != null)
                    node.BgWorker.CancelAsync();

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

            while (!bgWorker.CancellationPending && node.IsActive)
            {
                using (Ping ping = new Ping())
                {
                    try
                    {
                        ++node.PacketsSent;
                        if (ping.Send(node.DestinationAddress, 100, pingBuffer, pingOptions).Status == IPStatus.Success)
                            ++node.PacketsReceived;
                        else
                            ++node.PacketsLost;

                        node.ResetEvent.Set();
                    }
                    catch
                    {
                        e.Cancel = true;
                        node.ResetEvent.Set();
                        node.IsActive = false;
                        return;
                    }
                }
            }

            node.ResetEvent.Set();
        }
    }
}
