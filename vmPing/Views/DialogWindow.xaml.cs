using System.Windows;
using System.Windows.Media;
using vmPing.Properties;

namespace vmPing.Views
{
    /// <summary>
    /// DialogWindow is used to display popup dialog boxes.
    /// </summary>
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

            MyTitle.Text = title;
            Body.Text = body;
            OK.Content = confirmationText;

            if (!isCancelButtonVisible)
                Cancel.Visibility = Visibility.Collapsed;

            switch (icon)
            {
                case DialogIcon.Warning:
                    MyIcon.Source = (DrawingImage)Application.Current.Resources["icon.exclamation-triangle"];
                    break;
                case DialogIcon.Error:
                    MyIcon.Source = (DrawingImage)Application.Current.Resources["icon.exclamation-circle"];
                    break;
                case DialogIcon.Info:
                    MyIcon.Source = (DrawingImage)Application.Current.Resources["icon.info-circle"];
                    break;
                case DialogIcon.None:
                    MyIcon.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        public static DialogWindow ErrorWindow(string message)
        {
            System.Media.SystemSounds.Exclamation.Play();
            return new DialogWindow(
                icon: DialogIcon.Error,
                title: Strings.DialogTitle_Error,
                body: message,
                confirmationText: Strings.DialogButton_OK,
                isCancelButtonVisible: false);
        }

        public static DialogWindow WarningWindow(string message, string confirmButtonText)
        {
            System.Media.SystemSounds.Exclamation.Play();
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