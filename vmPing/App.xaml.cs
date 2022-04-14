using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace vmPing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Force software rendering. Otherwise application may have high GPU usage on some video cards.
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            // The following code snippet was taken from Stack Overflow answer here:
            // https://stackoverflow.com/questions/520115/stringformat-localization-issues-in-wpf/520334#520334

            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
                  typeof(FrameworkElement),
                  new FrameworkPropertyMetadata(
                      XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        // DEBUG: Use this for testing alternate locales.
        //public App()
        //{
        //    vmPing.Properties.Strings.Culture = new System.Globalization.CultureInfo("en-GB");
        //}
    }
}
