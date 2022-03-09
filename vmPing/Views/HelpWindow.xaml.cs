using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public static HelpWindow _OpenWindow = null;

        public HelpWindow()
        {
            InitializeComponent();
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;

            Version version = typeof(MainWindow).Assembly.GetName().Version;
            Version.Inlines.Clear();
            Version.Inlines.Add(new Run($"Version: {version.Major}.{version.Minor}.{version.Build}"));

            // Generate copyright text based on the current year.
            //Copyright.Text = $"Copyright \u00a9 {DateTime.Now.Year.ToString()} Ryan Smith";
            Copyright.Inlines.Clear();
            Copyright.Inlines.Add(new Run($"Copyright \u00a9 {DateTime.Now.Year.ToString()} Ryan Smith"));

            // Set initial focus to scrollviewer.  That way you can scroll the help window with the keyboard
            // without having to first click in the window.
            MainDocument.Focus();

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            }
            catch
            {
                // TODO
            }
            finally
            {
                e.Handled = true;
            }
        }

        private void Intro_Selected(object sender, RoutedEventArgs e)
        {
            Intro.BringIntoView();
        }

        private void BasicUsage_Selected(object sender, RoutedEventArgs e)
        {
            BasicUsage.BringIntoView();
        }

        private void ExtraFeatures_Selected(object sender, RoutedEventArgs e)
        {
            ExtraFeatures.BringIntoView();
        }

        private void Options_Selected(object sender, RoutedEventArgs e)
        {
            Options.BringIntoView();
        }

        private void CommandLineUsage_Selected(object sender, RoutedEventArgs e)
        {
            CommandLineUsage.BringIntoView();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _OpenWindow = this;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _OpenWindow = null;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Constants.HelpKeyBinding)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
