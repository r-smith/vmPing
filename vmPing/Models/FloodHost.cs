using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace vmPing.Models
{
    public class FloodHost : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _packetsSent;
        public long PacketsSent
        {
            get => _packetsSent;
            set
            {
                if (value != _packetsSent)
                {
                    _packetsSent = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _packetsReceived;
        public long PacketsReceived
        {
            get => _packetsReceived;
            set
            {
                if (value != _packetsReceived)
                {
                    _packetsReceived = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _packetsLost;
        public long PacketsLost
        {
            get => _packetsLost;
            set
            {
                if (value != _packetsLost)
                {
                    _packetsLost = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
