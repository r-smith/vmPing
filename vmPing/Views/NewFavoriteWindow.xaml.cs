using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Interop;
using vmPing.Properties;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class NewFavoriteWindow : Window
    {
        private List<string> _hostList;
        private int _columnCount;

        public NewFavoriteWindow(List<string> hostList, int columnCount, string title = "")
        {
            InitializeComponent();

            _hostList = hostList;
            _columnCount = columnCount;

            MyHosts.Text = string.Join(Environment.NewLine, hostList).Trim();
            MyColumnCount.Text = columnCount.ToString();
            MyTitle.Text = title;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate column count.
            if (!int.TryParse(MyColumnCount.Text, out _columnCount) || _columnCount < 1 || _columnCount > 10)
            {
                DialogWindow errorWindow = DialogWindow.ErrorWindow("Please enter a valid number of columns (between 1 and 10).");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyColumnCount.Focus();
                MyColumnCount.SelectAll();
                return;
            }

            // Validate favorite name.
            if (Favorite.IsTitleInvalid(MyTitle.Text))
            {
                DialogWindow errorWindow = DialogWindow.ErrorWindow(Strings.NewFavorite_Error_InvalidName);
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyTitle.Focus();
                MyTitle.SelectAll();
                return;
            }

            // Split into host lines, keeping blank lines as empty probes. Ensure at least one host was entered.
            _hostList = HostFile.Parse(MyHosts.Text.Trim(), keepBlankLines: true);
            if (_hostList.All(x => string.IsNullOrWhiteSpace(x)))
            {
                DialogWindow errorWindow = DialogWindow.ErrorWindow("You have not entered any hosts. Provide at least one host for this favorite set.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyHosts.Focus();
                MyHosts.SelectAll();
                return;
            }

            // Warn if a favorite with this title already exists.
            if (Favorite.TitleExists(MyTitle.Text))
            {
                DialogWindow warningWindow = DialogWindow.WarningWindow(
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
                // Checks passed. Saving.
                SaveFavorite();
            }
        }

        private void SaveFavorite()
        {
            Favorite.Save(MyTitle.Text, _hostList, _columnCount);
            DialogResult = true;
        }

        private void MyColumnCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideMinimizeAndMaximizeButtons(this);
        }

        private void MyHosts_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            try
            {
                // Only the first file is used when several are dropped.
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                MyHosts.Text = string.Join(Environment.NewLine, HostFile.Read(paths[0]));
            }
            catch (Exception ex)
            {
                DialogWindow dialog = DialogWindow.ErrorWindow($"File could not be opened: {ex.Message}");
                dialog.Owner = this;
                dialog.ShowDialog();
            }
        }

        private void MyHosts_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
