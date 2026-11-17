using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Interop;
using vmPing.Properties;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class ManageFavoritesWindow : Window
    {
        private const int DefaultColumnCount = 2;

        // The favorite being edited. Null when creating a new favorite or when nothing is selected.
        private Favorite _current;
        private bool _isNew;
        private bool _hasUnsavedChanges;
        private bool _isFillingEditor;
        private bool _isRefreshingList;

        public ManageFavoritesWindow()
        {
            InitializeComponent();

            // Select the favorite that's loaded in the main window, if any.
            Loaded += (sender, e) => RefreshFavoriteList((Owner as MainWindow)?.LoadedFavoriteTitle);
        }

        // Reloads the list and selects the favorite with the given title.
        private void RefreshFavoriteList(string titleToSelect)
        {
            List<Favorite> favorites = Favorite.GetAll();

            _isRefreshingList = true;
            Favorites.ItemsSource = favorites;
            Favorites.SelectedItem = favorites.FirstOrDefault(f => f.Title == titleToSelect);
            _isRefreshingList = false;

            EmptyListText.Visibility = favorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ShowFavorite(Favorites.SelectedItem as Favorite);
        }

        private void ShowFavorite(Favorite favorite)
        {
            _current = favorite;
            _isNew = false;
            if (favorite == null)
            {
                FillEditor(string.Empty, DefaultColumnCount, new List<string>());
            }
            else
            {
                FillEditor(favorite.Title, favorite.ColumnCount, favorite.Hostnames);
            }
        }

        private void FillEditor(string title, int columnCount, List<string> hostnames)
        {
            _isFillingEditor = true;
            FavoriteName.Text = title;
            FavoriteColumns.Text = columnCount.ToString();
            FavoriteHosts.Text = string.Join(Environment.NewLine, hostnames);
            _isFillingEditor = false;

            _hasUnsavedChanges = false;
            UpdateControls();
        }

        // While there are unsaved changes, the list and its buttons are disabled until the changes are saved or reverted.
        private void UpdateControls()
        {
            bool isEditing = _current != null || _isNew;
            bool hasSelection = _current != null && !_hasUnsavedChanges;

            Editor.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
            NoSelectionText.Visibility = isEditing ? Visibility.Collapsed : Visibility.Visible;
            UnsavedText.Visibility = _hasUnsavedChanges ? Visibility.Visible : Visibility.Hidden;

            SaveButton.IsEnabled = _hasUnsavedChanges;
            RevertButton.IsEnabled = _hasUnsavedChanges;
            Favorites.IsEnabled = !_hasUnsavedChanges;
            NewButton.IsEnabled = !_hasUnsavedChanges;
            DuplicateButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
            OpenButton.IsEnabled = hasSelection;
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFillingEditor || _hasUnsavedChanges)
            {
                return;
            }

            _hasUnsavedChanges = true;
            UpdateControls();
        }

        private void Favorites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isRefreshingList)
            {
                ShowFavorite(Favorites.SelectedItem as Favorite);
            }
        }

        private void Favorites_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Only open when an item was double-clicked, not the scroll bar or empty space.
            if (ItemsControl.ContainerFromElement(Favorites, (DependencyObject)e.OriginalSource) is ListBoxItem)
            {
                Open_Click(sender, e);
            }
        }

        private void Favorites_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Open_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                Delete_Click(sender, e);
                e.Handled = true;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (!NewButton.IsEnabled)
            {
                return;
            }

            _isRefreshingList = true;
            Favorites.SelectedItem = null;
            _isRefreshingList = false;

            _current = null;
            _isNew = true;
            FillEditor(string.Empty, DefaultColumnCount, new List<string>());
            FavoriteName.Focus();
        }

        private void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            if (!DuplicateButton.IsEnabled)
            {
                return;
            }

            string title = GetCopyTitle(_current.Title);
            Favorite.Save(title, _current.Hostnames, _current.ColumnCount, _current.Group);
            RefreshFavoriteList(title);

            FavoriteName.Focus();
            FavoriteName.SelectAll();
        }

        // Returns "Name (copy)", or "Name (copy 2)" and so on if that already exists.
        private static string GetCopyTitle(string title)
        {
            string copyTitle = $"{title} (copy)";
            for (int i = 2; Favorite.TitleExists(copyTitle); i++)
            {
                copyTitle = $"{title} (copy {i})";
            }
            return copyTitle;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!DeleteButton.IsEnabled)
            {
                return;
            }

            DialogWindow dialogWindow = new DialogWindow(
                DialogWindow.DialogIcon.Warning,
                Strings.DialogTitle_ConfirmDelete,
                $"{Strings.ManageFavorites_Warn_DeleteA} {_current.Title} {Strings.ManageFavorites_Warn_DeleteB}",
                Strings.DialogButton_Remove,
                true)
            {
                Owner = this
            };

            if (dialogWindow.ShowDialog() == true)
            {
                Favorite.Delete(_current.Title);
                RefreshFavoriteList(null);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!SaveButton.IsEnabled)
            {
                return;
            }

            string title = FavoriteName.Text.Trim();
            List<string> hostnames = HostFile.Parse(FavoriteHosts.Text);

            if (Favorite.IsTitleInvalid(title))
            {
                ShowError(Strings.NewFavorite_Error_InvalidName, FavoriteName);
                return;
            }
            if (!int.TryParse(FavoriteColumns.Text, out int columnCount) || columnCount < 1 || columnCount > 10)
            {
                ShowError("Please enter a valid number of columns (between 1 and 10).", FavoriteColumns);
                return;
            }
            if (hostnames.Count == 0)
            {
                ShowError("You have not entered any hosts. Provide at least one host for this favorite set.", FavoriteHosts);
                return;
            }

            // Warn before overwriting a different favorite with the same name.
            string originalTitle = _current?.Title;
            bool isRenamed = originalTitle != null && originalTitle != title;
            if ((originalTitle == null || isRenamed) && Favorite.TitleExists(title))
            {
                DialogWindow warningWindow = DialogWindow.WarningWindow(
                    message: $"{title} {Strings.NewFavorite_Warn_AlreadyExists}",
                    confirmButtonText: Strings.DialogButton_Overwrite);
                warningWindow.Owner = this;
                if (warningWindow.ShowDialog() != true)
                {
                    return;
                }
            }

            if (isRenamed)
            {
                Favorite.Rename(originalTitle: originalTitle, newTitle: title);

                // If the loaded favorite was renamed, update the main window title.
                if (Owner is MainWindow mainWindow && mainWindow.LoadedFavoriteTitle == originalTitle)
                {
                    mainWindow.LoadedFavoriteTitle = title;
                }
            }

            Favorite.Save(title, hostnames, columnCount);
            RefreshFavoriteList(title);
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            if (_isNew)
            {
                ShowFavorite(null);
            }
            else
            {
                ShowFavorite(_current);
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (!OpenButton.IsEnabled)
            {
                return;
            }

            if (Owner is MainWindow mainWindow)
            {
                mainWindow.LoadFavorite(_current.Title);
            }
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_hasUnsavedChanges)
            {
                return;
            }

            DialogWindow warningWindow = DialogWindow.WarningWindow(
                message: "You have unsaved changes. Close without saving?",
                confirmButtonText: "Discard");
            warningWindow.Owner = this;
            if (warningWindow.ShowDialog() != true)
            {
                e.Cancel = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Save_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                New_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ShowError(string message, TextBox textBox)
        {
            DialogWindow errorWindow = DialogWindow.ErrorWindow(message);
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            textBox.Focus();
            textBox.SelectAll();
        }

        private void FavoriteColumns_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, "[^0-9]"))
            {
                e.Handled = true;
            }
        }

        private void FavoriteHosts_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void FavoriteHosts_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            try
            {
                // Only the first file is used when several are dropped.
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                FavoriteHosts.Text = string.Join(Environment.NewLine, HostFile.Read(paths[0]));
            }
            catch (Exception ex)
            {
                DialogWindow dialog = DialogWindow.ErrorWindow($"File could not be opened: {ex.Message}");
                dialog.Owner = this;
                dialog.ShowDialog();
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideMinimizeAndMaximizeButtons(this);
        }
    }
}
