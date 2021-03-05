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
        // Hide minimize and maximize buttons.
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

        public MultiInputWindow()
        {
            InitializeComponent();

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] docPath = (string[])e.Data.GetData(DataFormats.FileDrop);
                    MyAddresses.Text = File.ReadAllText(docPath[0]);
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
    }
}
