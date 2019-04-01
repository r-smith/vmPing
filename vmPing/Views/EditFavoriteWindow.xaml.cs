using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// EditFavoriteWindow is used to rename a favorite set.
    /// </summary>
    public partial class EditFavoriteWindow : Window
    {
        private string _OriginalFavoriteTitle;

        public EditFavoriteWindow(string favoriteTitle)
        {
            InitializeComponent();

            Header.Text = $"{Properties.Strings.EditFavorite_Rename} {favoriteTitle}";
            MyFavoriteTitle.Text = favoriteTitle;
            MyFavoriteTitle.SelectAll();

            _OriginalFavoriteTitle = favoriteTitle;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate favorite name.
            if (Favorite.IsTitleInvalid(MyFavoriteTitle.Text))
            {
                var errorWindow = DialogWindow.ErrorWindow(Properties.Strings.NewFavorite_Error_InvalidName);
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyFavoriteTitle.Focus();
                MyFavoriteTitle.SelectAll();
                return;
            }

            if (_OriginalFavoriteTitle.Equals(MyFavoriteTitle.Text))
            {
                // User picked the same name.  Close edit window without making any changes.
                DialogResult = true;
                return;
            }

            // Update favorite set with new name.
            Favorite.Rename(_OriginalFavoriteTitle, MyFavoriteTitle.Text);
            DialogResult = true;
        }
    }
}
