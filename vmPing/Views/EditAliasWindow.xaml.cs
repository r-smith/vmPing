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

            tbTitle.Text = "Alias for: " + pingItem.Hostname;
            AliasTextBox.Text = pingItem.Alias;
            AliasTextBox.SelectAll();
            _CurrentPingItem = pingItem;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPingItem.Alias = AliasTextBox.Text;
            Alias.AddAlias(_CurrentPingItem.Hostname, AliasTextBox.Text);
            DialogResult = true;
        }
    }
}
