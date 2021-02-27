using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
    /// <summary>
    /// NewFavoriteWindow provides an interface for creating a favorite.  A favorite is a
    /// collection of hosts that can be recalled later.
    /// </summary>
    public partial class NewFavoriteWindow : Window
    {
        private List<string> HostList;
        private int ColumnCount;

        public NewFavoriteWindow(List<string> hostList, int columnCount)
        {
            InitializeComponent();

            HostList = hostList;
            ColumnCount = columnCount;

            MyHosts.Text = string.Join(Environment.NewLine, hostList).Trim();
            MyColumnCount.Text = columnCount.ToString();

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate column count.
            if (int.TryParse(MyColumnCount.Text, out ColumnCount) == false || ColumnCount < 1 || ColumnCount > 10)
            {
                var errorWindow = DialogWindow.ErrorWindow("Please enter a valid number of columns (between 1 and 10).");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyColumnCount.Focus();
                MyColumnCount.SelectAll();
                return;
            }

            // Validate favorite name.
            if (Favorite.IsTitleInvalid(MyTitle.Text))
            {
                var errorWindow = DialogWindow.ErrorWindow(Strings.NewFavorite_Error_InvalidName);
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyTitle.Focus();
                MyTitle.SelectAll();
                return;
            }

            // Split host list to array, trim each item, then convert to list.
            HostList = MyHosts.Text.Trim().Split(new char[] { ',', '\n' }).Select(host => host.Trim()).ToList();

            // Check if favorite title already exists.
            if (Favorite.TitleExists(MyTitle.Text))
            {
                var warningWindow = DialogWindow.WarningWindow(
                    message: $"{MyTitle.Text} {Strings.NewFavorite_Warn_AlreadyExists}",
                    confirmButtonText: Strings.DialogButton_Overwrite);
                warningWindow.Owner = this;
                if (warningWindow.ShowDialog() == true)
                {
                    // User opted to overwrite existing favorite entry.
                    SaveFavorite();
                }
            }
            else
            {
                // Checks passed.  Saving.
                SaveFavorite();
            }
        }

        private void SaveFavorite()
        {
            Favorite.Save(MyTitle.Text, HostList, ColumnCount);
            DialogResult = true;
        }

        private void MyColumnCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9.-]+");
            if (regex.IsMatch(e.Text))
                e.Handled = true;
        }
    }
}
