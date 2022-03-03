using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace vmPing.Controls
{
    public class AutoScrollListBox : ListBox
    {
        private bool IsAutoScrollEnabled = true;

        public AutoScrollListBox()
        {
            Loaded += ListBox_Loaded;
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            // When ListBox is loaded, automatically scroll to the bottom of the list.
            // This is to handle dragging/dropping probes.
            if (Items.Count > 1) ScrollIntoView(Items[Items.Count - 1]);

            // Find ScrollViewer child in the ListBox and subscribe to its LostMouseCapture event.
            if (VisualTreeHelper.GetChildrenCount(this) > 0 && VisualTreeHelper.GetChild(this, 0) is Decorator border)
            {
                ScrollViewer scroll = border.Child as ScrollViewer;
                scroll.LostMouseCapture += Scroll_LostMouseCapture;
            }
        }

        private void Scroll_LostMouseCapture(object sender, MouseEventArgs e)
        {
            // User released mouse after clicking or dragging the scrollbar.
            // Check the position of the scrollbar - If it's scrolled to
            // the bottom, then automatically re-enable auto-scrolling.
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            IsAutoScrollEnabled = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // An item was added to the ListBox.
                if (VisualTreeHelper.GetChild(this, 0) is Decorator border)
                {
                    // Scroll to the bottom of the ListBox. If user is currently clicking the scrollbar, then do nothing.
                    ScrollViewer scroll = border.Child as ScrollViewer;
                    if (!scroll.IsMouseCaptureWithin)
                        scroll?.ScrollToEnd();
                }
            }
        }
    }
}
