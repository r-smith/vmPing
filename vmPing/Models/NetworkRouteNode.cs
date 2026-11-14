using System;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace vmPing.Models
{
    class NetworkRouteNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private string _hostAddress;
        public string HostAddress
        {
            get { return _hostAddress; }
            set
            {
                if (value != _hostAddress)
                {
                    _hostAddress = value;
                    NotifyPropertyChanged("HostAddress");
                }
            }
        }


        private string _hostName;
        public string HostName
        {
            get { return _hostName; }
            set
            {
                if (value != _hostName)
                {
                    _hostName = value;
                    NotifyPropertyChanged("HostName");
                }
            }
        }


        private long _roundTripTime;
        public long RoundTripTime
        {
            get { return _roundTripTime; }
            set
            {
                if (value != _roundTripTime)
                {
                    _roundTripTime = value;
                    NotifyPropertyChanged("RoundTripTime");
                }
            }
        }

        public IPStatus ReplyStatus { get; set; }
        public int HopId { get; set; }


        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
