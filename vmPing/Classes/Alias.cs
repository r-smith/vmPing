using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;

namespace vmPing.Classes
{
    class Alias
    {
        public static Dictionary<string, string> GetAliases()
        {
            var aliases = new Dictionary<string, string>();

            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
            if (!File.Exists(path))
                return aliases;

            try
            {
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNodeList nodeAlias = xd.SelectNodes("/vmping/aliases/alias");

                foreach (XmlNode node in nodeAlias)
                    aliases.Add(node.Attributes["host"].Value, node.InnerText);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return aliases;
        }


        public static void AddAlias(string hostname, string alias)
        {
            if (Configuration.CheckAndInitializeConfigurationFile() == false)
                return;

            try
            {
                var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
                var xd = new XmlDocument();
                xd.Load(path);

                XmlNode nodeRoot = xd.SelectSingleNode("/vmping/aliases");

                // Check if title already exists.
                XmlNodeList nodeAliasSearch = xd.SelectNodes($"/vmping/aliases/favorite[@hosts={Configuration.GetEscapedXpath(hostname)}]");
                foreach (XmlNode node in nodeAliasSearch)
                {
                    // Title already exists.  Delete any old versions.
                    nodeRoot.RemoveChild(node);
                }

                XmlElement aliasEntry = xd.CreateElement("alias");
                aliasEntry.SetAttribute("host", hostname.ToUpper());
                aliasEntry.InnerText = alias;
                nodeRoot.AppendChild(aliasEntry);
                xd.Save(path);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
