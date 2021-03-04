using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for MultiInputWindow.xaml
    /// </summary>
    public partial class MultiInputWindow : Window
    {
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
    }
}
