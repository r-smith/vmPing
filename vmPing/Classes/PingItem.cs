using System;
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

        private string pingOuput;
        public string PingOutput
        {
            get { return pingOuput; }
            set
            {
                if (value != pingOuput)
                {
                    string[] lines = value.Split('\n').ToArray();
                    if (lines.GetUpperBound(0) > 2000)
                        value = string.Join("\n", lines, 1, lines.GetUpperBound(0));
                    else
                        value = string.Join("\n", lines);
                    pingOuput = value;
                    NotifyPropertyChanged("PingOutput");
                }
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
