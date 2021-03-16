using System;
using System.Collections.Generic;
using System.Xml;
using vmPing.Properties;

namespace vmPing.Classes
{
    class Alias
    {
        public static Dictionary<string, string> GetAliases()
        {
            if (!Configuration.Exists())
                return new Dictionary<string, string>();

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                var aliases = new Dictionary<string, string>();
                foreach (XmlNode node in xd.SelectNodes("/vmping/aliases/alias"))
                    aliases.Add(node.Attributes["host"].Value.ToLower(), node.InnerText);
                return aliases;
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_LoadAlias} {ex.Message}");
                return new Dictionary<string, string>();
            }
        }


        public static void AddAlias(string hostname, string alias)
        {
            if (!Configuration.IsReady())
                return;

            // Convert to lowercase for consistent lookups / comparisons.
            hostname = hostname.ToLower();

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                XmlNode nodeRoot = xd.SelectSingleNode("/vmping/aliases");

                // Check if title already exists.
                foreach (XmlNode node in xd.SelectNodes($"/vmping/aliases/alias[@host={Configuration.GetEscapedXpath(hostname)}]"))
                {
                    // Title already exists.  Delete any old versions.
                    nodeRoot.RemoveChild(node);
                }

                XmlElement aliasEntry = xd.CreateElement("alias");
                aliasEntry.SetAttribute("host", hostname);
                aliasEntry.InnerText = alias;
                nodeRoot.AppendChild(aliasEntry);
                xd.Save(Configuration.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_AddAlias} {ex.Message}");
            }
        }


        public static void DeleteAlias(string key)
        {
            if (!Configuration.Exists())
                return;

            // Convert to lowercase for consistent lookups / comparisons.
            key = key.ToLower();

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                // Search for alias.
                XmlNode nodeRoot = xd.SelectSingleNode("/vmping/aliases");
                foreach (XmlNode node in xd.SelectNodes($"/vmping/aliases/alias[@host={Configuration.GetEscapedXpath(key)}]"))
                {
                    // Found title.  Delete all versions.
                    nodeRoot.RemoveChild(node);
                }
                xd.Save(Configuration.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_DeleteAlias} {ex.Message}");
            }
        }


        public static bool IsNameInvalid(string name)
        {
            return string.IsNullOrWhiteSpace(name);
        }


        public static bool IsHostInvalid(string hostname)
        {
            return string.IsNullOrWhiteSpace(hostname);
        }
    }
}
