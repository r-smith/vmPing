using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vmPing.Configuration;

namespace vmPing.Probes
{
    public partial class Probe
    {
        private async Task PerformIcmpProbe(CancellationToken cancellationToken)
        {
            AddHistory($"*** Pinging {Hostname}:");

            if (await IsHostInvalid(Hostname, cancellationToken))
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    StopProbe(ProbeStatus.Error);
                }
                return;
            }

            using (Ping ping = new Ping())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    bool didPingFail = false;

                    try
                    {
                        // Send ping.
                        Statistics.Sent++;
                        PingReply reply = await ping.SendPingAsync(
                            hostNameOrAddress: Hostname,
                            timeout: ApplicationOptions.PingTimeout,
                            buffer: ApplicationOptions.Buffer,
                            options: ApplicationOptions.PingOptions);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (reply.Status == IPStatus.Success)
                        {
                            HandleReply(reply.RoundtripTime);
                        }
                        else
                        {
                            HandleNoReply();
                        }

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            // Update output.
                            DisplayIcmpReply(reply);

                            // Pause between probes.
                            await IcmpWait(reply.Status, cancellationToken);
                        }
                    }

                    catch (Exception ex)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        Statistics.Lost++;

                        // Check for status change.
                        if (Status == ProbeStatus.Inactive)
                        {
                            Status = ProbeStatus.Down;
                        }

                        if (Status != ProbeStatus.Down)
                        {
                            Status = ProbeStatus.Down;
                            OnStatusChange(ProbeStatus.Down, "error");
                        }

                        // Update output.
                        DisplayIcmpReply(null, ex);

                        // Pause between probes.
                        didPingFail = true;
                    }

                    if (didPingFail && !await Delay(ApplicationOptions.PingInterval, cancellationToken))
                    {
                        break;
                    }
                }
            }
        }

        private async Task IcmpWait(IPStatus ipStatus, CancellationToken cancellationToken)
        {
            if (ipStatus == IPStatus.TimedOut)
            {
                // Ping timed out. If the ping interval is greater than the timeout,
                // then sleep for [INTERVAL - TIMEOUT]
                // Otherwise, sleep for a fixed amount of 1 second
                if (ApplicationOptions.PingInterval > ApplicationOptions.PingTimeout)
                {
                    await Delay(ApplicationOptions.PingInterval - ApplicationOptions.PingTimeout, cancellationToken);
                }
                else
                {
                    await Delay(1000, cancellationToken);
                }
            }
            else
            {
                // For any other type of ping response, sleep for the global ping interval amount
                // before sending another ping.
                await Delay(ApplicationOptions.PingInterval, cancellationToken);
            }
        }

        private void DisplayIcmpReply(PingReply pingReply, Exception ex = null)
        {
            if (pingReply == null && ex == null)
            {
                return;
            }

            // Build output string based on the ping reply details.
            StringBuilder sb = new StringBuilder($"[{DateTime.Now.ToLongTimeString()}]  ");

            if (pingReply != null)
            {
                switch (pingReply.Status)
                {
                    case IPStatus.Success:
                        sb.Append("Reply from ");
                        sb.Append(pingReply.Address.ToString());
                        sb.Append(pingReply.RoundtripTime < 1
                            ? "  [<1ms]"
                            : $"  [{pingReply.RoundtripTime} ms]");
                        break;
                    case IPStatus.DestinationHostUnreachable:
                        sb.Append("Reply  [Host unreachable]");
                        break;
                    case IPStatus.DestinationNetworkUnreachable:
                        sb.Append("Reply  [Network unreachable]");
                        break;
                    case IPStatus.DestinationUnreachable:
                        sb.Append("Reply  [Unreachable]");
                        break;
                    case IPStatus.TimedOut:
                        sb.Append("Request timed out.");
                        break;
                    default:
                        sb.Append(pingReply.Status.ToString());
                        break;
                }
            }
            else
            {
                sb.Append(ex.InnerException is SocketException
                    ? "Unable to resolve hostname."
                    : ex.Message);
            }

            string output = sb.ToString();

            // Add response to the output window..
            AddHistory(output);

            // If enabled, log output.
            WriteToLog(output);
        }
    }
}