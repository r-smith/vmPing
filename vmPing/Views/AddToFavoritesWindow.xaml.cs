using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for AddToFavoritesWindow.xaml
    /// </summary>
    public partial class AddToFavoritesWindow : Window
    {
        public string FavoriteTitle;

        public AddToFavoritesWindow()
        {
            InitializeComponent();

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

            FavoriteTitle = MyTitle.Text;

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
                    DialogResult = true;
            }
            else
                DialogResult = true;
        }
    }
}
