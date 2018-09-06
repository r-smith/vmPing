using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for ManageFavoritesWindow.xaml
    /// </summary>
    public partial class ManageFavoritesWindow : Window
    {
        public static ManageFavoritesWindow openWindow = null;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button


        public ManageFavoritesWindow()
        {
            InitializeComponent();

            RefreshFavoriteList();
        }

        private void RefreshFavoriteList()
        {
            Favorites.ItemsSource = null;
            Favorites.Items.Clear();
            Favorites.ItemsSource = Favorite.GetFavoriteTitles();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Favorites.SelectedIndex < 0)
                return;

            var dialogWindow = new DialogWindow(
                DialogWindow.DialogIcon.Warning,
                "Confirm Delete",
                $"Are you sure you want to remove {Favorites.SelectedItem.ToString()} from your favorites?",
                "Remove",
                true);
            dialogWindow.Owner = this;
            if (dialogWindow.ShowDialog() == true)
            {
                Favorite.DeleteFavoriteEntry(Favorites.SelectedItem.ToString());
                RefreshFavoriteList();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            openWindow = this;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            openWindow = null;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            HideMinimizeAndMaximizeButtons();
        }

        protected void HideMinimizeAndMaximizeButtons()
        {
            IntPtr _windowHandle = new WindowInteropHelper(this).Handle;
            if (_windowHandle == null)
            {
                return;
            }

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }
    }
}
