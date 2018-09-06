using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for EditFavoriteWindow.xaml
    /// </summary>
    public partial class EditFavoriteWindow : Window
    {
        private string _OriginalFavoriteTitle;

        public EditFavoriteWindow(string favoriteTitle)
        {
            InitializeComponent();

            Header.Text = "Rename Favorite - " + favoriteTitle;
            MyFavoriteTitle.Text = favoriteTitle;
            MyFavoriteTitle.SelectAll();

            _OriginalFavoriteTitle = favoriteTitle;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MyFavoriteTitle.Text))
            {
                var dialogWindow = new DialogWindow(
                    DialogWindow.DialogIcon.Warning,
                    "Error",
                    $"Please enter a valid name for this favorite set.",
                    "OK",
                    false);
                dialogWindow.Owner = this;
                dialogWindow.ShowDialog();
                MyFavoriteTitle.Focus();
                MyFavoriteTitle.SelectAll();
                return;
            }

            if (_OriginalFavoriteTitle.Equals(MyFavoriteTitle.Text))
            {
                DialogResult = true;
                return;
            }

            Favorite.RenameFavoriteSet(_OriginalFavoriteTitle, MyFavoriteTitle.Text);

            DialogResult = true;
        }
    }
}
