using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Models;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class TracerouteWindow : Window
    {
        private const int MaxHops = 30;
        private const int PingTimeout = 2000;

        private readonly Traceroute _traceroute = new Traceroute();
        private CancellationTokenSource _cancelSource;

        public TracerouteWindow()
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);
            Util.CenterOverMainWindow(this);
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;

            DataContext = _traceroute;
            TraceData.ItemsSource = _traceroute.Hops;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopTrace();
        }

        private async void Trace_Click(object sender, RoutedEventArgs e)
        {
            if (_traceroute.IsActive)
            {
                StopTrace();
                TraceStatus.Text = "\u2022 Trace cancelled";
                Hostname.Focus();
                return;
            }

            if (Hostname.Text.Length == 0)
            {
                return;
            }

            // Reset width for IP address column.
            TraceData.Columns[1].Width = new DataGridLength(100.0);
            TraceData.Columns[1].Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto);

            TraceStatus.Text = "Tracing route...";
            TraceStatus.Visibility = Visibility.Visible;
            _traceroute.Hops.Clear();
            _traceroute.IsActive = true;

            _cancelSource = new CancellationTokenSource();
            await TraceAsync(Hostname.Text, _cancelSource.Token);
        }

        private void StopTrace()
        {
            _cancelSource?.Cancel();
            _traceroute.IsActive = false;
        }

        // Once the token is cancelled, the trace stops without touching the window again.
        private async Task TraceAsync(string host, CancellationToken token)
        {
            try
            {
                if (!IPAddress.TryParse(host, out IPAddress address))
                {
                    try
                    {
                        address = (await Dns.GetHostEntryAsync(host)).AddressList[0];
                    }
                    catch
                    {
                        if (!token.IsCancellationRequested)
                        {
                            TraceStatus.Text = "\u2022 Invalid hostname";
                        }
                        return;
                    }
                }

                byte[] buffer = ApplicationOptions.CreatePingData(Constants.DefaultPacketSize);
                Stopwatch stopwatch = new Stopwatch();

                using (Ping ping = new Ping())
                {
                    for (int ttl = 1; ttl <= MaxHops; ttl++)
                    {
                        PingOptions options = new PingOptions(ttl, true);

                        stopwatch.Restart();
                        PingReply reply = await ping.SendPingAsync(address, PingTimeout, buffer, options);
                        if (reply.Status == IPStatus.TimedOut)
                        {
                            // Try a hop a second time before reporting it as timed out.
                            stopwatch.Restart();
                            reply = await ping.SendPingAsync(address, PingTimeout, buffer, options);
                        }
                        stopwatch.Stop();

                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        AddHop(ttl, reply, stopwatch.ElapsedMilliseconds);

                        if (reply.Status == IPStatus.Success)
                        {
                            TraceStatus.Text = "\u2605 Trace complete";
                            break;
                        }

                        await Task.Delay(100);
                    }
                }
            }
            catch
            {
                // The ping could not be sent. End the trace with the hops found so far.
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    _traceroute.IsActive = false;
                    Hostname.Focus();
                }
            }
        }

        private void AddHop(int hopId, PingReply reply, long roundTripTime)
        {
            TracerouteHop hop = new TracerouteHop
            {
                HopId = hopId,
                HostAddress = reply.Status == IPStatus.TimedOut ? "Timed out" : reply.Address?.ToString(),
                RoundTripTime = roundTripTime
            };

            _traceroute.Hops.Add(hop);
            TraceData.ScrollIntoView(hop);
        }
    }
}
