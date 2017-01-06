using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for PopupNotificationWindow.xaml
    /// </summary>
    public partial class PopupNotificationWindow : Window
    {
        public PopupNotificationWindow(ObservableCollection<StatusChangeLog> statusChangeLog)
        {
            InitializeComponent();

            PositionWindow();

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

            switch (lvStatusChangeLog.Items.Count)
            {
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PositionWindow()
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.2));
            anim.Completed += (s, _) => this.Close();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}
