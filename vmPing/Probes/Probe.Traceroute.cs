using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using vmPing.Configuration;

namespace vmPing.Probes
{
    public partial class Probe
    {
        private async Task PerformTraceroute(CancellationToken cancellationToken)
        {
            IsActive = true;
            ClearHistory();
            Status = ProbeStatus.Scanner;

            AddHistory($"[\u2022] Tracing route to {Hostname}:");
            if (await IsHostInvalid(Hostname, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                StopProbe(ProbeStatus.Error);
                return;
            }
            AddHistory("");

            using (Ping ping = new Ping())
            {
                const int MaxHops = 30;
                const int Timeout = 2000;
                const string HopFormat = "{0,2}   {1,-15}   [{2} ms]";
                const string HopErrorFormat = "{0,2}   {1}";
                int ttl = 1;
                Stopwatch timer = new Stopwatch();

                while (!cancellationToken.IsCancellationRequested && ttl <= MaxHops)
                {
                    try
                    {
                        timer.Restart();
                        PingReply reply = await ping.SendPingAsync(
                            hostNameOrAddress: Hostname,
                            timeout: Timeout,
                            buffer: ApplicationOptions.CreatePingData(Constants.DefaultPacketSize),
                            options: new PingOptions() { Ttl = ttl });
                        timer.Stop();

                        string address = string.Empty;
                        if (reply.Address != null)
                        {
                            address = reply.Address.ToString();
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (reply.Status != IPStatus.TtlExpired && reply.Status != IPStatus.Success)
                        {
                            AddHistory(string.Format(HopErrorFormat, ttl, reply.Status));
                        }
                        else
                        {
                            AddHistory(string.Format(HopFormat, ttl, address, timer.ElapsedMilliseconds < 1 ? "<1" : timer.ElapsedMilliseconds.ToString()));
                        }

                        if (reply.Status == IPStatus.Success)
                        {
                            AddHistory($"{Environment.NewLine}\u2605 Trace complete");
                            break;
                        }

                        ttl++;
                        if (ttl > MaxHops)
                        {
                            AddHistory($"{Environment.NewLine}\u2605 Trace complete");
                        }

                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        AddHistory(ex.Message);
                        break;
                    }
                }

                IsActive = false;
            }

        }
    }
}