using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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
        private bool IsExisting = false;
        private string OriginalTitle;

        // Hide minimize and maximize buttons.
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button

        public NewFavoriteWindow(List<string> hostList, int columnCount, bool isEditExisting = false, string title = "")
        {
            InitializeComponent();

            HostList = hostList;
            ColumnCount = columnCount;
            IsExisting = isEditExisting;
            OriginalTitle = title;

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

            // Split MyHosts string to array, trim each item, then convert to list. Ensure at least one host was entered.
            HostList = MyHosts.Text.Trim().Split(new char[] { ',', '\n' }).Select(host => host.Trim()).ToList();
            if (HostList.All(x => string.IsNullOrWhiteSpace(x)))
            {
                var errorWindow = DialogWindow.ErrorWindow("You have not entered any hosts. Provide at least one host for this favorite set.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyHosts.Focus();
                MyHosts.SelectAll();
                return;
            }

            // If creating a new favorite: Check and display warning if title already exists.
            // If editing a favorite and title has changed: Check and display warning if title already exists.
            // If editing a favorite and title has not changed: Proceed with save. No warning displayed.
            if ( (!IsExisting && Favorite.TitleExists(MyTitle.Text))
                || (IsExisting && !string.Equals(OriginalTitle, MyTitle.Text) && Favorite.TitleExists(MyTitle.Text)) )
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
                // Checks passed. Saving.
                SaveFavorite();
            }
        }

        private void SaveFavorite()
        {
            // Check if renaming the title of an existing favorite set.
            if (IsExisting && !MyTitle.Text.Equals(OriginalTitle))
                Favorite.Rename(originalTitle: OriginalTitle, newTitle: MyTitle.Text);
            // Save.
            Favorite.Save(MyTitle.Text, HostList, ColumnCount);
            Application.Current.MainWindow.Title = MyTitle.Text + " - vmPing";
            DialogResult = true;
        }

        private void MyColumnCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9.-]+");
            if (regex.IsMatch(e.Text))
                e.Handled = true;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            HideMinimizeAndMaximizeButtons();
        }

        protected void HideMinimizeAndMaximizeButtons()
        {
            IntPtr _windowHandle = new WindowInteropHelper(this).Handle;
            if (_windowHandle == null)
            {
                return;
            }

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        private void MyHosts_Drop(object sender, DragEventArgs e)
        {
            const long MaxSizeInBytes = 10240;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    // Get path(s). We only work with the first element, in cases of multiple files.
                    string[] docPath = (string[])e.Data.GetData(DataFormats.FileDrop);

                    // Check file size.
                    long length = new FileInfo(docPath[0]).Length;
                    if (length > MaxSizeInBytes) throw new FileFormatException();

                    // Read file into a list of strings, so that each line can get checked.
                    var linesInFile = new List<string>(File.ReadAllLines(docPath[0]));

                    // Get a list of valid lines.
                    // Valid lines must not be empty and must being with a letter, digit, or '[' character (for IPv6).
                    var validLines = linesInFile
                        .Where(x => !string.IsNullOrWhiteSpace(x) &&
                                    (char.IsLetterOrDigit(x[0]) || x[0] == '['));

                    // Convert list to multiline string (with each line trimmed).
                    MyHosts.Text = string.Join(Environment.NewLine, validLines.Select(x => x.Trim()));
                }
                catch (FileFormatException)
                {
                    var dialog = DialogWindow.ErrorWindow(
                        $"The file is too large and cannot be opened. The maximum file size is {MaxSizeInBytes / 1024} kb.");
                    dialog.Owner = this;
                    dialog.ShowDialog();
                }
                catch
                {
                    var dialog = DialogWindow.ErrorWindow("File could not be opened. Make sure the file is a plain text file.");
                    dialog.Owner = this;
                    dialog.ShowDialog();
                }
            }
        }

        private void MyHosts_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
