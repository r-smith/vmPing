using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public static OptionsWindow openWindow = null;

        public OptionsWindow()
        {
            InitializeComponent();
            
            string pingIntervalText;
            int pingIntervalDivisor;
            int pingInterval = ApplicationOptions.PingInterval;
            int pingTimeout = ApplicationOptions.PingTimeout;

            if (ApplicationOptions.PingInterval >= 3600000 && ApplicationOptions.PingInterval % 3600000 == 0)
            {
                pingIntervalText = "Hours";
                pingIntervalDivisor = 3600000;
            }
            else if (ApplicationOptions.PingInterval >= 60000 && ApplicationOptions.PingInterval % 60000 == 0)
            {
                pingIntervalText = "Minutes";
                pingIntervalDivisor = 60000;
            }
            else
            {
                pingIntervalText = "Seconds";
                pingIntervalDivisor = 1000;
            }

            pingInterval /= pingIntervalDivisor;
            pingTimeout /= 1000;

            txtPingInterval.Text = pingInterval.ToString();
            txtPingTimeout.Text = pingTimeout.ToString();
            cboPingInterval.Text = pingIntervalText;
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (txtPingInterval.Text.Length == 0)
            {
                MessageBox.Show(
                    "A ping interval is required.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtPingInterval.Focus();
                return;
            }
            else if (txtPingTimeout.Text.Length == 0)
            {
                MessageBox.Show(
                    "A ping timeout is required.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtPingTimeout.Focus();
                return;
            }


            // Ping interval
            int pingInterval;
            int multiplier = 1000;

            switch (cboPingInterval.Text)
            {
                case "Seconds":
                    multiplier = 1000;
                    break;
                case "Minutes":
                    multiplier = 1000 * 60;
                    break;
                case "Hours":
                    multiplier = 1000 * 60 * 60;
                    break;
            }

            if (int.TryParse(txtPingInterval.Text, out pingInterval) && pingInterval > 0 && pingInterval <= 86400)
                pingInterval *= multiplier;
            else
                pingInterval = Constants.PING_INTERVAL;

            ApplicationOptions.PingInterval = pingInterval;

            // Ping timeout
            int pingTimeout;

            if (int.TryParse(txtPingTimeout.Text, out pingTimeout) && pingTimeout > 0 && pingTimeout <= 60)
                pingTimeout *= 1000;
            else
                pingTimeout = Constants.PING_TIMEOUT;

            ApplicationOptions.PingTimeout = pingTimeout;
            this.Close();
        }


        private void txtNumericTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9.-]+");
            if (regex.IsMatch(e.Text))
                e.Handled = true;
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
