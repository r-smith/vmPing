using System.Windows;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.UI
{
    public partial class EditAliasWindow : Window
    {
        private readonly string _Hostname;

        public EditAliasWindow(Probe pingItem) : this(pingItem.Hostname, pingItem.Alias)
        {
        }

        public EditAliasWindow(string hostname, string alias)
        {
            InitializeComponent();

            Header.Text = $"{Strings.EditAlias_AliasFor} {hostname}";
            MyAlias.Text = alias;
            MyAlias.SelectAll();
            _Hostname = hostname;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MyAlias.Text))
            {
                Alias.Delete(_Hostname);
            }
            else
            {
                Alias.Add(_Hostname, MyAlias.Text);
            }
            DialogResult = true;
        }
    }
}
