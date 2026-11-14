using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace vmPing.Probes
{
    public class PingStatistics : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private uint _sent;
        public uint Sent
        {
            get => _sent;
            set { _sent = value; OnPropertyChanged(); }
        }

        private uint _received;
        public uint Received
        {
            get => _received;
            set { _received = value; OnPropertyChanged(); }
        }

        private uint _lost;
        public uint Lost
        {
            get => _lost;
            set { _lost = value; OnPropertyChanged(); }
        }

        public void Reset()
        {
            _sent = _received = _lost = 0;
            OnPropertyChanged(nameof(Sent));
            OnPropertyChanged(nameof(Received));
            OnPropertyChanged(nameof(Lost));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
