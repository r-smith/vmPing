using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
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

        private static readonly object s_lock = new object();

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
        public long MinRtt { get; set; } = long.MaxValue;
        public PingStatistics Statistics { get; set; } = new PingStatistics();
        public CancellationTokenSource CancelSource { get; set; }

        private ObservableCollection<string> _history;
        public ObservableCollection<string> History
        {
            get => _history;
            set
            {
                if (value != _history)
                {
                    if (_history != null)
                    {
                        _history.CollectionChanged -= OnHistoryChanged;
                    }

                    _history = value;
                    _history.CollectionChanged += OnHistoryChanged;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HistoryAsString));
                }
            }
        }

        public string HistoryAsString => History != null
            ? string.Join(Environment.NewLine, History)
            : string.Empty;

        private ProbeType _type = ProbeType.Ping;
        public ProbeType Type
        {
            get => _type;
            set
            {
                if (value != _type)
                {
                    _type = value;
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
                }
            }
        }

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

        public void AddHistory(string historyItem)
        {
            const int MaxSize = 3600;

            History.Add(historyItem);
            if (History.Count > MaxSize)
            {
                History.RemoveAt(0);
            }
        }

        public void WriteFinalStatisticsToHistory()
        {
            if (Statistics == null || Statistics.Sent == 0)
            {
                return;
            }

            List<int> roundTripTimes = new List<int>();
            Regex rttRegex = new Regex($@"  \[(?<rtt><?\d+) ?{Strings.Milliseconds_Symbol}]$");

            foreach (string item in History)
            {
                Match match = rttRegex.Match(item);
                if (!match.Success)
                {
                    continue;
                }

                // RTT of `<1` is treated as 0 when computing stats.
                string value = match.Groups["rtt"].Value;
                roundTripTimes.Add(value == "<1" ? 0 : int.Parse(value));
            }

            // Display stats and round trip times.
            AddHistory(string.Empty);
            AddHistory(
                $"Sent {Statistics.Sent}, " +
                $"Received {Statistics.Received}, " +
                $"Lost {Statistics.Sent - Statistics.Received} " +
                $"({(100 * (Statistics.Sent - Statistics.Received)) / Statistics.Sent}% loss)");

            if (roundTripTimes.Count > 0)
            {
                AddHistory(
                    $"Minimum ({roundTripTimes.Min()}{Strings.Milliseconds_Symbol}), " +
                    $"Maximum ({roundTripTimes.Max()}{Strings.Milliseconds_Symbol}), " +
                    $"Average ({roundTripTimes.Average():0.##}{Strings.Milliseconds_Symbol})");
            }

            AddHistory(" ");
        }

        private void OnHistoryChanged(object sender, NotifyCollectionChangedEventArgs e) =>
            OnPropertyChanged(nameof(HistoryAsString));

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
