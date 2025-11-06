using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace vmPing.Classes
{
    public class PingStatistics : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private uint sent;
        public uint Sent
        {
            get => sent;
            set { sent = value; OnPropertyChanged(); }
        }

        private uint received;
        public uint Received
        {
            get => received;
            set { received = value; OnPropertyChanged(); }
        }

        private uint lost;
        public uint Lost
        {
            get => lost;
            set { lost = value; OnPropertyChanged(); }
        }

        private uint error;
        public uint Error
        {
            get => error;
            set { error = value; OnPropertyChanged(); }
        }

        public void Reset()
        {
            sent = received = lost = error = 0;
            OnPropertyChanged(nameof(Sent));
            OnPropertyChanged(nameof(Received));
            OnPropertyChanged(nameof(Lost));
            OnPropertyChanged(nameof(Error));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
