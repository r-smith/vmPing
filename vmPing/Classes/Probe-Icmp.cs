using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace vmPing.Classes
{
    public partial class Probe
    {
        private async void PerformIcmpProbe(CancellationToken cancellationToken)
        {
            InitializeProbe();
            await Application.Current.Dispatcher.BeginInvoke(
                new Action(() => AddHistory($"*** Pinging {Hostname}:")));

            if (await IsHostInvalid(Hostname, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                StopProbe(ProbeStatus.Error);
                return;
            }

            using (var ping = new Ping())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Send ping.
                        Statistics.Sent++;
                        PingReply reply = await ping.SendPingAsync(
                            hostNameOrAddress: Hostname,
                            timeout: ApplicationOptions.PingTimeout,
                            buffer: ApplicationOptions.Buffer,
                            options: ApplicationOptions.GetPingOptions);
                        if (cancellationToken.IsCancellationRequested) return;

                        if (reply.Status == IPStatus.Success)
                        {
                            Statistics.Received++;
                            IndeterminateCount = 0;

                            // Check for status change.
                            if (Status == ProbeStatus.Down)
                            {
                                TriggerStatusChange(new StatusChangeLog { Timestamp = DateTime.Now, Hostname = Hostname, Alias = Alias, Status = ProbeStatus.Up });
                                if (ApplicationOptions.IsEmailAlertEnabled)
                                    Util.SendEmail("up", Hostname, Alias);
                            }

                            Status = ProbeStatus.Up;
                        }
                        else
                        {
                            IndeterminateCount++;
                            if (Status == ProbeStatus.Up) Status = ProbeStatus.Indeterminate;
                            if (Status == ProbeStatus.Inactive) Status = ProbeStatus.Down;

                            // Check for status change.
                            if (Status == ProbeStatus.Indeterminate && IndeterminateCount >= ApplicationOptions.AlertThreshold)
                            {
                                Status = ProbeStatus.Down;
                                TriggerStatusChange(new StatusChangeLog { Timestamp = DateTime.Now, Hostname = Hostname, Alias = Alias, Status = ProbeStatus.Down });
                                if (ApplicationOptions.IsEmailAlertEnabled)
                                    Util.SendEmail("down", Hostname, Alias);
                            }

                            switch (reply.Status)
                            {
                                case IPStatus.TimedOut:
                                case IPStatus.DestinationHostUnreachable:
                                case IPStatus.DestinationNetworkUnreachable:
                                case IPStatus.DestinationPortUnreachable:
                                case IPStatus.DestinationUnreachable:
                                    Statistics.Lost++;
                                    break;
                                default:
                                    Statistics.Error++;
                                    break;
                            }
                        }

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            // Update output.
                            DisplayStatistics();
                            DisplayIcmpReply(reply);

                            // Pause between probes.
                            await IcmpWait(reply.Status);
                        }
                    }

                    catch (Exception ex)
                    {
                        if (ex.InnerException is SocketException)
                            await Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => AddHistory("Unable to resolve hostname.")));
                        else
                            await Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => AddHistory($"Error: {ex.Message}")));

                        // Check for status change.
                        if (Status == ProbeStatus.Up || Status == ProbeStatus.Down || Status == ProbeStatus.Indeterminate)
                        {
                            TriggerStatusChange(new StatusChangeLog { Timestamp = DateTime.Now, Hostname = Hostname, Alias = Alias, Status = ProbeStatus.Error });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                Util.SendEmail("error", Hostname, Alias);
                        }

                        StopProbe(ProbeStatus.Error);
                        return;
                    }
                }
            }
        }


        private async Task IcmpWait(IPStatus ipStatus)
        {
            if (ipStatus == IPStatus.TimedOut)
            {
                // Ping timed out.  If the ping interval is greater than the timeout,
                // then sleep for [INTERVAL - TIMEOUT]
                // Otherwise, sleep for a fixed amount of 1 second
                if (ApplicationOptions.PingInterval > ApplicationOptions.PingTimeout)
                    await Task.Delay(ApplicationOptions.PingInterval - ApplicationOptions.PingTimeout);
                else
                    await Task.Delay(1000);
            }
            else
                // For any other type of ping response, sleep for the global ping interval amount
                // before sending another ping.
                await Task.Delay(ApplicationOptions.PingInterval);
        }


        private void DisplayIcmpReply(PingReply pingReply)
        {
            if (pingReply == null) return;

            // Build output string based on the ping reply details.
            var pingOutput = new StringBuilder($"[{DateTime.Now.ToLongTimeString()}]  ");
            switch (pingReply.Status)
            {
                case IPStatus.Success:
                    pingOutput.Append("Reply from ");
                    pingOutput.Append(pingReply.Address.ToString());
                    if (pingReply.RoundtripTime < 1)
                        pingOutput.Append("  [<1ms]");
                    else
                        pingOutput.Append($"  [{pingReply.RoundtripTime} ms]");
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
                    pingOutput.Append(pingReply.Status.ToString());
                    break;
            }
            // Add response to the output window..
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => AddHistory(pingOutput.ToString())));

            // If enabled, log output.
            WriteToLog(pingOutput.ToString());
        }
    }
}