using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using vmPing.Models;
using vmPing.Properties;
using vmPing.Views;

namespace vmPing.Probes
{
    public partial class Probe : INotifyPropertyChanged
    {
        // Static members
        public static ObservableCollection<StatusChange> StatusChangeLog { get; } = new ObservableCollection<StatusChange>();
        public static StatusHistoryWindow StatusHistoryWindow;

        private static readonly object s_statusLogFileLock = new object();

        private static long s_activeCount;
        public static long ActiveCount
        {
            get => Interlocked.Read(ref s_activeCount);
            set => Interlocked.Exchange(ref s_activeCount, value);
        }
        public static event EventHandler ActiveCountChanged;

        // Instance members
        public event PropertyChangedEventHandler PropertyChanged;

        public IsolatedPingWindow IsolatedWindow { get; set; }
        public int IndeterminateCount { get; set; }
        public int HighLatencyCount { get; set; }
        public PingStatistics Statistics { get; set; } = new PingStatistics();
        public CancellationTokenSource CancelSource { get; set; }

        // Output lines. HistoryAdded is raised for each new line, and HistoryReset when the whole output changes.
        private readonly List<string> _history = new List<string>();
        public event EventHandler<string> HistoryAdded;
        public event EventHandler HistoryReset;

        private ProbeType _type = ProbeType.Ping;
        public ProbeType Type
        {
            get => _type;
            set
            {
                if (value != _type)
                {
                    _type = value;

                    // Output, statistics and status from the old type don't apply to the new one.
                    ClearHistory();
                    Statistics.Reset();
                    Status = ProbeStatus.Inactive;

                    OnPropertyChanged();
                }
            }
        }

        private string _hostname;
        public string Hostname
        {
            get => _hostname;
            set
            {
                if (value != _hostname)
                {
                    _hostname = value?.Trim();
                    OnPropertyChanged();
                }
            }
        }

        private string _alias;
        public string Alias
        {
            get => _alias;
            set
            {
                if (value != _alias)
                {
                    _alias = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayAlias));
                }
            }
        }

        // A name for the host from a favorite line like "10.0.0.1 # switch". Overrides the alias.
        private string _label;
        public string Label
        {
            get => _label;
            set
            {
                if (value != _label)
                {
                    _label = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayAlias));
                }
            }
        }

        // The name shown in place of the hostname: the label, else the alias. Empty if neither is set.
        public string DisplayAlias => !string.IsNullOrEmpty(Label) ? Label : Alias;

        private ProbeStatus _status = ProbeStatus.Inactive;
        public ProbeStatus Status
        {
            get => _status;
            set
            {
                if (value != _status)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (value == _isActive)
                {
                    return;
                }

                _isActive = value;
                OnPropertyChanged();

                if (value)
                {
                    Interlocked.Increment(ref s_activeCount);
                }
                else
                {
                    Interlocked.Decrement(ref s_activeCount);
                }

                ActiveCountChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        // Safe to call from any thread. The output is shown on screen,
        // so lines from a probe's background thread are posted to the UI thread.
        public void AddHistory(string line)
        {
            const int MaxLines = 3600;
            const int TrimLines = 600;

            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => AddHistory(line)));
                return;
            }

            _history.Add(line);

            // Remove old lines in batches, so the output only has to be rebuilt once in a while.
            if (_history.Count > MaxLines + TrimLines)
            {
                _history.RemoveRange(0, _history.Count - MaxLines);
                HistoryReset?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                HistoryAdded?.Invoke(this, line);
            }
        }

        public void ClearHistory()
        {
            _history.Clear();
            HistoryReset?.Invoke(this, EventArgs.Empty);
        }

        public string GetHistoryText()
        {
            return string.Join(Environment.NewLine, _history);
        }

        public void WriteFinalStatisticsToHistory()
        {
            if (Statistics == null || Statistics.Sent == 0)
            {
                return;
            }

            // Display stats and round trip times.
            AddHistory(string.Empty);
            AddHistory(
                $"Sent {Statistics.Sent}, " +
                $"Received {Statistics.Received}, " +
                $"Lost {Statistics.Sent - Statistics.Received} " +
                $"({(100 * (Statistics.Sent - Statistics.Received)) / Statistics.Sent}% loss)");

            if (Statistics.Received > 0)
            {
                AddHistory(
                    $"Minimum ({Statistics.MinimumTime}{Strings.Milliseconds_Symbol}), " +
                    $"Maximum ({Statistics.MaximumTime}{Strings.Milliseconds_Symbol}), " +
                    $"Average ({Statistics.AverageTime:0.##}{Strings.Milliseconds_Symbol})");
            }

            AddHistory(" ");
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
