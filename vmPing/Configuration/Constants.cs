using System.Windows.Input;

namespace vmPing.Configuration
{
    internal static class Constants
    {
        // Status changes are logged to this file in the log folder.
        public const string StatusLogFileName = "vmPing-status.txt";

        // Default probe options.
        public const int DefaultPacketSize = 32;
        // Ping data repeats these letters, the same as Windows ping.
        public const string PingDataPattern = "abcdefghijklmnopqrstuvw";
        public const int DefaultTimeout = 2000;       // In milliseconds.
        public const int DefaultTTL = 128;
        public const int DefaultInterval = 2000;      // In milliseconds.

        // Default audio alert file paths.
        public const string DefaultAudioDownFilePath = @"%WINDIR%\Media\Windows Notify Email.wav";
        public const string DefaultAudioUpFilePath = @"%WINDIR%\Media\Windows Unlock.wav";

        // Key bindings.
        public const Key StatusHistoryKeyBinding = Key.F12;
        public const Key HelpKeyBinding = Key.F1;
    }
}
