using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace vmPing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "836de31c-745b-41fd-bd5d-a393a8f528c6";

        [STAThread]
        public static void Main()
        {
            
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // Handle command line arguments of second instance
            // Load host from commandline
            if (args.Count == 2)
            {
                // If one host is provided we add to Probes Collection
                Views.MainWindow.Instance.AddProbe(args[1].ToUpper());
            }

            // Bring window to foreground
            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.WindowState = WindowState.Normal;
            }
            this.MainWindow.Activate();

            return true;
        }

        // DEBUG: Use this for testing alternate locales.
        //public App()
        //{
        //    vmPing.Properties.Strings.Culture = new System.Globalization.CultureInfo("en-GB");
        //}
    }
}
