using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace vmPing.Classes
{
    class NetworkRoute : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (value != isActive)
                {
                    isActive = value;
                    NotifyPropertyChanged("IsActive");
                }
            }
        }

        public string DestinationHost { get; set; }
        public IPAddress DestinationIp { get; set; }
        public int MaxHops { get; set; }
        public int PingTimeout { get; set; }
        public Stopwatch Timer { get; set; }

        public BackgroundWorker BgWorker { get; set; }
        public AutoResetEvent ResetEvent { get; set; }

        public ObservableCollection<NetworkRouteNode> networkRoute = new ObservableCollection<NetworkRouteNode>();

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
