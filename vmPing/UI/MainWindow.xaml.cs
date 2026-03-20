using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<PingResults> ProbeCollection { get; set; }
        private ApplicationSettings _applicationSettings;

        public MainWindow()
        {
            InitializeComponent();

            ProbeCollection = new ObservableCollection<PingResults>();
            DataContext = ProbeCollection;

            _applicationSettings = new ApplicationSettings();

            // Load saved window position and size
            LoadWindowPosition();

            // Handle window state changed
            this.StateChanged += MainWindow_StateChanged;
            this.LocationChanged += MainWindow_LocationChanged;
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void LoadWindowPosition()
        {
            try
            {
                if (ApplicationOptions.RememberWindowPosition)
                {
                    // Set window position
                    if (ApplicationOptions.WindowLeft >= 0 && ApplicationOptions.WindowTop >= 0)
                    {
                        this.Left = ApplicationOptions.WindowLeft;
                        this.Top = ApplicationOptions.WindowTop;
                    }

                    // Set window size
                    if (ApplicationOptions.WindowWidth > 0 && ApplicationOptions.WindowHeight > 0)
                    {
                        this.Width = ApplicationOptions.WindowWidth;
                        this.Height = ApplicationOptions.WindowHeight;
                    }

                    // Set window state
                    if (!string.IsNullOrEmpty(ApplicationOptions.WindowState))
                    {
                        if (Enum.TryParse<WindowState>(ApplicationOptions.WindowState, out var state))
                        {
                            this.WindowState = state;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading window position: {ex.Message}");
            }
        }

        private void SaveWindowPosition()
        {
            try
            {
                if (ApplicationOptions.RememberWindowPosition && this.WindowState == WindowState.Normal)
                {
                    ApplicationOptions.WindowLeft = this.Left;
                    ApplicationOptions.WindowTop = this.Top;
                    ApplicationOptions.WindowWidth = this.Width;
                    ApplicationOptions.WindowHeight = this.Height;
                    ApplicationOptions.WindowState = this.WindowState.ToString();

                    _applicationSettings.SaveSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving window position: {ex.Message}");
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            SaveWindowPosition();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowPosition();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            SaveWindowPosition();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPosition();
        }

        // Rest of your existing MainWindow code here...
        // Keep all your existing methods and event handlers
    }
}