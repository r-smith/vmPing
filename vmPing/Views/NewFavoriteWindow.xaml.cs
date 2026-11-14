using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        private readonly bool _isExisting = false;
        private readonly string _originalTitle;

        public NewFavoriteWindow(List<string> hostList, int columnCount, bool isEditExisting = false, string title = "")
        {
            InitializeComponent();

            _hostList = hostList;
            _columnCount = columnCount;
            _isExisting = isEditExisting;
            _originalTitle = title;

            MyHosts.Text = string.Join(Environment.NewLine, hostList).Trim();
            MyColumnCount.Text = columnCount.ToString();
            MyTitle.Text = title;

            if (isEditExisting)
            {
                Title = "Edit Favorite";
                Header.Text = "Edit an existing favorite";
                HeaderIcon.Source = (DrawingImage)Application.Current.Resources["icon.edit"];
            }

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

            // Split MyHosts string to array, trim each item, then convert to list. Ensure at least one host was entered.
            _hostList = MyHosts.Text.Trim().Split(new char[] { ',', '\n' }).Select(host => host.Trim()).ToList();
            if (_hostList.All(x => string.IsNullOrWhiteSpace(x)))
            {
                DialogWindow errorWindow = DialogWindow.ErrorWindow("You have not entered any hosts. Provide at least one host for this favorite set.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyHosts.Focus();
                MyHosts.SelectAll();
                return;
            }

            // If creating a new favorite: Check and display warning if title already exists.
            // If editing a favorite and title has changed: Check and display warning if title already exists.
            // If editing a favorite and title has not changed: Proceed with save. No warning displayed.
            if ( (!_isExisting && Favorite.TitleExists(MyTitle.Text))
                || (_isExisting && !string.Equals(_originalTitle, MyTitle.Text) && Favorite.TitleExists(MyTitle.Text)) )
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
            // Check if renaming the title of an existing favorite set.
            if (_isExisting && !MyTitle.Text.Equals(_originalTitle))
            {
                Favorite.Rename(originalTitle: _originalTitle, newTitle: MyTitle.Text);
            }
            // Save.
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
