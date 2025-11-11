using System;
using System.IO;
using System.Windows;
using vmPing.Classes;

namespace vmPing.UI
{
    public partial class NewConfigurationWindow : Window
    {
        public NewConfigurationWindow()
        {
            InitializeComponent();

            FilePath.Text = Configuration.FilePath;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (PortableMode.IsChecked == true)
            {
                Configuration.FilePath = GetPortableFilePath();
            }

            DialogResult = true;
        }

        private void PortableMode_Click(object sender, RoutedEventArgs e)
        {
            FilePath.Text = PortableMode.IsChecked == true
                ? GetPortableFilePath()
                : Configuration.FilePath;
        }

        private string GetPortableFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vmPing.xml");
        }
    }
}
