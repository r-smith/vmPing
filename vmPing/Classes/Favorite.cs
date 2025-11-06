using System;
using System.Collections.Generic;
using System.Xml;
using vmPing.Properties;

namespace vmPing.Classes
{
    class Favorite
    {
        private const string RootPath = "/vmping/favorites";
        private const string FavoritePath = "/vmping/favorites/favorite";
        private const string TitleAttribute = "title";
        private const string ColumnAttribute = "columncount";

        public List<string> Hostnames { get; set; }
        public int ColumnCount { get; set; }

        public static bool TitleExists(string title)
        {
            if (!Configuration.Exists() || string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);
                var node = xd.SelectSingleNode($"{FavoritePath}[@{TitleAttribute}={Configuration.GetEscapedXpath(title)}]");
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
            var titles = new List<string>();

            if (!Configuration.Exists())
            {
                return titles;
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                var nodes = xd.SelectNodes(FavoritePath);
                if (nodes == null)
                {
                    return titles;
                }

                foreach (XmlNode node in nodes)
                {
                    var title = node.Attributes?[TitleAttribute]?.Value;
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
            if (!Configuration.Exists() || string.IsNullOrWhiteSpace(title))
            {
                return new Favorite();
            }

            var favorite = new Favorite();

            try
            {
                favorite.Hostnames = new List<string>();

                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                var favoriteNode = xd.SelectSingleNode($"{FavoritePath}[@{TitleAttribute}={Configuration.GetEscapedXpath(title)}]")
                    ?? throw new KeyNotFoundException();

                // Columns.
                if (int.TryParse(favoriteNode.Attributes?[ColumnAttribute]?.Value, out int columns))
                {
                    favorite.ColumnCount = columns;
                }

                // Hostnames.
                var hostNodes = favoriteNode.SelectNodes("host");
                if (hostNodes != null)
                {
                    foreach (XmlNode node in hostNodes)
                    {
                        var hostname = node.InnerText?.Trim();
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
            if (Configuration.IsReady() == false || string.IsNullOrWhiteSpace(originalTitle) || string.IsNullOrWhiteSpace(newTitle))
            {
                return;
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                // Find favorite.
                var nodes = xd.SelectNodes($"{FavoritePath}[@{TitleAttribute}={Configuration.GetEscapedXpath(originalTitle)}]");
                if (nodes == null || nodes.Count == 0)
                {
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    // Rename.
                    node.Attributes[TitleAttribute].Value = newTitle;
                }

                xd.Save(Configuration.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }


        public static void Save(string title, List<string> hostnames, int columnCount)
        {
            if (Configuration.IsReady() == false || string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                var root = xd.SelectSingleNode(RootPath)
                    ?? throw new XmlException("Invalid configuration file.");

                // Remove favorite if title already exists.
                foreach (XmlNode node in xd.SelectNodes($"{FavoritePath}[@{TitleAttribute}={Configuration.GetEscapedXpath(title)}]"))
                {
                    root.RemoveChild(node);
                }

                // Create new favorite.
                var favorite = xd.CreateElement("favorite");
                favorite.SetAttribute(TitleAttribute, title);
                favorite.SetAttribute(ColumnAttribute, columnCount.ToString());

                foreach (var hostname in hostnames)
                {
                    if (string.IsNullOrWhiteSpace(hostname))
                    {
                        continue;
                    }

                    var hostElement = xd.CreateElement("host");
                    hostElement.InnerText = hostname;
                    favorite.AppendChild(hostElement);
                }

                root.AppendChild(favorite);
                xd.Save(Configuration.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }

        public static void Delete(string title)
        {
            if (!Configuration.Exists() || string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                // Find favorite.
                var root = xd.SelectSingleNode(RootPath);
                if (root == null)
                {
                    return;
                }

                var nodes = xd.SelectNodes($"{FavoritePath}[@{TitleAttribute}={Configuration.GetEscapedXpath(title)}]");
                if (nodes == null)
                {
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    // Delete.
                    root.RemoveChild(node);
                }

                xd.Save(Configuration.FilePath);
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
