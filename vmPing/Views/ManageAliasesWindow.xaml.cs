using System.Collections.Generic;
using System.Linq;
using System.Windows;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for ManageFavoritesWindow.xaml
    /// </summary>
    public partial class ManageAliasesWindow : Window
    {
        public ManageAliasesWindow()
        {
            InitializeComponent();

            RefreshAliasList();
        }

        private void RefreshAliasList()
        {
            lbAliases.ItemsSource = null;
            lbAliases.Items.Clear();
            var aliasList = Alias.GetAliases().ToList();
            aliasList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            lbAliases.ItemsSource = aliasList;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lbAliases.SelectedIndex < 0)
                return;

            var dialogWindow = new DialogWindow(
                DialogWindow.DialogIcon.Warning,
                "Confirm Delete",
                $"Are you sure you want to remove {((KeyValuePair<string, string>)lbAliases.SelectedItem).Value} from your aliases?",
                "Remove");
            dialogWindow.Owner = this;
            if (dialogWindow.ShowDialog() == true)
            {
                Alias.DeleteAlias(((KeyValuePair<string, string>)lbAliases.SelectedItem).Key);
                RefreshAliasList();
            }
        }
    }
}
