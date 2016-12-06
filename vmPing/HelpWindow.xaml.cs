using System;
using System.Windows;

namespace vmPing
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
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
    }
}
