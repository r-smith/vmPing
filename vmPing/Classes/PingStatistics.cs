namespace vmPing.Classes
{
    public class PingStatistics
    {
        public uint PingsSent { get; set; }
        public uint PingsReceived { get; set; }
        public uint PingsLost { get; set; }
        public uint PingsError { get; set; }
        public PingStatistics()
        {
            PingsSent = 0;
            PingsReceived = 0;
            PingsLost = 0;
            PingsError = 0;
        }
    }
}
