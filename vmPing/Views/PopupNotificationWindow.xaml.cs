using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// PopupNotificationWindow is an alert that gets displayed in the lower right corner of the screen.
    /// It is triggered when a monitored hosts changes status between down or up.
    /// </summary>
    public partial class PopupNotificationWindow : Window
    {
        private DispatcherTimer _AutoDismissTimer;

        public PopupNotificationWindow(ObservableCollection<StatusChangeLog> statusChangeLog)
        {
            InitializeComponent();

            ICollectionView filteredChangeLog = new CollectionViewSource { Source = statusChangeLog }.View;
            filteredChangeLog.Filter = item =>
            {
                StatusChangeLog entry = item as StatusChangeLog;
                return entry.HasStatusBeenCleared == false && entry.Status != ProbeStatus.Start && entry.Status != ProbeStatus.Stop;
            };
            StatusHistoryList.ItemsSource = filteredChangeLog;

            ((INotifyCollectionChanged)StatusHistoryList.Items).CollectionChanged += PopupNotificationWindow_CollectionChanged;

            _AutoDismissTimer = new DispatcherTimer();
            _AutoDismissTimer.Tick += new EventHandler(AutoDismissTimer_Tick);
            if (ApplicationOptions.IsAutoDismissEnabled)
            {
                _AutoDismissTimer.Interval = TimeSpan.FromMilliseconds(ApplicationOptions.AutoDismissMilliseconds);
                _AutoDismissTimer.Start();
            }
        }

        private void AutoDismissTimer_Tick(object sender, EventArgs e)
        {
            if (ApplicationOptions.IsAutoDismissEnabled)
                Close();
        }

        private void PopupNotificationWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ApplicationOptions.IsAutoDismissEnabled)
            {
                _AutoDismissTimer.Stop();
                _AutoDismissTimer.Interval = TimeSpan.FromMilliseconds(ApplicationOptions.AutoDismissMilliseconds);
                _AutoDismissTimer.Start();
            }
            ScaleWindowSize();
            ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            if (StatusHistoryList.Items.Count > 0)
            {
                if (VisualTreeHelper.GetChild(StatusHistoryList, 0) is Decorator border)
                {
                    if (border.Child is ScrollViewer scroll) scroll.ScrollToEnd();
                }
            }
        }

        private void ScaleWindowSize()
        {
            // ScaleWindowSize resizes the window baased on the number of displayed items.
            switch (StatusHistoryList.Items.Count)
            {
                case 1:
                    Height = 95;
                    break;
                case 2:
                    Height = 110;
                    break;
                case 3:
                    Height = 126;
                    break;
                case 4:
                    Height = 147;
                    break;
                case 5:
                    Height = 172;
                    break;
            }
            PositionWindow(width: Width);
        }

        private void PositionWindow(double width)
        {
            // PositionWindow places the window in the lower right corner of the screen.
            Rect desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - width;
            Top = desktopWorkingArea.Bottom - Height;
        }

        private void Window_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
            {
                // Application is minimized. Restore window.
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
            if (Application.Current.MainWindow.Visibility != Visibility.Visible)
            {
                // Application is hidden to tray. Toggle visibility. Event will trigger on MainWindow to restore the window.
                Application.Current.MainWindow.Visibility = Visibility.Visible;
            }
            Application.Current.MainWindow.Focus();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (Probe.StatusWindow == null || Probe.StatusWindow.IsLoaded == false)
            {
                var wnd = new StatusHistoryWindow(Probe.StatusChangeLog);
                Probe.StatusWindow = wnd;
                wnd.Show();
            }
            else if (Probe.StatusWindow.IsLoaded)
            {
                Probe.StatusWindow.Focus();
            }

            Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionWindow(width: e.NewSize.Width);
        }
    }
}
