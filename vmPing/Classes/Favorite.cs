using System;
using System.Collections.Generic;
using System.IO;
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

            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
            if (!File.Exists(path))
                return false;

            try
            {
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeTitleSearch = xd.SelectSingleNode($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(title)}]");
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

            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
            if (!File.Exists(path))
                return favoriteTitles;
            
            try
            {
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNodeList nodeTitle = xd.SelectNodes("/vmping/favorites/favorite");
                
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

        public static Favorite GetFavoriteContents(string favoriteTitle)
        {
            var favorite = new Favorite();

            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
            if (!File.Exists(path))
                return favorite;
            
            try
            {
                favorite.Hostnames = new List<string>();

                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeFavorite = xd.SelectSingleNode($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(favoriteTitle)}]");
                favorite.ColumnCount = int.Parse(nodeFavorite.Attributes["columncount"].Value);

                XmlNodeList nodeHost = xd.SelectNodes($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(favoriteTitle)}]/host");
                foreach (XmlNode node in nodeHost)
                    favorite.Hostnames.Add(node.InnerText);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return favorite;
        }

        public static void RenameFavoriteSet(string originalTitle, string newTitle)
        {
            if (Configuration.CheckAndInitializeConfigurationFile() == false)
                return;

            try
            {
                var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeRoot = xd.SelectSingleNode("/vmping/favorites");

                // Check if title already exists.
                XmlNodeList nodeTitleSearch = xd.SelectNodes($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(originalTitle)}]");
                foreach (XmlNode node in nodeTitleSearch)
                {
                    // Rename title attribue.
                    node.Attributes["title"].Value = newTitle;
                }

                xd.Save(path);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public static void SaveFavoriteSet(string title, List<string> hostnames, int columnCount)
        {
            if (Configuration.CheckAndInitializeConfigurationFile() == false)
                return;

            try
            {
                var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeRoot = xd.SelectSingleNode("/vmping/favorites");

                // Check if title already exists.
                XmlNodeList nodeTitleSearch = xd.SelectNodes($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(title)}]");
                foreach (XmlNode node in nodeTitleSearch)
                {
                    // Title already exists.  Delete any old versions.
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

        public static void DeleteFavoriteSet(string title)
        {
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
            if (!File.Exists(path))
                return;

            try
            {
                var xd = new XmlDocument();
                xd.Load(path);
                
                XmlNode nodeRoot = xd.SelectSingleNode("/vmping/favorites");

                // Search for favorite by title.
                XmlNodeList nodeTitleSearch = xd.SelectNodes($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(title)}]");
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
