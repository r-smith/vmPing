using System;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Media;

namespace vmPing.Classes
{
    public class PingItem : INotifyPropertyChanged
    {
        public static int NumberOfActivePings;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Hostname { get; set; }
        public BackgroundWorker PingBackgroundWorker { get; set; }
        public AutoResetEvent PingResetEvent { get; set; }
        public PingStatistics Statistics { get; set; }
        public PingReply Reply { get; set; }
        public Ping Sender { get; set; }
        public bool IsHostUp { get; set; }

        private bool isActive = false;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (value == true)
                    ++NumberOfActivePings;
                else
                    --NumberOfActivePings;
                NotifyPropertyChanged("NumberOfActivePings");

                if (value != isActive)
                {
                    isActive = value;
                    NotifyPropertyChanged("IsActive");
                }
            }
        }

        private Brush brush_OutputBackground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_INACTIVE);
        public Brush Brush_OutputBackground
        {
            get { return brush_OutputBackground; }
            set
            {
                if (value != brush_OutputBackground)
                {
                    brush_OutputBackground = value;
                    NotifyPropertyChanged("Brush_OutputBackground");
                }
            }
        }

        private Brush brush_OutputForeground = (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_INACTIVE);
        public Brush Brush_OutputForeground
        {
            get { return brush_OutputForeground; }
            set
            {
                if (value != brush_OutputForeground)
                {
                    brush_OutputForeground = value;
                    NotifyPropertyChanged("Brush_OutputForeground");
                }
            }
        }

        private Brush brush_StatsForeground = (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_INACTIVE);
        public Brush Brush_StatsForeground
        {
            get { return brush_StatsForeground; }
            set
            {
                if (value != brush_StatsForeground)
                {
                    brush_StatsForeground = value;
                    NotifyPropertyChanged("Brush_StatsForeground");
                }
            }
        }

        private string pingStatisticsText;
        public string PingStatisticsText
        {
            get { return pingStatisticsText; }
            set
            {
                if (value != pingStatisticsText)
                {
                    pingStatisticsText = value;
                    NotifyPropertyChanged("PingStatisticsText");
                }
            }
        }

        private string pingOuput;
        public string PingOutput
        {
            get { return pingOuput; }
            set
            {
                if (value != pingOuput)
                {
                    string[] lines = value.Split('\n').ToArray();
                    if (lines.GetUpperBound(0) > 2000)
                        value = string.Join("\n", lines, 1, lines.GetUpperBound(0));
                    else
                        value = string.Join("\n", lines);
                    pingOuput = value;
                    NotifyPropertyChanged("PingOutput");
                }
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
