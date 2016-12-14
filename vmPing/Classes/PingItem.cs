using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace vmPing.Classes
{
    public enum PingStatus
    {
        Up,
        Down,
        Error,
        Inactive
    }

    public class PingItem : INotifyPropertyChanged
    {
        public static int NumberOfActivePings;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Hostname { get; set; }
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
