using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public enum DialogIcon
        {
            Warning
        }

        public DialogWindow(DialogIcon dialogIcon, string dialogTitle, string dialogBody, string confirmationText)
        {
            InitializeComponent();

            MyTitle.Text = dialogTitle;
            Body.Text = dialogBody;
            OK.Content = confirmationText;

            switch (dialogIcon)
            {
                case DialogIcon.Warning:
                    MyIcon.Source = new BitmapImage(new Uri(@"/Resources/caution-40.png", UriKind.Relative));
                    break;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}