using System;
using System.ComponentModel;
using System.Threading;

namespace vmPing.Models
{
    public class FloodHostNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime StartTime { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    NotifyPropertyChanged("IsActive");
                }
            }
        }

        private long _packetsPerSecond;
        public long PacketsPerSecond
        {
            get { return _packetsPerSecond; }
            set
            {
                if (value != _packetsPerSecond)
                {
                    _packetsPerSecond = value;
                    NotifyPropertyChanged("PacketsPerSecond");
                }
            }
        }

        private long _packetsSent;
        public long PacketsSent
        {
            get { return _packetsSent; }
            set
            {
                if (value != _packetsSent)
                {
                    _packetsSent = value;
                    NotifyPropertyChanged("PacketsSent");
                }
            }
        }

        private long _packetsReceived;
        public long PacketsReceived
        {
            get { return _packetsReceived; }
            set
            {
                if (value != _packetsReceived)
                {
                    _packetsReceived = value;
                    NotifyPropertyChanged("PacketsReceived");
                }
            }
        }

        private long _packetsLost;
        public long PacketsLost
        {
            get { return _packetsLost; }
            set
            {
                if (value != _packetsLost)
                {
                    _packetsLost = value;
                    NotifyPropertyChanged("PacketsLost");
                }
            }
        }

        public string DestinationAddress { get; set; }
        public BackgroundWorker BgWorker { get; set; }
        public AutoResetEvent ResetEvent { get; set; }


        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
