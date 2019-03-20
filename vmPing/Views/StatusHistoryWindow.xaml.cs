using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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

            StatusHistoryList.ItemsSource = statusChangeLog;

            ((INotifyCollectionChanged)StatusHistoryList.Items).CollectionChanged += PopupNotificationWindow_CollectionChanged;

            // When initially displaying the window, automatically scroll to the most recent entry.
            if (StatusHistoryList.Items.Count > 0)
                StatusHistoryList.ScrollIntoView(StatusHistoryList.Items[StatusHistoryList.Items.Count - 1]);
        }

        private void PopupNotificationWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (StatusHistoryList.Items.Count > 0)
            {
                if (VisualTreeHelper.GetChild(StatusHistoryList, 0) is Decorator border)
                {
                    if (border.Child is ScrollViewer scroll) scroll.ScrollToEnd();
                }
            }
        }
    }
}
