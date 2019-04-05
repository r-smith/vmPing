using System.Windows;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
    /// <summary>
    /// EditAliasWindow is used to rename an alias.
    /// </summary>
    public partial class EditAliasWindow : Window
    {
        private string _Hostname;

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
                Alias.DeleteAlias(_Hostname);
            }
            else
            {
                Alias.AddAlias(_Hostname, MyAlias.Text);
            }
            DialogResult = true;
        }
    }
}
