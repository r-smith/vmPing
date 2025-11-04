using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace vmPing.UI
{
    public partial class MultiInputWindow : Window
    {
        // Constants for hiding minimize and maximize buttons.
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button

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
            // Hide minimize and maximize buttons.
            IntPtr handle = new WindowInteropHelper(this).Handle;
            if (handle == null)
            {
                return;
            }

            SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        private void ShowError(string message)
        {
            var dialog = DialogWindow.ErrorWindow(message);
            dialog.Owner = this;
            dialog.ShowDialog();
        }
    }
}
