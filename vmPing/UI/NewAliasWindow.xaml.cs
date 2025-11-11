using System.Windows;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.UI
{
    public partial class NewAliasWindow : Window
    {
        public NewAliasWindow()
        {
            InitializeComponent();

            // Set initial keyboard focus.
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate hostname.
            if (Alias.IsHostInvalid(Hostname.Text))
            {
                ShowError(Strings.NewAlias_Error_InvalidHost);
                Hostname.Focus();
                Hostname.SelectAll();
                return;
            }

            // Validate alias name.
            if (Alias.IsNameInvalid(NewAlias.Text))
            {
                ShowError(Strings.NewAlias_Error_InvalidAlias);
                NewAlias.Focus();
                NewAlias.SelectAll();
                return;
            }

            // Validation passed. Add alias.
            Alias.Add(Hostname.Text.Trim(), NewAlias.Text);
            DialogResult = true;
        }

        private void ShowError(string message)
        {
            var errorWindow = DialogWindow.ErrorWindow(message);
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }
}
