using System;
using System.Windows;
using vmPing.Configuration;
using vmPing.Probes;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class IsolatedPingWindow : Window
    {
        public IsolatedPingWindow(Probe pingItem)
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);
            Util.CenterOverMainWindow(this);
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
            pingItem.IsolatedWindow = this;
            DataContext = pingItem;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            (DataContext as Probe).IsolatedWindow = null;
            DataContext = null;
        }
    }
}
