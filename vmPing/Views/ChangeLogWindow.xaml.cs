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
    /// Interaction logic for ChangeLogWindow.xaml
    /// </summary>
    public partial class ChangeLogWindow : Window
    {
        public ChangeLogWindow(ObservableCollection<StatusChangeLog> statusChangeLog)
        {
            InitializeComponent();
            
            lvStatusChangeLog.ItemsSource = statusChangeLog;
            ((INotifyCollectionChanged)lvStatusChangeLog.Items).CollectionChanged += PopupNotificationWindow_CollectionChanged;
        }

        private void PopupNotificationWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (lvStatusChangeLog.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(lvStatusChangeLog, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }
    }
}
