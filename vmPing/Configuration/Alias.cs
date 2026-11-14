using System;
using System.Collections.Generic;
using System.Xml;
using vmPing.Properties;

namespace vmPing.Configuration
{
    class Alias
    {
        private const string RootPath = "/vmping/aliases";
        private const string AliasPath = "/vmping/aliases/alias";
        private const string HostAttribute = "host";

        public static Dictionary<string, string> GetAll()
        {
            Dictionary<string, string> aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!ConfigFile.Exists())
            {
                return aliases;
            }

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                XmlNodeList nodes = xd.SelectNodes(AliasPath);
                if (nodes == null)
                {
                    return aliases;
                }

                foreach (XmlNode node in nodes)
                {
                    string host = node.Attributes?[HostAttribute]?.Value;
                    if (!string.IsNullOrEmpty(host))
                    {
                        aliases[host.ToLowerInvariant()] = node.InnerText?.Trim() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_LoadAlias} {ex.Message}");
            }

            return aliases;
        }

        public static void Add(string hostname, string alias)
        {
            if (!ConfigFile.IsReady() || string.IsNullOrWhiteSpace(hostname))
            {
                return;
            }

            hostname = hostname.ToLowerInvariant();

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                XmlNode root = xd.SelectSingleNode(RootPath)
                    ?? throw new XmlException("Invalid configuration file.");

                // Remove alias if it already exists.
                foreach (XmlNode node in xd.SelectNodes($"{AliasPath}[@{HostAttribute}={ConfigFile.GetEscapedXpath(hostname)}]"))
                {
                    root.RemoveChild(node);
                }

                // Add alias.
                XmlElement aliasNode = xd.CreateElement("alias");
                aliasNode.SetAttribute(HostAttribute, hostname);
                aliasNode.InnerText = alias;
                root.AppendChild(aliasNode);

                xd.Save(ConfigFile.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_AddAlias} {ex.Message}");
            }
        }

        public static void Delete(string hostname)
        {
            if (!ConfigFile.Exists() || string.IsNullOrWhiteSpace(hostname))
            {
                return;
            }

            hostname = hostname.ToLowerInvariant();

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ConfigFile.FilePath);

                // Search for alias.
                XmlNode root = xd.SelectSingleNode(RootPath);
                if (root == null)
                {
                    return;
                }

                XmlNodeList nodes = xd.SelectNodes($"{AliasPath}[@{HostAttribute}={ConfigFile.GetEscapedXpath(hostname)}]");
                if (nodes == null)
                {
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    root.RemoveChild(node);
                }

                xd.Save(ConfigFile.FilePath);
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
