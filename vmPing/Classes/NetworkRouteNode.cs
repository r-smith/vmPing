using System;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace vmPing.Classes
{
    class NetworkRouteNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private string hostAddress;
        public string HostAddress
        {
            get { return hostAddress; }
            set
            {
                if (value != hostAddress)
                {
                    hostAddress = value;
                    NotifyPropertyChanged("HostAddress");
                }
            }
        }


        private string hostName;
        public string HostName
        {
            get { return hostName; }
            set
            {
                if (value != hostName)
                {
                    hostName = value;
                    NotifyPropertyChanged("HostName");
                }
            }
        }


        private long roundTripTime;
        public long RoundTripTime
        {
            get { return roundTripTime; }
            set
            {
                if (value != roundTripTime)
                {
                    roundTripTime = value;
                    NotifyPropertyChanged("RoundTripTime");
                }
            }
        }

        public IPStatus ReplyStatus { get; set; }
        public int HopId { get; set; }


        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
