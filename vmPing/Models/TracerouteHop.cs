namespace vmPing.Models
{
    internal class TracerouteHop
    {
        public int HopId { get; set; }
        public string HostAddress { get; set; }
        public long RoundTripTime { get; set; }
    }
}
