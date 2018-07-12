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
    /// Interaction logic for EditAliasWindow.xaml
    /// </summary>
    public partial class EditAliasWindow : Window
    {
        private PingItem _CurrentPingItem;

        public EditAliasWindow(PingItem pingItem)
        {
            InitializeComponent();

            tbTitle.Text = "Alias for: " + pingItem.Hostname;
            AliasTextBox.Text = pingItem.Alias;
            AliasTextBox.SelectAll();
            _CurrentPingItem = pingItem;

            // Set initial focus to text box.
            Loaded += (sender, e) =>
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _CurrentPingItem.Alias = AliasTextBox.Text;
            DialogResult = true;
        }
    }
}
