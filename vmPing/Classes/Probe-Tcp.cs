using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using vmPing.Properties;

namespace vmPing.Classes
{
    public partial class Probe
    {
        private async void PerformTcpProbe(CancellationToken cancellationToken)
        {
            InitializeProbe();

            var hostAndPort = Hostname.Split(':');
            string host = hostAndPort[0];
            int portnumber;
            bool isPortOpen = false;

            // Check if port is invalid.
            if (!(int.TryParse(hostAndPort[1], out portnumber) && portnumber >= 1 && portnumber <= 65535))
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
                    return;
                StopProbe(ProbeStatus.Error);
                return;
            }

            int errorCode = 0;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Restart();

                using (var client = new TcpClient())
                {
                    Statistics.Sent++;
                    DisplayStatistics();

                    try
                    {
                        var result = client.BeginConnect(host, portnumber, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                        if (!success)
                        {
                            throw new SocketException();
                        }

                        client.EndConnect(result);

                        // Check if this task was cancelled.
                        if (cancellationToken.IsCancellationRequested) return;

                        // Check for status change.
                        if (Status == ProbeStatus.Down)
                        {
                            TriggerStatusChange(new StatusChangeLog { Timestamp = DateTime.Now, Hostname = Hostname, Status = ProbeStatus.Up });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                Util.SendEmail("up", Hostname);
                        }

                        Statistics.Received++;
                        IndeterminateCount = 0;
                        Status = ProbeStatus.Up;
                        isPortOpen = true;
                    }
                    catch (SocketException ex)
                    {
                        const int WSAHOST_NOT_FOUND = 11001;

                        stopwatch.Stop();

                        // Check if this task was cancelled.
                        if (cancellationToken.IsCancellationRequested) return;

                        if (Status == ProbeStatus.Up)
                            Status = ProbeStatus.Indeterminate;
                        if (Status == ProbeStatus.Inactive)
                            Status = ProbeStatus.Down;
                        IndeterminateCount++;

                        // Check for status change.
                        if (Status == ProbeStatus.Indeterminate && IndeterminateCount >= ApplicationOptions.AlertThreshold)
                        {
                            Status = ProbeStatus.Down;
                            TriggerStatusChange(new StatusChangeLog { Timestamp = DateTime.Now, Hostname = Hostname, Status = ProbeStatus.Down });
                            if (ApplicationOptions.IsEmailAlertEnabled)
                                Util.SendEmail("down", Hostname);
                        }

                        // If hostname cannot be resolved, report error and stop.
                        if (ex.ErrorCode == WSAHOST_NOT_FOUND)
                        {
                            await Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => AddHistory("Unable to resolve hostname.")));

                            StopProbe(ProbeStatus.Error);
                            return;
                        }

                        Statistics.Lost++;
                        isPortOpen = false;
                        errorCode = ex.ErrorCode;
                    }
                    client.Close();
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    // Update output.
                    DisplayTcpReply(isPortOpen, portnumber, errorCode, stopwatch.ElapsedMilliseconds);
                    DisplayStatistics();

                    // Pause between probes.
                    await Task.Delay(ApplicationOptions.PingInterval);
                }
            }
        }


        private void DisplayTcpReply(bool isPortOpen, int portnumber, int errorCode, long elapsedTime)
        {
            // Prefix the ping reply output with a timestamp.
            var pingOutput = new StringBuilder($"[{DateTime.Now.ToLongTimeString()}]  {Strings.Probe_Port} {portnumber.ToString()}: ");
            if (isPortOpen)
                pingOutput.Append($"{Strings.Probe_PortOpen}  [{elapsedTime.ToString()}{Strings.Milliseconds_Symbol}]");
            else
            {
                pingOutput.Append(Strings.Probe_PortClosed);
            }

            // Add response to the output window.
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => AddHistory(pingOutput.ToString())));

            // If enabled, log output.
            WriteToLog(pingOutput.ToString());
        }
    }
}