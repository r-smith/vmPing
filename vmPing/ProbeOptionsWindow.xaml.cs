using System.Windows;
using System.Windows.Input;

namespace vmPing
{
    /// <summary>
    /// Interaction logic for ProbeOptionsWindow.xaml
    /// </summary>
    public partial class ProbeOptionsWindow : Window
    {
        ApplicationOptions _applicationOptions = new ApplicationOptions();

        public ProbeOptionsWindow(ApplicationOptions appOptions)
        {
            InitializeComponent();

            _applicationOptions = appOptions;

            string pingIntervalText;
            int pingIntervalDivisor;
            int pingInterval = appOptions.PingInterval;
            int pingTimeout = appOptions.PingTimeout;

            if (appOptions.PingInterval >= 3600000 && appOptions.PingInterval % 3600000 == 0)
            {
                pingIntervalText = "Hours";
                pingIntervalDivisor = 3600000;
            }
            else if (appOptions.PingInterval >= 60000 && appOptions.PingInterval % 60000 == 0)
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
            bool error = false;

            if (txtPingInterval.Text.Length == 0)
            {
                MessageBox.Show(
                    "A ping interval is required.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                error = true;
                txtPingInterval.Focus();
            }
            else if (txtPingTimeout.Text.Length == 0)
            {
                MessageBox.Show(
                    "A ping timeout is required.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                error = true;
                txtPingTimeout.Focus();
            }


            if (!error)
            {
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

                _applicationOptions.PingInterval = pingInterval;

                // Ping timeout
                int pingTimeout;

                if (int.TryParse(txtPingTimeout.Text, out pingTimeout) && pingTimeout > 0 && pingTimeout <= 60)
                    pingTimeout *= 1000;
                else
                    pingTimeout = Constants.PING_TIMEOUT;

                _applicationOptions.PingTimeout = pingTimeout;

                DialogResult = true;
            }
        }

        private void txtNumericTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9.-]+");
            if (regex.IsMatch(e.Text))
                e.Handled = true;
        }
    }
}
