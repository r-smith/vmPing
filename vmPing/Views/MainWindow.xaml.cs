using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Probes;
using vmPing.Properties;

namespace vmPing.Views
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Probe> _probes = new ObservableCollection<Probe>();
        private Dictionary<string, string> _aliases = new Dictionary<string, string>();
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public static readonly RoutedCommand OptionsCommand = new RoutedCommand();
        public static readonly RoutedCommand StartStopCommand = new RoutedCommand();
        public static readonly RoutedCommand HelpCommand = new RoutedCommand();
        public static readonly RoutedCommand NewInstanceCommand = new RoutedCommand();
        public static readonly RoutedCommand TracerouteCommand = new RoutedCommand();
        public static readonly RoutedCommand FloodHostCommand = new RoutedCommand();
        public static readonly RoutedCommand AddProbeCommand = new RoutedCommand();
        public static readonly RoutedCommand MultiInputCommand = new RoutedCommand();
        public static readonly RoutedCommand StatusHistoryCommand = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            InitializeCommandBindings();
            LoadFavorites();
            LoadAliases();
            ConfigFile.Load();
            RefreshGuiState();

            // Set items source for main GUI ItemsControl.
            ProbeItemsControl.ItemsSource = _probes;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set initial ColumnCount slider value.
            ColumnCount.Value = ApplicationOptions.InitialColumnCount > 0
                ? ApplicationOptions.InitialColumnCount
                : 2;

            // Parse command line arguments. Get any host addresses entered on command line.
            List<string> cliHosts = CommandLine.ParseArguments();

            // Add initial probes.
            if (cliHosts.Count > 0)
            {
                // Host addresses were entered on the command line.
                // Add addresses to probe collection and begin pinging.
                AddProbe(cliHosts.Count);
                for (int i = 0; i < cliHosts.Count; ++i)
                {
                    _probes[i].Hostname = cliHosts[i];
                    _probes[i].Alias = GetAlias(_probes[i].Hostname);
                    _probes[i].StartStop();
                }
            }
            else
            {
                // No addresses entered on the command line.
                // Add initial blank probes.
                AddProbe(
                    (ApplicationOptions.InitialProbeCount > 0)
                        ? ApplicationOptions.InitialProbeCount
                        : 2);

                // Determine startup mode.
                switch (ApplicationOptions.InitialStartMode)
                {
                    case StartMode.MultiInput:
                        RefreshColumnCount();
                        MultiInputWindowExecute(null, null);
                        break;
                    case StartMode.Favorite:
                        if (ApplicationOptions.InitialFavorite != null
                            && !string.IsNullOrWhiteSpace(ApplicationOptions.InitialFavorite))
                        {
                            LoadFavorite(ApplicationOptions.InitialFavorite);
                        }
                        break;
                }
            }

            RefreshColumnCount();
        }

        private void RefreshGuiState()
        {
            // Set popup option on menu bar.
            PopupAlways.IsChecked = false;
            PopupNever.IsChecked = false;
            PopupWhenMinimized.IsChecked = false;

            switch (ApplicationOptions.PopupOption)
            {
                case PopupNotificationOption.Always:
                    PopupAlways.IsChecked = true;
                    break;
                case PopupNotificationOption.Never:
                    PopupNever.IsChecked = true;
                    break;
                case PopupNotificationOption.WhenMinimized:
                    PopupWhenMinimized.IsChecked = true;
                    break;
            }

            // Set always on top state.
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
            if (Probe.StatusHistoryWindow != null && Probe.StatusHistoryWindow.IsLoaded)
            {
                Probe.StatusHistoryWindow.Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
            }
            if (HelpWindow.OpenWindow != null)
            {
                HelpWindow.OpenWindow.Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
            }
            foreach (Probe probe in _probes)
            {
                if (probe.IsolatedWindow != null && probe.IsolatedWindow.IsLoaded)
                {
                    probe.IsolatedWindow.Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
                }
            }
        }

        private void RefreshColumnCount()
        {
            // Update ColumnCount.Tag to be whichever is lower: ColumnCount.Value or _probes.Count.
            // The actual number of grid columns is bound to the tag value.
            ColumnCount.Tag = ColumnCount.Value > _probes.Count
                ? _probes.Count
                : (int)ColumnCount.Value;
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(OptionsCommand, OptionsExecute));
            CommandBindings.Add(new CommandBinding(StartStopCommand, StartStopExecute));
            CommandBindings.Add(new CommandBinding(HelpCommand, HelpExecute));
            CommandBindings.Add(new CommandBinding(NewInstanceCommand, NewInstanceExecute));
            CommandBindings.Add(new CommandBinding(TracerouteCommand, TracerouteExecute));
            CommandBindings.Add(new CommandBinding(FloodHostCommand, FloodHostExecute));
            CommandBindings.Add(new CommandBinding(AddProbeCommand, AddProbeExecute));
            CommandBindings.Add(new CommandBinding(MultiInputCommand, MultiInputWindowExecute));
            CommandBindings.Add(new CommandBinding(StatusHistoryCommand, StatusHistoryExecute));
            
            InputBindings.Add(new InputBinding(
                OptionsCommand,
                new KeyGesture(Key.F10)));
            InputBindings.Add(new InputBinding(
                StartStopCommand,
                new KeyGesture(Key.F5)));
            InputBindings.Add(new InputBinding(
                HelpCommand,
                new KeyGesture(Constants.HelpKeyBinding)));
            InputBindings.Add(new InputBinding(
                NewInstanceCommand,
                new KeyGesture(Key.N, ModifierKeys.Control)));
            InputBindings.Add(new InputBinding(
                TracerouteCommand,
                new KeyGesture(Key.T, ModifierKeys.Control)));
            InputBindings.Add(new InputBinding(
                FloodHostCommand,
                new KeyGesture(Key.F, ModifierKeys.Control)));
            InputBindings.Add(new InputBinding(
                AddProbeCommand,
                new KeyGesture(Key.A, ModifierKeys.Control)));
            InputBindings.Add(new InputBinding(
                MultiInputCommand,
                new KeyGesture(Key.F2)));
            InputBindings.Add(new InputBinding(
                StatusHistoryCommand,
                new KeyGesture(Constants.StatusHistoryKeyBinding)));

            OptionsMenu.Command = OptionsCommand;
            StartStopMenu.Command = StartStopCommand;
            HelpMenu.Command = HelpCommand;
            NewInstanceMenu.Command = NewInstanceCommand;
            TracerouteMenu.Command = TracerouteCommand;
            FloodHostMenu.Command = FloodHostCommand;
            AddProbeMenu.Command = AddProbeCommand;
            MultiInputMenu.Command = MultiInputCommand;
            StatusHistoryMenu.Command = StatusHistoryCommand;
        }

        public void AddProbe(int numberOfProbes = 1)
        {
            for (; numberOfProbes > 0; --numberOfProbes)
            {
                _probes.Add(new Probe());
            }
        }

        public void ProbeStartStop_Click(object sender, EventArgs e)
        {
            ((Probe)((Button)sender).DataContext).StartStop();
        }

        private void ColumnCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // When the ColumnCount slider value is changed, update Tag to be the lesser of
            // ColumnCount.Value and _probes.Count.
            // The visual column count is bound to the Tag value.
            RefreshColumnCount();
        }

        private void Hostname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Probe probe = (sender as TextBox).DataContext as Probe;
                probe.StartStop();

                if (_probes.IndexOf(probe) < _probes.Count - 1)
                {
                    ContentPresenter cp = ProbeItemsControl.ItemContainerGenerator.ContainerFromIndex(_probes.IndexOf(probe) + 1) as ContentPresenter;
                    TextBox tb = (TextBox)cp.ContentTemplate.FindName("Hostname", cp);
                    tb?.Focus();
                }
            }
        }

        private void RemoveProbe_Click(object sender, RoutedEventArgs e)
        {
            if (_probes.Count <= 1)
            {
                return;
            }

            Probe probe = (sender as Button).DataContext as Probe;
            if (probe.IsActive)
            {
                // Stop/cancel active probe.
                probe.StartStop();
            }
            _probes.Remove(probe);
            RefreshColumnCount();
        }

        private void MultiInputWindowExecute(object sender, ExecutedRoutedEventArgs e)
        {
            // Get list of current addresses to send to multi-input window.
            List<string> addresses = new List<string>();
            for (int i = 0; i < _probes.Count; ++i)
            {
                if (!string.IsNullOrWhiteSpace(_probes[i].Hostname))
                {
                    addresses.Add(_probes[i].Hostname.Trim());
                }
            }

            MultiInputWindow wnd = new MultiInputWindow(addresses)
            {
                Owner = this
            };
            if (wnd.ShowDialog() == true)
            {
                RemoveAllProbes();

                if (wnd.Addresses.Count < 1)
                {
                    AddProbe();
                }
                else
                {
                    AddProbe(numberOfProbes: wnd.Addresses.Count);
                    for (int i = 0; i < wnd.Addresses.Count; ++i)
                    {
                        _probes[i].Hostname = wnd.Addresses[i];
                        _probes[i].Alias = GetAlias(_probes[i].Hostname);
                        _probes[i].StartStop();
                    }
                }

                // Trigger refresh on ColumnCount (To update binding on window grid, if needed).
                double count = ColumnCount.Value;
                ColumnCount.Value = 1;
                ColumnCount.Value = count;
            }
        }

        private void StartStopExecute(object sender, ExecutedRoutedEventArgs e)
        {
            string toggleStatus = StartStopMenuHeader.Text;

            foreach (Probe probe in _probes)
            {
                if (toggleStatus == Strings.Toolbar_StopAll && probe.IsActive)
                {
                    probe.StartStop();
                }
                else if (toggleStatus == Strings.Toolbar_StartAll && !probe.IsActive)
                {
                    probe.StartStop();
                }
            }
        }

        private void HelpExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (HelpWindow.OpenWindow == null)
            {
                new HelpWindow().Show();
            }
            else
            {
                HelpWindow.OpenWindow.Activate();
            }
        }

        private void NewInstanceExecute(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName =
                    System.Reflection.Assembly.GetExecutingAssembly().Location;
                p.Start();
            }

            catch (Exception ex)
            {
                DialogWindow errorWindow = DialogWindow.ErrorWindow($"{Strings.Error_FailedToLaunch} {ex.Message}");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }

        private void TracerouteExecute(object sender, ExecutedRoutedEventArgs e)
        {
            new TracerouteWindow().Show();
        }

        private void FloodHostExecute(object sender, ExecutedRoutedEventArgs e)
        {
            new FloodHostWindow().Show();
        }

        private void AddProbeExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _probes.Add(new Probe());
            RefreshColumnCount();
        }

        private void OptionsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            // Open the options window.
            OptionsWindow optionsWnd = new OptionsWindow
            {
                Owner = this
            };
            if (optionsWnd.ShowDialog() == true)
            {
                RefreshGuiState();
                RefreshProbeColors();
            }
        }

        private void RefreshProbeColors()
        {
            for (int i = 0; i < _probes.Count; ++i)
            {
                _probes[i].Status = _probes[i].Status;
            }
        }

        private void RemoveAllProbes()
        {
            foreach (Probe probe in _probes)
            {
                if (probe.IsActive)
                {
                    probe.StartStop();
                }
            }
            _probes.Clear();
            Probe.ActiveCount = 0;
        }

        private void LoadFavorites()
        {
            // Clear existing favorites menu.
            for (int i = FavoritesMenu.Items.Count - 1; i > 2; --i)
            {
                FavoritesMenu.Items.RemoveAt(i);
            }

            // Load favorites.
            foreach (string fav in Favorite.GetTitles())
            {
                MenuItem menuItem = new MenuItem
                {
                    Header = fav
                };
                menuItem.Click += (s, r) =>
                {
                    LoadFavorite((s as MenuItem).Header.ToString());
                };

                FavoritesMenu.Items.Add(menuItem);
            }
        }

        private void LoadFavorite(string favoriteTitle)
        {
            RemoveAllProbes();

            Favorite favorite = Favorite.Load(favoriteTitle);
            if (favorite.Hostnames.Count < 1)
            {
                AddProbe();
            }
            else
            {
                AddProbe(numberOfProbes: favorite.Hostnames.Count);
                for (int i = 0; i < favorite.Hostnames.Count; ++i)
                {
                    _probes[i].Hostname = favorite.Hostnames[i];
                    _probes[i].Alias = GetAlias(_probes[i].Hostname);
                    _probes[i].StartStop();
                }
            }

            ColumnCount.Value = 1;  // Ensure window's grid column binding is updated, if needed.
            ColumnCount.Value = favorite.ColumnCount;
            Title = $"{favoriteTitle} - vmPing";
        }

        private void LoadAliases()
        {
            _aliases = Alias.GetAll();
            List<KeyValuePair<string, string>> aliasList = _aliases.ToList();
            aliasList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            // Clear existing aliases menu.
            for (int i = AliasesMenu.Items.Count - 1; i > 1; --i)
            {
                AliasesMenu.Items.RemoveAt(i);
            }

            // Load aliases.
            foreach (KeyValuePair<string, string> alias in aliasList)
            {
                AliasesMenu.Items.Add(BuildAliasMenuItem(alias));
            }

            foreach (Probe probe in _probes)
            {
                probe.Alias = GetAlias(probe.Hostname) ?? string.Empty;
            }
        }

        private string GetAlias(string hostname)
        {
            if (hostname != null && _aliases.TryGetValue(hostname, out string alias))
            {
                return alias;
            }
            return null;
        }

        private MenuItem BuildAliasMenuItem(KeyValuePair<string, string> alias)
        {
            MenuItem menuItem = new MenuItem
            {
                Header = alias.Value,
                Tag = alias.Key
            };

            menuItem.Click += (s, r) =>
            {
                MenuItem selectedAlias = s as MenuItem;

                bool didFindEmptyHost = false;
                for (int i = 0; i < _probes.Count; ++i)
                {
                    if (string.IsNullOrWhiteSpace(_probes[i].Hostname))
                    {
                        _probes[i].Hostname = (string)selectedAlias.Tag;
                        _probes[i].StartStop();
                        didFindEmptyHost = true;
                        break;
                    }
                }

                if (!didFindEmptyHost)
                {
                    AddProbe();
                    _probes[_probes.Count - 1].Hostname = (string)selectedAlias.Tag;
                    _probes[_probes.Count - 1].StartStop();
                }
            };

            return menuItem;
        }

        private void CreateFavorite_Click(object sender, RoutedEventArgs e)
        {
            // Display new favorite window => Pass in current addresses and column count.
            // If window title ends with " - vmPing", then user currently has a
            // favorite loaded. Pass the title of that favorite to the new window.
            const string TitleSuffix = " - vmPing";
            NewFavoriteWindow newFavoriteWindow = new NewFavoriteWindow(
                hostList: _probes.Select(x => x.Hostname).ToList(),
                columnCount: (int)ColumnCount.Value,
                title: Title.EndsWith(TitleSuffix) ? Title.Remove(Title.Length - TitleSuffix.Length) : string.Empty);
            newFavoriteWindow.Owner = this;
            if (newFavoriteWindow.ShowDialog() == true)
            {
                Title = $"{newFavoriteWindow.MyTitle.Text} - vmPing";
                LoadFavorites();
            }
        }

        private void ManageFavorites_Click(object sender, RoutedEventArgs e)
        {
            // Open the favorites window.
            ManageFavoritesWindow manageFavoritesWindow = new ManageFavoritesWindow
            {
                Owner = this
            };
            manageFavoritesWindow.ShowDialog();
            LoadFavorites();
        }

        private void ManageAliases_Click(object sender, RoutedEventArgs e)
        {
            // Open the aliases window.
            ManageAliasesWindow manageAliasesWindow = new ManageAliasesWindow
            {
                Owner = this
            };
            manageAliasesWindow.ShowDialog();
            LoadAliases();
        }

        private void PopupAlways_Click(object sender, RoutedEventArgs e)
        {
            PopupAlways.IsChecked = true;
            PopupNever.IsChecked = false;
            PopupWhenMinimized.IsChecked = false;
            ApplicationOptions.PopupOption = PopupNotificationOption.Always;
        }

        private void PopupNever_Click(object sender, RoutedEventArgs e)
        {
            PopupAlways.IsChecked = false;
            PopupNever.IsChecked = true;
            PopupWhenMinimized.IsChecked = false;
            ApplicationOptions.PopupOption = PopupNotificationOption.Never;
        }

        private void PopupWhenMinimized_Click(object sender, RoutedEventArgs e)
        {
            PopupAlways.IsChecked = false;
            PopupNever.IsChecked = false;
            PopupWhenMinimized.IsChecked = true;
            ApplicationOptions.PopupOption = PopupNotificationOption.WhenMinimized;
        }

        private void IsolatedView_Click(object sender, RoutedEventArgs e)
        {
            Probe probe = (sender as Button).DataContext as Probe;
            if (probe.IsolatedWindow == null || !probe.IsolatedWindow.IsLoaded)
            {
                new IsolatedPingWindow(probe).Show();
            }
            else if (probe.IsolatedWindow.IsLoaded)
            {
                probe.IsolatedWindow.Focus();
            }
        }

        private void EditAlias_Click(object sender, RoutedEventArgs e)
        {
            Probe probe = (sender as Button).DataContext as Probe;

            if (string.IsNullOrEmpty(probe.Hostname))
            {
                return;
            }

            probe.Alias = GetAlias(probe.Hostname) ?? string.Empty;

            EditAliasWindow wnd = new EditAliasWindow(probe)
            {
                Owner = this
            };

            if (wnd.ShowDialog() == true)
            {
                LoadAliases();
            }
            Focus();
        }

        private void StatusHistoryExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (Probe.StatusHistoryWindow == null || !Probe.StatusHistoryWindow.IsLoaded)
            {
                StatusHistoryWindow wnd = new StatusHistoryWindow(Probe.StatusChangeLog);
                Probe.StatusHistoryWindow = wnd;
                wnd.Show();
            }
            else if (Probe.StatusHistoryWindow.IsLoaded)
            {
                Probe.StatusHistoryWindow.Focus();
            }
        }

        private void Hostname_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to textbox on newly added monitors.  If the hostname field is blank for any existing monitors, do not change focus.
            for (int i = 0; i < _probes.Count - 1; ++i)
            {
                if (string.IsNullOrEmpty(_probes[i].Hostname))
                {
                    return;
                }
            }
            ((TextBox)sender).Focus();
        }

        private void Hostname_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if there is an alias for the hostname as you type.
            Probe probe = (sender as TextBox).DataContext as Probe;
            if (probe.Hostname != null)
            {
                probe.Alias = GetAlias(probe.Hostname);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Set initial focus first text box.
            if (_probes.Count > 0)
            {
                ContentPresenter cp = ProbeItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
                TextBox tb = (TextBox)cp.ContentTemplate.FindName("Hostname", cp);
                tb?.Focus();
            }
        }

        private void Logo_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            // This event is tied to the background image that appears in each probe window.
            // After a probe is started, this event removes the image from the ItemsControl.
            Image image = (sender as Image);
            if (image.Visibility == Visibility.Collapsed)
            {
                image.Visibility = Visibility.Collapsed;
                image.Source = null;
            }
        }

        private void ProbeTitle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject data = new DataObject();
                data.SetData("Source", (sender as Label).DataContext as Probe);
                DragDrop.DoDragDrop(sender as DependencyObject, data, DragDropEffects.Move);
                e.Handled = true;
            }
        }

        private void History_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void Probe_Drop(object sender, DragEventArgs e)
        {
            Probe source = e.Data.GetData("Source") as Probe;
            if (source != null)
            {
                int newIndex;
                if (sender is Label)
                {
                    newIndex = _probes.IndexOf((sender as Label).DataContext as Probe);
                    e.Handled = true;
                }
                else if (sender is DockPanel)
                {
                    newIndex = _probes.IndexOf((sender as DockPanel).DataContext as Probe);
                    e.Handled = true;
                }
                else
                {
                    return;
                }

                int prevIndex = _probes.IndexOf(source);
                if (newIndex != prevIndex)
                {
                    _probes.RemoveAt(prevIndex);
                    _probes.Insert(newIndex, source);
                }
            }
        }

        private void HideToTray()
        {
            Visibility = Visibility.Hidden;
            WindowState = WindowState.Minimized;
            try
            {
                if (_notifyIcon == null)
                {
                    // Build context menu for tray icon.
                    System.Windows.Forms.ContextMenuStrip menuStrip = new System.Windows.Forms.ContextMenuStrip();
                    System.Windows.Forms.ToolStripMenuItem menuOptions = new System.Windows.Forms.ToolStripMenuItem("Options");
                    menuOptions.Click += (s, args) => OptionsExecute(null, null);
                    System.Windows.Forms.ToolStripMenuItem menuStatusHistory = new System.Windows.Forms.ToolStripMenuItem("Status History");
                    menuStatusHistory.Click += (s, args) => StatusHistoryExecute(null, null);
                    System.Windows.Forms.ToolStripMenuItem menuExit = new System.Windows.Forms.ToolStripMenuItem("Exit vmPing");
                    menuExit.Click += (s, args) => Application.Current.Shutdown();

                    menuStrip.Items.Add(menuOptions);
                    menuStrip.Items.Add(menuStatusHistory);
                    menuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
                    menuStrip.Items.Add(menuExit);

                    // Create tray icon.
                    _notifyIcon = new System.Windows.Forms.NotifyIcon
                    {
                        Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/vmPing.ico")).Stream),
                        Text = "vmPing",
                        ContextMenuStrip = menuStrip
                    };
                    _notifyIcon.MouseUp += NotifyIcon_MouseUp;
                }
                _notifyIcon.Visible = true;
            }
            catch
            {
                Visibility = Visibility.Visible;
            }
        }

        private void RestoreFromTray()
        {
            _notifyIcon.Visible = false;
            WindowState = WindowState.Minimized;
            Visibility = Visibility.Visible;
            Show();
            WindowState = WindowState.Normal;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && ApplicationOptions.IsMinimizeToTrayEnabled)
            {
                HideToTray();
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Clear notify icon - handles notify icon cleanup when vmPing is minimized to tray
            // and user clicks a popup alert window to restore the vmPing window.
            if (IsVisible && _notifyIcon != null && _notifyIcon.Visible)
            {
                RestoreFromTray();
            }
        }

        private void NotifyIcon_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Left click. Restore application window.
                RestoreFromTray();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Right click. Display context menu.
                System.Reflection.MethodInfo mi = typeof(System.Windows.Forms.NotifyIcon)
                    .GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                mi.Invoke(_notifyIcon, null);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ApplicationOptions.IsExitToTrayEnabled)
            {
                HideToTray();
                e.Cancel = true;
            }
            else
            {
                _notifyIcon?.Dispose();
            }
        }
    }
}