using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for MultiInputWindow.xaml
    /// </summary>
    public partial class MultiInputWindow : Window
    {
        // Imports and constants for hiding minimize and maximize buttons.
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
                // Split addresses text to array, trim each item, then convert to list. Ensure at least one host was entered.
                List<string> addressList = MyAddresses.Text.Trim().Split(new char[] { ',', '\n' }).Select(host => host.Trim()).ToList();
                if (addressList.All(x => string.IsNullOrWhiteSpace(x)))
                {
                    // Nothing but whitespace was entered. Should an error display? Currently, an empty list is returned.
                }
                return addressList;
            }
        }

        public MultiInputWindow(List<string> addresses = null)
        {
            InitializeComponent();

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            // Pre-populate textbox if any addresses were supplied.
            if (addresses != null
                && addresses.Count > 0
                && !addresses.All(x => string.IsNullOrWhiteSpace(x))
                )
            {
                // Convert list to multiline string and select all text.
                MyAddresses.Text = string.Join(Environment.NewLine, addresses);
                MyAddresses.SelectAll();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void MyAddresses_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void MyAddresses_Drop(object sender, DragEventArgs e)
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
                    MyAddresses.Text = string.Join(Environment.NewLine, validLines.Select(x => x.Trim()));
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

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // Hide minimize and maximize buttons.
            IntPtr _windowHandle = new WindowInteropHelper(this).Handle;
            if (_windowHandle == null)
            {
                return;
            }

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }
    }
}
