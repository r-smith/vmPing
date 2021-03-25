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
            sent = 0;
            received = 0;
            lost = 0;
            error = 0;
            OnPropertyChanged(name: "Sent");
            OnPropertyChanged(name: "Received");
            OnPropertyChanged(name: "Lost");
            OnPropertyChanged(name: "Error");
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
