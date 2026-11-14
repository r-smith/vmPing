using System;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Interop;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();

            PopulateGeneralOptions();
            PopulateStartupOptions();
            PopulateNotificationOptions();
            PopulateEmailAlertOptions();
            PopulateAudioAlertOptions();
            PopulateLogOutputOptions();
            PopulateAdvancedOptions();
            PopulateDisplayOptions();
            PopulateLayoutOptions();
        }

        private bool? ShowError(string message, TabItem tabItem, Control control, bool isWarning = false)
        {
            // Switch to specified tab.
            tabItem?.Focus();

            // Show warning or error?
            DialogWindow errorWindow;
            if (isWarning == true)
            {
                errorWindow = DialogWindow.WarningWindow(message, "Save");
            }
            else
            {
                errorWindow = DialogWindow.ErrorWindow(message);
            }

            // Display dialog and capture result.
            errorWindow.Owner = this;
            var result = errorWindow.ShowDialog();

            // Set focus to specified control.
            control?.Focus();

            return result;
        }

        private void PopulateGeneralOptions()
        {
            string pingIntervalUnits;
            int pingIntervalDivisor;
            int pingInterval = ApplicationOptions.PingInterval;
            int pingTimeout = ApplicationOptions.PingTimeout;

            if (ApplicationOptions.PingInterval >= 3600000 && ApplicationOptions.PingInterval % 3600000 == 0)
            {
                pingIntervalUnits = "hours";
                pingIntervalDivisor = 3600000;
            }
            else if (ApplicationOptions.PingInterval >= 60000 && ApplicationOptions.PingInterval % 60000 == 0)
            {
                pingIntervalUnits = "minutes";
                pingIntervalDivisor = 60000;
            }
            else
            {
                pingIntervalUnits = "seconds";
                pingIntervalDivisor = 1000;
            }

            pingInterval /= pingIntervalDivisor;
            pingTimeout /= 1000;

            PingInterval.Text = pingInterval.ToString();
            PingTimeout.Text = pingTimeout.ToString();
            AlertThreshold.Text = ApplicationOptions.AlertThreshold.ToString();
            PingIntervalUnits.Text = pingIntervalUnits;

            // High latency detection.
            LatencyDetectionMode.SelectedIndex = (int)ApplicationOptions.LatencyDetectionMode;
            HighLatencyMilliseconds.Text = ApplicationOptions.HighLatencyMilliseconds.ToString();
            HighLatencyTriggerCount.Text = ApplicationOptions.HighLatencyAlertTiggerCount.ToString();
        }

        private void PopulateStartupOptions()
        {
            InitialProbeCount.Text = ApplicationOptions.InitialProbeCount.ToString();
            InitialColumnCount.Text = ApplicationOptions.InitialColumnCount.ToString();
            StartupMode.SelectedIndex = (int)ApplicationOptions.InitialStartMode;
            InitialFavorite.ItemsSource = Favorite.GetTitles();
            InitialFavorite.Text = ApplicationOptions.InitialFavorite ?? string.Empty;
        }

        private void PopulateNotificationOptions()
        {
            PopupsDisabledOption.IsChecked = false;
            PopupsMinimizedOption.IsChecked = false;
            PopupsAlwaysOption.IsChecked = false;
            switch (ApplicationOptions.PopupOption)
            {
                case PopupNotificationOption.Never:
                    PopupsDisabledOption.IsChecked = true;
                    break;
                case PopupNotificationOption.WhenMinimized:
                    PopupsMinimizedOption.IsChecked = true;
                    break;
                case PopupNotificationOption.Always:
                    PopupsAlwaysOption.IsChecked = true;
                    break;
            }
            IsAutoDismissEnabled.IsChecked = ApplicationOptions.IsAutoDismissEnabled;
            AutoDismissInterval.Text = (ApplicationOptions.AutoDismissMilliseconds / 1000).ToString();
        }

        private void PopulateEmailAlertOptions()
        {
            IsEmailAlertsEnabled.IsChecked = ApplicationOptions.IsEmailAlertEnabled;
            IsSmtpAuthenticationRequired.IsChecked = ApplicationOptions.IsEmailAuthenticationRequired;
            IsSmtpSslEnabled.IsChecked = ApplicationOptions.IsEmailSslEnabled;
            SmtpServer.Text = ApplicationOptions.EmailServer;
            SmtpPort.Text = ApplicationOptions.EmailPort;
            SmtpUsername.Text = ApplicationOptions.EmailUser;
            SmtpPassword.Password = ApplicationOptions.EmailPassword;
            EmailRecipientAddress.Text = ApplicationOptions.EmailRecipient;
            EmailFromAddress.Text = ApplicationOptions.EmailFromAddress;
        }
        private void PopulateAudioAlertOptions()
        {
            IsAudioDownAlertEnabled.IsChecked = ApplicationOptions.IsAudioDownAlertEnabled;
            AudioDownFilePath.Text = ApplicationOptions.AudioDownFilePath;
            IsAudioUpAlertEnabled.IsChecked = ApplicationOptions.IsAudioUpAlertEnabled;
            AudioUpFilePath.Text = ApplicationOptions.AudioUpFilePath;
        }

        private void PopulateLogOutputOptions()
        {
            LogPath.Text = ApplicationOptions.LogPath;
            IsLogOutputEnabled.IsChecked = ApplicationOptions.IsLogOutputEnabled;
            LogStatusChangesPath.Text = ApplicationOptions.LogStatusChangesPath;
            IsLogStatusChangesEnabled.IsChecked = ApplicationOptions.IsLogStatusChangesEnabled;
        }

        private void PopulateAdvancedOptions()
        {
            TTL.Text = ApplicationOptions.TTL.ToString();
            DontFragment.IsChecked = ApplicationOptions.DontFragment;

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

        private void PopulateDisplayOptions()
        {
            IsAlwaysOnTopEnabled.IsChecked = ApplicationOptions.IsAlwaysOnTopEnabled;
            IsMinimizeToTrayEnabled.IsChecked = ApplicationOptions.IsMinimizeToTrayEnabled;
            IsExitToTrayEnabled.IsChecked = ApplicationOptions.IsExitToTrayEnabled;
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
            ForegroundColor_Stats_Indeterminate.Text = ApplicationOptions.ForegroundColor_Stats_Indeterminate;
            ForegroundColor_Alias_Inactive.Text = ApplicationOptions.ForegroundColor_Alias_Inactive;
            ForegroundColor_Alias_Up.Text = ApplicationOptions.ForegroundColor_Alias_Up;
            ForegroundColor_Alias_Down.Text = ApplicationOptions.ForegroundColor_Alias_Down;
            ForegroundColor_Alias_Error.Text = ApplicationOptions.ForegroundColor_Alias_Error;
            ForegroundColor_Alias_Indeterminate.Text = ApplicationOptions.ForegroundColor_Alias_Indeterminate;
        }


        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Check every tab before applying anything.
            if (!ValidateOptions())
            {
                return;
            }

            SaveGeneralOptions();
            SaveStartupOptions();
            SaveNotificationOptions();
            SaveEmailAlertOptions();
            SaveAudioAlertOptions();
            SaveLogOutputOptions();
            SaveAdvancedOptions();
            SaveLayoutOptions();
            SaveDisplayOptions();

            if (SaveAsDefaults.IsChecked == true)
            {
                ConfigFile.Save();
            }

            DialogResult = true;
        }

        private bool ValidateOptions()
        {
            return ValidateGeneralOptions()
                && ValidateStartupOptions()
                && ValidateNotificationOptions()
                && ValidateEmailAlertOptions()
                && ValidateAudioAlertOptions()
                && ValidateLogOutputOptions()
                && ValidateAdvancedOptions()
                && ValidateLayoutOptions();
        }

        private static bool IsInRange(string text, int min, int max)
        {
            return int.TryParse(text, out int value) && value >= min && value <= max;
        }

        private int GetPingIntervalMultiplier()
        {
            switch (PingIntervalUnits.Text)
            {
                case "minutes":
                    return 1000 * 60;
                case "hours":
                    return 1000 * 60 * 60;
                default:
                    return 1000;
            }
        }

        private bool ValidateGeneralOptions()
        {
            // Ping interval. Limited to 24 hours, the same as the command line option.
            const long MaxIntervalMilliseconds = 24 * 60 * 60 * 1000;
            if (!int.TryParse(PingInterval.Text, out int pingInterval) || pingInterval < 1
                || (long)pingInterval * GetPingIntervalMultiplier() > MaxIntervalMilliseconds)
            {
                ShowError("Please enter a ping interval between 1 second and 24 hours.", GeneralTab, PingInterval);
                return false;
            }
            if (!IsInRange(PingTimeout.Text, 1, 60))
            {
                ShowError("Please enter a ping timeout between 1 and 60 seconds.", GeneralTab, PingTimeout);
                return false;
            }
            if (!IsInRange(AlertThreshold.Text, 1, 60))
            {
                ShowError("Please enter an alert threshold between 1 and 60.", GeneralTab, AlertThreshold);
                return false;
            }

            // The high latency threshold and trigger count are only used when a mode is selected.
            if ((LatencyMode)LatencyDetectionMode.SelectedIndex != LatencyMode.Off)
            {
                if (!IsInRange(HighLatencyMilliseconds.Text, 1, 60000))
                {
                    ShowError("Please enter a high latency threshold between 1 and 60,000 milliseconds.", GeneralTab, HighLatencyMilliseconds);
                    return false;
                }
                if (!IsInRange(HighLatencyTriggerCount.Text, 1, 60))
                {
                    ShowError("Please enter a number of slow pings between 1 and 60.", GeneralTab, HighLatencyTriggerCount);
                    return false;
                }
            }

            return true;
        }

        private void SaveGeneralOptions()
        {
            ApplicationOptions.PingInterval = int.Parse(PingInterval.Text) * GetPingIntervalMultiplier();
            ApplicationOptions.PingTimeout = int.Parse(PingTimeout.Text) * 1000;
            ApplicationOptions.AlertThreshold = int.Parse(AlertThreshold.Text);

            ApplicationOptions.LatencyDetectionMode = (LatencyMode)LatencyDetectionMode.SelectedIndex;
            if (ApplicationOptions.LatencyDetectionMode != LatencyMode.Off)
            {
                ApplicationOptions.HighLatencyMilliseconds = long.Parse(HighLatencyMilliseconds.Text);
                ApplicationOptions.HighLatencyAlertTiggerCount = int.Parse(HighLatencyTriggerCount.Text);
            }
        }

        private bool ValidateStartupOptions()
        {
            // The probe and column counts are not used when starting with a favorite.
            if (StartupMode.SelectedIndex == (int)StartMode.Favorite)
            {
                return true;
            }

            if (!IsInRange(InitialProbeCount.Text, 1, 20))
            {
                ShowError("Please enter an initial probe count between 1 and 20.", StartupTab, InitialProbeCount);
                return false;
            }
            if (!IsInRange(InitialColumnCount.Text, 1, 10))
            {
                ShowError("Please enter an initial column count between 1 and 10.", StartupTab, InitialColumnCount);
                return false;
            }

            return true;
        }

        private void SaveStartupOptions()
        {
            ApplicationOptions.InitialStartMode = (StartMode)StartupMode.SelectedIndex;
            if (ApplicationOptions.InitialStartMode == StartMode.Favorite)
            {
                ApplicationOptions.InitialFavorite = InitialFavorite.Text;
            }
            else
            {
                ApplicationOptions.InitialProbeCount = int.Parse(InitialProbeCount.Text);
                ApplicationOptions.InitialColumnCount = int.Parse(InitialColumnCount.Text);
            }
        }

        private bool ValidateNotificationOptions()
        {
            if (IsAutoDismissEnabled.IsChecked == true && !IsInRange(AutoDismissInterval.Text, 1, 99))
            {
                ShowError("Please enter a valid number of seconds for the auto-dismiss interval.", PopupAlertsTab, AutoDismissInterval);
                return false;
            }

            return true;
        }

        private void SaveNotificationOptions()
        {
            ApplicationOptions.IsAutoDismissEnabled = IsAutoDismissEnabled.IsChecked == true;
            if (ApplicationOptions.IsAutoDismissEnabled)
            {
                ApplicationOptions.AutoDismissMilliseconds = int.Parse(AutoDismissInterval.Text) * 1000;
            }

            if (PopupsMinimizedOption.IsChecked == true)
            {
                ApplicationOptions.PopupOption = PopupNotificationOption.WhenMinimized;
            }
            else if (PopupsAlwaysOption.IsChecked == true)
            {
                ApplicationOptions.PopupOption = PopupNotificationOption.Always;
            }
            else
            {
                ApplicationOptions.PopupOption = PopupNotificationOption.Never;
            }
        }

        private bool ValidateAdvancedOptions()
        {
            if (!IsInRange(TTL.Text, 1, 255))
            {
                ShowError("Please enter a valid time to live (TTL) between 1 and 255.", AdvancedTab, TTL);
                return false;
            }
            if (PacketSizeOption.IsChecked == true && !IsInRange(PacketSize.Text, 0, 65500))
            {
                ShowError("Please enter a valid data size between 0 and 65,500.", AdvancedTab, PacketSize);
                return false;
            }

            return true;
        }

        private void SaveAdvancedOptions()
        {
            ApplicationOptions.TTL = int.Parse(TTL.Text);

            if (PacketSizeOption.IsChecked == true)
            {
                ApplicationOptions.Buffer = new byte[int.Parse(PacketSize.Text)];
                ApplicationOptions.UseCustomBuffer = false;

                // Fill buffer with default text.
                if (ApplicationOptions.Buffer.Length >= 33)
                {
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(Constants.DefaultIcmpData), 0, ApplicationOptions.Buffer, 0, 33);
                }
            }
            else
            {
                // Use custom packet data.
                ApplicationOptions.Buffer = Encoding.ASCII.GetBytes(PacketData.Text);
                ApplicationOptions.UseCustomBuffer = true;
            }

            ApplicationOptions.DontFragment = DontFragment.IsChecked == true;

            // Update ping options (TTL / Don't fragment settings)
            ApplicationOptions.UpdatePingOptions();
        }

        private bool ValidateEmailAlertOptions()
        {
            if (IsEmailAlertsEnabled.IsChecked != true)
            {
                return true;
            }

            if (SmtpServer.Text.Length == 0)
            {
                ShowError("Please enter a valid SMTP server address.", EmailAlertsTab, SmtpServer);
                return false;
            }
            if (!IsInRange(SmtpPort.Text, 1, 65535))
            {
                ShowError("Please enter a valid port number for your SMTP server.", EmailAlertsTab, SmtpPort);
                return false;
            }
            if (EmailRecipientAddress.Text.Length == 0)
            {
                ShowError("Please enter a valid recipient email address. This address will receive email alerts from vmPing.", EmailAlertsTab, EmailRecipientAddress);
                return false;
            }
            if (EmailFromAddress.Text.Length == 0)
            {
                ShowError("Please enter a valid 'from' address. This address appears as the sender for any alerts that are sent.", EmailAlertsTab, EmailFromAddress);
                return false;
            }
            if (IsSmtpAuthenticationRequired.IsChecked == true && SmtpUsername.Text.Length == 0)
            {
                ShowError("Please enter a valid username for your mail server.", EmailAlertsTab, SmtpUsername);
                return false;
            }

            return true;
        }

        private void SaveEmailAlertOptions()
        {
            ApplicationOptions.IsEmailAlertEnabled = IsEmailAlertsEnabled.IsChecked == true;
            if (!ApplicationOptions.IsEmailAlertEnabled)
            {
                return;
            }

            ApplicationOptions.IsEmailAuthenticationRequired = IsSmtpAuthenticationRequired.IsChecked == true;
            if (!ApplicationOptions.IsEmailAuthenticationRequired)
            {
                SmtpUsername.Text = string.Empty;
                SmtpPassword.Password = string.Empty;
            }

            ApplicationOptions.EmailServer = SmtpServer.Text;
            ApplicationOptions.EmailPort = SmtpPort.Text;
            ApplicationOptions.EmailUser = SmtpUsername.Text;
            ApplicationOptions.EmailPassword = SmtpPassword.Password;
            ApplicationOptions.EmailRecipient = EmailRecipientAddress.Text;
            ApplicationOptions.EmailFromAddress = EmailFromAddress.Text;
            ApplicationOptions.IsEmailSslEnabled = IsSmtpSslEnabled.IsChecked == true;
        }

        private bool ValidateAudioAlertOptions()
        {
            if (IsAudioDownAlertEnabled.IsChecked == true && !File.Exists(AudioDownFilePath.Text))
            {
                ShowError("The specified path does not exist. Please enter a valid path.", AudioAlertTab, AudioDownFilePath);
                return false;
            }
            if (IsAudioUpAlertEnabled.IsChecked == true && !File.Exists(AudioUpFilePath.Text))
            {
                ShowError("The specified path does not exist. Please enter a valid path.", AudioAlertTab, AudioUpFilePath);
                return false;
            }

            return true;
        }

        private void SaveAudioAlertOptions()
        {
            ApplicationOptions.IsAudioDownAlertEnabled = IsAudioDownAlertEnabled.IsChecked == true;
            if (ApplicationOptions.IsAudioDownAlertEnabled)
            {
                ApplicationOptions.AudioDownFilePath = AudioDownFilePath.Text;
            }

            ApplicationOptions.IsAudioUpAlertEnabled = IsAudioUpAlertEnabled.IsChecked == true;
            if (ApplicationOptions.IsAudioUpAlertEnabled)
            {
                ApplicationOptions.AudioUpFilePath = AudioUpFilePath.Text;
            }
        }

        private bool ValidateLogOutputOptions()
        {
            if (IsLogOutputEnabled.IsChecked == true && !Directory.Exists(LogPath.Text))
            {
                ShowError("The specified path does not exist.  Please enter a valid path.", LogOutputTab, LogPath);
                return false;
            }
            if (IsLogStatusChangesEnabled.IsChecked == true && !IsValidLogFilePath(LogStatusChangesPath.Text))
            {
                ShowError("The specified path does not exist.  Please enter a valid path.", LogOutputTab, LogStatusChangesPath);
                return false;
            }

            return true;
        }

        // The file itself doesn't need to exist yet, but its folder does.
        private static bool IsValidLogFilePath(string path)
        {
            try
            {
                string fileName = Path.GetFileName(path);
                return fileName.Length > 0
                    && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
                    && Directory.Exists(Path.GetDirectoryName(path));
            }
            catch
            {
                return false;
            }
        }

        private void SaveLogOutputOptions()
        {
            ApplicationOptions.IsLogOutputEnabled = IsLogOutputEnabled.IsChecked == true;
            if (ApplicationOptions.IsLogOutputEnabled)
            {
                ApplicationOptions.LogPath = LogPath.Text;
            }

            ApplicationOptions.IsLogStatusChangesEnabled = IsLogStatusChangesEnabled.IsChecked == true;
            if (ApplicationOptions.IsLogStatusChangesEnabled)
            {
                ApplicationOptions.LogStatusChangesPath = LogStatusChangesPath.Text;
            }
        }

        private void SaveDisplayOptions()
        {
            ApplicationOptions.IsAlwaysOnTopEnabled = IsAlwaysOnTopEnabled.IsChecked == true;
            ApplicationOptions.IsMinimizeToTrayEnabled = IsMinimizeToTrayEnabled.IsChecked == true;
            ApplicationOptions.IsExitToTrayEnabled = IsExitToTrayEnabled.IsChecked == true;
        }

        private bool ValidateLayoutOptions()
        {
            foreach (var control in ColorsDockPanel.GetChildren())
            {
                if (control is TextBox box && !Util.IsValidHtmlColor(box.Text))
                {
                    ShowError("Please enter a valid HTML color code.  Accepted formats are #RGB, #RRGGBB, and #AARRGGBB.  Example: #3266CF", LayoutTab, box);
                    box.SelectAll();
                    return false;
                }
            }

            return true;
        }

        private void SaveLayoutOptions()
        {
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
        }

        private void NumericTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void HtmlColor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[#a-fA-F0-9]");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void EmailRecipientAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EmailFromAddress.Text.Length == 0 && EmailRecipientAddress.Text.IndexOf('@') >= 0)
            {
                EmailFromAddress.Text = "vmPing" + EmailRecipientAddress.Text.Substring(EmailRecipientAddress.Text.IndexOf('@'));
            }
        }

        private void IsEmailAlertsEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (IsEmailAlertsEnabled.IsChecked == true && SmtpServer.Text.Length == 0)
            {
                SmtpServer.Focus();
            }
        }

        private void IsSmtpAuthenticationRequired_Click(object sender, RoutedEventArgs e)
        {
            if (IsSmtpAuthenticationRequired.IsChecked == true)
            {
                SmtpUsername.Focus();
            }
        }

        private async void TestEmail_Click(object sender, RoutedEventArgs e)
        {
            TestEmailButton.IsEnabled = false;
            TestEmailButton.Content = "Sending...";
            var serverAddress = SmtpServer.Text;
            var serverPort = SmtpPort.Text;
            var isSslEnabled = IsSmtpSslEnabled.IsChecked == true;
            var isAuthRequired = IsSmtpAuthenticationRequired.IsChecked == true;
            var username = SmtpUsername.Text;
            var password = SmtpPassword.SecurePassword;
            var mailFrom = EmailFromAddress.Text;
            var mailRecipient = EmailRecipientAddress.Text;

            await Task.Run(() =>
            {
                try
                {
                    Email.SendTest(
                        serverAddress,
                        serverPort,
                        isSslEnabled,
                        isAuthRequired,
                        username,
                        password,
                        mailFrom,
                        mailRecipient);
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            if (IsLoaded)
                            {
                                var dialogWindow = new DialogWindow(
                                    DialogWindow.DialogIcon.Info,
                                    "Email Test",
                                    "A test email was sent.",
                                    "OK",
                                    false)
                                {
                                    Owner = this
                                };
                                dialogWindow.ShowDialog();
                            }
                        }));
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            if (IsLoaded)
                            {
                                ShowError($"Test failed: {ex.Message}", EmailAlertsTab, TestEmailButton);
                            }
                        }));
                }
            });
            TestEmailButton.IsEnabled = true;
            TestEmailButton.Content = "Test";
        }

        private void BrowseLogPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a location for the log files.";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    LogPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseLogStatusChangesPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a location for the log files.";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    LogStatusChangesPath.Text = dialog.SelectedPath + "\\vmping-status.txt";
                }
            }
        }

        private void AudioDownBrowse_Click(object sender, RoutedEventArgs e)
        {
            AudioFileBrowse(AudioDownFilePath);
        }

        private void AudioUpBrowse_Click(object sender, RoutedEventArgs e)
        {
            AudioFileBrowse(AudioUpFilePath);
        }

        private void AudioFileBrowse(TextBox tb)
        {
            using (var audiofileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                audiofileDialog.Title = "Select an audio file";
                audiofileDialog.RestoreDirectory = true;
                audiofileDialog.Multiselect = false;
                audiofileDialog.Filter = "WAV files (*.wav)|*.wav|All files|*.*";
                audiofileDialog.DefaultExt = ".wav";

                if (audiofileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tb.Text = audiofileDialog.FileName;
                }
            }
        }

        private void AudioDownPlay_Click(object sender, RoutedEventArgs e)
        {
            AudioFilePlay(AudioDownFilePath.Text);
        }

        private void AudioUpPlay_Click(object sender, RoutedEventArgs e)
        {
            AudioFilePlay(AudioUpFilePath.Text);
        }

        private void AudioFilePlay(string path)
        {
            try
            {
                using (var player = new SoundPlayer(path))
                {
                    player.Play();
                }
            }
            catch
            {
                ShowError("Unable to play audio file.", AudioAlertTab, AudioAlertTab);
            }
        }

        private void IsAudioDownAlertEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (AudioDownFilePath.Text.Length == 0)
            {
                if (File.Exists(Environment.ExpandEnvironmentVariables(Constants.DefaultAudioDownFilePath)))
                {
                    AudioDownFilePath.Text = Environment.ExpandEnvironmentVariables(Constants.DefaultAudioDownFilePath);
                }
            }
        }

        private void IsAudioUpAlertEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (AudioUpFilePath.Text.Length == 0)
            {
                if (File.Exists(Environment.ExpandEnvironmentVariables(Constants.DefaultAudioUpFilePath)))
                {
                    AudioUpFilePath.Text = Environment.ExpandEnvironmentVariables(Constants.DefaultAudioUpFilePath);
                }
            }
        }

        private void UpdateByteCount()
        {
            var regex = new Regex("^\\d+$");
            if (PacketSizeOption.IsChecked == true)
            {
                if (PacketSize != null && regex.IsMatch(PacketSize.Text))
                {
                    Bytes.Text = (int.Parse(PacketSize.Text) + 28).ToString();
                }
                else
                {
                    Bytes.Text = "?";
                }
            }
            else
            {
                Bytes.Text = (PacketData.Text.Length + 28).ToString();
            }
        }

        private void PacketData_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateByteCount();
        }

        private void PacketSizeOption_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                UpdateByteCount();
            }
        }

        private void UseCustomPacketOption_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                UpdateByteCount();
            }
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
            ForegroundColor_Stats_Indeterminate.Text = Constants.Color_Statistics_Foreground_Indeterminate;
            ForegroundColor_Alias_Inactive.Text = Constants.Color_Alias_Foreground_Inactive;
            ForegroundColor_Alias_Up.Text = Constants.Color_Alias_Foreground_Up;
            ForegroundColor_Alias_Down.Text = Constants.Color_Alias_Foreground_Down;
            ForegroundColor_Alias_Error.Text = Constants.Color_Alias_Foreground_Error;
            ForegroundColor_Alias_Indeterminate.Text = Constants.Color_Alias_Foreground_Indeterminate;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideMinimizeAndMaximizeButtons(this);
        }
    }
}