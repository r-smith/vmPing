using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using vmPing.Configuration;
using vmPing.Properties;

namespace vmPing.Probes
{
    public partial class Probe
    {
        private async void PerformTcpProbe(CancellationToken cancellationToken)
        {
            string host = Hostname.Substring(0, Hostname.LastIndexOf(':'));
            host = host.Trim(new[] { '[', ']' });
            string port = Hostname.Substring(Hostname.LastIndexOf(':') + 1);
            int portnumber;
            bool isPortOpen = false;

            // Check if port is invalid.
            if (!(int.TryParse(port, out portnumber) && portnumber >= 1 && portnumber <= 65535))
            {
                // Error.
                await Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => AddHistory("Invalid port number.")));

                StopProbe(ProbeStatus.Error);
                return;
            }

            await Application.Current.Dispatcher.BeginInvoke(
                new Action(() => AddHistory($"*** Pinging {host} on port {portnumber}:")));

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
                await Application.Current.Dispatcher.BeginInvoke(
                            new Action(() => AddHistory("Error: " + ex.Message)));
                StopProbe(ProbeStatus.Error);
                return;
            }

            int errorCode = 0;
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Restart();

                using (TcpClient client = new TcpClient(ipVersion))
                {
                    Statistics.Sent++;

                    try
                    {
                        IAsyncResult result = client.BeginConnect(host, portnumber, null, null);
                        bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                        if (!success)
                        {
                            throw new SocketException();
                        }

                        client.EndConnect(result);

                        // Check if this task was cancelled.
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        // Connected to port.
                        stopwatch.Stop();
                        isPortOpen = true;
                        HandleReply(stopwatch.ElapsedMilliseconds);
                    }
                    catch (SocketException ex)
                    {
                        const int WSAHOST_NOT_FOUND = 11001;

                        stopwatch.Stop();

                        // Check if this task was cancelled.
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        // If hostname cannot be resolved, report error and stop.
                        if (ex.ErrorCode == WSAHOST_NOT_FOUND)
                        {
                            await Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => AddHistory("Unable to resolve hostname.")));

                            StopProbe(ProbeStatus.Error);
                            return;
                        }

                        HandleNoReply();
                        isPortOpen = false;
                        errorCode = ex.ErrorCode;
                    }
                    catch (Exception ex)
                    {
                        // Unexpected error ocurred. Report error and stop probe.
                        await Application.Current.Dispatcher.BeginInvoke(
                            new Action(() => AddHistory("Error: " + ex.Message)));
                        StopProbe(ProbeStatus.Error);
                        return;
                    }
                    client.Close();
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    // Update output.
                    DisplayTcpReply(isPortOpen, portnumber, errorCode, stopwatch.ElapsedMilliseconds);

                    // Pause between probes.
                    await Task.Delay(ApplicationOptions.PingInterval);
                }
            }
        }

        private void DisplayTcpReply(bool isPortOpen, int portnumber, int errorCode, long elapsedTime)
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
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => AddHistory(sb.ToString())));

            // If enabled, log output.
            WriteToLog(sb.ToString());
        }
    }
}