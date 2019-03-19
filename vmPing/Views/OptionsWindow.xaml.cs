using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            PopulateGeneralOptions();
            PopulateEmailAlertOptions();
            PopulateLogOutputOptions();
            PopulateAdvancedOptions();
            PopulateLayoutOptions();
        }

        private void PopulateGeneralOptions()
        {
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
            txtAlertThreshold.Text = ApplicationOptions.AlertThreshold.ToString();
            cboPingInterval.Text = pingIntervalText;
        }

        private void PopulateEmailAlertOptions()
        {
            IsEmailAlertsEnabled.IsChecked = ApplicationOptions.IsEmailAlertEnabled;
            IsSmtpAuthenticationRequired.IsChecked = ApplicationOptions.IsEmailAuthenticationRequired;
            SmtpServer.Text = ApplicationOptions.EmailServer;
            SmtpPort.Text = ApplicationOptions.EmailPort;
            SmtpUsername.Text = ApplicationOptions.EmailUser;
            SmtpPassword.Password = ApplicationOptions.EmailPassword;
            EmailRecipientAddress.Text = ApplicationOptions.EmailRecipient;
            EmailFromAddress.Text = ApplicationOptions.EmailFromAddress;
        }

        private void PopulateLogOutputOptions()
        {
            LogPath.Text = ApplicationOptions.LogPath;
            IsLogOutputEnabled.IsChecked = ApplicationOptions.IsLogOutputEnabled;
        }

        private void PopulateAdvancedOptions()
        {
            TTL.Text = ApplicationOptions.TTL.ToString();
            if (ApplicationOptions.DontFragment)
                DontFragment.IsChecked = true;
            else
                Fragment.IsChecked = true;

            if (ApplicationOptions.UseCustomBuffer)
            {
                UseCustomPacketOption.IsChecked = true;
                PacketData.Text = Encoding.ASCII.GetString(ApplicationOptions.Buffer);
            }
            else
            {
                PacketSizeOption.IsChecked = true;
                PacketSize.Text = ApplicationOptions.Buffer.Length.ToString();
            }

            UpdateByteCount();
        }

        private void PopulateLayoutOptions()
        {
            BackgroundColor_Probe_Inactive.Text = ApplicationOptions.BackgroundColor_Probe_Inactive;
            BackgroundColor_Probe_Up.Text = ApplicationOptions.BackgroundColor_Probe_Up;
            BackgroundColor_Probe_Down.Text = ApplicationOptions.BackgroundColor_Probe_Down;
            BackgroundColor_Probe_Error.Text = ApplicationOptions.BackgroundColor_Probe_Error;
            BackgroundColor_Probe_Indeterminate.Text = ApplicationOptions.BackgroundColor_Probe_Indeterminate;
            ForegroundColor_Probe_Inactive.Text = ApplicationOptions.ForegroundColor_Probe_Inactive;
            ForegroundColor_Probe_Up.Text = ApplicationOptions.ForegroundColor_Probe_Up;
            ForegroundColor_Probe_Down.Text = ApplicationOptions.ForegroundColor_Probe_Down;
            ForegroundColor_Probe_Error.Text = ApplicationOptions.ForegroundColor_Probe_Error;
            ForegroundColor_Probe_Indeterminate.Text = ApplicationOptions.ForegroundColor_Probe_Indeterminate;
            ForegroundColor_Stats_Inactive.Text = ApplicationOptions.ForegroundColor_Stats_Inactive;
            ForegroundColor_Stats_Up.Text = ApplicationOptions.ForegroundColor_Stats_Up;
            ForegroundColor_Stats_Down.Text = ApplicationOptions.ForegroundColor_Stats_Down;
            ForegroundColor_Stats_Error.Text = ApplicationOptions.ForegroundColor_Stats_Error;
            ForegroundColor_Stats_Indeterminate.Text = ApplicationOptions.ForegroundColor_Stats_Inactive;
            ForegroundColor_Alias_Inactive.Text = ApplicationOptions.ForegroundColor_Alias_Inactive;
            ForegroundColor_Alias_Up.Text = ApplicationOptions.ForegroundColor_Alias_Up;
            ForegroundColor_Alias_Down.Text = ApplicationOptions.ForegroundColor_Alias_Down;
            ForegroundColor_Alias_Error.Text = ApplicationOptions.ForegroundColor_Alias_Error;
            ForegroundColor_Alias_Indeterminate.Text = ApplicationOptions.ForegroundColor_Alias_Indeterminate;
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (SaveGeneralOptions() == false)
                return;

            if (SaveEmailAlertOptions() == false)
                return;

            if (SaveLogOutputOptions() == false)
                return;

            if (SaveAdvancedOptions() == false)
                return;

            if (SaveLayoutOptions() == false)
                return;

            if (SaveAsDefaults.IsChecked == true)
                Configuration.WriteConfigurationOptions();

            Close();
        }


        private bool SaveGeneralOptions()
        {
            if (txtPingInterval.Text.Length == 0)
            {
                GeneralTab.Focus();
                MessageBox.Show(
                    "Please enter a valid ping interval.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtPingInterval.Focus();
                return false;
            }
            else if (txtPingTimeout.Text.Length == 0)
            {
                GeneralTab.Focus();
                MessageBox.Show(
                    "Please enter a valid ping timeout.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtPingTimeout.Focus();
                return false;
            }
            else if (txtAlertThreshold.Text.Length == 0)
            {
                GeneralTab.Focus();
                MessageBox.Show(
                    "Please enter an alert threshold.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtPingTimeout.Focus();
                return false;
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

            // Alert threshold
            int alertThreshold;

            var isThresholdValid = int.TryParse(txtAlertThreshold.Text, out alertThreshold) && alertThreshold > 0 && alertThreshold <= 60;
            if (!isThresholdValid)
                alertThreshold = 1;

            ApplicationOptions.AlertThreshold = alertThreshold;

            return true;
        }


        private bool SaveAdvancedOptions()
        {
            // Validate input.

            var regex = new Regex("^\\d+$");

            // Validate TTL.
            if (!regex.IsMatch(TTL.Text) || int.Parse(TTL.Text) < 1 || int.Parse(TTL.Text) > 255)
            {
                AdvancedTab.Focus();
                MessageBox.Show(
                    "Please enter a valid TTL between 1 and 255.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                TTL.Focus();
                return false;
            }

            // Apply TTL.
            ApplicationOptions.TTL = int.Parse(TTL.Text);

            // Validate packet size.
            if (PacketSizeOption.IsChecked == true)
            {
                if (!regex.IsMatch(PacketSize.Text) || int.Parse(PacketSize.Text) < 0 || int.Parse(PacketSize.Text) > 65500)
                {
                    AdvancedTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid ICMP data size between 0 and 65,500.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    PacketSize.Focus();
                    return false;
                }

                // Apply packet size.
                ApplicationOptions.Buffer = new byte[int.Parse(PacketSize.Text)];
                ApplicationOptions.UseCustomBuffer = false;

                // Fill buffer with default text.
                if (ApplicationOptions.Buffer.Length >= 33)
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(Constants.PING_DATA), 0, ApplicationOptions.Buffer, 0, 33);
            }
            else
            {
                // Use custom packet data.
                ApplicationOptions.Buffer = Encoding.ASCII.GetBytes(PacketData.Text);
                ApplicationOptions.UseCustomBuffer = true;
            }

            // Apply fragment / don't fragment option.
            if (DontFragment.IsChecked == true)
                ApplicationOptions.DontFragment = true;
            else
                ApplicationOptions.DontFragment = false;

            // Update ping options (TTL / Don't fragment settings)
            ApplicationOptions.UpdatePingOptions();

            return true;
        }


        private bool SaveEmailAlertOptions()
        {
            // Validate input.
            if (IsEmailAlertsEnabled.IsChecked == true)
            {
                var regex = new Regex("^\\d+$");

                if (SmtpServer.Text.Length == 0)
                {
                    EmailAlertsTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid address for your outgoing mail server.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    SmtpServer.Focus();
                    return false;
                }
                else if (SmtpPort.Text.Length == 0 || !regex.IsMatch(SmtpPort.Text))
                {
                    EmailAlertsTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid port number.  The standard is 25.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    SmtpPort.Focus();
                    return false;
                }
                else if (EmailRecipientAddress.Text.Length == 0)
                {
                    EmailAlertsTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid recipient email address.  This is the address that will receive alerts.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    EmailRecipientAddress.Focus();
                    return false;
                }
                else if (EmailFromAddress.Text.Length == 0)
                {
                    EmailAlertsTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid 'from' address.  This address will appear as the sender for any alerts that are sent.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    EmailFromAddress.Focus();
                    return false;
                }
                if (IsSmtpAuthenticationRequired.IsChecked == true)
                {
                    ApplicationOptions.IsEmailAuthenticationRequired = true;
                    if (SmtpUsername.Text.Length == 0)
                    {
                        EmailAlertsTab.Focus();
                        MessageBox.Show(
                            "Please enter a valid username for authenticating to your mail server.",
                            "vmPing Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        SmtpUsername.Focus();
                        return false;
                    }
                }
                else
                {
                    ApplicationOptions.IsEmailAuthenticationRequired = false;
                    SmtpUsername.Text = string.Empty;
                    SmtpPassword.Password = string.Empty;
                }

                ApplicationOptions.IsEmailAlertEnabled = true;
                ApplicationOptions.EmailServer = SmtpServer.Text;
                ApplicationOptions.EmailPort = SmtpPort.Text;
                ApplicationOptions.EmailUser = SmtpUsername.Text;
                ApplicationOptions.EmailPassword = SmtpPassword.Password;
                ApplicationOptions.EmailRecipient = EmailRecipientAddress.Text;
                ApplicationOptions.EmailFromAddress = EmailFromAddress.Text;

                if (IsSmtpAuthenticationRequired.IsChecked == true && SaveAsDefaults.IsChecked == true)
                {
                    MessageBox.Show(
                        "You have chosen to save your SMTP credentials to disk." + Environment.NewLine + Environment.NewLine +
                        "While the data is stored in an encrypted format, anyone with access to your vmPing configuration file can decrypt the data.",
                        "vmPing Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                return true;
            }
            else
            {
                ApplicationOptions.IsEmailAlertEnabled = false;
                return true;
            }
        }

        private bool SaveLogOutputOptions()
        {
            if (IsLogOutputEnabled.IsChecked == true)
            {
                if (!Directory.Exists(LogPath.Text))
                {
                    LogOutputTab.Focus();
                    MessageBox.Show(
                        "The specified path does not exist.  Please enter a valid path.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    LogPath.Focus();
                    return false;
                }

                ApplicationOptions.IsLogOutputEnabled = true;
                ApplicationOptions.LogPath = LogPath.Text;
            }
            else
            {
                ApplicationOptions.IsLogOutputEnabled = false;
            }

            return true;
        }


        private bool SaveLayoutOptions()
        {
            // Validate input.
            foreach (var control in ColorsDockPanel.GetChildren())
            {
                if (control is TextBox)
                {
                    if (!Configuration.IsValidHtmlColor(((TextBox)control).Text))
                    {
                        LayoutTab.Focus();
                        MessageBox.Show(
                            "Please enter a valid HTML color code.  Accepted formats are #RGB, #RRGGBB, and #AARRGGBB.  Example: #3266CF",
                            "vmPing Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        ((TextBox)control).Focus();
                        ((TextBox)control).SelectAll();

                        return false;
                    }
                }
            }

            ApplicationOptions.BackgroundColor_Probe_Inactive = BackgroundColor_Probe_Inactive.Text;
            ApplicationOptions.BackgroundColor_Probe_Up = BackgroundColor_Probe_Up.Text;
            ApplicationOptions.BackgroundColor_Probe_Down = BackgroundColor_Probe_Down.Text;
            ApplicationOptions.BackgroundColor_Probe_Indeterminate = BackgroundColor_Probe_Indeterminate.Text;
            ApplicationOptions.BackgroundColor_Probe_Error = BackgroundColor_Probe_Error.Text;
            ApplicationOptions.ForegroundColor_Probe_Inactive = ForegroundColor_Probe_Inactive.Text;
            ApplicationOptions.ForegroundColor_Probe_Up = ForegroundColor_Probe_Up.Text;
            ApplicationOptions.ForegroundColor_Probe_Down = ForegroundColor_Probe_Down.Text;
            ApplicationOptions.ForegroundColor_Probe_Indeterminate = ForegroundColor_Probe_Indeterminate.Text;
            ApplicationOptions.ForegroundColor_Probe_Error = ForegroundColor_Probe_Error.Text;
            ApplicationOptions.ForegroundColor_Stats_Inactive = ForegroundColor_Stats_Inactive.Text;
            ApplicationOptions.ForegroundColor_Stats_Up = ForegroundColor_Stats_Up.Text;
            ApplicationOptions.ForegroundColor_Stats_Down = ForegroundColor_Stats_Down.Text;
            ApplicationOptions.ForegroundColor_Stats_Indeterminate = ForegroundColor_Stats_Indeterminate.Text;
            ApplicationOptions.ForegroundColor_Stats_Error = ForegroundColor_Stats_Error.Text;
            ApplicationOptions.ForegroundColor_Alias_Inactive = ForegroundColor_Alias_Inactive.Text;
            ApplicationOptions.ForegroundColor_Alias_Up = ForegroundColor_Alias_Up.Text;
            ApplicationOptions.ForegroundColor_Alias_Down = ForegroundColor_Alias_Down.Text;
            ApplicationOptions.ForegroundColor_Alias_Indeterminate = ForegroundColor_Alias_Indeterminate.Text;
            ApplicationOptions.ForegroundColor_Alias_Error = ForegroundColor_Alias_Error.Text;

            return true;
        }


        private void txtNumericTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9.-]+");
            if (regex.IsMatch(e.Text))
                e.Handled = true;
        }


        private void HtmlColor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[#a-fA-F0-9]");
            if (!regex.IsMatch(e.Text))
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

        private void EmailRecipientAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EmailFromAddress.Text.Length == 0 && EmailRecipientAddress.Text.IndexOf('@') >= 0)
                EmailFromAddress.Text = "vmPing" + EmailRecipientAddress.Text.Substring(EmailRecipientAddress.Text.IndexOf('@'));
        }

        private void IsEmailAlertsEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (IsEmailAlertsEnabled.IsChecked == true && SmtpServer.Text.Length == 0)
                SmtpServer.Focus();
        }

        private void IsSmtpAuthenticationRequired_Click(object sender, RoutedEventArgs e)
        {
            if (IsSmtpAuthenticationRequired.IsChecked == true)
                SmtpUsername.Focus();
        }

        private void BrowseLogPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select a location for the log files.";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                LogPath.Text = dialog.SelectedPath;
        }

        private void UpdateByteCount()
        {
            var regex = new Regex("^\\d+$");
            if (PacketSizeOption.IsChecked == true)
            {
                if (PacketSize != null && regex.IsMatch(PacketSize.Text))
                    Bytes.Text = (int.Parse(PacketSize.Text) + 28).ToString();
                else
                    Bytes.Text = "?";
            }
            else
            {
                Bytes.Text = (PacketData.Text.Length + 28).ToString();
            }
        }

        private void PacketData_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateByteCount();
        }

        private void PacketSizeOption_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                UpdateByteCount();
        }

        private void UseCustomPacketOption_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                UpdateByteCount();
        }

        private void RestoreDefaultColors_Click(object sender, RoutedEventArgs e)
        {
            BackgroundColor_Probe_Inactive.Text = Constants.Color_Probe_Background_Inactive;
            BackgroundColor_Probe_Up.Text = Constants.Color_Probe_Background_Up;
            BackgroundColor_Probe_Down.Text = Constants.Color_Probe_Background_Down;
            BackgroundColor_Probe_Error.Text = Constants.Color_Probe_Background_Error;
            BackgroundColor_Probe_Indeterminate.Text = Constants.Color_Probe_Background_Indeterminate;
            ForegroundColor_Probe_Inactive.Text = Constants.Color_Probe_Foreground_Inactive;
            ForegroundColor_Probe_Up.Text = Constants.Color_Probe_Foreground_Up;
            ForegroundColor_Probe_Down.Text = Constants.Color_Probe_Foreground_Down;
            ForegroundColor_Probe_Error.Text = Constants.Color_Probe_Foreground_Error;
            ForegroundColor_Probe_Indeterminate.Text = Constants.Color_Probe_Foreground_Indeterminate;
            ForegroundColor_Stats_Inactive.Text = Constants.Color_Statistics_Foreground_Inactive;
            ForegroundColor_Stats_Up.Text = Constants.Color_Statistics_Foreground_Up;
            ForegroundColor_Stats_Down.Text = Constants.Color_Statistics_Foreground_Down;
            ForegroundColor_Stats_Error.Text = Constants.Color_Statistics_Foreground_Error;
            ForegroundColor_Stats_Indeterminate.Text = Constants.Color_Statistics_Foreground_Inactive;
            ForegroundColor_Alias_Inactive.Text = Constants.Color_Alias_Foreground_Inactive;
            ForegroundColor_Alias_Up.Text = Constants.Color_Alias_Foreground_Up;
            ForegroundColor_Alias_Down.Text = Constants.Color_Alias_Foreground_Down;
            ForegroundColor_Alias_Error.Text = Constants.Color_Alias_Foreground_Error;
            ForegroundColor_Alias_Indeterminate.Text = Constants.Color_Alias_Foreground_Inactive;
        }
    }
}
