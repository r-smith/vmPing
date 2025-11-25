using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using vmPing.Properties;
using vmPing.UI;

namespace vmPing.Classes
{
    public enum ProbeStatus
    {
        Up,
        Down,
        Error,
        LatencyHigh,
        LatencyNormal,
        Indeterminate,
        Inactive,
        Scanner,
        Start,
        Stop
    }

    public enum ProbeType
    {
        Dns,
        Ping,
        Traceroute,
        Comment
    }

    public partial class Probe : INotifyPropertyChanged
    {
        // Static members
        public static ObservableCollection<StatusChangeLog> StatusChangeLog { get; } = new ObservableCollection<StatusChangeLog>();
        public static StatusHistoryWindow StatusHistoryWindow;

        private static readonly Mutex mutex = new Mutex();

        private static long activeCount;
        public static long ActiveCount
        {
            get => Interlocked.Read(ref activeCount);
            set => Interlocked.Exchange(ref activeCount, value);
        }
        public static event EventHandler ActiveCountChanged;

        // Instance members
        public event PropertyChangedEventHandler PropertyChanged;

        public IsolatedPingWindow IsolatedWindow { get; set; }
        public int IndeterminateCount { get; set; }
        public int HighLatencyCount { get; set; }
        public long MinRtt { get; set; } = long.MaxValue;
        public PingStatistics Statistics { get; set; } = new PingStatistics();
        public int SelStart { get; set; }
        public int SelLength { get; set; }
        public CancellationTokenSource CancelSource { get; set; }

        private ObservableCollection<string> history;
        public ObservableCollection<string> History
        {
            get => history;
            set
            {
                if (value != history)
                {
                    if (history != null)
                    {
                        history.CollectionChanged -= OnHistoryChanged;
                    }

                    history = value;
                    history.CollectionChanged += OnHistoryChanged;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HistoryAsString));
                }
            }
        }

        public string HistoryAsString => History != null
            ? string.Join(Environment.NewLine, History)
            : string.Empty;

        private ProbeType type = ProbeType.Ping;
        public ProbeType Type
        {
            get => type;
            set
            {
                if (value != type)
                {
                    type = value;
                    OnPropertyChanged();
                }
            }
        }

        private string hostname;
        public string Hostname
        {
            get => hostname;
            set
            {
                if (value != hostname)
                {
                    hostname = value?.Trim();
                    OnPropertyChanged();
                }
            }
        }

        private string alias;
        public string Alias
        {
            get => alias;
            set
            {
                if (value != alias)
                {
                    alias = value;
                    OnPropertyChanged();
                }
            }
        }

        private ProbeStatus status = ProbeStatus.Inactive;
        public ProbeStatus Status
        {
            get => status;
            set
            {
                if (value != status)
                {
                    status = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isActive = false;
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value == isActive)
                {
                    return;
                }

                isActive = value;
                OnPropertyChanged();

                if (value)
                {
                    Interlocked.Increment(ref activeCount);
                }
                else
                {
                    Interlocked.Decrement(ref activeCount);
                }

                ActiveCountChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private string statisticsText;
        public string StatisticsText
        {
            get => statisticsText;
            set
            {
                if (value != statisticsText)
                {
                    statisticsText = value;
                    OnPropertyChanged();
                }
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

            var roundTripTimes = new List<int>();
            var rttRegex = new Regex($@"  \[(?<rtt><?\d+) ?{Strings.Milliseconds_Symbol}]$");

            foreach (string item in History)
            {
                var match = rttRegex.Match(item);
                if (!match.Success)
                {
                    continue;
                }

                // RTT of `<1` is treated as 0 when computing stats.
                var value = match.Groups["rtt"].Value;
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
