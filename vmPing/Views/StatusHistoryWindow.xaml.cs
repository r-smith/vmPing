using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// StatusHistoryWindow displays the history of status changes (when a host changes between down or up).
    /// </summary>
    public partial class StatusHistoryWindow : Window
    {
        ICollectionView _statusHistoryView;

        public StatusHistoryWindow(ObservableCollection<StatusChangeLog> statusChangeLog)
        {
            InitializeComponent();

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
                StatusHistory.ScrollIntoView(StatusHistory.Items[StatusHistory.Items.Count - 1]);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
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
                var filterText = AddressFilterField.Text.ToUpper();
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
    }
}
