using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Models;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class FloodHostWindow : Window
    {
        private readonly FloodHost _floodHost = new FloodHost();
        private CancellationTokenSource _cancelSource;

        public FloodHostWindow()
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);
            Util.CenterOverMainWindow(this);
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;

            DataContext = _floodHost;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopFlood();
        }

        private void FloodButton_Click(object sender, RoutedEventArgs e)
        {
            Information.Visibility = Visibility.Collapsed;

            if (_floodHost.IsActive)
            {
                StopFlood();
            }
            else
            {
                StartFlood();
            }
        }

        private void StartFlood()
        {
            if (Hostname.Text.Length == 0)
            {
                return;
            }

            _floodHost.PacketsSent = 0;
            _floodHost.PacketsReceived = 0;
            _floodHost.PacketsLost = 0;
            _floodHost.IsActive = true;

            _cancelSource = new CancellationTokenSource();
            CancellationToken token = _cancelSource.Token;
            string address = Hostname.Text;
            Task.Run(() => SendPings(address, token));
        }

        private void StopFlood()
        {
            _cancelSource?.Cancel();
            _floodHost.IsActive = false;
        }

        // Runs on a background thread until the token is cancelled.
        private void SendPings(string address, CancellationToken token)
        {
            byte[] buffer = ApplicationOptions.CreatePingData(Constants.DefaultPacketSize);
            PingOptions options = new PingOptions(Constants.DefaultTTL, true);

            try
            {
                using (Ping ping = new Ping())
                {
                    while (!token.IsCancellationRequested)
                    {
                        PingReply reply = ping.Send(address, 100, buffer, options);

                        // Don't count a reply that arrives after the flood was stopped.
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        _floodHost.PacketsSent++;
                        if (reply.Status == IPStatus.Success)
                        {
                            _floodHost.PacketsReceived++;
                        }
                        else
                        {
                            _floodHost.PacketsLost++;
                        }
                    }
                }
            }
            catch
            {
                // The host could not be resolved or the ping could not be sent.
                if (!token.IsCancellationRequested)
                {
                    _floodHost.IsActive = false;
                }
            }
        }
    }
}
