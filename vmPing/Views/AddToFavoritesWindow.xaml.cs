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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            FavoriteTitle = txtTitle.Text;

            if (Favorite.DoesTitleExist(txtTitle.Text))
            {
                var errorWindow = new ErrorWindow(
                    ErrorWindow.DialogIcon.Warning,
                    "Warning",
                    $"{txtTitle.Text} already exists.  Would you like to overwrite?",
                    "Overwrite");
                errorWindow.Owner = this;
                if (errorWindow.ShowDialog() == true)
                    DialogResult = true;
            }
            else
                DialogResult = true;
        }
    }
}
