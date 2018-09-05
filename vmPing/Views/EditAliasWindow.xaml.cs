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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPingItem.Alias = MyAlias.Text;
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
