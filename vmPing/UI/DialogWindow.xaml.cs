using System.Windows;
using System.Windows.Media;
using vmPing.Properties;

namespace vmPing.UI
{
    public partial class DialogWindow : Window
    {
        public enum DialogIcon
        {
            Warning,
            Error,
            Info,
            None
        }

        public DialogWindow(DialogIcon icon, string title, string body, string confirmationText, bool isCancelButtonVisible)
        {
            InitializeComponent();

            MessageHeader.Text = title;
            MessageBody.Text = body;
            OK.Content = confirmationText;
            Cancel.Visibility = isCancelButtonVisible ? Visibility.Visible : Visibility.Collapsed;
            SetIcon(icon);
        }

        private void SetIcon(DialogIcon icon)
        {
            if (icon == DialogIcon.None)
            {
                MessageImage.Visibility = Visibility.Collapsed;
                return;
            }

            string resourceKey = null;

            switch (icon)
            {
                case DialogIcon.Warning:
                    resourceKey = "icon.exclamation-triangle";
                    break;
                case DialogIcon.Error:
                    resourceKey = "icon.exclamation-circle";
                    break;
                case DialogIcon.Info:
                    resourceKey = "icon.info-circle";
                    break;
            }

            if (resourceKey != null)
            {
                MessageImage.Source = TryGetDrawingImage(resourceKey);
            }
        }

        private DrawingImage TryGetDrawingImage(string key)
        {
            return Application.Current.TryFindResource(key) as DrawingImage;
        }

        private static void PlayExclamationSound()
        {
            System.Media.SystemSounds.Exclamation.Play();
        }

        public static DialogWindow ErrorWindow(string message)
        {
            PlayExclamationSound();
            return new DialogWindow(
                icon: DialogIcon.Error,
                title: Strings.DialogTitle_Error,
                body: message,
                confirmationText: Strings.DialogButton_OK,
                isCancelButtonVisible: false);
        }

        public static DialogWindow WarningWindow(string message, string confirmButtonText)
        {
            PlayExclamationSound();
            return new DialogWindow(
                icon: DialogIcon.Warning,
                title: Strings.DialogTitle_Warning,
                body: message,
                confirmationText: confirmButtonText,
                isCancelButtonVisible: true);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}