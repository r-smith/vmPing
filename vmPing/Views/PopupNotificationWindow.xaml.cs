using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// PopupNotificationWindow is an alert that gets displayed in the lower right corner of the screen.
    /// It is triggered when a monitored hosts changes status between down or up.
    /// </summary>
    public partial class PopupNotificationWindow : Window
    {
        public PopupNotificationWindow(ObservableCollection<StatusChangeLog> statusChangeLog)
        {
            InitializeComponent();

            PositionWindow();

            ICollectionView filteredChangeLog = new CollectionViewSource { Source = statusChangeLog }.View;
            filteredChangeLog.Filter = p => (p as StatusChangeLog).HasStatusBeenCleared == false;
            StatusHistoryList.ItemsSource = filteredChangeLog;

            ((INotifyCollectionChanged)StatusHistoryList.Items).CollectionChanged += PopupNotificationWindow_CollectionChanged;
        }

        private void PopupNotificationWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
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
                    PositionWindow();
                    break;
                case 2:
                    Height = 110;
                    PositionWindow();
                    break;
                case 3:
                    Height = 126;
                    PositionWindow();
                    break;
                case 4:
                    Height = 147;
                    PositionWindow();
                    break;
                case 5:
                    Height = 172;
                    PositionWindow();
                    break;
            }
        }

        private void PositionWindow()
        {
            // PositionWindow places the window in the lower right corner of the screen.
            Rect desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            anim.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, anim);
        }

        private void Window_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                Application.Current.MainWindow.WindowState = WindowState.Normal;
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
    }
}
