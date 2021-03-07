using System;
using System.Windows;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for NewConfigurationWindow.xaml
    /// </summary>
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
                Configuration.FilePath = AppDomain.CurrentDomain.BaseDirectory + "vmPing.xml";
            DialogResult = true;
        }

        private void PortableMode_Click(object sender, RoutedEventArgs e)
        {
            if (PortableMode.IsChecked == true)
                FilePath.Text = AppDomain.CurrentDomain.BaseDirectory + "vmPing.xml";
            else
                FilePath.Text = Configuration.FilePath;
        }
    }
}
