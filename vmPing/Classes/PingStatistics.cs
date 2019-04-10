namespace vmPing.Classes
{
    public class PingStatistics
    {
        public uint Sent { get; set; }
        public uint Received { get; set; }
        public uint Lost { get; set; }
        public uint Error { get; set; }
    }
}
