using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for AddToFavoritesWindow.xaml
    /// </summary>
    public partial class NewFavoriteWindow : Window
    {
        private List<string> HostList;
        private int ColumnCount;

        public NewFavoriteWindow(List<string> hostList, int columnCount)
        {
            InitializeComponent();

            Contents.ItemsSource = hostList;

            HostList = hostList;
            ColumnCount = columnCount;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MyTitle.Text))
            {
                var dialogWindow = new DialogWindow(
                    DialogWindow.DialogIcon.Warning,
                    "Error",
                    $"Please enter a valid name for this favorite set.",
                    "OK",
                    false);
                dialogWindow.Owner = this;
                dialogWindow.ShowDialog();
                MyTitle.Focus();
                MyTitle.SelectAll();
                return;
            }

            if (Favorite.DoesTitleExist(MyTitle.Text))
            {
                var dialogWindow = new DialogWindow(
                    DialogWindow.DialogIcon.Warning,
                    "Warning",
                    $"{MyTitle.Text} already exists.  Would you like to overwrite?",
                    "Overwrite",
                    true);
                dialogWindow.Owner = this;
                if (dialogWindow.ShowDialog() == true)
                {
                    Favorite.SaveFavoriteSet(MyTitle.Text, HostList, ColumnCount);
                    DialogResult = true;
                }
            }
            else
            {
                Favorite.SaveFavoriteSet(MyTitle.Text, HostList, ColumnCount);
                DialogResult = true;
            }
        }
    }
}
