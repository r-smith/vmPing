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

                XmlNodeList nodeTitle = xd.SelectNodes("/favorites/favorite");
                
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

        public static Favorite GetFavoriteEntry(string favoriteTitle)
        {
            var favorite = new Favorite();

            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            if (!File.Exists(path))
                return favorite;
            
            try
            {
                favorite.Hostnames = new List<string>();

                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeFavorite = xd.SelectSingleNode($"/favorites/favorite[@title='{favoriteTitle}']");
                favorite.ColumnCount = int.Parse(nodeFavorite.Attributes["columncount"].Value);

                XmlNodeList nodeHost = xd.SelectNodes($"/favorites/favorite[@title='{favoriteTitle}']/host");
                foreach (XmlNode node in nodeHost)
                    favorite.Hostnames.Add(node.InnerText);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return favorite;
        }

        public static void AddFavoriteEntry(string title, List<string> hostnames, int columnCount)
        {
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            if (!File.Exists(path))
            {
                try
                {
                    string[] lines = { "<favorites>", "</favorites>" };
                    File.WriteAllLines(path, lines);
                }
                catch
                {
                    return;
                }
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeRoot = xd.SelectSingleNode("/favorites");

                // Check if title already exists.
                XmlNodeList nodeTitleSearch = xd.SelectNodes($"/favorites/favorite[@title='{title}']");
                foreach (XmlNode node in nodeTitleSearch)
                {
                    // Title already exists.  Delete any old versions.
                    //node.RemoveAll();
                    nodeRoot.RemoveChild(node);
                }

                
                XmlElement favorite = xd.CreateElement("favorite");
                favorite.SetAttribute("title", title);
                favorite.SetAttribute("columncount", columnCount.ToString());
                foreach (string hostname in hostnames)
                {
                    var xmlElement = xd.CreateElement("host");
                    xmlElement.InnerText = hostname;
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
    }
}
