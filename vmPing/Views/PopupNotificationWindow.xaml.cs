using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using vmPing.Configuration;
using vmPing.Models;
using vmPing.Probes;

namespace vmPing.Views
{
    public partial class PopupNotificationWindow : Window
    {
        // The window is this much bigger than the card on each side, to leave room for its shadow.
        // Matches the margin around the card in the XAML.
        private const double ShadowSpace = 12;

        private DispatcherTimer _autoDismissTimer;

        public PopupNotificationWindow(ObservableCollection<StatusChange> statusChangeLog)
        {
            InitializeComponent();

            ICollectionView filteredChangeLog = new CollectionViewSource { Source = statusChangeLog }.View;
            filteredChangeLog.Filter = item =>
            {
                StatusChange entry = item as StatusChange;
                return !entry.HasStatusBeenCleared && entry.Status != ProbeStatus.Start && entry.Status != ProbeStatus.Stop;
            };
            StatusHistoryList.ItemsSource = filteredChangeLog;

            ((INotifyCollectionChanged)StatusHistoryList.Items).CollectionChanged += PopupNotificationWindow_CollectionChanged;

            _autoDismissTimer = new DispatcherTimer();
            _autoDismissTimer.Tick += new EventHandler(AutoDismissTimer_Tick);
            if (ApplicationOptions.IsAutoDismissEnabled)
            {
                _autoDismissTimer.Interval = TimeSpan.FromMilliseconds(ApplicationOptions.AutoDismissMilliseconds);
                _autoDismissTimer.Start();
            }
        }

        private void AutoDismissTimer_Tick(object sender, EventArgs e)
        {
            if (ApplicationOptions.IsAutoDismissEnabled)
            {
                Close();
            }
        }

        private void PopupNotificationWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ApplicationOptions.IsAutoDismissEnabled)
            {
                _autoDismissTimer.Stop();
                _autoDismissTimer.Interval = TimeSpan.FromMilliseconds(ApplicationOptions.AutoDismissMilliseconds);
                _autoDismissTimer.Start();
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
                    if (border.Child is ScrollViewer scroll)
                    {
                        scroll.ScrollToEnd();
                    }
                }
            }
        }

        private void ScaleWindowSize()
        {
            // ScaleWindowSize resizes the window baased on the number of displayed items.
            switch (StatusHistoryList.Items.Count)
            {
                case 1:
                    Height = 95 + 2 * ShadowSpace;
                    break;
                case 2:
                    Height = 110 + 2 * ShadowSpace;
                    break;
                case 3:
                    Height = 126 + 2 * ShadowSpace;
                    break;
                case 4:
                    Height = 147 + 2 * ShadowSpace;
                    break;
                case 5:
                    Height = 172 + 2 * ShadowSpace;
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
            if (Probe.StatusHistoryWindow == null || !Probe.StatusHistoryWindow.IsLoaded)
            {
                StatusHistoryWindow wnd = new StatusHistoryWindow(Probe.StatusChangeLog, restorePosition: true);
                Probe.StatusHistoryWindow = wnd;
                wnd.Show();
            }
            else if (Probe.StatusHistoryWindow.IsLoaded)
            {
                Probe.StatusHistoryWindow.Focus();
            }

            Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionWindow(width: e.NewSize.Width);
        }
    }
}
