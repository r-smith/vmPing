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

        // Round-trip times of the replies received, in milliseconds.
        public long MinimumTime { get; private set; }
        public long MaximumTime { get; private set; }
        public double AverageTime => _timeCount == 0 ? 0 : (double)_totalTime / _timeCount;

        private long _totalTime;
        private long _timeCount;

        public void AddRoundTripTime(long milliseconds)
        {
            if (_timeCount == 0 || milliseconds < MinimumTime)
            {
                MinimumTime = milliseconds;
            }
            if (milliseconds > MaximumTime)
            {
                MaximumTime = milliseconds;
            }

            _totalTime += milliseconds;
            _timeCount++;
        }

        public void Reset()
        {
            _sent = _received = _lost = 0;
            MinimumTime = MaximumTime = 0;
            _totalTime = _timeCount = 0;
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
