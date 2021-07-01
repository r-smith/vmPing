using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// StatusHistoryWindow displays the history of status changes (when a host changes between down or up).
    /// </summary>
    public partial class StatusHistoryWindow : Window
    {
        public StatusHistoryWindow(ObservableCollection<StatusChangeLog> statusChangeLog)
        {
            InitializeComponent();

            StatusHistory.ItemsSource = statusChangeLog;

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
    }
}
