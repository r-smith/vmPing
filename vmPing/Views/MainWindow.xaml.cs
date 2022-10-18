using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Probe> _ProbeCollection = new ObservableCollection<Probe>();
        private Dictionary<string, string> _Aliases = new Dictionary<string, string>();
        private ObservableCollection<ProbeStatus> _TrayNegativeStatusList = new ObservableCollection<ProbeStatus>();

        private System.Windows.Forms.NotifyIcon NotifyIcon;

        public static RoutedCommand OptionsCommand = new RoutedCommand();
        public static RoutedCommand StartStopCommand = new RoutedCommand();
        public static RoutedCommand HelpCommand = new RoutedCommand();
        public static RoutedCommand NewInstanceCommand = new RoutedCommand();
        public static RoutedCommand TracerouteCommand = new RoutedCommand();
        public static RoutedCommand FloodHostCommand = new RoutedCommand();
        public static RoutedCommand AddProbeCommand = new RoutedCommand();
        public static RoutedCommand MultiInputCommand = new RoutedCommand();
        public static RoutedCommand StatusHistoryCommand = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
            InitializeAplication();
        }

        private void InitializeAplication()
        {
            InitializeCommandBindings();
            LoadFavorites();
            LoadAliases();
            Configuration.Load();
            RefreshGuiState();

            // Set items source for main GUI ItemsControl.
            ProbeItemsControl.ItemsSource = _ProbeCollection;

            _TrayNegativeStatusList.Add(ProbeStatus.Down);
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
                    _ProbeCollection[i].Hostname = cliHosts[i];
                    _ProbeCollection[i].Alias = _Aliases.ContainsKey(_ProbeCollection[i].Hostname.ToLower())
                        ? _Aliases[_ProbeCollection[i].Hostname.ToLower()]
                        : null;
                    _ProbeCollection[i].StartStop();
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

                // Determine statup mode.
                switch (ApplicationOptions.InitialStartMode)
                {
                    case ApplicationOptions.StartMode.MultiInput:
                        RefreshColumnCount();
                        MultiInputWindowExecute(null, null);
                        break;
                    case ApplicationOptions.StartMode.Favorite:
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
                case ApplicationOptions.PopupNotificationOption.Always:
                    PopupAlways.IsChecked = true;
                    break;
                case ApplicationOptions.PopupNotificationOption.Never:
                    PopupNever.IsChecked = true;
                    break;
                case ApplicationOptions.PopupNotificationOption.WhenMinimized:
                    PopupWhenMinimized.IsChecked = true;
                    break;
            }

            // Set always on top state.
            Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
            if (Probe.StatusWindow != null && Probe.StatusWindow.IsLoaded)
            {
                Probe.StatusWindow.Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
            }
            if (HelpWindow._OpenWindow != null)
            {
                HelpWindow._OpenWindow.Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
            }
            foreach (Probe probe in _ProbeCollection)
            {
                if (probe.IsolatedWindow != null && probe.IsolatedWindow.IsLoaded)
                {
                    probe.IsolatedWindow.Topmost = ApplicationOptions.IsAlwaysOnTopEnabled;
                }
            }
        }

        private void RefreshColumnCount()
        {
            // Update ColumnCount.Tag to be whichever is lower: ColumnCount.Value or _ProbeCollection.Count.
            // The actual number of grid columns is bound to the tag value.
            ColumnCount.Tag = ColumnCount.Value > _ProbeCollection.Count
                ? _ProbeCollection.Count
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
                var probe = new Probe();
                _ProbeCollection.Add(probe);
                probe.PropertyChanged += ProbeOnPropertyChanged;
            }

        }

        private void ProbeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (NotifyIcon == null)
                return;

            if (e.PropertyName == "Status")
            {
                // Status changed, recalculate color of tray icon

                var green = true;

                foreach (var probe in _ProbeCollection)
                {
                    if (_TrayNegativeStatusList.Contains(probe.Status))
                        green = false;
                }

                if (green)
                {
                    NotifyIcon.Icon = new System.Drawing.Icon(Application
                        .GetResourceStream(new Uri("pack://application:,,,/vmPing-green.ico")).Stream);
                }
                else
                {
                    NotifyIcon.Icon = new System.Drawing.Icon(Application
                        .GetResourceStream(new Uri("pack://application:,,,/vmPing.ico")).Stream);
                }
            }
        }

        public void ProbeStartStop_Click(object sender, EventArgs e)
        {
            ((Probe)((Button)sender).DataContext).StartStop();
        }

        private void ColumnCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // When the ColumnCount slider value is changed, update Tag to be the lesser of
            // ColumnCount.Value and _ProbeCollection.Count.
            // The visual column count is bound to the Tag value.
            RefreshColumnCount();
        }

        private void Hostname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var probe = (sender as TextBox).DataContext as Probe;
                probe.StartStop();

                if (_ProbeCollection.IndexOf(probe) < _ProbeCollection.Count - 1)
                {
                    var cp = ProbeItemsControl.ItemContainerGenerator.ContainerFromIndex(_ProbeCollection.IndexOf(probe) + 1) as ContentPresenter;
                    var tb = (TextBox)cp.ContentTemplate.FindName("Hostname", cp);

                    if (tb != null)
                        tb.Focus();
                }
            }
        }

        private void RemoveProbe_Click(object sender, RoutedEventArgs e)
        {
            if (_ProbeCollection.Count <= 1)
                return;

            var probe = (sender as Button).DataContext as Probe;
            if (probe.IsActive)
            {
                // Stop/cancel active probe.
                probe.StartStop();
            }
            _ProbeCollection.Remove(probe);
            RefreshColumnCount();
        }

        private void MultiInputWindowExecute(object sender, ExecutedRoutedEventArgs e)
        {
            // Get list of current addresses to send to multi-input window.
            var addresses = new List<string>();
            for (int i = 0; i < _ProbeCollection.Count; ++i)
            {
                if (!string.IsNullOrWhiteSpace(_ProbeCollection[i].Hostname))
                    addresses.Add(_ProbeCollection[i].Hostname.Trim());
            }

            var wnd = new MultiInputWindow(addresses);
            wnd.Owner = this;
            if (wnd.ShowDialog() == true)
            {
                RemoveAllProbes();

                if (wnd.Addresses.Count < 1)
                    AddProbe();
                else
                {
                    AddProbe(numberOfProbes: wnd.Addresses.Count);
                    for (int i = 0; i < wnd.Addresses.Count; ++i)
                    {
                        _ProbeCollection[i].Hostname = wnd.Addresses[i];
                        _ProbeCollection[i].Alias = _Aliases.ContainsKey(_ProbeCollection[i].Hostname.ToLower())
                            ? _Aliases[_ProbeCollection[i].Hostname.ToLower()]
                            : null;
                        _ProbeCollection[i].StartStop();
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

            foreach (var probe in _ProbeCollection)
            {
                if (toggleStatus == Strings.Toolbar_StopAll && probe.IsActive)
                    probe.StartStop();
                else if (toggleStatus == Strings.Toolbar_StartAll && !probe.IsActive)
                    probe.StartStop();
            }
        }

        private void HelpExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (HelpWindow._OpenWindow == null)
            {
                new HelpWindow().Show();
            }
            else
            {
                HelpWindow._OpenWindow.Activate();
            }
        }

        private void NewInstanceExecute(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName =
                    System.Reflection.Assembly.GetExecutingAssembly().Location;
                p.Start();
            }

            catch (Exception ex)
            {
                var errorWindow = DialogWindow.ErrorWindow($"{Strings.Error_FailedToLaunch} {ex.Message}");
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
            _ProbeCollection.Add(new Probe());
            RefreshColumnCount();
        }

        private void OptionsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            // Open the options window.
            var optionsWnd = new OptionsWindow();
            optionsWnd.Owner = this;
            if (optionsWnd.ShowDialog() == true)
            {
                RefreshGuiState();
                RefreshProbeColors();
            }
        }

        private void RefreshProbeColors()
        {
            for (int i = 0; i < _ProbeCollection.Count; ++i)
                _ProbeCollection[i].Status = _ProbeCollection[i].Status;
        }

        private void RemoveAllProbes()
        {
            foreach (var probe in _ProbeCollection)
            {
                if (probe.IsActive)
                    probe.StartStop();
            }
            _ProbeCollection.Clear();
            Probe.ActiveCount = 0;
        }

        private void LoadFavorites()
        {
            // Clear existing favorites menu.
            for (int i = FavoritesMenu.Items.Count - 1; i > 2; --i)
                FavoritesMenu.Items.RemoveAt(i);

            // Load favorites.
            foreach (var fav in Favorite.GetTitles())
            {
                var menuItem = new MenuItem();
                menuItem.Header = fav;
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

            var favorite = Favorite.GetContents(favoriteTitle);
            if (favorite.Hostnames.Count < 1)
                AddProbe();
            else
            {
                AddProbe(numberOfProbes: favorite.Hostnames.Count);
                for (int i = 0; i < favorite.Hostnames.Count; ++i)
                {
                    _ProbeCollection[i].Hostname = favorite.Hostnames[i];
                    _ProbeCollection[i].Alias = _Aliases.ContainsKey(_ProbeCollection[i].Hostname.ToLower())
                        ? _Aliases[_ProbeCollection[i].Hostname.ToLower()]
                        : null;
                    _ProbeCollection[i].StartStop();
                }
            }

            ColumnCount.Value = 1;  // Ensure window's grid column binding is updated, if needed.
            ColumnCount.Value = favorite.ColumnCount;
            this.Title = $"{favoriteTitle} - vmPing";
        }

        private void LoadAliases()
        {
            _Aliases = Alias.GetAliases();
            var aliasList = _Aliases.ToList();
            aliasList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            // Clear existing aliases menu.
            for (int i = AliasesMenu.Items.Count - 1; i > 1; --i)
                AliasesMenu.Items.RemoveAt(i);

            // Load aliases.
            foreach (var alias in aliasList)
            {
                AliasesMenu.Items.Add(BuildAliasMenuItem(alias, false));
            }

            foreach (var probe in _ProbeCollection)
            {
                probe.Alias = probe.Hostname != null && _Aliases.ContainsKey(probe.Hostname.ToLower())
                    ? _Aliases[probe.Hostname.ToLower()]
                    : string.Empty;
            }
        }

        private MenuItem BuildAliasMenuItem(KeyValuePair<string, string> alias, bool isContextMenu)
        {
            var menuItem = new MenuItem();
            menuItem.Header = alias.Value;

            if (isContextMenu)
            {
                menuItem.Click += (s, r) =>
                {
                    var selectedMenuItem = s as MenuItem;
                    var selectedAlias = (Probe)selectedMenuItem.DataContext;
                    selectedAlias.Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedMenuItem.Header.ToString()).Key;
                    selectedAlias.StartStop();
                };
            }
            else
            {
                menuItem.Click += (s, r) =>
                {
                    var selectedAlias = s as MenuItem;

                    var didFindEmptyHost = false;
                    for (int i = 0; i < _ProbeCollection.Count; ++i)
                    {
                        if (string.IsNullOrWhiteSpace(_ProbeCollection[i].Hostname))
                        {
                            _ProbeCollection[i].Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedAlias.Header.ToString()).Key;
                            _ProbeCollection[i].StartStop();
                            didFindEmptyHost = true;
                            break;
                        }
                    }

                    if (!didFindEmptyHost)
                    {
                        AddProbe();
                        _ProbeCollection[_ProbeCollection.Count - 1].Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedAlias.Header.ToString()).Key;
                        _ProbeCollection[_ProbeCollection.Count - 1].StartStop();
                    }
                };
            }

            return menuItem;
        }

        private void CreateFavorite_Click(object sender, RoutedEventArgs e)
        {
            // Display new favorite window => Pass in current addresses and column count.
            // If window title ends with " - vmPing", then user currently has a
            // favorite loaded. Pass the title of that favorite to the new window.
            const string favTitle = " - vmPing";
            var newFavoriteWindow = new NewFavoriteWindow(
                hostList: _ProbeCollection.Select(x => x.Hostname).ToList(),
                columnCount: (int)ColumnCount.Value,
                title: Title.EndsWith(favTitle) ? Title.Remove(Title.Length - favTitle.Length) : string.Empty);
            newFavoriteWindow.Owner = this;
            if (newFavoriteWindow.ShowDialog() == true)
            {
                LoadFavorites();
            }
        }

        private void ManageFavorites_Click(object sender, RoutedEventArgs e)
        {
            // Open the favorites window.
            var manageFavoritesWindow = new ManageFavoritesWindow();
            manageFavoritesWindow.Owner = this;
            manageFavoritesWindow.ShowDialog();
            LoadFavorites();
        }

        private void ManageAliases_Click(object sender, RoutedEventArgs e)
        {
            // Open the aliases window.
            var manageAliasesWindow = new ManageAliasesWindow();
            manageAliasesWindow.Owner = this;
            manageAliasesWindow.ShowDialog();
            LoadAliases();
        }

        private void PopupAlways_Click(object sender, RoutedEventArgs e)
        {
            PopupAlways.IsChecked = true;
            PopupNever.IsChecked = false;
            PopupWhenMinimized.IsChecked = false;
            ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Always;
        }

        private void PopupNever_Click(object sender, RoutedEventArgs e)
        {
            PopupAlways.IsChecked = false;
            PopupNever.IsChecked = true;
            PopupWhenMinimized.IsChecked = false;
            ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Never;
        }

        private void PopupWhenMinimized_Click(object sender, RoutedEventArgs e)
        {
            PopupAlways.IsChecked = false;
            PopupNever.IsChecked = false;
            PopupWhenMinimized.IsChecked = true;
            ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.WhenMinimized;
        }

        private void IsolatedView_Click(object sender, RoutedEventArgs e)
        {
            var probe = (sender as Button).DataContext as Probe;
            if (probe.IsolatedWindow == null || probe.IsolatedWindow.IsLoaded == false)
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
            var probe = (sender as Button).DataContext as Probe;

            if (string.IsNullOrEmpty(probe.Hostname))
                return;

            if (_Aliases.ContainsKey(probe.Hostname.ToLower()))
                probe.Alias = _Aliases[probe.Hostname.ToLower()];
            else
                probe.Alias = string.Empty;

            var wnd = new EditAliasWindow(probe);
            wnd.Owner = this;

            if (wnd.ShowDialog() == true)
            {
                LoadAliases();
            }
            Focus();
        }

        private void StatusHistoryExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (Probe.StatusWindow == null || Probe.StatusWindow.IsLoaded == false)
            {
                var wnd = new StatusHistoryWindow(Probe.StatusChangeLog);
                Probe.StatusWindow = wnd;
                wnd.Show();
            }
            else if (Probe.StatusWindow.IsLoaded)
            {
                Probe.StatusWindow.Focus();
            }
        }

        private void Hostname_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to textbox on newly added monitors.  If the hostname field is blank for any existing monitors, do not change focus.
            for (int i = 0; i < _ProbeCollection.Count - 1; ++i)
            {
                if (string.IsNullOrEmpty(_ProbeCollection[i].Hostname))
                    return;
            }
            ((TextBox)sender).Focus();
        }

        private void Hostname_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if there is an alias for the hostname as you type.
            var probe = (sender as TextBox).DataContext as Probe;
            if (probe.Hostname != null)
            {
                probe.Alias = _Aliases.ContainsKey(probe.Hostname.ToLower())
                    ? _Aliases[probe.Hostname.ToLower()]
                    : null;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Set initial focus first text box.
            if (_ProbeCollection.Count > 0)
            {
                var cp = ProbeItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
                var tb = (TextBox)cp.ContentTemplate.FindName("Hostname", cp);

                if (tb != null)
                    tb.Focus();
            }
        }

        private void Logo_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            // This event is tied to the background image that appears in each probe window.
            // After a probe is started, this event removes the image from the ItemsControl.
            var image = (sender as Image);
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
                var data = new DataObject();
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
            var source = e.Data.GetData("Source") as Probe;
            if (source != null)
            {
                int newIndex;
                if (sender is Label)
                {
                    newIndex = _ProbeCollection.IndexOf((sender as Label).DataContext as Probe);
                    e.Handled = true;
                }
                else if (sender is DockPanel)
                {
                    newIndex = _ProbeCollection.IndexOf((sender as DockPanel).DataContext as Probe);
                    e.Handled = true;
                }
                else
                {
                    return;
                }

                int prevIndex = _ProbeCollection.IndexOf(source);
                if (newIndex != prevIndex)
                {
                    _ProbeCollection.RemoveAt(prevIndex);
                    _ProbeCollection.Insert(newIndex, source);
                }
            }
        }

        private void HideToTray()
        {
            Visibility = Visibility.Hidden;
            WindowState = WindowState.Minimized;
            try
            {
                if (NotifyIcon == null)
                {
                    // Build context menu for tray icon.
                    System.Windows.Forms.ContextMenuStrip menuStrip = new System.Windows.Forms.ContextMenuStrip();
                    System.Windows.Forms.ToolStripMenuItem menuOptions = new System.Windows.Forms.ToolStripMenuItem("Options");
                    menuOptions.Click += (s, args) => OptionsExecute(null, null);
                    System.Windows.Forms.ToolStripMenuItem menuStatusHistory = new System.Windows.Forms.ToolStripMenuItem("Status History");
                    menuStatusHistory.Click += (s, args) => StatusHistoryExecute(null, null);
                    System.Windows.Forms.ToolStripMenuItem menuExit = new System.Windows.Forms.ToolStripMenuItem("Exit vmPing");
                    menuExit.Click += (s, args) =>
                    {
                        NotifyIcon.Dispose();
                        Application.Current.Shutdown();
                    };

                    menuStrip.Items.Add(menuOptions);
                    menuStrip.Items.Add(menuStatusHistory);
                    menuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
                    menuStrip.Items.Add(menuExit);

                    // Create tray icon.
                    NotifyIcon = new System.Windows.Forms.NotifyIcon
                    {
                        Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/vmPing-green.ico")).Stream),
                        Text = "vmPing",
                        ContextMenuStrip = menuStrip
                    };
                    NotifyIcon.MouseUp += NotifyIcon_MouseUp;
                }
                if (NotifyIcon != null)
                    NotifyIcon.Visible = true;
            }
            catch
            {
                Visibility = Visibility.Visible;
            }
        }

        private void RestoreFromTray()
        {
            NotifyIcon.Visible = false;
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
            if (IsVisible && NotifyIcon != null && NotifyIcon.Visible)
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
                mi.Invoke(NotifyIcon, null);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ApplicationOptions.IsExitToTrayEnabled)
            {
                HideToTray();
                e.Cancel = true;
            }
            else if (NotifyIcon != null)
            {
                NotifyIcon.Dispose();
            }
        }

        private void History_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.SelectionStart = (tb.DataContext as Probe).SelStart;
            tb.SelectionLength = (tb.DataContext as Probe).SelLength;
            if (!tb.IsMouseCaptureWithin && tb.SelectionLength == 0)
                tb.ScrollToEnd();
        }

        private void History_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            (tb.DataContext as Probe).SelStart = tb.SelectionStart;
            (tb.DataContext as Probe).SelLength = tb.SelectionLength;
        }
    }
}