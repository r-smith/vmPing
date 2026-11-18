using System;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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
            PopulateWindowOptions();
            PopulateThemeOptions();

            // Watch for changes only after the options are filled in.
            WatchStartupOptions();
        }

        // Startup options only matter when vmPing starts, which needs them saved.
        // So changing any of them checks "Save as vmPing defaults".
        private void WatchStartupOptions()
        {
            UIElement startupOptions = (UIElement)StartupTab.Content;
            startupOptions.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(StartupOption_Changed));
            startupOptions.AddHandler(Selector.SelectionChangedEvent, new SelectionChangedEventHandler(StartupOption_Changed));
            startupOptions.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(StartupOption_Changed));
            startupOptions.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(StartupOption_Changed));
        }

        private void StartupOption_Changed(object sender, RoutedEventArgs e)
        {
            SaveAsDefaults.IsChecked = true;
        }

        private bool? ShowError(string message, TabItem tabItem, Control control, bool isWarning = false)
        {
            // Switch to specified tab.
            tabItem?.Focus();

            // Show warning or error?
            DialogWindow errorWindow;
            if (isWarning)
            {
                errorWindow = DialogWindow.WarningWindow(message, "Save");
            }
            else
            {
                errorWindow = DialogWindow.ErrorWindow(message);
            }

            // Display dialog and capture result.
            errorWindow.Owner = this;
            bool? result = errorWindow.ShowDialog();

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
            HighLatencyTriggerCount.Text = ApplicationOptions.HighLatencyAlertTriggerCount.ToString();
        }

        private void PopulateStartupOptions()
        {
            InitialProbeCount.Text = ApplicationOptions.InitialProbeCount.ToString();
            InitialColumnCount.Text = ApplicationOptions.InitialColumnCount.ToString();
            StartupMode.SelectedIndex = (int)ApplicationOptions.InitialStartMode;
            InitialFavorite.ItemsSource = Favorite.GetTitles();
            InitialFavorite.Text = ApplicationOptions.InitialFavorite ?? string.Empty;
            IsStartMinimizedEnabled.IsChecked = ApplicationOptions.IsStartMinimizedEnabled;

            WindowSizeRememberOption.IsChecked = ApplicationOptions.WindowSizeOption == WindowStartupOption.Remember;
            WindowSizeFixedOption.IsChecked = ApplicationOptions.WindowSizeOption == WindowStartupOption.Fixed;
            FixedWindowWidth.Text = ApplicationOptions.WindowWidth.ToString();
            FixedWindowHeight.Text = ApplicationOptions.WindowHeight.ToString();

            WindowPositionDefaultOption.IsChecked = ApplicationOptions.WindowPositionOption == WindowStartupOption.Default;
            WindowPositionRememberOption.IsChecked = ApplicationOptions.WindowPositionOption == WindowStartupOption.Remember;
            WindowPositionFixedOption.IsChecked = ApplicationOptions.WindowPositionOption == WindowStartupOption.Fixed;
            FixedWindowLeft.Text = ApplicationOptions.WindowLeft.ToString();
            FixedWindowTop.Text = ApplicationOptions.WindowTop.ToString();
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
            LogFolder.Text = ApplicationOptions.LogFolder;
            IsLogOutputEnabled.IsChecked = ApplicationOptions.IsLogOutputEnabled;
            IsLogStatusChangesEnabled.IsChecked = ApplicationOptions.IsLogStatusChangesEnabled;
        }

        private void PopulateAdvancedOptions()
        {
            TTL.Text = ApplicationOptions.TTL.ToString();
            DontFragment.IsChecked = ApplicationOptions.DontFragment;

            if (ApplicationOptions.UseCustomPacketData)
            {
                UseCustomPacketOption.IsChecked = true;
            }
            else
            {
                PacketSizeOption.IsChecked = true;
            }
            PacketSize.Text = ApplicationOptions.PacketSize.ToString();
            PacketData.Text = ApplicationOptions.CustomPacketData;

            UpdateByteCount();
        }

        private void PopulateWindowOptions()
        {
            IsAlwaysOnTopEnabled.IsChecked = ApplicationOptions.IsAlwaysOnTopEnabled;
            IsMinimizeToTrayEnabled.IsChecked = ApplicationOptions.IsMinimizeToTrayEnabled;
            IsExitToTrayEnabled.IsChecked = ApplicationOptions.IsExitToTrayEnabled;
            IsAlwaysShowTrayIconEnabled.IsChecked = ApplicationOptions.IsAlwaysShowTrayIconEnabled;
        }

        private void PopulateThemeOptions()
        {
            ThemeSystemOption.IsChecked = ApplicationOptions.Theme == ThemeMode.System;
            ThemeLightOption.IsChecked = ApplicationOptions.Theme == ThemeMode.Light;
            ThemeDarkOption.IsChecked = ApplicationOptions.Theme == ThemeMode.Dark;
            ProbeBackgroundInactive.Text = ApplicationOptions.ProbeBackgroundInactive;
            ProbeBackgroundUp.Text = ApplicationOptions.ProbeBackgroundUp;
            ProbeBackgroundDown.Text = ApplicationOptions.ProbeBackgroundDown;
            ProbeBackgroundError.Text = ApplicationOptions.ProbeBackgroundError;
            ProbeBackgroundIndeterminate.Text = ApplicationOptions.ProbeBackgroundIndeterminate;
            ProbeForegroundInactive.Text = ApplicationOptions.ProbeForegroundInactive;
            ProbeForegroundUp.Text = ApplicationOptions.ProbeForegroundUp;
            ProbeForegroundDown.Text = ApplicationOptions.ProbeForegroundDown;
            ProbeForegroundError.Text = ApplicationOptions.ProbeForegroundError;
            ProbeForegroundIndeterminate.Text = ApplicationOptions.ProbeForegroundIndeterminate;
            StatisticsForegroundInactive.Text = ApplicationOptions.StatisticsForegroundInactive;
            StatisticsForegroundUp.Text = ApplicationOptions.StatisticsForegroundUp;
            StatisticsForegroundDown.Text = ApplicationOptions.StatisticsForegroundDown;
            StatisticsForegroundError.Text = ApplicationOptions.StatisticsForegroundError;
            StatisticsForegroundIndeterminate.Text = ApplicationOptions.StatisticsForegroundIndeterminate;
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
            SaveThemeOptions();
            SaveWindowOptions();

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
                && ValidateThemeOptions();
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
                ApplicationOptions.HighLatencyAlertTriggerCount = int.Parse(HighLatencyTriggerCount.Text);
            }
        }

        private bool ValidateStartupOptions()
        {
            if (WindowSizeFixedOption.IsChecked == true)
            {
                if (!IsInRange(FixedWindowWidth.Text, 350, 9999))
                {
                    ShowError("Please enter a window width between 350 and 9999.", StartupTab, FixedWindowWidth);
                    return false;
                }
                if (!IsInRange(FixedWindowHeight.Text, 200, 9999))
                {
                    ShowError("Please enter a window height between 200 and 9999.", StartupTab, FixedWindowHeight);
                    return false;
                }
            }

            if (WindowPositionFixedOption.IsChecked == true)
            {
                if (!IsInRange(FixedWindowLeft.Text, -9999, 9999))
                {
                    ShowError("Please enter a window left position between -9999 and 9999.", StartupTab, FixedWindowLeft);
                    return false;
                }
                if (!IsInRange(FixedWindowTop.Text, -9999, 9999))
                {
                    ShowError("Please enter a window top position between -9999 and 9999.", StartupTab, FixedWindowTop);
                    return false;
                }
            }

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

            ApplicationOptions.IsStartMinimizedEnabled = IsStartMinimizedEnabled.IsChecked == true;

            if (WindowSizeFixedOption.IsChecked == true)
            {
                ApplicationOptions.WindowSizeOption = WindowStartupOption.Fixed;
                ApplicationOptions.WindowWidth = int.Parse(FixedWindowWidth.Text);
                ApplicationOptions.WindowHeight = int.Parse(FixedWindowHeight.Text);
            }
            else
            {
                ApplicationOptions.WindowSizeOption = WindowStartupOption.Remember;
            }

            if (WindowPositionFixedOption.IsChecked == true)
            {
                ApplicationOptions.WindowPositionOption = WindowStartupOption.Fixed;
                ApplicationOptions.WindowLeft = int.Parse(FixedWindowLeft.Text);
                ApplicationOptions.WindowTop = int.Parse(FixedWindowTop.Text);
            }
            else
            {
                ApplicationOptions.WindowPositionOption = WindowPositionRememberOption.IsChecked == true
                    ? WindowStartupOption.Remember
                    : WindowStartupOption.Default;
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
            if (UseCustomPacketOption.IsChecked == true && HasControlCharacters(PacketData.Text))
            {
                ShowError("Custom packet data can't contain control characters, other than tabs and new lines.", AdvancedTab, PacketData);
                return false;
            }

            return true;
        }

        private void SaveAdvancedOptions()
        {
            ApplicationOptions.TTL = int.Parse(TTL.Text);

            ApplicationOptions.UseCustomPacketData = UseCustomPacketOption.IsChecked == true;
            if (ApplicationOptions.UseCustomPacketData)
            {
                ApplicationOptions.CustomPacketData = PacketData.Text;
            }
            else
            {
                ApplicationOptions.PacketSize = int.Parse(PacketSize.Text);
            }

            ApplicationOptions.DontFragment = DontFragment.IsChecked == true;

            // Update ping options (TTL, don't fragment and ping data).
            ApplicationOptions.UpdatePingOptions();
        }

        // Control characters can't be saved to the config file. Tabs and new lines are fine.
        private static bool HasControlCharacters(string text)
        {
            foreach (char c in text)
            {
                if (c < ' ' && c != '\t' && c != '\r' && c != '\n')
                {
                    return true;
                }
            }
            return false;
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
            // The folder only matters when a log is turned on.
            if (IsLogOutputEnabled.IsChecked != true && IsLogStatusChangesEnabled.IsChecked != true)
            {
                return true;
            }

            string folder = LogFolder.Text.Trim();
            if (!Directory.Exists(folder))
            {
                ShowError("The log folder does not exist. Please choose an existing folder.", LogOutputTab, LogFolder);
                return false;
            }

            // Check now that vmPing can write there, instead of failing later when the first line is logged.
            if (!CanWriteToFolder(folder, out string error))
            {
                ShowError($"vmPing can't write to the log folder. {error}", LogOutputTab, LogFolder);
                return false;
            }

            return true;
        }

        // Creates a temporary file in the folder to check that it can be written to.
        // The file is deleted as soon as it's closed.
        private static bool CanWriteToFolder(string folder, out string error)
        {
            try
            {
                string testPath = Path.Combine(folder, Path.GetRandomFileName());
                using (new FileStream(testPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
                {
                }
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private void SaveLogOutputOptions()
        {
            ApplicationOptions.LogFolder = LogFolder.Text.Trim();
            ApplicationOptions.IsLogOutputEnabled = IsLogOutputEnabled.IsChecked == true;
            ApplicationOptions.IsLogStatusChangesEnabled = IsLogStatusChangesEnabled.IsChecked == true;
        }

        private void SaveWindowOptions()
        {
            ApplicationOptions.IsAlwaysOnTopEnabled = IsAlwaysOnTopEnabled.IsChecked == true;
            ApplicationOptions.IsMinimizeToTrayEnabled = IsMinimizeToTrayEnabled.IsChecked == true;
            ApplicationOptions.IsExitToTrayEnabled = IsExitToTrayEnabled.IsChecked == true;
            ApplicationOptions.IsAlwaysShowTrayIconEnabled = IsAlwaysShowTrayIconEnabled.IsChecked == true;
        }

        private bool ValidateThemeOptions()
        {
            foreach (Visual control in ThemeDockPanel.GetChildren())
            {
                if (control is TextBox box && !Util.IsValidHtmlColor(box.Text))
                {
                    ShowError("Please enter a valid HTML color code.  Accepted formats are #RGB, #RRGGBB, and #AARRGGBB.  Example: #3266CF", ThemeTab, box);
                    box.SelectAll();
                    return false;
                }
            }

            return true;
        }

        private void SaveThemeOptions()
        {
            if (ThemeLightOption.IsChecked == true)
            {
                ApplicationOptions.Theme = ThemeMode.Light;
            }
            else if (ThemeDarkOption.IsChecked == true)
            {
                ApplicationOptions.Theme = ThemeMode.Dark;
            }
            else
            {
                ApplicationOptions.Theme = ThemeMode.System;
            }
            ThemeManager.SetMode(ApplicationOptions.Theme);

            ApplicationOptions.ProbeBackgroundInactive = ProbeBackgroundInactive.Text;
            ApplicationOptions.ProbeBackgroundUp = ProbeBackgroundUp.Text;
            ApplicationOptions.ProbeBackgroundDown = ProbeBackgroundDown.Text;
            ApplicationOptions.ProbeBackgroundIndeterminate = ProbeBackgroundIndeterminate.Text;
            ApplicationOptions.ProbeBackgroundError = ProbeBackgroundError.Text;
            ApplicationOptions.ProbeForegroundInactive = ProbeForegroundInactive.Text;
            ApplicationOptions.ProbeForegroundUp = ProbeForegroundUp.Text;
            ApplicationOptions.ProbeForegroundDown = ProbeForegroundDown.Text;
            ApplicationOptions.ProbeForegroundIndeterminate = ProbeForegroundIndeterminate.Text;
            ApplicationOptions.ProbeForegroundError = ProbeForegroundError.Text;
            ApplicationOptions.StatisticsForegroundInactive = StatisticsForegroundInactive.Text;
            ApplicationOptions.StatisticsForegroundUp = StatisticsForegroundUp.Text;
            ApplicationOptions.StatisticsForegroundDown = StatisticsForegroundDown.Text;
            ApplicationOptions.StatisticsForegroundIndeterminate = StatisticsForegroundIndeterminate.Text;
            ApplicationOptions.StatisticsForegroundError = StatisticsForegroundError.Text;
        }

        // Allows digits and a minus sign, for positions left of or above the main monitor.
        private void SignedNumericTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9-]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void NumericTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void HtmlColor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[#a-fA-F0-9]");
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
            string serverAddress = SmtpServer.Text;
            string serverPort = SmtpPort.Text;
            bool isSslEnabled = IsSmtpSslEnabled.IsChecked == true;
            bool isAuthRequired = IsSmtpAuthenticationRequired.IsChecked == true;
            string username = SmtpUsername.Text;
            System.Security.SecureString password = SmtpPassword.SecurePassword;
            string mailFrom = EmailFromAddress.Text;
            string mailRecipient = EmailRecipientAddress.Text;

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
                                DialogWindow dialogWindow = new DialogWindow(
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

        private void BrowseLogFolder_Click(object sender, RoutedEventArgs e)
        {
            string folder = FolderPicker.Show(this, "Select a folder for the log files", LogFolder.Text.Trim());
            if (folder != null)
            {
                LogFolder.Text = folder;
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
            using (System.Windows.Forms.OpenFileDialog audiofileDialog = new System.Windows.Forms.OpenFileDialog())
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
                using (SoundPlayer player = new SoundPlayer(path))
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
            Regex regex = new Regex("^\\d+$");
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
            ProbeBackgroundInactive.Text = Constants.DefaultProbeBackgroundInactive;
            ProbeBackgroundUp.Text = Constants.DefaultProbeBackgroundUp;
            ProbeBackgroundDown.Text = Constants.DefaultProbeBackgroundDown;
            ProbeBackgroundError.Text = Constants.DefaultProbeBackgroundError;
            ProbeBackgroundIndeterminate.Text = Constants.DefaultProbeBackgroundIndeterminate;
            ProbeForegroundInactive.Text = Constants.DefaultProbeForegroundInactive;
            ProbeForegroundUp.Text = Constants.DefaultProbeForegroundUp;
            ProbeForegroundDown.Text = Constants.DefaultProbeForegroundDown;
            ProbeForegroundError.Text = Constants.DefaultProbeForegroundError;
            ProbeForegroundIndeterminate.Text = Constants.DefaultProbeForegroundIndeterminate;
            StatisticsForegroundInactive.Text = Constants.DefaultStatisticsForegroundInactive;
            StatisticsForegroundUp.Text = Constants.DefaultStatisticsForegroundUp;
            StatisticsForegroundDown.Text = Constants.DefaultStatisticsForegroundDown;
            StatisticsForegroundError.Text = Constants.DefaultStatisticsForegroundError;
            StatisticsForegroundIndeterminate.Text = Constants.DefaultStatisticsForegroundIndeterminate;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideMinimizeAndMaximizeButtons(this);
        }
    }
}