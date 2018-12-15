using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace vmPing.Classes
{
    class Configuration
    {
        public static bool CheckAndInitializeConfigurationFile()
        {
            var rootPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing");
            var path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
            if (!Directory.Exists(rootPath))
                try
                {
                    Directory.CreateDirectory(rootPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create directory for vmPing configuration file.  " + ex.Message);
                    return false;
                }

            if (!File.Exists(path))
            {
                try
                {
                    // Generate new xml configuration file.
                    var xmlFile = new XmlDocument();
                    var rootNode = xmlFile.CreateElement("vmping");
                    xmlFile.AppendChild(rootNode);

                    rootNode.AppendChild(xmlFile.CreateElement("aliases"));
                    rootNode.AppendChild(xmlFile.CreateElement("configuration"));
                    rootNode.AppendChild(xmlFile.CreateElement("favorites"));

                    xmlFile.Save(path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create vmPing configuration file.  " + ex.Message);
                    return false;
                }
            }

            return true;
        }

        public static void UpgradeConfigurationFile()
        {
            var rootPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing");
            var oldPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPingFavorites.xml");
            var newPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\vmPing\vmPing.xml");
			var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();


			if (!Directory.Exists(rootPath))
                return;
            if (File.Exists(newPath))
			{
				//TODO : Version Control
				return;
			}
            if (!File.Exists(oldPath))
                return;
            else
            {
                // Upgrade old configuration file.
                var newXmlFile = new XmlDocument();
                var newRootNode = newXmlFile.CreateElement("vmping");
                newXmlFile.AppendChild(newRootNode);

                var oldXmlFile = new XmlDocument();
                oldXmlFile.Load(oldPath);
                var oldRootNode = oldXmlFile.FirstChild;

                newRootNode.AppendChild(newXmlFile.CreateElement("aliases"));
                newRootNode.AppendChild(newXmlFile.CreateElement("configuration"));
                newRootNode.AppendChild(newXmlFile.ImportNode(oldRootNode, true));

                newXmlFile.Save(newPath);
            }
        }


        public static string GetEscapedXpath(string xpath)
        {
            if (!xpath.Contains("'"))
                return '\'' + xpath + '\'';
            else if (!xpath.Contains("\""))
                return '"' + xpath + '"';
            else
                return "concat('" + xpath.Replace("'", "',\"'\",'") + "')";
        }
    }
}
