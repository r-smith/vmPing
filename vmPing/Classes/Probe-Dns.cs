using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;

namespace vmPing.Classes
{
    public partial class Probe
    {
        private async void PerformDnsLookup(CancellationToken cancellationToken)
        {
            IsActive = true;
            History = new ObservableCollection<string>();
            Status = ProbeStatus.Scanner;

            try
            {
                AddHistory($"[\u2022] Resolving {Hostname}:{Environment.NewLine}");
                switch (Uri.CheckHostName(Hostname))
                {
                    case UriHostNameType.IPv4:
                    case UriHostNameType.IPv6:
                        var host = await Dns.GetHostEntryAsync(Hostname);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (host != null)
                            AddHistory($"    {host.HostName}");
                        break;
                    case UriHostNameType.Dns:
                        var ipAddresses = await Dns.GetHostAddressesAsync(Hostname);
                        cancellationToken.ThrowIfCancellationRequested();
                        foreach (var ip in ipAddresses)
                            AddHistory($"    {ip}");
                        break;
                    default:
                        throw new Exception();
                }
                AddHistory($"{Environment.NewLine}{Environment.NewLine}\u2605 Done");
            }
            catch
            {
                if (!cancellationToken.IsCancellationRequested)
                    AddHistory($"{Environment.NewLine}\u2605 Unable to resolve hostname");
            }
            finally
            {
                IsActive = false;
            }
        }
    }
}