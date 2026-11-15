using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vmPing.Configuration;
using vmPing.Properties;

namespace vmPing.Probes
{
    public partial class Probe
    {
        private async Task PerformTcpProbe(CancellationToken cancellationToken)
        {
            string host = Hostname.Substring(0, Hostname.LastIndexOf(':'));
            host = host.Trim(new[] { '[', ']' });
            string port = Hostname.Substring(Hostname.LastIndexOf(':') + 1);
            int portnumber;

            // Check if port is invalid.
            if (!(int.TryParse(port, out portnumber) && portnumber >= 1 && portnumber <= 65535))
            {
                // Error.
                AddHistory("Invalid port number.");

                StopProbe(ProbeStatus.Error);
                return;
            }

            AddHistory($"*** Pinging {GetDisplayName(host)} on port {portnumber}:");

            // Check whether a hostname or an IP address was provided.  If hostname, resolve and print IP.
            if (await IsHostInvalid(host, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                StopProbe(ProbeStatus.Error);
                return;
            }

            // Determine whether target is IPv4 or IPv6. Necessary for TcpClient class.
            AddressFamily ipVersion = AddressFamily.InterNetwork;
            try
            {
                switch (Uri.CheckHostName(host))
                {
                    case UriHostNameType.IPv4:
                        ipVersion = AddressFamily.InterNetwork;
                        break;
                    case UriHostNameType.IPv6:
                        ipVersion = AddressFamily.InterNetworkV6;
                        break;
                    case UriHostNameType.Dns:
                        IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(host);
                        cancellationToken.ThrowIfCancellationRequested();
                        host = ipAddresses[0].ToString();
                        ipVersion = ipAddresses[0].AddressFamily;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                AddHistory("Error: " + ex.Message);
                StopProbe(ProbeStatus.Error);
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            while (!cancellationToken.IsCancellationRequested)
            {
                bool isPortOpen;
                Statistics.Sent++;
                stopwatch.Restart();

                try
                {
                    using (TcpClient client = new TcpClient(ipVersion))
                    {
                        isPortOpen = await TryConnectAsync(client, host, portnumber);
                    }
                }
                catch (Exception ex)
                {
                    // Unexpected error ocurred. Report error and stop probe.
                    AddHistory("Error: " + ex.Message);
                    StopProbe(ProbeStatus.Error);
                    return;
                }

                stopwatch.Stop();

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (isPortOpen)
                {
                    HandleReply(stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    HandleNoReply();
                }

                // Update output.
                DisplayTcpReply(isPortOpen, portnumber, stopwatch.ElapsedMilliseconds);

                // Pause between probes.
                if (!await Delay(ApplicationOptions.PingInterval, cancellationToken))
                {
                    return;
                }
            }
        }

        // Returns true if the port accepted a connection within the ping timeout.
        private static async Task<bool> TryConnectAsync(TcpClient client, string host, int port)
        {
            Task connect = client.ConnectAsync(host, port);
            Task timeout = Task.Delay(ApplicationOptions.PingTimeout);

            if (await Task.WhenAny(connect, timeout) != connect)
            {
                return false;
            }

            try
            {
                await connect;
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        private void DisplayTcpReply(bool isPortOpen, int portnumber, long elapsedTime)
        {
            // Prefix the ping reply output with a timestamp.
            StringBuilder sb = new StringBuilder($"[{DateTime.Now.ToLongTimeString()}]  {Strings.Probe_Port} {portnumber}: ");
            if (isPortOpen)
            {
                sb.Append($"{Strings.Probe_PortOpen}  [{elapsedTime}{Strings.Milliseconds_Symbol}]");
            }
            else
            {
                sb.Append(Strings.Probe_PortClosed);
            }

            // Add response to the output window.
            AddHistory(sb.ToString());

            // If enabled, log output.
            WriteToLog(sb.ToString());
        }
    }
}