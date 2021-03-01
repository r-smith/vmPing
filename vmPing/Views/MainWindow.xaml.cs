using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static RoutedCommand ProbeOptionsCommand = new RoutedCommand();
        public static RoutedCommand StartStopCommand = new RoutedCommand();
        public static RoutedCommand HelpCommand = new RoutedCommand();
        public static RoutedCommand NewInstanceCommand = new RoutedCommand();
        public static RoutedCommand TraceRouteCommand = new RoutedCommand();
        public static RoutedCommand FloodHostCommand = new RoutedCommand();
        public static RoutedCommand AddMonitorCommand = new RoutedCommand();


        public MainWindow()
        {
            InitializeComponent();
            InitializeAplication();
        }


        private void InitializeAplication()
        {
            InitializeCommandBindings();
            Configuration.UpgradeConfigurationFile();
            LoadFavorites();
            LoadAliases();
            Configuration.Load();
            UpdatePopupOptionIsCheckedState();

            List<string> hosts = CommandLine.ParseArguments();
            if (hosts.Count > 0)
            {
                AddProbe(hosts.Count);
                for (int i = 0; i < hosts.Count; ++i)
                {
                    _ProbeCollection[i].Hostname = hosts[i].ToUpper();
                    _ProbeCollection[i].Alias = _Aliases.ContainsKey(_ProbeCollection[i].Hostname) ? _Aliases[_ProbeCollection[i].Hostname] : null;
                    _ProbeCollection[i].StartStop();
                }
            }
            else
            {
                AddProbe(2);
            }

            ColumnCount.Value = _ProbeCollection.Count;
            ProbeItemsControl.ItemsSource = _ProbeCollection;
        }


        private void UpdatePopupOptionIsCheckedState()
        {
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
        }


        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ProbeOptionsCommand, ProbeOptionsExecute));
            CommandBindings.Add(new CommandBinding(StartStopCommand, StartStopExecute));
            CommandBindings.Add(new CommandBinding(HelpCommand, HelpExecute));
            CommandBindings.Add(new CommandBinding(NewInstanceCommand, NewInstanceExecute));
            CommandBindings.Add(new CommandBinding(TraceRouteCommand, TraceRouteExecute));
            CommandBindings.Add(new CommandBinding(FloodHostCommand, FloodHostExecute));
            CommandBindings.Add(new CommandBinding(AddMonitorCommand, AddMonitorExecute));

            var kgProbeOptions = new KeyGesture(Key.F10);
            var kgStartStop = new KeyGesture(Key.F5);
            var kgHelp = new KeyGesture(Key.F1);
            var kgNewInstance = new KeyGesture(Key.N, ModifierKeys.Control);
            var kgTraceRoute = new KeyGesture(Key.T, ModifierKeys.Control);
            var kgFloodHost = new KeyGesture(Key.F, ModifierKeys.Control);
            var kgAddMonitor = new KeyGesture(Key.A, ModifierKeys.Control);
            InputBindings.Add(new InputBinding(ProbeOptionsCommand, kgProbeOptions));
            InputBindings.Add(new InputBinding(StartStopCommand, kgStartStop));
            InputBindings.Add(new InputBinding(HelpCommand, kgHelp));
            InputBindings.Add(new InputBinding(NewInstanceCommand, kgNewInstance));
            InputBindings.Add(new InputBinding(TraceRouteCommand, kgTraceRoute));
            InputBindings.Add(new InputBinding(FloodHostCommand, kgFloodHost));
            InputBindings.Add(new InputBinding(AddMonitorCommand, kgAddMonitor));

            StartStopMenu.Command = StartStopCommand;
            HelpMenu.Command = HelpCommand;
            NewInstanceMenu.Command = NewInstanceCommand;
            TraceRouteMenu.Command = TraceRouteCommand;
            FloodHostMenu.Command = FloodHostCommand;
            AddMonitorMenu.Command = AddMonitorCommand;
        }


        public void AddProbe(int numberOfProbes = 1)
        {
            for (; numberOfProbes > 0; --numberOfProbes)
                _ProbeCollection.Add(new Probe());
        }


        public void ProbeStartStop_Click(object sender, EventArgs e)
        {
            ((Probe)((Button)sender).DataContext).StartStop();
        }


        private void ColumnCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ColumnCount.Value > _ProbeCollection.Count)
                ColumnCount.Value = _ProbeCollection.Count;
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
            if (ColumnCount.Value > _ProbeCollection.Count)
                ColumnCount.Value = _ProbeCollection.Count;
        }


        private void ProbeOptionsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DisplayOptionsWindow();
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


        private void TraceRouteExecute(object sender, ExecutedRoutedEventArgs e)
        {
            new TraceRouteWindow().Show();
        }


        private void FloodHostExecute(object sender, ExecutedRoutedEventArgs e)
        {
            new FloodHostWindow().Show();
        }


        private void AddMonitorExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _ProbeCollection.Add(new Probe());
        }


        private void mnuProbeOptions_Click(object sender, RoutedEventArgs e)
        {
            DisplayOptionsWindow();
        }


        private void DisplayOptionsWindow()
        {
            if (OptionsWindow.openWindow == null)
            {
                // Open the options window.
                var optionsWnd = new OptionsWindow();
                optionsWnd.Owner = this;
                optionsWnd.ShowDialog();
                UpdatePopupOptionIsCheckedState();
            }
            else
            {
                // Options window is already open.  Activate it.
                OptionsWindow.openWindow.Activate();
            }
        }


        private void RemoveAllProbes()
        {
            foreach (var probe in _ProbeCollection)
            {
                if (probe.IsActive)
                    probe.CancelSource.Cancel();
            }
            _ProbeCollection.Clear();
            Probe.ActiveCount = 0;
        }

        private void LoadFavorites()
        {
            // Clear existing favorites menu.
            for (int i = mnuFavorites.Items.Count - 1; i > 2; --i)
                mnuFavorites.Items.RemoveAt(i);

            // Load favorites.
            foreach (var fav in Favorite.GetTitles())
            {
                var menuItem = new MenuItem();
                menuItem.Header = fav;
                menuItem.Click += (s, r) =>
                {
                    RemoveAllProbes();

                    var selectedFavorite = s as MenuItem;
                    var favorite = Favorite.GetContents(selectedFavorite.Header.ToString());
                    if (favorite.Hostnames.Count < 1)
                        AddProbe();
                    else
                    {
                        AddProbe(numberOfProbes: favorite.Hostnames.Count);
                        for (int i = 0; i < favorite.Hostnames.Count; ++i)
                        {
                            _ProbeCollection[i].Hostname = favorite.Hostnames[i].ToUpper();
                            _ProbeCollection[i].Alias = _Aliases.ContainsKey(_ProbeCollection[i].Hostname) ? _Aliases[_ProbeCollection[i].Hostname] : null;
                            _ProbeCollection[i].StartStop();
                        }
                    }

                    ColumnCount.Value = favorite.ColumnCount;
                };

                mnuFavorites.Items.Add(menuItem);
            }
        }


        private void LoadAliases()
        {
            _Aliases = Alias.GetAliases();
            var aliasList = _Aliases.ToList();
            aliasList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            // Clear existing aliases menu.
            for (int i = mnuAliases.Items.Count - 1; i > 1; --i)
                mnuAliases.Items.RemoveAt(i);

            // Load aliases.
            foreach (var alias in aliasList)
            {
                mnuAliases.Items.Add(BuildAliasMenuItem(alias, false));
            }

            foreach (var probe in _ProbeCollection)
            {
                probe.Alias = probe.Hostname != null && _Aliases.ContainsKey(probe.Hostname)
                    ? _Aliases[probe.Hostname]
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


        private void mnuAddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            // Display add to favorites window.
            var currentHostList = new List<string>();

            for (int i = 0; i < _ProbeCollection.Count; ++i)
                currentHostList.Add(_ProbeCollection[i].Hostname);

            var addToFavoritesWindow = new NewFavoriteWindow(currentHostList, (int)ColumnCount.Value);
            addToFavoritesWindow.Owner = this;
            if (addToFavoritesWindow.ShowDialog() == true)
            {
                LoadFavorites();
            }
        }

        private void mnuManageFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (ManageFavoritesWindow.openWindow == null)
            {
                // Open the favorites window.
                var manageFavoritesWindow = new ManageFavoritesWindow();
                manageFavoritesWindow.Owner = this;
                manageFavoritesWindow.ShowDialog();
                LoadFavorites();
            }
            else
            {
                // Favorites window is already open.  Activate it.
                ManageFavoritesWindow.openWindow.Activate();
            }
        }

        private void mnuManageAliases_Click(object sender, RoutedEventArgs e)
        {
            if (ManageAliasesWindow.openWindow == null)
            {
                // Open the aliases window.
                var manageAliasesWindow = new ManageAliasesWindow();
                manageAliasesWindow.Owner = this;
                manageAliasesWindow.ShowDialog();
                LoadAliases();
            }
            else
            {
                // Aliases window is already open.  Activate it.
                ManageAliasesWindow.openWindow.Activate();
            }
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

            if (_Aliases.ContainsKey(probe.Hostname))
                probe.Alias = _Aliases[probe.Hostname];
            else
                probe.Alias = string.Empty;

            var wnd = new EditAliasWindow(probe);
            wnd.Owner = this;

            if (wnd.ShowDialog() == true)
            {
                LoadAliases();
            }
        }

        private void mnuStatusHistory_Click(object sender, RoutedEventArgs e)
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
                probe.Alias = _Aliases.ContainsKey(probe.Hostname) ? _Aliases[probe.Hostname] : null;
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

        private void DockPanel_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_ProbeCollection.Count < 1)
                    return;

                var data = new DataObject();
                data.SetData("Source", (sender as ListBox).DataContext as Probe);
                DragDrop.DoDragDrop(sender as DependencyObject, data, DragDropEffects.Move);
                e.Handled = true;
            }
        }

        private void DockPanel_Drop(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData("Source") as Probe;
            if (source != null)
            {
                int newIndex = _ProbeCollection.IndexOf((sender as ListBox).DataContext as Probe);
                _ProbeCollection.RemoveAt(_ProbeCollection.IndexOf(source));
                _ProbeCollection.Insert(newIndex, source);
            }
        }
    }
}