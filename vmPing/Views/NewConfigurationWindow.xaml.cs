using System;
using System.IO;
using System.Windows;
using vmPing.Configuration;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class NewConfigurationWindow : Window
    {
        public NewConfigurationWindow()
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);

            FilePath.Text = ConfigFile.FilePath;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (PortableMode.IsChecked == true)
            {
                ConfigFile.FilePath = GetPortableFilePath();
            }

            DialogResult = true;
        }

        private void PortableMode_Click(object sender, RoutedEventArgs e)
        {
            FilePath.Text = PortableMode.IsChecked == true
                ? GetPortableFilePath()
                : ConfigFile.FilePath;
        }

        private string GetPortableFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vmPing.xml");
        }
    }
}
