using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using vmPing.Interop;

namespace vmPing.Views
{
    public partial class MultiInputWindow : Window
    {
        public List<string> Addresses
        {
            get
            {
                // Split and trim multi-address text. Split occurs on both newlines and commas.
                // Return as a list with empty entries removed.
                return MultiAddress.Text
                    .Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
            }
        }

        public MultiInputWindow(List<string> addresses = null)
        {
            InitializeComponent();

            // Set initial keyboard focus to text box.
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            // Pre-populate textbox if any addresses were supplied.
            if (addresses != null
                && addresses.Count > 0
                && !addresses.All(x => string.IsNullOrWhiteSpace(x))
                )
            {
                // Convert list to multiline string and select all text.
                MultiAddress.Text = string.Join(Environment.NewLine, addresses);
                MultiAddress.SelectAll();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void MultiAddress_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void MultiAddress_Drop(object sender, DragEventArgs e)
        {
            const long MaxSizeInBytes = 10240;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            try
            {
                // Get paths of dropped files.
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (paths == null || paths.Length == 0)
                {
                    return;
                }

                // Only 1 file drop is supported.
                if (paths.Length > 1)
                {
                    ShowError("Please drop only one file at a time.");
                    return;
                }

                // Check filesize.
                var fileInfo = new FileInfo(paths[0]);
                if (fileInfo.Length > MaxSizeInBytes)
                {
                    ShowError($"\"{paths[0]}\" is too large. The maximum file size is {MaxSizeInBytes / 1024} KB.");
                    return;
                }

                // Extract valid lines: valid lines are non-empty and begin with a letter, number, or the `[` character (for IPv6).
                var validLines = File.ReadLines(paths[0])
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line) &&
                        (char.IsLetterOrDigit(line[0]) || line[0] == '['));

                // Convert list to multiline string.
                MultiAddress.Text = string.Join(Environment.NewLine, validLines);
            }
            catch (Exception ex) 
            {
                ShowError($"File could not be opened: {ex.Message}");
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideMinimizeAndMaximizeButtons(this);
        }

        private void ShowError(string message)
        {
            var dialog = DialogWindow.ErrorWindow(message);
            dialog.Owner = this;
            dialog.ShowDialog();
        }
    }
}
