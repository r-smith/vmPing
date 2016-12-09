using System.Windows;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for ManageFavoritesWindow.xaml
    /// </summary>
    public partial class ManageFavoritesWindow : Window
    {
        public ManageFavoritesWindow()
        {
            InitializeComponent();

            RefreshFavoriteList();
        }

        private void RefreshFavoriteList()
        {
            lbFavorites.ItemsSource = null;
            lbFavorites.Items.Clear();
            lbFavorites.ItemsSource = Favorite.GetFavoriteTitles();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lbFavorites.SelectedIndex < 0)
                return;

            var dialogWindow = new DialogWindow(
                DialogWindow.DialogIcon.Warning,
                "Confirm Delete",
                $"Are you sure you want to remove {lbFavorites.SelectedItem.ToString()} from your favorites?",
                "Remove");
            dialogWindow.Owner = this;
            if (dialogWindow.ShowDialog() == true)
            {
                Favorite.DeleteFavoriteEntry(lbFavorites.SelectedItem.ToString());
                RefreshFavoriteList();
            }
        }
    }
}
