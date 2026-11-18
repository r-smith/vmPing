using System.Windows;
using System.Windows.Input;
using vmPing.Services;

namespace vmPing.Views
{
    public partial class RenameGroupWindow : Window
    {
        public string NewName { get; private set; }

        public RenameGroupWindow(string groupName)
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);

            GroupName.Text = groupName;
            GroupName.SelectAll();

            // Set initial keyboard focus.
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newName = GroupName.Text.Trim();
            if (newName.Length == 0)
            {
                DialogWindow errorWindow = DialogWindow.ErrorWindow("Please enter a name for the group.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                GroupName.Focus();
                return;
            }

            NewName = newName;
            DialogResult = true;
        }
    }
}
