using System;
using System.IO;
using System.Xml.Linq;
using vmPing.Classes;

namespace vmPing.Classes
{
    public class ApplicationSettings
    {
        private readonly string _settingsPath;
        private readonly string _settingsFile = "vmping_settings.xml";

        public ApplicationSettings()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "vmPing"
            );

            if (!Directory.Exists(_settingsPath))
            {
                Directory.CreateDirectory(_settingsPath);
            }
        }

        public void LoadSettings()
        {
            try
            {
                string filePath = Path.Combine(_settingsPath, _settingsFile);
                if (File.Exists(filePath))
                {
                    XDocument doc = XDocument.Load(filePath);
                    XElement root = doc.Root;

                    // Load window position settings
                    if (double.TryParse(root.Element("WindowLeft")?.Value, out var left))
                        ApplicationOptions.WindowLeft = left;
                    if (double.TryParse(root.Element("WindowTop")?.Value, out var top))
                        ApplicationOptions.WindowTop = top;
                    if (double.TryParse(root.Element("WindowWidth")?.Value, out var width))
                        ApplicationOptions.WindowWidth = width;
                    if (double.TryParse(root.Element("WindowHeight")?.Value, out var height))
                        ApplicationOptions.WindowHeight = height;
                    
                    var windowState = root.Element("WindowState")?.Value;
                    if (!string.IsNullOrEmpty(windowState))
                        ApplicationOptions.WindowState = windowState;

                    if (bool.TryParse(root.Element("RememberWindowPosition")?.Value, out var remember))
                        ApplicationOptions.RememberWindowPosition = remember;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        public void SaveSettings()
        {
            try
            {
                XDocument doc = new XDocument(
                    new XElement("Settings",
                        new XElement("WindowLeft", ApplicationOptions.WindowLeft),
                        new XElement("WindowTop", ApplicationOptions.WindowTop),
                        new XElement("WindowWidth", ApplicationOptions.WindowWidth),
                        new XElement("WindowHeight", ApplicationOptions.WindowHeight),
                        new XElement("WindowState", ApplicationOptions.WindowState),
                        new XElement("RememberWindowPosition", ApplicationOptions.RememberWindowPosition)
                    )
                );

                string filePath = Path.Combine(_settingsPath, _settingsFile);
                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}