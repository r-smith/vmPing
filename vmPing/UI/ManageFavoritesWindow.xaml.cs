using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.UI
{
    public partial class ManageFavoritesWindow : Window
    {
        private Favorite _selectedFavorite = null;

        // Imports and constants for hiding minimize and maximize buttons.
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
            HideContentsSection();
        }

        private void HideContentsSection()
        {
            ContentsSection.Visibility = Visibility.Collapsed;
            Contents.ItemsSource = null;
            Contents.Items.Clear();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Favorites.SelectedIndex < 0)
            {
                return;
            }

            var dialogWindow = new DialogWindow(
                DialogWindow.DialogIcon.Warning,
                Strings.DialogTitle_ConfirmDelete,
                $"{Strings.ManageFavorites_Warn_DeleteA} {Favorites.SelectedItem} {Strings.ManageFavorites_Warn_DeleteB}",
                Strings.DialogButton_Remove,
                true)
            {
                Owner = this
            };

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
                Grid.SetColumnSpan(Favorites, 3);
                Favorites.BorderThickness = new Thickness(1.0);
                HideContentsSection();
                return;
            }

            _selectedFavorite = Favorite.Load(Favorites.SelectedItem.ToString());

            Grid.SetColumnSpan(Favorites, 1);
            ContentsSection.Visibility = Visibility.Visible;
            Contents.ItemsSource = _selectedFavorite.Hostnames;
            FavoriteTitle.Text = Favorites.SelectedItem.ToString();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Favorites.SelectedIndex < 0)
            {
                return;
            }

            var newFavoriteWindow = new NewFavoriteWindow(
                hostList: _selectedFavorite.Hostnames,
                columnCount: _selectedFavorite.ColumnCount,
                isEditExisting: true,
                title: Favorites.SelectedItem.ToString())
            {
                Owner = this
            };

            if (newFavoriteWindow.ShowDialog() == true)
            {
                RefreshFavoriteList();
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var newFavoriteWindow = new NewFavoriteWindow(new List<string>(), 2)
            {
                Owner = this
            };

            if (newFavoriteWindow.ShowDialog() == true)
            {
                RefreshFavoriteList();
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // Hide minimize and maximize buttons.
            var handle = new WindowInteropHelper(this).Handle;
            if (handle == null)
            {
                return;
            }

            SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
