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
            Favorites.ItemsSource = Favorite.GetTitles();

            ContentsSection.Visibility = Visibility.Collapsed;
            Contents.ItemsSource = null;
            Contents.Items.Clear();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Favorites.SelectedIndex < 0)
                return;

            var dialogWindow = new DialogWindow(
                DialogWindow.DialogIcon.Warning,
                Properties.Strings.DialogTitle_ConfirmDelete,
                $"{Properties.Strings.ManageFavorites_Warn_DeleteA} {Favorites.SelectedItem.ToString()} {Properties.Strings.ManageFavorites_Warn_DeleteB}",
                Properties.Strings.DialogButton_Remove,
                true);
            dialogWindow.Owner = this;
            if (dialogWindow.ShowDialog() == true)
            {
                Favorite.Delete(Favorites.SelectedItem.ToString());
                RefreshFavoriteList();
            }
        }

        private void Favorites_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Favorites.SelectedIndex < 0)
            {
                ContentsSection.Visibility = Visibility.Collapsed;
                return;
            }

            var favorite = Favorite.GetContents(Favorites.SelectedItem.ToString());
            ContentsSection.Visibility = Visibility.Visible;
            Contents.ItemsSource = null;
            Contents.Items.Clear();
            Contents.ItemsSource = favorite.Hostnames;
        }

        private void CloseContents_Click(object sender, RoutedEventArgs e)
        {
            ContentsSection.Visibility = Visibility.Collapsed;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Favorites.SelectedIndex < 0)
                return;

            var editFavoriteWindow = new EditFavoriteWindow(Favorites.SelectedItem.ToString());
            editFavoriteWindow.Owner = this;

            if (editFavoriteWindow.ShowDialog() == true)
            {
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
