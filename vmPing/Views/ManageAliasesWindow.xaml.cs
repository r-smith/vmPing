using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Interop;
using vmPing.Properties;

namespace vmPing.Views
{
    public partial class ManageAliasesWindow : Window
    {
        public ManageAliasesWindow()
        {
            InitializeComponent();
            RefreshAliasList();
        }

        public void RefreshAliasList()
        {
            List<KeyValuePair<string, string>> aliases = Alias.GetAll()
                .OrderBy(alias => alias.Value, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            Aliases.ItemsSource = aliases;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<string, string>? selected = GetSelectedAlias();
            if (selected == null)
            {
                return;
            }

            DialogWindow dialogWindow = new DialogWindow(
                icon: DialogWindow.DialogIcon.Warning,
                title: Strings.DialogTitle_ConfirmDelete,
                body: $"{Strings.ManageAliases_Warn_DeleteA} {selected.Value.Value} {Strings.ManageAliases_Warn_DeleteB}",
                confirmationText: Strings.DialogButton_Remove,
                isCancelButtonVisible: true)
            {
                Owner = this
            };

            if (dialogWindow.ShowDialog() == true)
            {
                Alias.Delete(selected.Value.Key);
                RefreshAliasList();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<string, string>? selected = GetSelectedAlias();
            if (selected == null)
            {
                return;
            }

            EditAliasWindow editWindow = new EditAliasWindow(selected.Value.Key, selected.Value.Value)
            {
                Owner = this
            };

            if (editWindow.ShowDialog() == true)
            {
                RefreshAliasList();
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            NewAliasWindow newWindow = new NewAliasWindow
            {
                Owner = this
            };

            if (newWindow.ShowDialog() == true)
            {
                RefreshAliasList();
            }
        }

        private KeyValuePair<string, string>? GetSelectedAlias()
        {
            KeyValuePair<string, string>? selected = Aliases.SelectedItem as KeyValuePair<string, string>?;
            return selected.HasValue ? selected : null;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideMinimizeAndMaximizeButtons(this);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
