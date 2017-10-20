using System;
using System.Diagnostics;
using System.Windows;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public static HelpWindow openWindow = null;

        public HelpWindow()
        {
            InitializeComponent();

            Version version = typeof(MainWindow).Assembly.GetName().Version;
            tbVersion.Text = $"Version: {version.Major}.{version.Minor}.{version.Build}";

            // Generate copyright text based on the current year.
            txtCopyright.Text = $"Copyright \u00a9 {DateTime.Now.Year.ToString()} Ryan Smith";

            // Set initial focus to scrollviewer.  That way you can scroll the help window with the keyboard
            // without having to first click in the window.
            mainScrollViewer.Focus();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            }
            catch
            {
            }
            finally
            {
                e.Handled = true;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            openWindow = this;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            openWindow = null;
        }
    }
}
