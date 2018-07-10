using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace vmPing.Classes
{
    class Favorite
    {
        public List<string> Hostnames { get; set; }
        public int ColumnCount { get; set; }


        public static bool DoesTitleExist(string title)
        {
            bool doesTitleExist = false;

            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            if (!File.Exists(path))
                return false;

            try
            {
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeTitleSearch = xd.SelectSingleNode($"/favorites/favorite[@title='{title}']");
                if (nodeTitleSearch != null)
                    doesTitleExist = true;
            }
            catch
            {
            }

            return doesTitleExist;
        }
        public static void UpgradeFavorite()
        {
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            if (!File.Exists(path))
                return;

            var xd = new XmlDocument();
            xd.Load(path);
            XmlNode nodeRoot = xd.SelectSingleNode("/favorites");
            XmlNodeList nodeTitle = xd.SelectNodes("/favorites/favorite");
            var list = new List<FavoriteItem>();
   
            foreach (XmlNode node in nodeTitle)
            {
                var title = node.Attributes["title"].Value;
                var collumn = node.Attributes["columncount"].Value;
                var fav = new FavoriteItem();
                var hosts = new FavoriteHostItem();
                fav.Title = title;
                fav.ColumnCount = int.Parse(collumn);

                list.Add(fav);

                XmlNode nodeFavorite = xd.SelectSingleNode($"/favorites/favorite[@title='{title}']");
                XmlNodeList nodeHost = xd.SelectNodes($"/favorites/favorite[@title='{title}']/host");

                foreach (XmlNode host in nodeHost)
                {
                    fav.Hosts.Add(new FavoriteHostItem { HostAddr = host.InnerText, FriendlyName = "FRIENDLY NAME HERE" });
                }

                list.Add(fav);
            }
            File.Delete(path);
            foreach (FavoriteItem favitem in list)
            {
                AddFavoriteEntry(favitem, favitem.ColumnCount);
            }
        }
        public static List<string> GetFavoriteTitles()
        {
            var favoriteTitles = new List<string>();

            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            if (!File.Exists(path))
                return favoriteTitles;

                try
                {
                var xd = new XmlDocument();
                xd.Load(path);

                /* Upgrade previous version */
                XmlNodeList oldNodeTitle = xd.SelectNodes("/favorites/favorite");
                if (oldNodeTitle.Count > 0)
                {
                    UpgradeFavorite();
                    xd.Load(path);
                }

                XmlNodeList nodeTitle = xd.SelectNodes("/Favorites/Favorite");

                foreach (XmlNode node in nodeTitle)
                    favoriteTitles.Add(node.Attributes["title"].Value);

            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            favoriteTitles.Sort();
            return favoriteTitles;
        }

        public static FavoriteItem GetFavoriteEntry(string favoriteTitle)
        {
            var favorite = new FavoriteItem();

            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            if (!File.Exists(path))
                return favorite;

            try
            {
                favorite.Hosts = new List<FavoriteHostItem>();

                var xd = new XmlDocument();
                xd.Load(path);
                XmlNode nodeFavorite = xd.SelectSingleNode($"/Favorites/Favorite[@title='{favoriteTitle}']");
                favorite.ColumnCount = int.Parse(nodeFavorite.Attributes["columncount"].Value);

                XmlNodeList nodeHost = xd.SelectNodes($"/Favorites/Favorite[@title='{favoriteTitle}']/Host");

                foreach (XmlNode node in nodeHost)
                {
                    XmlNode nodeHostAddr = node.SelectSingleNode($"HostAddr");
                    XmlNode nodeHostFriendly = node.SelectSingleNode($"HostFriendlyName");
                    favorite.Hosts.Add(new FavoriteHostItem
                    {
                        FriendlyName = nodeHostFriendly.InnerText,
                        HostAddr = nodeHostAddr.InnerText
                    });
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return favorite;
        }

        public static void AddFavoriteEntry( FavoriteItem hostnames, int columnCount)
        {
            var rootPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing");
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);
            if (!File.Exists(path))
            {
                try
                {
                    string[] lines = { "<Favorites>", "</Favorites>" };
                    File.WriteAllLines(path, lines);
                }
                catch
                {
                    MessageBox.Show("HEY!");
                    return;
                }
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeRoot = xd.SelectSingleNode("/Favorites");

                // Check if title already exists.
                XmlNodeList nodeTitleSearch = xd.SelectNodes($"/Favorites/Favorite[@title='{hostnames.Title}']");
                foreach (XmlNode node in nodeTitleSearch)
                {
                    // Title already exists.  Delete any old versions.
                    nodeRoot.RemoveChild(node);
                }

                
                XmlElement favorite = xd.CreateElement("Favorite");
                favorite.SetAttribute("title", hostnames.Title);
                favorite.SetAttribute("columncount", columnCount.ToString());
                foreach (FavoriteHostItem item in hostnames.Hosts)
                {
                    var xmlElement = xd.CreateElement("Host");
                    var xmlHostAddr = xd.CreateElement("HostAddr");
                    var xmlHostFriendlyName = xd.CreateElement("HostFriendlyName");
                    xmlHostFriendlyName.InnerText = item.FriendlyName;
                    xmlHostAddr.InnerText = item.HostAddr;
                    xmlElement.AppendChild(xmlHostFriendlyName);
                    xmlElement.AppendChild(xmlHostAddr);
                    favorite.AppendChild(xmlElement);
                }
                nodeRoot.AppendChild(favorite);
                xd.Save(path);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public static void DeleteFavoriteEntry(string title)
        {
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            if (!File.Exists(path))
                return;

            try
            {
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeRoot = xd.SelectSingleNode("/Favorites");

                // Search for favorite by title.
                XmlNodeList nodeTitleSearch = xd.SelectNodes($"/Favorites/Favorite[@title='{title}']");
                foreach (XmlNode node in nodeTitleSearch)
                {
                    // Found title.  Delete all versions.
                    nodeRoot.RemoveChild(node);
                }
                xd.Save(path);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
