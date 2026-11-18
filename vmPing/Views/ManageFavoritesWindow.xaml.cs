using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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
        private string _originalContents = string.Empty;
        private Point _dragStartPoint;
        private Favorite _dragFavorite;
        private GroupItem _highlightedGroup;
        private bool _isRefreshingList;

        public ManageFavoritesWindow()
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);

            // Button access keys only work with Alt, so typing in the list to find a favorite can't press a button.
            AccessKeyManager.AddAccessKeyPressedHandler(this, (sender, e) =>
            {
                if ((Keyboard.Modifiers & ModifierKeys.Alt) == 0)
                {
                    e.Target = null;
                    e.Handled = true;
                }
            });

            // Select the favorite that's loaded in the main window, if any.
            Loaded += (sender, e) => RefreshFavoriteList((Owner as MainWindow)?.LoadedFavoriteTitle);
        }

        // Reloads the list and selects the favorite with the given title.
        private void RefreshFavoriteList(string titleToSelect)
        {
            List<Favorite> favorites = Favorite.GetAll();

            // Favorites are already sorted with grouped favorites first, so the groups show in that order.
            ListCollectionView view = new ListCollectionView(favorites);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Favorite.Group)));

            _isRefreshingList = true;
            Favorites.ItemsSource = view;
            Favorites.SelectedItem = favorites.FirstOrDefault(f => f.Title == titleToSelect);
            _isRefreshingList = false;

            FavoriteGroup.ItemsSource = favorites.Select(f => f.Group).Where(g => g.Length > 0).Distinct().ToList();

            EmptyListText.Visibility = favorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ShowFavorite(Favorites.SelectedItem as Favorite);
        }

        private void ShowFavorite(Favorite favorite)
        {
            _current = favorite;
            _isNew = false;
            if (favorite == null)
            {
                FillEditor(string.Empty, string.Empty, DefaultColumnCount, new List<string>());
            }
            else
            {
                FillEditor(favorite.Title, favorite.Group, favorite.ColumnCount, favorite.Hostnames);
            }
        }

        private void FillEditor(string title, string group, int columnCount, List<string> hostnames)
        {
            _isFillingEditor = true;
            FavoriteName.Text = title;
            FavoriteGroup.Text = group;
            FavoriteColumns.Text = columnCount.ToString();
            FavoriteHosts.Text = string.Join(Environment.NewLine, hostnames);
            _isFillingEditor = false;

            _originalContents = GetEditorContents();
            _hasUnsavedChanges = false;
            UpdateControls();
        }

        // Everything in the editor, to tell whether it was changed.
        private string GetEditorContents()
        {
            return string.Join("\n", FavoriteName.Text, FavoriteGroup.Text, FavoriteColumns.Text, FavoriteHosts.Text);
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

        // Compares with the original contents, so changing something back clears the unsaved changes.
        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFillingEditor)
            {
                return;
            }

            bool hasUnsavedChanges = GetEditorContents() != _originalContents;
            if (hasUnsavedChanges != _hasUnsavedChanges)
            {
                _hasUnsavedChanges = hasUnsavedChanges;
                UpdateControls();
            }
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

        private void Favorites_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(Favorites);
            _dragFavorite = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject)?.DataContext as Favorite;
        }

        private void Favorites_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragFavorite == null || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            // Only start once the mouse has moved a few pixels, so a click doesn't start a drag.
            Vector moved = e.GetPosition(Favorites) - _dragStartPoint;
            if (Math.Abs(moved.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(moved.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            Favorite favorite = _dragFavorite;
            _dragFavorite = null;

            DragLabel.Text = favorite.Title;
            MoveDragLabel();
            DragLabelPopup.IsOpen = true;
            try
            {
                DragDrop.DoDragDrop(Favorites, new DataObject(typeof(Favorite), favorite), DragDropEffects.Move);
            }
            finally
            {
                DragLabelPopup.IsOpen = false;
                HighlightGroup(null);
            }
        }

        // Raised throughout the drag, wherever the mouse is.
        private void Favorites_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            MoveDragLabel();

            // Show the normal pointer instead of the drag cursor.
            e.UseDefaultCursors = false;
            Mouse.SetCursor(Cursors.Arrow);
            e.Handled = true;
        }

        // Keeps the drag label below and to the right of the mouse pointer.
        // It must not be under the pointer, or it would get the drop instead of the list.
        private void MoveDragLabel()
        {
            System.Drawing.Point mouse = System.Windows.Forms.Control.MousePosition;
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            DragLabelPopup.HorizontalOffset = (mouse.X / dpi.DpiScaleX) + 16;
            DragLabelPopup.VerticalOffset = (mouse.Y / dpi.DpiScaleY) + 20;
        }

        private void Favorites_DragOver(object sender, DragEventArgs e)
        {
            Favorite favorite = e.Data.GetData(typeof(Favorite)) as Favorite;
            string group = GetDropGroup(e, out GroupItem groupItem);
            bool canDrop = favorite != null && group != favorite.Group;

            HighlightGroup(canDrop ? groupItem : null);
            e.Effects = canDrop ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void Favorites_DragLeave(object sender, DragEventArgs e)
        {
            HighlightGroup(null);
        }

        private void HighlightGroup(GroupItem groupItem)
        {
            if (groupItem == _highlightedGroup)
            {
                return;
            }

            _highlightedGroup?.ClearValue(BackgroundProperty);
            _highlightedGroup = groupItem;
            if (groupItem != null)
            {
                groupItem.SetResourceReference(BackgroundProperty, "Theme.Item.Hover.Background");
            }
        }

        private void Favorites_Drop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(typeof(Favorite)) is Favorite favorite))
            {
                return;
            }

            HighlightGroup(null);
            string group = GetDropGroup(e, out _);
            if (group != favorite.Group)
            {
                Favorite.SetGroup(favorite.Title, group);
                RefreshFavoriteList(favorite.Title);
            }
        }

        // The group under the mouse, and where it is in the list. Below the last favorite means no group.
        private string GetDropGroup(DragEventArgs e, out GroupItem groupItem)
        {
            DependencyObject element = e.OriginalSource as DependencyObject;
            groupItem = FindAncestor<GroupItem>(element);

            if (FindAncestor<ListBoxItem>(element)?.DataContext is Favorite favorite)
            {
                return favorite.Group;
            }
            if (groupItem?.Content is CollectionViewGroup group)
            {
                return group.Name as string ?? string.Empty;
            }

            // Below the list, so highlight the favorites without a group, if there are any.
            groupItem = null;
            foreach (CollectionViewGroup ungrouped in Favorites.Items.Groups)
            {
                if ((ungrouped.Name as string) == string.Empty)
                {
                    groupItem = Favorites.ItemContainerGenerator.ContainerFromItem(ungrouped) as GroupItem;
                }
            }
            return string.Empty;
        }

        private static T FindAncestor<T>(DependencyObject element) where T : DependencyObject
        {
            while (element != null && !(element is T))
            {
                element = element is Visual
                    ? VisualTreeHelper.GetParent(element)
                    : LogicalTreeHelper.GetParent(element);
            }
            return element as T;
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
            FillEditor(string.Empty, string.Empty, DefaultColumnCount, new List<string>());
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
            string group = GetGroupName(FavoriteGroup.Text);
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
                ShowError("You have not entered any hosts. Provide at least one host for this favorite.", FavoriteHosts);
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

            Favorite.Save(title, hostnames, columnCount, group);
            RefreshFavoriteList(title);
        }

        // The excluded group is skipped, so a group can be renamed to change its case.
        private string GetGroupName(string text, string excludedGroup = null)
        {
            return Favorite.MatchGroup(text, FavoriteGroup.Items.Cast<string>().Where(g => g != excludedGroup));
        }

        private void RenameGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is CollectionViewGroup group))
            {
                return;
            }

            string groupName = (string)group.Name;
            RenameGroupWindow renameWindow = new RenameGroupWindow(groupName)
            {
                Owner = this
            };
            if (renameWindow.ShowDialog() != true)
            {
                return;
            }

            // Renaming to an existing group moves the favorites into it.
            string newName = GetGroupName(renameWindow.NewName, excludedGroup: groupName);
            if (newName != groupName)
            {
                Favorite.RenameGroup(groupName, newName);
                RefreshFavoriteList(_current?.Title);
            }
        }

        private void RemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is CollectionViewGroup group))
            {
                return;
            }

            string groupName = (string)group.Name;
            DialogWindow warningWindow = DialogWindow.WarningWindow(
                message: $"Remove the group {groupName}? Its favorites won't be deleted.",
                confirmButtonText: Strings.DialogButton_Remove);
            warningWindow.Owner = this;
            if (warningWindow.ShowDialog() == true)
            {
                Favorite.RenameGroup(groupName, string.Empty);
                RefreshFavoriteList(_current?.Title);
            }
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
