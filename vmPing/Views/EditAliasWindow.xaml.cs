using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for EditAliasWindow.xaml
    /// </summary>
    public partial class EditAliasWindow : Window
    {
        private PingItem _CurrentPingItem;

        public EditAliasWindow(PingItem pingItem)
        {
            InitializeComponent();

            Header.Text = "Alias for: " + pingItem.Hostname;
            MyAlias.Text = pingItem.Alias;
            MyAlias.SelectAll();
            _CurrentPingItem = pingItem;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public EditAliasWindow(string hostname, string alias)
        {
            InitializeComponent();

            Header.Text = "Alias for: " + hostname;
            MyAlias.Text = alias;
            MyAlias.SelectAll();
            _CurrentPingItem = new PingItem { Hostname = hostname, Alias = alias };

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MyAlias.Text))
            {
                Alias.DeleteAlias(_CurrentPingItem.Hostname);
            }
            else
            {
                Alias.AddAlias(_CurrentPingItem.Hostname, MyAlias.Text);
            }
            DialogResult = true;
        }
    }
}
