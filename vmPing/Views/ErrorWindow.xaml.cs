using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public enum DialogIcon
        {
            Warning
        }

        public ErrorWindow(DialogIcon dialogIcon, string dialogTitle, string dialogBody, string confirmationText)
        {
            InitializeComponent();

            tbDialogTitle.Text = dialogTitle;
            tbDialogBody.Text = dialogBody;
            btnSave.Content = confirmationText;

            switch (dialogIcon)
            {
                case DialogIcon.Warning:
                    imgDialogIcon.Source = new BitmapImage(new Uri(@"/Resources/caution-40.png", UriKind.Relative));
                    break;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}