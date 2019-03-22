namespace vmPing.Classes
{
    public class PingStatistics
    {
        public uint Sent { get; set; } = 0;
        public uint Received { get; set; } = 0;
        public uint Lost { get; set; } = 0;
        public uint Error { get; set; } = 0;
    }
}
