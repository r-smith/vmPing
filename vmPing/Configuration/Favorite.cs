using System;
using System.Collections.Generic;
using System.Xml;
using vmPing.Properties;

namespace vmPing.Configuration
{
    internal class Favorite
    {
        private const string RootPath = "/vmping/favorites";
        private const string FavoritePath = "/vmping/favorites/favorite";
        private const string TitleAttribute = "title";
        private const string ColumnAttribute = "columncount";

        public List<string> Hostnames { get; set; } = new List<string>();
        public int ColumnCount { get; set; }

        public static bool TitleExists(string title)
        {
            if (!ConfigFile.Exists() || string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);
                XmlNode node = xd.SelectSingleNode($"{FavoritePath}[@{TitleAttribute}={ConfigFile.GetEscapedXpath(title)}]");
                return node != null;
            }
            catch (Exception ex)
            {
                Util.ShowError($"Failed to read configuration file. {ex.Message}");
                return false;
            }
        }

        public static List<string> GetTitles()
        {
            List<string> titles = new List<string>();

            if (!ConfigFile.Exists())
            {
                return titles;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                XmlNodeList nodes = xd.SelectNodes(FavoritePath);
                if (nodes == null)
                {
                    return titles;
                }

                foreach (XmlNode node in nodes)
                {
                    string title = node.Attributes?[TitleAttribute]?.Value;
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        titles.Add(title);
                    }
                }

                titles.Sort(StringComparer.OrdinalIgnoreCase);
            }

            catch (Exception ex)
            {
                Util.ShowError($"Failed to read configuration file. {ex.Message}");
            }

            return titles;
        }

        public static Favorite Load(string title)
        {
            if (!ConfigFile.Exists() || string.IsNullOrWhiteSpace(title))
            {
                return new Favorite();
            }

            Favorite favorite = new Favorite();

            try
            {
                favorite.Hostnames = new List<string>();

                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                XmlNode favoriteNode = xd.SelectSingleNode($"{FavoritePath}[@{TitleAttribute}={ConfigFile.GetEscapedXpath(title)}]")
                    ?? throw new KeyNotFoundException();

                // Columns.
                if (int.TryParse(favoriteNode.Attributes?[ColumnAttribute]?.Value, out int columns))
                {
                    favorite.ColumnCount = columns;
                }

                // Hostnames.
                XmlNodeList hostNodes = favoriteNode.SelectNodes("host");
                if (hostNodes != null)
                {
                    foreach (XmlNode node in hostNodes)
                    {
                        string hostname = node.InnerText?.Trim();
                        if (!string.IsNullOrEmpty(hostname))
                        {
                            favorite.Hostnames.Add(hostname);
                        }
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                Util.ShowError($"The requested favorite was not found: {title}");
            }
            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_ReadConfig} {ex.Message}");
            }

            return favorite;
        }

        public static void Rename(string originalTitle, string newTitle)
        {
            if (!ConfigFile.IsReady() || string.IsNullOrWhiteSpace(originalTitle) || string.IsNullOrWhiteSpace(newTitle))
            {
                return;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                // Find favorite.
                XmlNodeList nodes = xd.SelectNodes($"{FavoritePath}[@{TitleAttribute}={ConfigFile.GetEscapedXpath(originalTitle)}]");
                if (nodes == null || nodes.Count == 0)
                {
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    // Rename.
                    node.Attributes[TitleAttribute].Value = newTitle;
                }

                xd.Save(ConfigFile.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }


        public static void Save(string title, List<string> hostnames, int columnCount)
        {
            if (!ConfigFile.IsReady() || string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                XmlNode root = xd.SelectSingleNode(RootPath)
                    ?? throw new XmlException("Invalid configuration file.");

                // Remove favorite if title already exists.
                foreach (XmlNode node in xd.SelectNodes($"{FavoritePath}[@{TitleAttribute}={ConfigFile.GetEscapedXpath(title)}]"))
                {
                    root.RemoveChild(node);
                }

                // Create new favorite.
                XmlElement favorite = xd.CreateElement("favorite");
                favorite.SetAttribute(TitleAttribute, title);
                favorite.SetAttribute(ColumnAttribute, columnCount.ToString());

                foreach (string hostname in hostnames)
                {
                    if (string.IsNullOrWhiteSpace(hostname))
                    {
                        continue;
                    }

                    XmlElement hostElement = xd.CreateElement("host");
                    hostElement.InnerText = hostname;
                    favorite.AppendChild(hostElement);
                }

                root.AppendChild(favorite);
                xd.Save(ConfigFile.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }

        public static void Delete(string title)
        {
            if (!ConfigFile.Exists() || string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                // Find favorite.
                XmlNode root = xd.SelectSingleNode(RootPath);
                if (root == null)
                {
                    return;
                }

                XmlNodeList nodes = xd.SelectNodes($"{FavoritePath}[@{TitleAttribute}={ConfigFile.GetEscapedXpath(title)}]");
                if (nodes == null)
                {
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    // Delete.
                    root.RemoveChild(node);
                }

                xd.Save(ConfigFile.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }

        public static bool IsTitleInvalid(string title)
        {
            return string.IsNullOrWhiteSpace(title);
        }
    }
}
