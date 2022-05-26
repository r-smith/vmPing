using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// StatusHistoryWindow displays the history of status changes (when a host changes between down or up).
    /// </summary>
    public partial class StatusHistoryWindow : Window
    {
        ICollectionView _statusHistoryView;
        private static bool IsWindowStateSet = false;
        private static double _Left;
        private static double _Top;
        private static double _Width;
        private static double _Height;
        private static WindowState _WindowState;

        public StatusHistoryWindow(ObservableCollection<StatusChangeLog> statusChangeLog)
        {
            InitializeComponent();

            if (IsWindowStateSet)
            {
                WindowState = _WindowState;
                if (_Left < SystemParameters.VirtualScreenWidth) Left = _Left;
                Top = _Top;
                Width = _Width;
                Height = _Height;
            }
            RefreshMaximizeRestoreButton();
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;

            _statusHistoryView = CollectionViewSource.GetDefaultView(statusChangeLog);
            _statusHistoryView.Filter = AddressFilter;
            StatusHistory.ItemsSource = _statusHistoryView;

            ((INotifyCollectionChanged)StatusHistory.Items).CollectionChanged += StatusHistory_CollectionChanged;

            // When initially displaying the window, automatically scroll to the most recent entry.
            if (StatusHistory.Items.Count > 0)
                StatusHistory.ScrollIntoView(StatusHistory.Items[StatusHistory.Items.Count - 1]);
        }

        private void StatusHistory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (StatusHistory.Items.Count > 0)
            {
                // Autoscroll to bottom only if no sorting has been set on any column (default)
                // or if timestamp column [1] is set to sort ascending.
                if (StatusHistory.Columns[1]?.SortDirection != ListSortDirection.Ascending)
                {
                    for (int i = 0; i < StatusHistory.Columns.Count; ++i)
                    {
                        if (StatusHistory.Columns[i].SortDirection != null)
                            return;
                    }
                }
                if (VisualTreeHelper.GetChild(StatusHistory, 0) is Decorator border)
                {
                    ScrollViewer scroll = border.Child as ScrollViewer;
                    scroll?.ScrollToEnd();
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Constants.StatusHistoryKeyBinding)
            {
                e.Handled = true;
                Close();
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            _statusHistoryView.Refresh();
        }

        private bool AddressFilter(object item)
        {
            bool statusMatch = false;
            var entry = item as StatusChangeLog;

            if (FilterStart.IsChecked == true && entry.Status == ProbeStatus.Start)
                statusMatch = true;
            if (FilterStop.IsChecked == true && entry.Status == ProbeStatus.Stop)
                statusMatch = true;
            if (FilterUp.IsChecked == true && entry.Status == ProbeStatus.Up)
                statusMatch = true;
            if (FilterDown.IsChecked == true && entry.Status == ProbeStatus.Down)
                statusMatch = true;

            if (statusMatch)
            {
                var filterText = FilterField.Text.ToUpper();
                if (!string.IsNullOrEmpty(entry.Alias) && entry.Alias.ToUpper().Contains(filterText))
                    return true;
                else if (!string.IsNullOrEmpty(entry.Hostname) && entry.Hostname.ToUpper().Contains(filterText))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            _statusHistoryView.Refresh();
        }

        private void FilterClear_Click(object sender, RoutedEventArgs e)
        {
            FilterField.Clear();
            _statusHistoryView.Refresh();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog exportDialog = new System.Windows.Forms.SaveFileDialog())
            {
                exportDialog.Title = "Export";
                exportDialog.RestoreDirectory = true;
                exportDialog.OverwritePrompt = true;
                exportDialog.AddExtension = true;
                exportDialog.AutoUpgradeEnabled = true;
                exportDialog.Filter = "CSV (Comma delimited)|*.csv|Text (Tab delimited)|*.txt|Text (Space delimited)|*.txt";
                exportDialog.FileName = "status-history.csv";
                if (exportDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && exportDialog.FileName != "")
                {
                    switch (exportDialog.FilterIndex)
                    {
                        // Note: FilterIndex is not zero-based. The first index is 1.
                        case 1:
                            // CSV.
                            WriteCollectionToFile(filePath: exportDialog.FileName, delimeter: ',');
                            break;
                        case 2:
                            // Tab Delimited.
                            WriteCollectionToFile(filePath: exportDialog.FileName, delimeter: '\t');
                            break;
                        case 3:
                            // Space delimited.
                            WriteCollectionToFile(filePath: exportDialog.FileName, delimeter: ' ');
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void WriteCollectionToFile(string filePath, char delimeter)
        {
            Cursor = Cursors.Wait;
            StringBuilder sb = new StringBuilder();
            foreach (StatusChangeLog s in _statusHistoryView)
            {
                sb.AppendLine($"{s.Timestamp}{delimeter}{s.Hostname}{delimeter}{s.Alias?.Replace(",", "")}{delimeter}{s.StatusAsString}");
            }

            try
            {
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.Write(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                DialogWindow.ErrorWindow($"Failed to write to '{filePath}'. {ex.Message}");
            }
            Cursor = Cursors.Arrow;
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            RefreshMaximizeRestoreButton();
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (WindowState == WindowState.Maximized)
            {
                maximizeButton.Visibility = Visibility.Collapsed;
                restoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                maximizeButton.Visibility = Visibility.Visible;
                restoreButton.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        protected virtual IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    GetMonitorInfo(monitor, ref monitorInfo);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }
            return IntPtr.Zero;
        }

        private const int WM_GETMINMAXINFO = 0x0024;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Remember window position and state.
            IsWindowStateSet = true;
            switch (WindowState)
            {
                case WindowState.Maximized:
                    _WindowState = WindowState;
                    break;
                case WindowState.Minimized:
                    _WindowState = WindowState.Normal;
                    _Left = Left;
                    _Top = Top;
                    _Width = ActualWidth;
                    _Height = ActualHeight;
                    break;
                case WindowState.Normal:
                    _WindowState = WindowState.Normal;
                    _Left = Left;
                    _Top = Top;
                    _Width = Width;
                    _Height = Height;
                    break;
                default:
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
