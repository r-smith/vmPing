using System;
using System.Windows;
using System.Windows.Controls;
using vmPing.Configuration;
using vmPing.Probes;

namespace vmPing.Views
{
    public partial class IsolatedPingWindow : Window
    {
        private int _selectionStart = 0;
        private int _selectionLength = 0;

        public IsolatedPingWindow(Probe pingItem)
        {
            InitializeComponent();
            Util.CenterOverMainWindow(this);
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
            pingItem.IsolatedWindow = this;
            DataContext = pingItem;
        }

        private void History_TextChanged(object sender, TextChangedEventArgs e)
        {
            History.SelectionStart = _selectionStart;
            History.SelectionLength = _selectionLength;
            if (!History.IsMouseCaptureWithin && History.SelectionLength == 0)
            {
                History.ScrollToEnd();
            }
        }

        private void History_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _selectionStart = History.SelectionStart;
            _selectionLength = History.SelectionLength;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            (DataContext as Probe).IsolatedWindow = null;
            DataContext = null;
        }
    }
}
