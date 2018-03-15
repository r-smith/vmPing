using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for EmailAlertWindow.xaml
    /// </summary>
    public partial class EmailAlertWindow : Window
    {
        public EmailAlertWindow()
        {
            InitializeComponent();
            
            txtEmailServer.Text = ApplicationOptions.EmailServer;
            txtEmailUser.Text = ApplicationOptions.EmailUser;
            txtEmailPassword.Password = ApplicationOptions.EmailPassword;
            txtEmailPort.Text = ApplicationOptions.EmailPort;
            txtEmailRecipient.Text = ApplicationOptions.EmailRecipient;
            txtEmailFromAddress.Text = ApplicationOptions.EmailFromAddress;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (txtEmailServer.Text.Length == 0)
            {
                MessageBox.Show(
                    "An email server address is required.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtEmailServer.Focus();
                return;
            }
            else if (txtEmailRecipient.Text.Length == 0)
            {
                MessageBox.Show(
                    "A recipient email address is required.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtEmailRecipient.Focus();
                return;
            }
            else if (txtEmailFromAddress.Text.Length == 0)
            {
                MessageBox.Show(
                    "A message FROM address is required.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtEmailFromAddress.Focus();
                return;
            }

            ApplicationOptions.EmailAlert = true;
            ApplicationOptions.EmailServer = txtEmailServer.Text;
            ApplicationOptions.EmailUser = txtEmailUser.Text;
            ApplicationOptions.EmailPassword = txtEmailPassword.Password;
            ApplicationOptions.EmailPort = txtEmailPort.Text;
            ApplicationOptions.EmailRecipient = txtEmailRecipient.Text;
            ApplicationOptions.EmailFromAddress = txtEmailFromAddress.Text;

            DialogResult = true;
        }


        private void txtEmailRecipient_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtEmailFromAddress.Text.Length == 0 && txtEmailRecipient.Text.IndexOf('@') >= 0)
                txtEmailFromAddress.Text = "vmPing" + txtEmailRecipient.Text.Substring(txtEmailRecipient.Text.IndexOf('@'));
        }
    }
}
