using System;
using System.Collections.Generic;
using System.Xml;
using vmPing.Properties;

namespace vmPing.Classes
{
    class Alias
    {
        private const string RootPath = "/vmping/aliases";
        private const string AliasPath = "/vmping/aliases/alias";
        private const string HostAttribute = "host";

        public static Dictionary<string, string> GetAll()
        {
            var aliases = new Dictionary<string, string>();

            if (!Configuration.Exists())
            {
                return aliases;
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                var nodes = xd.SelectNodes(AliasPath);
                if (nodes == null)
                {
                    return aliases;
                }

                foreach (XmlNode node in nodes)
                {
                    var host = node.Attributes?[HostAttribute]?.Value;
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
            if (!Configuration.IsReady() || string.IsNullOrWhiteSpace(hostname))
            {
                return;
            }

            hostname = hostname.ToLowerInvariant();

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                var root = xd.SelectSingleNode(RootPath);
                if (root == null)
                {
                    return;
                }

                // Remove alias if it already exists.
                foreach (XmlNode node in xd.SelectNodes($"{AliasPath}[@{HostAttribute}={Configuration.GetEscapedXpath(hostname)}]"))
                {
                    root.RemoveChild(node);
                }

                // Add alias.
                var aliasNode = xd.CreateElement("alias");
                aliasNode.SetAttribute(HostAttribute, hostname);
                aliasNode.InnerText = alias;
                root.AppendChild(aliasNode);

                xd.Save(Configuration.FilePath);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_AddAlias} {ex.Message}");
            }
        }

        public static void Delete(string hostname)
        {
            if (!Configuration.Exists() || string.IsNullOrWhiteSpace(hostname))
            {
                return;
            }

            hostname = hostname.ToLowerInvariant();

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.FilePath);

                // Search for alias.
                var root = xd.SelectSingleNode(RootPath);
                if (root == null)
                {
                    return;
                }

                var nodes = xd.SelectNodes($"{AliasPath}[@{HostAttribute}={Configuration.GetEscapedXpath(hostname)}]");
                if (nodes == null)
                {
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    root.RemoveChild(node);
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
