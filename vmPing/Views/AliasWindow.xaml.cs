using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Properties;
using vmPing.Services;

namespace vmPing.Views
{
    // Creates a new alias, or edits or removes an existing one.
    public partial class AliasWindow : Window
    {
        // The host of the alias being edited. Null when creating a new alias.
        private readonly string _originalHost;

        public AliasWindow(string hostname = "")
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);

            Hostname.Text = hostname;
            if (!string.IsNullOrEmpty(hostname) && Alias.GetAll().TryGetValue(hostname, out string alias))
            {
                _originalHost = hostname;
                NewAlias.Text = alias;
                Title = "Edit Alias";
                Header.Text = "Edit an alias";
                RemoveButton.Visibility = Visibility.Visible;
            }

            // Start in the alias box, with any existing alias selected.
            Loaded += (sender, e) =>
            {
                NewAlias.Focus();
                NewAlias.SelectAll();
            };
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string hostname = Hostname.Text.Trim();
            string alias = NewAlias.Text.Trim();

            if (Alias.IsNameInvalid(alias))
            {
                ShowError(Strings.NewAlias_Error_InvalidAlias);
                NewAlias.Focus();
                NewAlias.SelectAll();
                return;
            }
            if (Alias.IsHostInvalid(hostname))
            {
                ShowError(Strings.NewAlias_Error_InvalidHost);
                Hostname.Focus();
                Hostname.SelectAll();
                return;
            }

            // Warn before replacing the alias of a different host.
            bool isHostChanged = !string.Equals(hostname, _originalHost, StringComparison.OrdinalIgnoreCase);
            Dictionary<string, string> aliases = Alias.GetAll();
            if (isHostChanged && aliases.TryGetValue(hostname, out string existingAlias))
            {
                DialogWindow warningWindow = DialogWindow.WarningWindow(
                    message: $"{hostname} already has the alias {existingAlias}. Would you like to replace it?",
                    confirmButtonText: Strings.DialogButton_Overwrite);
                warningWindow.Owner = this;
                if (warningWindow.ShowDialog() != true)
                {
                    return;
                }
            }

            // Changing the host moves the alias to the new host.
            if (_originalHost != null && isHostChanged)
            {
                Alias.Delete(_originalHost);
            }
            Alias.Add(hostname, alias);
            DialogResult = true;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            DialogWindow dialogWindow = new DialogWindow(
                icon: DialogWindow.DialogIcon.Warning,
                title: Strings.DialogTitle_ConfirmDelete,
                body: $"{Strings.ManageAliases_Warn_DeleteA} {NewAlias.Text.Trim()} {Strings.ManageAliases_Warn_DeleteB}",
                confirmationText: Strings.DialogButton_Remove,
                isCancelButtonVisible: true)
            {
                Owner = this
            };

            if (dialogWindow.ShowDialog() == true)
            {
                Alias.Delete(_originalHost);
                DialogResult = true;
            }
        }

        private void ShowError(string message)
        {
            DialogWindow errorWindow = DialogWindow.ErrorWindow(message);
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }
}
