using System.Windows;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Logique d'interaction pour Detail.xaml
    /// </summary>
    public partial class DetailWindow : Window
    {
        public DetailWindow(PingItem item )
        {
            InitializeComponent();
            DataContext = item;
            this.Title = item.FriendlyName;
        }
    }
}
