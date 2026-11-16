using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using vmPing.Configuration;
using vmPing.Interop;
using vmPing.Probes;
using vmPing.Properties;

namespace vmPing.Views
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Probe> _probes = new ObservableCollection<Probe>();
        private Dictionary<string, string> _aliases = new Dictionary<string, string>();
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isHiddenToTray;
        private TextBox _probeTypeMenuHostname;
        private string _loadedFavoriteTitle;
        private object _dragHandle;
        private Point _dragStartPoint;
        private Probe _draggedProbe;

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
            ProbeTypeMenu.CustomPopupPlacementCallback = PlaceProbeTypeMenu;
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            InitializeCommandBindings();
            LoadFavorites();
            LoadAliases();
            ConfigFile.Load();
            ApplyStartupWindowPlacement();
            RefreshTrayIcon();

            // Set before the window is shown, so it opens minimized instead of appearing first.
            if (CommandLine.HasMinimizedArgument())
            {
                WindowState = WindowState.Minimized;
            }
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

            // Started minimized with minimize to tray on: move to the tray now that the window exists.
            if (WindowState == WindowState.Minimized && ApplicationOptions.IsMinimizeToTrayEnabled)
            {
                HideToTray();
            }
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

            // Probes fill the columns of a row before starting a new row.
            RefreshColumnCount();
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

        private void Hostname_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox hostname = (TextBox)sender;

            if (ProbeTypeMenu.IsOpen)
            {
                // Typing a probe type's shortcut key selects it.
                ListBoxItem item = ProbeTypeList.Items.Cast<ListBoxItem>()
                    .FirstOrDefault(x => string.Equals(TextSearch.GetText(x), e.Text, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    e.Handled = true;
                    SelectProbeType((ProbeType)item.Tag);
                    return;
                }

                // Any other key closes the menu and is typed as usual.
                ProbeTypeMenu.IsOpen = false;
            }

            // Typing / opens the probe type menu when the hostname box is empty or all its text is selected.
            // The hostname is kept, so the same host can be switched to another probe type.
            if (e.Text == "/" && hostname.SelectionLength == hostname.Text.Length)
            {
                e.Handled = true;
                OpenProbeTypeMenu(hostname);
            }
        }

        private void Hostname_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox hostname = (TextBox)sender;

            if (ProbeTypeMenu.IsOpen)
            {
                switch (e.Key)
                {
                    case Key.Up:
                    case Key.Down:
                        int count = ProbeTypeList.Items.Count;
                        int step = e.Key == Key.Down ? 1 : count - 1;
                        ProbeTypeList.SelectedIndex = (ProbeTypeList.SelectedIndex + step) % count;
                        e.Handled = true;
                        return;
                    case Key.Enter:
                        SelectProbeType((ProbeType)((ListBoxItem)ProbeTypeList.SelectedItem).Tag);
                        e.Handled = true;
                        return;
                    case Key.Escape:
                        ProbeTypeMenu.IsOpen = false;
                        e.Handled = true;
                        return;
                    case Key.Back:
                        ProbeTypeMenu.IsOpen = false;
                        e.Handled = true;
                        return;
                }
            }

            // Backspace in an empty hostname box switches back to ping.
            Probe probe = (Probe)hostname.DataContext;
            if (e.Key == Key.Back && hostname.Text.Length == 0 && probe.Type != ProbeType.Ping)
            {
                probe.Type = ProbeType.Ping;
                e.Handled = true;
            }
        }

        private void ProbeTypeLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            if (((Probe)label.DataContext).IsActive)
            {
                return;
            }

            // The label and the hostname box are in the same grid.
            TextBox hostname = ((Grid)label.Parent).Children.OfType<TextBox>().First();
            OpenProbeTypeMenu(hostname);
        }

        private void ProbeTypeList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(ProbeTypeList, (DependencyObject)e.OriginalSource) is ListBoxItem item)
            {
                SelectProbeType((ProbeType)item.Tag);
            }
        }

        private void OpenProbeTypeMenu(TextBox hostname)
        {
            // Keyboard focus stays in the hostname box. Its key handlers control the menu.
            _probeTypeMenuHostname = hostname;
            ProbeType currentType = ((Probe)hostname.DataContext).Type;
            ProbeTypeList.SelectedItem = ProbeTypeList.Items.Cast<ListBoxItem>().First(x => (ProbeType)x.Tag == currentType);
            ProbeTypeMenu.PlacementTarget = hostname;
            ProbeTypeMenu.IsOpen = true;
            hostname.Focus();
        }

        // Opens the menu below the hostname box, lined up with its left edge, or above it if there's no room.
        // Placement="Bottom" can line up with the right edge instead, depending on a Windows handedness setting.
        private static CustomPopupPlacement[] PlaceProbeTypeMenu(Size popupSize, Size targetSize, Point offset)
        {
            return new[]
            {
                new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Horizontal),
                new CustomPopupPlacement(new Point(0, -popupSize.Height), PopupPrimaryAxis.Horizontal)
            };
        }

        private void SelectProbeType(ProbeType type)
        {
            ProbeTypeMenu.IsOpen = false;
            ((Probe)_probeTypeMenuHostname.DataContext).Type = type;
            _probeTypeMenuHostname.Focus();
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
            AddProbe();
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
                RefreshTrayIcon();
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
                    SetHostFromFavoriteLine(_probes[i], favorite.Hostnames[i]);
                    _probes[i].StartStop();
                }
            }

            ColumnCount.Value = 1;  // Ensure window's grid column binding is updated, if needed.
            ColumnCount.Value = favorite.ColumnCount;
            LoadedFavoriteTitle = favoriteTitle;
        }

        // A favorite line is a host, a host with a label ("10.0.0.1 # switch"), or a comment ("# Core switches").
        private void SetHostFromFavoriteLine(Probe probe, string line)
        {
            int commentStart = line.IndexOf('#');
            if (commentStart > 0)
            {
                probe.Hostname = line.Substring(0, commentStart);
                probe.Label = line.Substring(commentStart + 1).Trim();
            }
            else
            {
                probe.Hostname = line;
            }
            probe.Alias = GetAlias(probe.Hostname);
        }

        // The title of the loaded favorite, shown as the window title. Null if no favorite is loaded.
        public string LoadedFavoriteTitle
        {
            get => _loadedFavoriteTitle;
            set
            {
                _loadedFavoriteTitle = value;
                Title = string.IsNullOrWhiteSpace(value) ? "vmPing" : value;
            }
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
                // Use the first empty probe, or add a new one.
                Probe probe = _probes.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Hostname));
                if (probe == null)
                {
                    AddProbe();
                    probe = _probes[_probes.Count - 1];
                }

                // Set the alias here. A new probe has no hostname box yet to look it up as the text changes.
                probe.Hostname = alias.Key;
                probe.Alias = alias.Value;
                probe.StartStop();
            };

            return menuItem;
        }

        private void CreateFavorite_Click(object sender, RoutedEventArgs e)
        {
            // Display new favorite window => Pass in current addresses and column count.
            // If a favorite is loaded, pass its title to the new window.
            NewFavoriteWindow newFavoriteWindow = new NewFavoriteWindow(
                hostList: _probes.Select(x => string.IsNullOrEmpty(x.Label) ? x.Hostname : $"{x.Hostname} # {x.Label}").ToList(),
                columnCount: (int)ColumnCount.Value,
                title: LoadedFavoriteTitle ?? string.Empty);
            newFavoriteWindow.Owner = this;
            if (newFavoriteWindow.ShowDialog() == true)
            {
                LoadedFavoriteTitle = newFavoriteWindow.MyTitle.Text;
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

        private void Comment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Comments are edited in the probe's hostname box.
            if (((FrameworkElement)sender).FindName("Hostname") is TextBox hostname)
            {
                // Allow focus until the box loses it again.
                hostname.Focusable = true;
                hostname.Focus();
                hostname.CaretIndex = hostname.Text.Length;
            }
        }

        private void Hostname_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Undo the focus allowed when a comment was clicked, so the style decides again.
            ((TextBox)sender).ClearValue(FocusableProperty);
        }

        private void Hostname_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if there is an alias for the hostname as you type.
            TextBox hostname = (TextBox)sender;
            Probe probe = (Probe)hostname.DataContext;
            if (probe.Hostname != null)
            {
                probe.Alias = GetAlias(probe.Hostname);
            }

            // A label from a favorite named the old host, so editing the hostname clears it.
            // Only when the user is typing, not when the hostname is set from code.
            if (hostname.IsKeyboardFocused)
            {
                probe.Label = null;
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

        // A probe is moved by dragging its title bar. It moves live as it's dragged over
        // other probes, so dropping it just leaves it where it is.

        private void ProbeTitle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragHandle = sender;
            _dragStartPoint = e.GetPosition(this);
        }

        private void ProbeTitle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || sender != _dragHandle)
            {
                return;
            }

            // Only start once the mouse has moved a few pixels, so a click doesn't start a drag.
            Vector moved = e.GetPosition(this) - _dragStartPoint;
            if (Math.Abs(moved.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(moved.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            _dragHandle = null;
            _draggedProbe = (Probe)((FrameworkElement)sender).DataContext;
            int startIndex = _probes.IndexOf(_draggedProbe);
            SetProbeOpacity(_draggedProbe, 0.4);

            DataObject data = new DataObject();
            data.SetData("Source", _draggedProbe);
            DragDropEffects result = DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);

            // Dropped outside the probes, or cancelled with Escape: put the probe back.
            if (result == DragDropEffects.None)
            {
                int index = _probes.IndexOf(_draggedProbe);
                if (index != startIndex)
                {
                    _probes.Move(index, startIndex);
                }
            }

            SetProbeOpacity(_draggedProbe, 1.0);
            _draggedProbe = null;
            e.Handled = true;
        }

        private void Probe_DragEnter(object sender, DragEventArgs e)
        {
            Probe target = ((FrameworkElement)sender).DataContext as Probe;
            if (_draggedProbe == null || target == null || target == _draggedProbe)
            {
                return;
            }

            _probes.Move(_probes.IndexOf(_draggedProbe), _probes.IndexOf(target));

            // The move can give the probe a new container, so fade it again once the layout is updated.
            Probe draggedProbe = _draggedProbe;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_draggedProbe == draggedProbe)
                {
                    SetProbeOpacity(draggedProbe, 0.4);
                }
            }), DispatcherPriority.Loaded);
        }

        private void Probe_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = _draggedProbe != null ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void Probe_Drop(object sender, DragEventArgs e)
        {
            // The probe was already moved while dragging.
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void SetProbeOpacity(Probe probe, double opacity)
        {
            if (ProbeItemsControl.ItemContainerGenerator.ContainerFromItem(probe) is UIElement container)
            {
                container.Opacity = opacity;
            }
        }

        private void HideToTray()
        {
            try
            {
                CreateTrayIcon();
                _notifyIcon.Visible = true;
            }
            catch
            {
                // Without a tray icon there'd be no way back to the window, so keep it shown.
                return;
            }

            Visibility = Visibility.Hidden;
            WindowState = WindowState.Minimized;
            _isHiddenToTray = true;
        }

        private void RestoreFromTray()
        {
            _isHiddenToTray = false;
            WindowState = WindowState.Minimized;
            Visibility = Visibility.Visible;
            Show();
            WindowState = WindowState.Normal;
            RefreshTrayIcon();
        }

        // Shows the tray icon if it's set to always show or the window is hidden to the tray. Otherwise hides it.
        private void RefreshTrayIcon()
        {
            bool shouldShow = ApplicationOptions.IsAlwaysShowTrayIconEnabled || _isHiddenToTray;
            if (shouldShow)
            {
                try
                {
                    CreateTrayIcon();
                }
                catch
                {
                    return;
                }
            }

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = shouldShow;
            }
        }

        private void CreateTrayIcon()
        {
            if (_notifyIcon != null)
            {
                return;
            }

            // Build context menu for tray icon.
            System.Windows.Forms.ContextMenuStrip menuStrip = new System.Windows.Forms.ContextMenuStrip();
            System.Windows.Forms.ToolStripMenuItem menuOptions = new System.Windows.Forms.ToolStripMenuItem("Options");
            menuOptions.Click += (s, args) => OptionsExecute(null, null);
            System.Windows.Forms.ToolStripMenuItem menuStatusHistory = new System.Windows.Forms.ToolStripMenuItem("Status History");
            menuStatusHistory.Click += (s, args) => StatusHistoryExecute(null, null);
            System.Windows.Forms.ToolStripMenuItem menuExit = new System.Windows.Forms.ToolStripMenuItem("Exit vmPing");
            menuExit.Click += (s, args) =>
            {
                SaveWindowPlacement();
                _notifyIcon.Dispose();
                Application.Current.Shutdown();
            };

            menuStrip.Items.Add(menuOptions);
            menuStrip.Items.Add(menuStatusHistory);
            menuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menuStrip.Items.Add(menuExit);

            // Create tray icon.
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(
                    Application.GetResourceStream(new Uri("pack://application:,,,/vmPing.ico")).Stream,
                    System.Windows.Forms.SystemInformation.SmallIconSize),
                Text = "vmPing",
                ContextMenuStrip = menuStrip
            };
            _notifyIcon.MouseUp += NotifyIcon_MouseUp;
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
            // Handles a popup alert showing the window while it's hidden to the tray.
            if (IsVisible && _isHiddenToTray)
            {
                RestoreFromTray();
            }
        }

        private void NotifyIcon_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Left click. Restore the window, or bring it to the front if it's already shown.
                if (_isHiddenToTray)
                {
                    RestoreFromTray();
                }
                else
                {
                    if (WindowState == WindowState.Minimized)
                    {
                        WindowState = WindowState.Normal;
                    }
                    Activate();
                }
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
                SaveWindowPlacement();
                _notifyIcon?.Dispose();
            }
        }

        // Applies the size and position options before the window is shown.
        private void ApplyStartupWindowPlacement()
        {
            if (ApplicationOptions.WindowSizeOption != WindowStartupOption.Default)
            {
                Width = ApplicationOptions.WindowWidth;
                Height = ApplicationOptions.WindowHeight;
            }

            if (ApplicationOptions.WindowPositionOption != WindowStartupOption.Default)
            {
                Left = ApplicationOptions.WindowLeft;
                Top = ApplicationOptions.WindowTop;
            }

            // Set after the position, so it maximizes on the right monitor.
            if (ApplicationOptions.WindowSizeOption == WindowStartupOption.Remember && ApplicationOptions.IsWindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // The window has a handle now, so its monitor is known. Never start partly or fully off screen:
            // fit it inside the work area of the nearest monitor. A saved position may be on a monitor
            // that's no longer connected.
            if (ApplicationOptions.WindowSizeOption == WindowStartupOption.Default
                && ApplicationOptions.WindowPositionOption == WindowStartupOption.Default)
            {
                return;
            }

            Rect workArea = NativeMethods.GetWorkArea(this);
            Width = Math.Min(Width, workArea.Width);
            Height = Math.Min(Height, workArea.Height);
            if (!double.IsNaN(Left) && !double.IsNaN(Top))
            {
                Left = Math.Max(workArea.Left, Math.Min(Left, workArea.Right - Width));
                Top = Math.Max(workArea.Top, Math.Min(Top, workArea.Bottom - Height));
            }
        }

        private void SaveWindowPlacement()
        {
            bool saveSize = ApplicationOptions.WindowSizeOption == WindowStartupOption.Remember;
            bool savePosition = ApplicationOptions.WindowPositionOption == WindowStartupOption.Remember;
            if (!saveSize && !savePosition)
            {
                return;
            }

            // When maximized or minimized, RestoreBounds holds the normal size and position.
            Rect bounds = WindowState == WindowState.Normal
                ? new Rect(Left, Top, ActualWidth, ActualHeight)
                : RestoreBounds;
            if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            ApplicationOptions.WindowWidth = (int)bounds.Width;
            ApplicationOptions.WindowHeight = (int)bounds.Height;
            ApplicationOptions.IsWindowMaximized = WindowState == WindowState.Maximized;
            ApplicationOptions.WindowLeft = (int)bounds.Left;
            ApplicationOptions.WindowTop = (int)bounds.Top;
            ConfigFile.SaveWindowPlacement(saveSize, savePosition);
        }
    }
}