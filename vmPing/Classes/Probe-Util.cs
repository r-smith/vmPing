using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using vmPing.Views;

namespace vmPing.Classes
{
    public partial class Probe
    {
        public void StartStop()
        {
            if (string.IsNullOrEmpty(Hostname)) return;
            if (IsActive)
            {
                // Stopping probe.
                StopProbe(ProbeStatus.Inactive);
                WriteFinalStatisticsToHistory();
            }
            else
            {
                // Starting probe.
                CancelSource = new CancellationTokenSource();
                if (Hostname.Count(f => f == ':') == 1)
                    Task.Run(() => PerformTcpProbe(CancelSource.Token), CancelSource.Token);
                else
                    Task.Run(() => PerformIcmpProbe(CancelSource.Token), CancelSource.Token);
            }
        }


        private void InitializeProbe()
        {
            IsActive = true;
            StatisticsText = string.Empty;
            History = new ObservableCollection<string>();
            Statistics = new PingStatistics();
        }


        private void StopProbe(ProbeStatus status)
        {
            CancelSource.Cancel();
            Status = status;
            IsActive = false;
        }


        private async Task<bool> IsHostInvalid(string host, CancellationToken cancellationToken)
        {
            try
            {
                switch (Uri.CheckHostName(host))
                {
                    case UriHostNameType.IPv4:
                    case UriHostNameType.IPv6:
                        // IP address was entered.  No further action necessary.
                        break;
                    case UriHostNameType.Dns:
                        var ipAddresses = await Dns.GetHostAddressesAsync(host);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (ipAddresses.Length > 0)
                            await Application.Current.Dispatcher.BeginInvoke(
                                new Action(() => AddHistory($"*** [{ipAddresses[0]}]")));
                        break;
                    default:
                        throw new Exception();
                }
                return false;
            }
            catch
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => AddHistory("Unable to resolve hostname.")));
                }
                return true;
            }
        }


        private async void WriteToLog(string message)
        {
            // If logging is enabled, write the response to a file.
            if (ApplicationOptions.IsLogOutputEnabled && ApplicationOptions.LogPath.Length > 0)
            {
                var logPath = $@"{ApplicationOptions.LogPath}\{Hostname}.txt";
                using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter(@logPath, true))
                {
                    await outputFile.WriteLineAsync(message.Insert(1, DateTime.Now.ToShortDateString() + " "));
                }
            }
        }


        private void DisplayStatistics()
        {
            // TODO: This should be a computed property.
            StatisticsText =
                $"Sent: {Statistics.Sent} Received: {Statistics.Received} Lost: {Statistics.Lost}";
        }


        private void TriggerStatusChange(StatusChangeLog status)
        {
            if (ApplicationOptions.PopupOption == ApplicationOptions.PopupNotificationOption.Always ||
                (ApplicationOptions.PopupOption == ApplicationOptions.PopupNotificationOption.WhenMinimized &&
                Application.Current.MainWindow.WindowState == WindowState.Minimized))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        mutex.WaitOne();
                        if (!Application.Current.Windows.OfType<PopupNotificationWindow>().Any())
                        {
                            // Mark all existing status changes as read.
                            for (int i = 0; i < StatusChangeLog.Count; ++i)
                                StatusChangeLog[i].HasStatusBeenCleared = true;
                        }
                        StatusChangeLog.Add(status);
                        mutex.ReleaseMutex();

                        if (StatusWindow != null && StatusWindow.IsLoaded)
                        {
                            if (StatusWindow.WindowState == WindowState.Minimized)
                                StatusWindow.WindowState = WindowState.Normal;
                            StatusWindow.Focus();
                        }
                        else if (!Application.Current.Windows.OfType<PopupNotificationWindow>().Any())
                        {
                            new PopupNotificationWindow(StatusChangeLog).Show();
                        }
                    }));
            }

            else
            {
                mutex.WaitOne();
                StatusChangeLog.Add(status);
                mutex.ReleaseMutex();
            }
        }
    }
}