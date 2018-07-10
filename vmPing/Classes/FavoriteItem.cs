using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vmPing.Classes
{
    public class FavoriteItem
    {
        public string Title { get; set; }
        public int ColumnCount { get; set; }
        public List<FavoriteHostItem> Hosts;

        public FavoriteItem()
        {
            Hosts = new List<FavoriteHostItem>();
        }
    }

    public class FavoriteHostItem
    {
        public string FriendlyName { get; set; }
        public string HostAddr { get; set; }
    }
}
