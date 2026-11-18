using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace vmPing.Probes
{
    public partial class Probe
    {
        private async Task PerformDnsLookup(CancellationToken cancellationToken)
        {
            IsActive = true;
            ClearHistory();
            Status = ProbeStatus.Tool;

            try
            {
                AddHistory($"[\u2022] Resolving {Hostname}:{Environment.NewLine}");
                switch (Uri.CheckHostName(Hostname))
                {
                    case UriHostNameType.IPv4:
                    case UriHostNameType.IPv6:
                        IPHostEntry host = await Dns.GetHostEntryAsync(Hostname);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (host != null)
                        {
                            AddHistory($"    {host.HostName}");
                        }

                        break;
                    case UriHostNameType.Dns:
                        IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(Hostname);
                        cancellationToken.ThrowIfCancellationRequested();
                        foreach (IPAddress ip in ipAddresses)
                        {
                            AddHistory($"    {ip}");
                        }

                        break;
                    default:
                        throw new Exception();
                }
                AddHistory($"{Environment.NewLine}{Environment.NewLine}\u2605 Done");
            }
            catch
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    AddHistory($"{Environment.NewLine}\u2605 Unable to resolve hostname");
                }
            }
            finally
            {
                IsActive = false;
            }
        }
    }
}
