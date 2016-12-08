using System;
using System.ComponentModel;
using System.Threading;

namespace vmPing.Classes
{
    public class FloodHostNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime StartTime { get; set; }

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

        private long packetsPerSecond;
        public long PacketsPerSecond
        {
            get { return packetsPerSecond; }
            set
            {
                if (value != packetsPerSecond)
                {
                    packetsPerSecond = value;
                    NotifyPropertyChanged("PacketsPerSecond");
                }
            }
        }

        private long packetsSent;
        public long PacketsSent
        {
            get { return packetsSent; }
            set
            {
                if (value != packetsSent)
                {
                    packetsSent = value;
                    NotifyPropertyChanged("PacketsSent");
                }
            }
        }

        private long packetsReceived;
        public long PacketsReceived
        {
            get { return packetsReceived; }
            set
            {
                if (value != packetsReceived)
                {
                    packetsReceived = value;
                    NotifyPropertyChanged("PacketsReceived");
                }
            }
        }

        private long packetsLost;
        public long PacketsLost
        {
            get { return packetsLost; }
            set
            {
                if (value != packetsLost)
                {
                    packetsLost = value;
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
