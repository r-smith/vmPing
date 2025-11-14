using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using vmPing.UI;

namespace vmPing.Classes
{
    public partial class Probe
    {
        public void StartStop()
        {
            if (string.IsNullOrWhiteSpace(Hostname))
            {
                return;
            }

            if (IsActive)
            {
                // Stopping probe.
                StopProbe(ProbeStatus.Inactive);
                return;
            }

            // Starting probe.
            CancelSource = new CancellationTokenSource();

            if (Hostname.StartsWith("#"))
            {
                Type = ProbeType.Comment;
                return;
            }

            if (Hostname.StartsWith("D/"))
            {
                Type = ProbeType.Dns;
                Hostname = Hostname.Substring(2);
                PerformDnsLookup(CancelSource.Token);
                return;
            }

            if (Hostname.StartsWith("T/"))
            {
                Type = ProbeType.Traceroute;
                Hostname = Hostname.Substring(2);
                PerformTraceroute(CancelSource.Token);
                return;
            }

            Type = ProbeType.Ping;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (mutex)
                {
                    StatusChangeLog.Add(new StatusChangeLog
                    {
                        Timestamp = DateTime.Now,
                        Hostname = Hostname,
                        Alias = Alias,
                        Status = ProbeStatus.Start
                    });
                }
            }));

            if (IsTcpPing(Hostname))
            {
                Task.Run(() => PerformTcpProbe(CancelSource.Token), CancelSource.Token);
            }
            else
            {
                Task.Run(() => PerformIcmpProbe(CancelSource.Token), CancelSource.Token);
            }
        }

        private static bool IsTcpPing(string hostname)
        {
            return hostname.Count(f => f == ':') == 1 || hostname.Contains("]:");
        }

        private void InitializeProbe()
        {
            IsActive = true;
            Status = ProbeStatus.Inactive;
            Statistics.Reset();
            History = new ObservableCollection<string>();
        }

        private void StopProbe(ProbeStatus status)
        {
            CancelSource.Cancel();
            Status = status;
            IsActive = false;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (mutex)
                {
                    if (status != ProbeStatus.Error)
                    {
                        WriteFinalStatisticsToHistory();
                    }

                    StatusChangeLog.Add(new StatusChangeLog
                    {
                        Timestamp = DateTime.Now,
                        Hostname = Hostname,
                        Alias = Alias,
                        Status = ProbeStatus.Stop
                    });
                }
            }));
        }

        private async Task<bool> IsHostInvalid(string host, CancellationToken token)
        {
            try
            {
                switch (Uri.CheckHostName(host))
                {
                    case UriHostNameType.IPv4:
                    case UriHostNameType.IPv6:
                        // IP address was entered. No further action necessary.
                        break;
                    case UriHostNameType.Dns:
                        var ipAddresses = await Dns.GetHostAddressesAsync(host);
                        token.ThrowIfCancellationRequested();
                        if (ipAddresses.Length > 0)
                        {
                            await Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => AddHistory($"    ({ipAddresses[0]})")));
                        }
                        break;
                    default:
                        throw new Exception();
                }
                return false;
            }
            catch
            {
                if (!token.IsCancellationRequested)
                {
                    await Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => AddHistory($"{Environment.NewLine}Unable to resolve hostname")));
                }
                return true;
            }
        }

        private void WriteToLog(string message)
        {
            if (!ApplicationOptions.IsLogOutputEnabled || string.IsNullOrEmpty(ApplicationOptions.LogPath))
            {
                return;
            }

            string logPath = Path.Combine(ApplicationOptions.LogPath, $"{Util.GetSafeFilename(Hostname)}.txt");

            try
            {
                File.AppendAllText(logPath, message.Insert(1, $"{DateTime.Now.ToShortDateString()} ") + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ApplicationOptions.IsLogOutputEnabled = false;
                ShowError($"Failed writing to log file. Logging has been disabled. Error: {ex.Message}");
            }
        }

        private void WriteToStatusChangesLog(StatusChangeLog status)
        {
            if (!ApplicationOptions.IsLogStatusChangesEnabled || string.IsNullOrEmpty(ApplicationOptions.LogStatusChangesPath))
            {
                return;
            }

            try
            {
                File.AppendAllText(ApplicationOptions.LogStatusChangesPath,
                    $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}\t{status.Hostname}\t{status.Alias}\t{status.StatusAsString}");
            }
            catch (Exception ex)
            {
                ApplicationOptions.IsLogStatusChangesEnabled = false;
                ShowError($"Failed writing to log file. Logging has been disabled. Error: {ex.Message}");
            }
        }

        private void DisplayStatistics()
        {
            StatisticsText = $"Sent: {Statistics.Sent} Received: {Statistics.Received} Lost: {Statistics.Lost}";
        }

        private void TriggerStatusChange(StatusChangeLog status)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                bool shouldPopup = ApplicationOptions.PopupOption == ApplicationOptions.PopupNotificationOption.Always
                    || (ApplicationOptions.PopupOption == ApplicationOptions.PopupNotificationOption.WhenMinimized
                    && Application.Current.MainWindow.WindowState == WindowState.Minimized);

                lock (mutex)
                {
                    if (shouldPopup && !Application.Current.Windows.OfType<PopupNotificationWindow>().Any())
                    {
                        foreach (var entry in StatusChangeLog)
                        {
                            entry.HasStatusBeenCleared = true;
                        }
                    }

                    StatusChangeLog.Add(status);
                }

                if (shouldPopup && !Application.Current.Windows.OfType<PopupNotificationWindow>().Any())
                {
                    new PopupNotificationWindow(StatusChangeLog).Show();
                }
            }));

            if (ApplicationOptions.IsLogStatusChangesEnabled)
            {
                lock (mutex)
                {
                    WriteToStatusChangesLog(status);
                }
            }

            if ((ApplicationOptions.IsAudioDownAlertEnabled) && (status.Status == ProbeStatus.Down))
            {
                try
                {
                    using (SoundPlayer player = new SoundPlayer(ApplicationOptions.AudioDownFilePath))
                    {
                        player.Play();
                    }
                }
                catch (Exception ex)
                {
                    ApplicationOptions.IsAudioDownAlertEnabled = false;
                    ShowError($"Failed to play audio file. Audio alerts have been disabled. Error: {ex.Message}");
                }
            }
            else if ((ApplicationOptions.IsAudioUpAlertEnabled) && (status.Status == ProbeStatus.Up))
            {
                try
                {
                    using (SoundPlayer player = new SoundPlayer(ApplicationOptions.AudioUpFilePath))
                    {
                        player.Play();
                    }
                }
                catch (Exception ex)
                {
                    ApplicationOptions.IsAudioUpAlertEnabled = false;
                    ShowError($"Failed to play audio file. Audio alerts have been disabled. Error: {ex.Message}");
                }
            }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                DialogWindow.ErrorWindow(message).ShowDialog()));
        }
    }
}