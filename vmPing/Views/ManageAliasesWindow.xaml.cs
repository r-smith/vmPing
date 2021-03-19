using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for ManageFavoritesWindow.xaml
    /// </summary>
    public partial class ManageAliasesWindow : Window
    {
        // Imports and constants for hiding minimize and maximize buttons.
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button

        public ManageAliasesWindow()
        {
            InitializeComponent();

            RefreshAliasList();
        }

        public void RefreshAliasList()
        {
            AliasesDataGrid.ItemsSource = null;

            var aliasList = Alias.GetAliases().ToList();
            aliasList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            AliasesDataGrid.ItemsSource = aliasList;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (AliasesDataGrid.SelectedIndex < 0)
                return;

            var dialogWindow = new DialogWindow(
                DialogWindow.DialogIcon.Warning,
                Strings.DialogTitle_ConfirmDelete,
                $"{Strings.ManageAliases_Warn_DeleteA} {((KeyValuePair<string, string>)AliasesDataGrid.SelectedItem).Value} {Strings.ManageAliases_Warn_DeleteB}",
                Strings.DialogButton_Remove,
                true);
            dialogWindow.Owner = this;
            if (dialogWindow.ShowDialog() == true)
            {
                Alias.DeleteAlias(((KeyValuePair<string, string>)AliasesDataGrid.SelectedItem).Key);
                RefreshAliasList();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (AliasesDataGrid.SelectedIndex < 0)
                return;

            var editAliasWindow = new EditAliasWindow(((KeyValuePair<string, string>)AliasesDataGrid.SelectedItem).Key, ((KeyValuePair<string, string>)AliasesDataGrid.SelectedItem).Value);
            editAliasWindow.Owner = this;

            if (editAliasWindow.ShowDialog() == true)
            {
                RefreshAliasList();
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var newAliasWindow = new NewAliasWindow();
            newAliasWindow.Owner = this;
            if (newAliasWindow.ShowDialog() == true)
            {
                RefreshAliasList();
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // Hide minimize and maximize buttons.
            IntPtr _windowHandle = new WindowInteropHelper(this).Handle;
            if (_windowHandle == null)
            {
                return;
            }

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
