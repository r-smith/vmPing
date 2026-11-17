using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using vmPing.Configuration;
using vmPing.Properties;

namespace vmPing.Views
{
    // Saves the probes open in the main window as a favorite.
    public partial class NewFavoriteWindow : Window
    {
        private readonly List<string> _hostList;
        private readonly int _columnCount;

        public string SavedTitle { get; private set; }

        public NewFavoriteWindow(List<string> hostList, int columnCount, string title = "")
        {
            InitializeComponent();

            // Empty probes aren't saved.
            _hostList = hostList.Where(h => !string.IsNullOrWhiteSpace(h)).ToList();
            _columnCount = columnCount;

            MyTitle.Text = title;
            MyTitle.SelectAll();

            // List the existing groups, and start with the group of the favorite being saved over, if any.
            List<Favorite> favorites = Favorite.GetAll();
            MyGroup.ItemsSource = favorites.Select(f => f.Group).Where(g => g.Length > 0).Distinct().ToList();
            MyGroup.Text = favorites.FirstOrDefault(f => f.Title == title)?.Group ?? string.Empty;

            string probes = _hostList.Count == 1 ? "1 probe" : $"{_hostList.Count} probes";
            string columns = columnCount == 1 ? "1 column" : $"{columnCount} columns";
            Summary.Text = $"Your {probes} will be saved in {columns}.";

            // Set initial focus to the title.
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string title = MyTitle.Text.Trim();

            if (Favorite.IsTitleInvalid(title))
            {
                ShowError(Strings.NewFavorite_Error_InvalidName);
                MyTitle.Focus();
                MyTitle.SelectAll();
                return;
            }
            if (_hostList.Count == 0)
            {
                ShowError("There are no hosts to save. Enter at least one host first.");
                return;
            }

            // Warn if a favorite with this title already exists.
            if (Favorite.TitleExists(title))
            {
                DialogWindow warningWindow = DialogWindow.WarningWindow(
                    message: $"{title} {Strings.NewFavorite_Warn_AlreadyExists}",
                    confirmButtonText: Strings.DialogButton_Overwrite);
                warningWindow.Owner = this;
                if (warningWindow.ShowDialog() != true)
                {
                    return;
                }
            }

            string group = Favorite.MatchGroup(MyGroup.Text, MyGroup.Items.Cast<string>());
            Favorite.Save(title, _hostList, _columnCount, group);
            SavedTitle = title;
            DialogResult = true;
        }

        private void ShowError(string message)
        {
            DialogWindow errorWindow = DialogWindow.ErrorWindow(message);
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }
}
