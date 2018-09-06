using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for NewAliasWindow.xaml
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
            if (string.IsNullOrWhiteSpace(MyHost.Text))
            {
                var dialogWindow = new DialogWindow(
                    DialogWindow.DialogIcon.Warning,
                    "Error",
                    $"Please enter a valid host name.",
                    "OK",
                    false);
                dialogWindow.Owner = this;
                dialogWindow.ShowDialog();
                MyHost.Focus();
                MyHost.SelectAll();
                return;
            }

            if (string.IsNullOrWhiteSpace(MyAlias.Text))
            {
                var dialogWindow = new DialogWindow(
                    DialogWindow.DialogIcon.Warning,
                    "Error",
                    $"Please enter a valid alias.",
                    "OK",
                    false);
                dialogWindow.Owner = this;
                dialogWindow.ShowDialog();
                MyAlias.Focus();
                MyAlias.SelectAll();
                return;
            }

            Alias.AddAlias(MyHost.Text.ToUpper(), MyAlias.Text);
            DialogResult = true;
        }
    }
}
