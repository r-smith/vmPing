using System;
using System.Windows;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class UsageWindow : Window
    {
        public UsageWindow()
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);

            Version version = typeof(MainWindow).Assembly.GetName().Version;
            AppVersion.Text = $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
