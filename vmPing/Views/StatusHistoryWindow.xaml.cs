using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using vmPing.Configuration;
using vmPing.Models;
using vmPing.Probes;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class StatusHistoryWindow : Window
    {
        private readonly ICollectionView _statusHistoryView;
        private static bool s_isWindowStateSet = false;
        private static double s_left;
        private static double s_top;
        private static double s_width;
        private static double s_height;
        private static WindowState s_windowState;

        public StatusHistoryWindow(ObservableCollection<StatusChange> statusChangeLog, bool restorePosition = false)
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);

            if (s_isWindowStateSet)
            {
                WindowState = s_windowState;
                Width = s_width;
                Height = s_height;
            }

            // Reopen at the last position when opened from a popup alert.
            // Otherwise, open over the main window.
            if (s_isWindowStateSet && restorePosition)
            {
                if (s_left < SystemParameters.VirtualScreenWidth)
                {
                    Left = s_left;
                }
                Top = s_top;
            }
            else
            {
                Util.CenterOverMainWindow(this);
            }
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;

            _statusHistoryView = CollectionViewSource.GetDefaultView(statusChangeLog);
            _statusHistoryView.Filter = AddressFilter;
            StatusHistory.ItemsSource = _statusHistoryView;

            ((INotifyCollectionChanged)StatusHistory.Items).CollectionChanged += StatusHistory_CollectionChanged;

            // When initially displaying the window, automatically scroll to the most recent entry.
            if (StatusHistory.Items.Count > 0)
            {
                StatusHistory.ScrollIntoView(StatusHistory.Items[StatusHistory.Items.Count - 1]);
            }
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
                        {
                            return;
                        }
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
            StatusChange entry = item as StatusChange;

            if (FilterStart.IsChecked == true && entry.Status == ProbeStatus.Start)
            {
                statusMatch = true;
            }
            if (FilterStop.IsChecked == true && entry.Status == ProbeStatus.Stop)
            {
                statusMatch = true;
            }
            if (FilterUp.IsChecked == true && entry.Status == ProbeStatus.Up)
            {
                statusMatch = true;
            }
            if (FilterDown.IsChecked == true && entry.Status == ProbeStatus.Down)
            {
                statusMatch = true;
            }
            if (FilterLatency.IsChecked == true
                && (entry.Status == ProbeStatus.LatencyHigh || entry.Status == ProbeStatus.LatencyNormal))
            {
                statusMatch = true;
            }

            if (statusMatch)
            {
                string filterText = FilterField.Text.ToUpper();
                if (!string.IsNullOrEmpty(entry.Alias) && entry.Alias.ToUpper().Contains(filterText))
                {
                    return true;
                }
                else if (!string.IsNullOrEmpty(entry.Hostname) && entry.Hostname.ToUpper().Contains(filterText))
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
            foreach (StatusChange s in _statusHistoryView)
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
                DialogWindow errorWindow = DialogWindow.ErrorWindow($"Failed to write to '{filePath}'. {ex.Message}");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
            Cursor = Cursors.Arrow;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Remember window position and state.
            s_isWindowStateSet = true;
            switch (WindowState)
            {
                case WindowState.Maximized:
                    s_windowState = WindowState;
                    break;
                case WindowState.Minimized:
                    s_windowState = WindowState.Normal;
                    s_left = Left;
                    s_top = Top;
                    s_width = ActualWidth;
                    s_height = ActualHeight;
                    break;
                case WindowState.Normal:
                    s_windowState = WindowState.Normal;
                    s_left = Left;
                    s_top = Top;
                    s_width = Width;
                    s_height = Height;
                    break;
                default:
                    break;
            }
        }
    }
}
