using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading;
using vmPing.Views;

namespace vmPing.Classes
{
    public enum PingStatus
    {
        Up,
        Down,
        Error,
        Indeterminate,
        Inactive
    }

    public class PingItem : INotifyPropertyChanged
    {
        public static int NumberOfActivePings;

        public event PropertyChangedEventHandler PropertyChanged;
        public IsolatedPingWindow IsolatedWindow { get; set; }
        public int DownCount { get; set; }
        public BackgroundWorker PingBackgroundWorker { get; set; }
        public AutoResetEvent PingResetEvent { get; set; }
        public PingStatistics Statistics { get; set; }
        public PingReply Reply { get; set; }
        public Ping Sender { get; set; }
        
        private ObservableCollection<string> history;
        public ObservableCollection<string> History
        {
            get { return history; }
            set
            {
                if (value != history)
                {
                    history = value;
                    NotifyPropertyChanged("History");
                }
            }
        }

        private string hostname;
        public string Hostname
        {
            get { return hostname; }
            set
            {
                if (value != hostname)
                {
                    hostname = value;
                    NotifyPropertyChanged("Hostname");
                }
            }
        }


        private string alias;
        public string Alias
        {
            get { return alias; }
            set
            {
                if (value != alias)
                {
                    alias = value;
                    NotifyPropertyChanged("Alias");
                }
            }
        }

        private PingStatus status = PingStatus.Inactive;
        public PingStatus Status
        {
            get { return status; }
            set
            {
                if (value != status)
                {
                    status = value;
                    NotifyPropertyChanged("Status");
                }
            }
        }

        private bool isActive = false;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (value == true)
                    ++NumberOfActivePings;
                else
                    --NumberOfActivePings;
                NotifyPropertyChanged("NumberOfActivePings");

                if (value != isActive)
                {
                    isActive = value;
                    NotifyPropertyChanged("IsActive");
                }
            }
        }

        private string pingStatisticsText;
        public string PingStatisticsText
        {
            get { return pingStatisticsText; }
            set
            {
                if (value != pingStatisticsText)
                {
                    pingStatisticsText = value;
                    NotifyPropertyChanged("PingStatisticsText");
                }
            }
        }

        public void AddHistory(string historyItem)
        {
            if (history.Count >= 3600)
                history.RemoveAt(0);
            history.Add(historyItem);
        }

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
