using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string GroupAttribute = "group";

        public string Title { get; set; }
        public string Group { get; set; } = string.Empty;
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

        // All favorites, with grouped favorites first. Sorted by group, then title.
        public static List<Favorite> GetAll()
        {
            List<Favorite> favorites = new List<Favorite>();

            if (!ConfigFile.Exists())
            {
                return favorites;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                XmlNodeList nodes = xd.SelectNodes(FavoritePath);
                if (nodes == null)
                {
                    return favorites;
                }

                foreach (XmlNode node in nodes)
                {
                    Favorite favorite = FromNode(node);
                    if (!string.IsNullOrWhiteSpace(favorite.Title))
                    {
                        favorites.Add(favorite);
                    }
                }

                favorites.Sort(CompareByGroupAndTitle);
            }
            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_ReadConfig} {ex.Message}");
            }

            return favorites;
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
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                XmlNode favoriteNode = xd.SelectSingleNode($"{FavoritePath}[@{TitleAttribute}={ConfigFile.GetEscapedXpath(title)}]")
                    ?? throw new KeyNotFoundException();

                favorite = FromNode(favoriteNode);
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

        // Moves a favorite to a group. An empty group means no group.
        public static void SetGroup(string title, string group)
        {
            if (!ConfigFile.IsReady() || string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                group = group?.Trim();
                foreach (XmlElement node in xd.SelectNodes($"{FavoritePath}[@{TitleAttribute}={ConfigFile.GetEscapedXpath(title)}]"))
                {
                    if (string.IsNullOrEmpty(group))
                    {
                        node.RemoveAttribute(GroupAttribute);
                    }
                    else
                    {
                        node.SetAttribute(GroupAttribute, group);
                    }
                }

                ConfigFile.WriteSafely(xd.Save);
            }
            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }

        // Renames a group in every favorite that's in it. An empty new name removes the group.
        public static void RenameGroup(string group, string newGroup)
        {
            if (!ConfigFile.IsReady() || string.IsNullOrWhiteSpace(group))
            {
                return;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                newGroup = newGroup?.Trim();
                foreach (XmlElement node in xd.SelectNodes($"{FavoritePath}[@{GroupAttribute}={ConfigFile.GetEscapedXpath(group)}]"))
                {
                    if (string.IsNullOrEmpty(newGroup))
                    {
                        node.RemoveAttribute(GroupAttribute);
                    }
                    else
                    {
                        node.SetAttribute(GroupAttribute, newGroup);
                    }
                }

                ConfigFile.WriteSafely(xd.Save);
            }
            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
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

                ConfigFile.WriteSafely(xd.Save);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }


        // A null group keeps the favorite's current group. An empty group means no group.
        public static void Save(string title, List<string> hostnames, int columnCount, string group = null)
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
                    if (group == null)
                    {
                        group = node.Attributes?[GroupAttribute]?.Value;
                    }
                    root.RemoveChild(node);
                }

                // Create new favorite.
                XmlElement favorite = xd.CreateElement("favorite");
                favorite.SetAttribute(TitleAttribute, title);
                favorite.SetAttribute(ColumnAttribute, columnCount.ToString());

                group = group?.Trim();
                if (!string.IsNullOrEmpty(group))
                {
                    favorite.SetAttribute(GroupAttribute, group);
                }

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
                ConfigFile.WriteSafely(xd.Save);
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

                ConfigFile.WriteSafely(xd.Save);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
            }
        }

        // Returns the existing group's spelling when only the case is different, so it doesn't become a second group.
        public static string MatchGroup(string group, IEnumerable<string> existingGroups)
        {
            group = group.Trim();
            return existingGroups.FirstOrDefault(g => string.Equals(g, group, StringComparison.OrdinalIgnoreCase)) ?? group;
        }

        public static bool IsTitleInvalid(string title)
        {
            return string.IsNullOrWhiteSpace(title);
        }

        private static Favorite FromNode(XmlNode node)
        {
            Favorite favorite = new Favorite
            {
                Title = node.Attributes?[TitleAttribute]?.Value,
                Group = node.Attributes?[GroupAttribute]?.Value?.Trim() ?? string.Empty
            };

            if (int.TryParse(node.Attributes?[ColumnAttribute]?.Value, out int columns))
            {
                favorite.ColumnCount = columns;
            }

            XmlNodeList hostNodes = node.SelectNodes("host");
            if (hostNodes != null)
            {
                foreach (XmlNode hostNode in hostNodes)
                {
                    string hostname = hostNode.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(hostname))
                    {
                        favorite.Hostnames.Add(hostname);
                    }
                }
            }

            return favorite;
        }

        private static int CompareByGroupAndTitle(Favorite a, Favorite b)
        {
            bool isAGrouped = a.Group.Length > 0;
            bool isBGrouped = b.Group.Length > 0;
            if (isAGrouped != isBGrouped)
            {
                return isAGrouped ? -1 : 1;
            }

            int result = StringComparer.OrdinalIgnoreCase.Compare(a.Group, b.Group);
            if (result != 0)
            {
                return result;
            }

            return StringComparer.OrdinalIgnoreCase.Compare(a.Title, b.Title);
        }
    }
}
