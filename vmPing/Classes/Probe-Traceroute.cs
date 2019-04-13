using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vmPing.Classes
{
    public partial class Probe
    {
        private async void PerformTraceroute(CancellationToken cancellationToken)
        {
            IsActive = true;
            History = new ObservableCollection<string>();
            Status = ProbeStatus.Scanner;

            AddHistory($"[\u2022] Tracing route to {Hostname}:");
            if (await IsHostInvalid(Hostname, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                StopProbe(ProbeStatus.Error);
                return;
            }
            AddHistory("");

            using (var ping = new Ping())
            {
                const int MaxHops = 30;
                const int Timeout = 2000;
                const string stringFormat = "{0,2}   {1,-15}   [{2} ms]";
                const string stringErrorFormat = "{0,2}   {1}";
                var ttl = 1;
                var timer = new Stopwatch();

                while (!cancellationToken.IsCancellationRequested && ttl <= MaxHops)
                {
                    try
                    {
                        timer.Restart();
                        PingReply reply = await ping.SendPingAsync(
                            hostNameOrAddress: Hostname,
                            timeout: Timeout,
                            buffer: Encoding.ASCII.GetBytes(Constants.DefaultIcmpData),
                            options: new PingOptions() { Ttl = ttl });
                        timer.Stop();

                        var address = string.Empty;
                        if (reply.Address != null)
                            address = reply.Address.ToString();

                        if (cancellationToken.IsCancellationRequested)
                            break;

                        if (reply.Status != IPStatus.TtlExpired && reply.Status != IPStatus.Success)
                            AddHistory(string.Format(stringErrorFormat, ttl, reply.Status));
                        else
                            AddHistory(string.Format(stringFormat, ttl, address, timer.ElapsedMilliseconds < 1 ? "<1" : timer.ElapsedMilliseconds.ToString()));

                        if (reply.Status == IPStatus.Success)
                        {
                            AddHistory($"{Environment.NewLine}\u2605 Trace complete");
                            break;
                        }

                        ttl++;
                        if (ttl > MaxHops) AddHistory($"{Environment.NewLine}\u2605 Trace complete");
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