using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.UI
{
    public partial class EditAliasWindow : Window
    {
        private readonly string _hostname;

        public EditAliasWindow(Probe pingItem) : this(pingItem.Hostname, pingItem.Alias)
        {
        }

        public EditAliasWindow(string hostname, string alias)
        {
            InitializeComponent();

            _hostname = hostname;

            Hostname.Text = _hostname;
            NewAlias.Text = alias;
            NewAlias.SelectAll();
            
            // Set initial keyboard focus.
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newAlias = NewAlias.Text?.Trim();

            if (string.IsNullOrWhiteSpace(newAlias))
            {
                Alias.Delete(_hostname);
            }
            else
            {
                Alias.Add(_hostname, newAlias);
            }

            DialogResult = true;
        }
    }
}
