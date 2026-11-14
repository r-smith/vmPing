using System;
using System.Windows;

namespace vmPing.Views
{
    public partial class UsageWindow : Window
    {
        public UsageWindow()
        {
            InitializeComponent();

            Version version = typeof(MainWindow).Assembly.GetName().Version;
            AppVersion.Text = $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
