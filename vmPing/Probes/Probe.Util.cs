using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using vmPing.Configuration;
using vmPing.Models;
using vmPing.Services;
using vmPing.Views;

namespace vmPing.Probes
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

            // Hostnames starting with # are comments and aren't probed.
            if (Hostname.StartsWith("#"))
            {
                return;
            }

            if (Type == ProbeType.Dns)
            {
                _ = PerformDnsLookup(CancelSource.Token);
                return;
            }

            if (Type == ProbeType.Traceroute)
            {
                _ = PerformTraceroute(CancelSource.Token);
                return;
            }

            AddStatusHistory(ProbeStatus.Start);

            // Set IsActive before the background task starts.
            InitializeProbe();

            CancellationToken token = CancelSource.Token;
            if (IsTcpPing(Hostname))
            {
                Task.Run(() => PerformTcpProbe(token), token);
            }
            else
            {
                Task.Run(() => PerformIcmpProbe(token), token);
            }
        }

        // Waits for the given time. Returns false if the probe was stopped while waiting.
        // WhenAny finishes without throwing when the delay is cancelled.
        private static async Task<bool> Delay(int milliseconds, CancellationToken cancellationToken)
        {
            await Task.WhenAny(Task.Delay(milliseconds, cancellationToken));
            return !cancellationToken.IsCancellationRequested;
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
            IndeterminateCount = 0;
            HighLatencyCount = 0;
            _isLatencyHigh = false;
            ClearHistory();
        }

        private void StopProbe(ProbeStatus status)
        {
            CancelSource.Cancel();
            Status = status;
            IsActive = false;

            if (status != ProbeStatus.Error)
            {
                // Post, so the statistics come after any output the probe has already posted.
                Application.Current.Dispatcher.BeginInvoke(new Action(WriteFinalStatisticsToHistory));
            }

            AddStatusHistory(ProbeStatus.Stop);
        }

        private void AddStatusHistory(ProbeStatus status, bool isHidden = false)
        {
            StatusChange entry = new StatusChange
            {
                Timestamp = DateTime.Now,
                Hostname = Hostname,
                Alias = Alias,
                Status = status,
                HasStatusBeenCleared = isHidden
            };

            // The status history is shown on screen, so it is only changed on the UI thread.
            Application.Current.Dispatcher.BeginInvoke(new Action(() => StatusChangeLog.Add(entry)));
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
                        IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(host);
                        token.ThrowIfCancellationRequested();
                        if (ipAddresses.Length > 0)
                        {
                            AddHistory($"    ({ipAddresses[0]})");
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
                    AddHistory($"{Environment.NewLine}Unable to resolve hostname");
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

        private void WriteToStatusChangesLog(StatusChange status)
        {
            if (!ApplicationOptions.IsLogStatusChangesEnabled || string.IsNullOrEmpty(ApplicationOptions.LogStatusChangesPath))
            {
                return;
            }

            try
            {
                File.AppendAllText(ApplicationOptions.LogStatusChangesPath,
                    $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}\t{status.Hostname}\t{status.Alias}\t{status.StatusAsString}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                ApplicationOptions.IsLogStatusChangesEnabled = false;
                ShowError($"Failed writing to log file. Logging has been disabled. Error: {ex.Message}");
            }
        }

        private void OnStatusChange(ProbeStatus newStatus, string alertType)
        {
            Status = newStatus;
            TriggerStatusChange(new StatusChange
            {
                Timestamp = DateTime.Now,
                Hostname = Hostname,
                Alias = Alias,
                Status = newStatus
            });

            if (ApplicationOptions.IsEmailAlertEnabled)
            {
                Email.SendAlert(alertType, Hostname, Alias);
            }
        }

        private void TriggerStatusChange(StatusChange status)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                bool shouldPopup = ApplicationOptions.PopupOption == PopupNotificationOption.Always
                    || (ApplicationOptions.PopupOption == PopupNotificationOption.WhenMinimized
                    && Application.Current.MainWindow.WindowState == WindowState.Minimized);
                bool isNewPopup = shouldPopup && !Application.Current.Windows.OfType<PopupNotificationWindow>().Any();

                // A new popup only shows changes that happened since the last one.
                if (isNewPopup)
                {
                    foreach (StatusChange entry in StatusChangeLog)
                    {
                        entry.HasStatusBeenCleared = true;
                    }
                }

                StatusChangeLog.Add(status);

                if (isNewPopup)
                {
                    new PopupNotificationWindow(StatusChangeLog).Show();
                }
            }));

            if (ApplicationOptions.IsLogStatusChangesEnabled)
            {
                // Probes can write to this file from different threads at the same time.
                lock (s_statusLogFileLock)
                {
                    WriteToStatusChangesLog(status);
                }
            }

            if (ApplicationOptions.IsAudioDownAlertEnabled && status.Status == ProbeStatus.Down)
            {
                ApplicationOptions.IsAudioDownAlertEnabled = PlayAudio(ApplicationOptions.AudioDownFilePath);
            }
            else if (ApplicationOptions.IsAudioUpAlertEnabled && status.Status == ProbeStatus.Up)
            {
                ApplicationOptions.IsAudioUpAlertEnabled = PlayAudio(ApplicationOptions.AudioUpFilePath);
            }
        }

        // Returns false if the file could not be played.
        private bool PlayAudio(string path)
        {
            try
            {
                using (SoundPlayer player = new SoundPlayer(path))
                {
                    player.Play();
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to play audio file. Audio alerts have been disabled. Error: {ex.Message}");
                return false;
            }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                DialogWindow.ErrorWindow(message).ShowDialog()));
        }
    }
}