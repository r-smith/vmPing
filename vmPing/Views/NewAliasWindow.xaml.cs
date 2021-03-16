using System.Windows;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
    /// <summary>
    /// NewAliasWindow provides an interface for creating an alias.  An alias maps a friendly display name
    /// to a hostname.  If a host has an alias, the alias is displayed at the top of the probe window.
    /// </summary>
    public partial class NewAliasWindow : Window
    {
        public NewAliasWindow()
        {
            InitializeComponent();

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate hostname.
            if (Alias.IsHostInvalid(MyHost.Text))
            {
                var errorWindow = DialogWindow.ErrorWindow(Strings.NewAlias_Error_InvalidHost);
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyHost.Focus();
                MyHost.SelectAll();
                return;
            }

            // Validate alias name.
            if (Alias.IsNameInvalid(MyAlias.Text))
            {
                var errorWindow = DialogWindow.ErrorWindow(Strings.NewAlias_Error_InvalidAlias);
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyAlias.Focus();
                MyAlias.SelectAll();
                return;
            }

            // Checks passed.  Add alias entry.
            Alias.AddAlias(MyHost.Text, MyAlias.Text);
            DialogResult = true;
        }
    }
}
