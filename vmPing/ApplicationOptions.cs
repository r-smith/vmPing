namespace vmPing
{
    public class ApplicationOptions
    {
        int pingInterval = Constants.PING_INTERVAL;
        public int PingInterval
        {
            get { return this.pingInterval; }
            set { this.pingInterval = value; }
        }

        int pingTimeout = Constants.PING_TIMEOUT;
        public int PingTimeout
        {
            get { return this.pingTimeout; }
            set { this.pingTimeout = value; }
        }

        public bool EmailAlert { get; set; }
        public bool AlwaysOnTop { get; set; }
        public string EmailServer { get; set; }
        public string EmailRecipient { get; set; }
        public string EmailFromAddress { get; set; }
        public bool LogOutput { get; set; }
        public string LogPath { get; set; }
    }
}
