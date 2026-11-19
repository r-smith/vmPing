using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using vmPing.Configuration;
using vmPing.Interop;
using vmPing.Services;

namespace vmPing.Views
{
    // Lists the built-in and custom themes. Custom themes can be renamed, edited and deleted.
    // Built-in themes are read-only, and Duplicate makes an editable copy. Nothing changes until OK.
    public partial class ManageThemesWindow : Window
    {
        private const string ThemeFileFilter = "vmPing theme (*.xml)|*.xml|All files|*.*";

        private readonly ObservableCollection<ColorTheme> _themes;

        // The theme whose colors are in the grid.
        private ColorTheme _shownTheme;

        // True while the window is being filled in for a theme, so it isn't taken as an edit.
        private bool _isShowingTheme;

        // Set on OK: the custom themes, and the theme that was selected.
        public List<ColorTheme> CustomThemes { get; private set; }
        public ColorTheme SelectedTheme { get; private set; }

        public ManageThemesWindow(IEnumerable<ColorTheme> customThemes, ColorTheme selected)
        {
            InitializeComponent();
            ThemeManager.RegisterWindow(this);

            _themes = new ObservableCollection<ColorTheme>
            {
                ColorTheme.CreateSystem(),
                ColorTheme.CreateLight(),
                ColorTheme.CreateDark()
            };
            foreach (ColorTheme theme in customThemes)
            {
                _themes.Add(theme.Clone());
            }
            ThemeList.ItemsSource = _themes;

            // Start on the theme selected in Options.
            ThemeList.SelectedItem = FindTheme(selected) ?? _themes[0];
            ThemeList.ScrollIntoView(ThemeList.SelectedItem);
        }

        // The theme in this window's list that matches one from Options: the same built-in, or a custom theme with the same name.
        private ColorTheme FindTheme(ColorTheme theme)
        {
            if (theme == null)
            {
                return null;
            }

            if (theme.IsBuiltIn)
            {
                return _themes.FirstOrDefault(t => t.IsBuiltIn && t.IsSystem == theme.IsSystem && (t.IsSystem || t.IsDark == theme.IsDark));
            }

            return _themes.FirstOrDefault(t => !t.IsBuiltIn && string.Equals(t.Name, theme.Name, StringComparison.OrdinalIgnoreCase));
        }

        private void ThemeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ThemeList.SelectedItem is ColorTheme theme) || theme == _shownTheme)
            {
                return;
            }

            // Keep the shown theme's edits. A color that isn't valid keeps the theme selected until it's fixed.
            if (_shownTheme != null && !_shownTheme.IsBuiltIn)
            {
                if (!ValidateProbeColors())
                {
                    ThemeList.SelectedItem = _shownTheme;
                    return;
                }
                ReadProbeColors(_shownTheme.Colors);
            }

            ShowTheme(theme);
        }

        // Built-in themes are shown read-only, with a note in place of the name.
        private void ShowTheme(ColorTheme theme)
        {
            _isShowingTheme = true;
            _shownTheme = theme;
            ShowProbeColors(ThemeManager.GetProbeColors(theme));
            ThemeName.Text = theme.IsBuiltIn ? string.Empty : theme.Name;
            _isShowingTheme = false;

            ThemeNamePanel.Visibility = theme.IsBuiltIn ? Visibility.Hidden : Visibility.Visible;
            BuiltInNote.Visibility = theme.IsBuiltIn ? Visibility.Visible : Visibility.Hidden;
            DeleteThemeButton.IsEnabled = !theme.IsBuiltIn;
            ExportThemeButton.IsEnabled = !theme.IsBuiltIn;
            foreach (Visual control in ProbeColorGrid.GetChildren())
            {
                if (control is TextBox colorBox)
                {
                    colorBox.IsReadOnly = theme.IsBuiltIn;
                }
                else if (control is Button swatch)
                {
                    swatch.IsEnabled = !theme.IsBuiltIn;
                }
            }
        }

        private void ThemeName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isShowingTheme && _shownTheme != null && !_shownTheme.IsBuiltIn)
            {
                _shownTheme.Name = ThemeName.Text;
            }
        }

        // Adds a copy of the shown theme, named "<name> copy", and selects it with its name ready to type over.
        // Use Windows setting copies Light or Dark, whichever Windows is in.
        private void DuplicateTheme_Click(object sender, RoutedEventArgs e)
        {
            if (!_shownTheme.IsBuiltIn)
            {
                if (!ValidateProbeColors())
                {
                    return;
                }
                ReadProbeColors(_shownTheme.Colors);
            }

            bool isDark = ThemeManager.IsDarkTheme(_shownTheme);
            string baseName = _shownTheme.IsSystem
                ? (isDark ? ColorTheme.DarkName : ColorTheme.LightName)
                : _shownTheme.Name.Trim();
            ColorTheme copy = new ColorTheme
            {
                Name = ColorTheme.GetUniqueName($"{baseName} copy", _themes),
                IsDark = isDark,
                Colors = ThemeManager.GetProbeColors(_shownTheme).Clone()
            };
            _themes.Add(copy);

            ThemeList.SelectedItem = copy;
            ThemeList.ScrollIntoView(copy);
            ThemeName.Focus();
            ThemeName.SelectAll();
        }

        // Cancel brings a deleted theme back, so there's no confirmation.
        private void DeleteTheme_Click(object sender, RoutedEventArgs e)
        {
            ColorTheme theme = _shownTheme;
            if (theme == null || theme.IsBuiltIn)
            {
                return;
            }

            _shownTheme = null;
            _themes.Remove(theme);

            // Select the built-in theme with the same mode.
            ThemeList.SelectedItem = _themes.First(t => t.IsBuiltIn && !t.IsSystem && t.IsDark == theme.IsDark);
        }

        // Adds a theme from a file. A name that's already used gets a number added instead of replacing the other theme.
        private void ImportTheme_Click(object sender, RoutedEventArgs e)
        {
            // Keep the shown theme's edits first, since the imported theme is selected next.
            if (!_shownTheme.IsBuiltIn)
            {
                if (!ValidateProbeColors())
                {
                    return;
                }
                ReadProbeColors(_shownTheme.Colors);
            }

            string filePath;
            using (System.Windows.Forms.OpenFileDialog importDialog = new System.Windows.Forms.OpenFileDialog())
            {
                importDialog.Title = "Import theme";
                importDialog.RestoreDirectory = true;
                importDialog.Filter = ThemeFileFilter;
                if (importDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                filePath = importDialog.FileName;
            }

            ColorTheme theme;
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(filePath);
                theme = document.DocumentElement?.Name == "theme"
                    ? ConfigFile.ReadTheme(document.DocumentElement)
                    : null;
            }
            catch (Exception ex)
            {
                ShowError($"The theme couldn't be imported. {ex.Message}", null);
                return;
            }

            if (theme == null)
            {
                ShowError("This file isn't a vmPing theme.", null);
                return;
            }

            theme.Name = ColorTheme.GetUniqueName(theme.Name, _themes);
            _themes.Add(theme);
            ThemeList.SelectedItem = theme;
            ThemeList.ScrollIntoView(theme);
        }

        // Saves the shown theme to its own file, in the same format as a theme in the config file.
        private void ExportTheme_Click(object sender, RoutedEventArgs e)
        {
            if (_shownTheme.IsBuiltIn || !ValidateProbeColors())
            {
                return;
            }
            ReadProbeColors(_shownTheme.Colors);

            string name = _shownTheme.Name?.Trim() ?? string.Empty;
            if (name.Length == 0)
            {
                ShowError("Please enter a name for the theme.", ThemeName);
                return;
            }

            using (System.Windows.Forms.SaveFileDialog exportDialog = new System.Windows.Forms.SaveFileDialog())
            {
                exportDialog.Title = "Export theme";
                exportDialog.RestoreDirectory = true;
                exportDialog.OverwritePrompt = true;
                exportDialog.AddExtension = true;
                exportDialog.Filter = ThemeFileFilter;
                exportDialog.FileName = ToFileName(name) + ".xml";
                if (exportDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                ColorTheme theme = _shownTheme.Clone();
                theme.Name = name;
                try
                {
                    // No <?xml ...?> line at the top: it's optional, and UTF-8 is the default without it.
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        OmitXmlDeclaration = true,
                        Indent = true
                    };
                    using (XmlWriter writer = XmlWriter.Create(exportDialog.FileName, settings))
                    {
                        ConfigFile.GenerateThemeNode(theme).Save(writer);
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"The theme couldn't be exported. {ex.Message}", null);
                }
            }
        }

        // A theme name as a file name: characters Windows doesn't allow become underscores.
        private static string ToFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!_shownTheme.IsBuiltIn)
            {
                if (!ValidateProbeColors())
                {
                    return;
                }
                ReadProbeColors(_shownTheme.Colors);
            }

            if (!ValidateNames())
            {
                return;
            }

            CustomThemes = _themes.Where(t => !t.IsBuiltIn).ToList();
            foreach (ColorTheme theme in CustomThemes)
            {
                theme.Name = theme.Name.Trim();
            }
            SelectedTheme = _shownTheme;
            DialogResult = true;
        }

        // Custom theme names can't be empty, a built-in name, or used twice.
        private bool ValidateNames()
        {
            foreach (ColorTheme theme in _themes.Where(t => !t.IsBuiltIn))
            {
                string name = theme.Name?.Trim() ?? string.Empty;
                string problem = null;
                if (name.Length == 0)
                {
                    problem = "Please enter a name for the theme.";
                }
                else if (ColorTheme.IsBuiltInName(name))
                {
                    problem = $"\"{name}\" is the name of a built-in theme. Please choose another name.";
                }
                else if (_themes.Any(t => t != theme && !t.IsBuiltIn && string.Equals(t.Name?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
                {
                    problem = $"There's already a theme named \"{name}\". Please choose another name.";
                }

                if (problem != null)
                {
                    ThemeList.SelectedItem = theme;
                    ShowError(problem, ThemeName);
                    ThemeName.SelectAll();
                    return false;
                }
            }

            return true;
        }

        private bool ValidateProbeColors()
        {
            foreach (Visual control in ProbeColorGrid.GetChildren())
            {
                if (control is TextBox box && !Util.IsValidHtmlColor(box.Text))
                {
                    ShowError("Please enter a valid HTML color code.  Accepted formats are #RGB, #RRGGBB, and #AARRGGBB.  Example: #3266CF", box);
                    box.SelectAll();
                    return false;
                }
            }

            return true;
        }

        private void ShowError(string message, Control control)
        {
            DialogWindow errorWindow = DialogWindow.ErrorWindow(message);
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            control?.Focus();
        }

        // Opens the Windows color picker for the swatch's color box. The picker has no
        // transparency, so a color's existing alpha is kept.
        private void ColorSwatch_Click(object sender, RoutedEventArgs e)
        {
            TextBox colorBox = (TextBox)((Button)sender).Tag;
            byte alpha = 255;

            using (System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog())
            {
                dialog.FullOpen = true;
                try
                {
                    Color current = (Color)ColorConverter.ConvertFromString(colorBox.Text.Trim());
                    dialog.Color = System.Drawing.Color.FromArgb(current.R, current.G, current.B);
                    alpha = current.A;
                }
                catch (Exception)
                {
                    // Not a valid color yet: start from the picker's default.
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.Drawing.Color picked = dialog.Color;
                    colorBox.Text = alpha == 255
                        ? $"#{picked.R:x2}{picked.G:x2}{picked.B:x2}"
                        : $"#{alpha:x2}{picked.R:x2}{picked.G:x2}{picked.B:x2}";
                }
            }
        }

        private void HtmlColor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[#a-fA-F0-9]");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void ShowProbeColors(ProbeColors colors)
        {
            ProbeBackgroundInactive.Text = colors.BackgroundInactive;
            ProbeBackgroundUp.Text = colors.BackgroundUp;
            ProbeBackgroundDown.Text = colors.BackgroundDown;
            ProbeBackgroundIndeterminate.Text = colors.BackgroundIndeterminate;
            ProbeBackgroundError.Text = colors.BackgroundError;
            ProbeBackgroundTool.Text = colors.BackgroundTool;
            ProbeForegroundInactive.Text = colors.ForegroundInactive;
            ProbeForegroundUp.Text = colors.ForegroundUp;
            ProbeForegroundDown.Text = colors.ForegroundDown;
            ProbeForegroundIndeterminate.Text = colors.ForegroundIndeterminate;
            ProbeForegroundError.Text = colors.ForegroundError;
            ProbeForegroundTool.Text = colors.ForegroundTool;
            StatisticsForegroundInactive.Text = colors.StatisticsInactive;
            StatisticsForegroundUp.Text = colors.StatisticsUp;
            StatisticsForegroundDown.Text = colors.StatisticsDown;
            StatisticsForegroundIndeterminate.Text = colors.StatisticsIndeterminate;
            StatisticsForegroundError.Text = colors.StatisticsError;
        }

        private void ReadProbeColors(ProbeColors colors)
        {
            colors.BackgroundInactive = ProbeBackgroundInactive.Text;
            colors.BackgroundUp = ProbeBackgroundUp.Text;
            colors.BackgroundDown = ProbeBackgroundDown.Text;
            colors.BackgroundIndeterminate = ProbeBackgroundIndeterminate.Text;
            colors.BackgroundError = ProbeBackgroundError.Text;
            colors.BackgroundTool = ProbeBackgroundTool.Text;
            colors.ForegroundInactive = ProbeForegroundInactive.Text;
            colors.ForegroundUp = ProbeForegroundUp.Text;
            colors.ForegroundDown = ProbeForegroundDown.Text;
            colors.ForegroundIndeterminate = ProbeForegroundIndeterminate.Text;
            colors.ForegroundError = ProbeForegroundError.Text;
            colors.ForegroundTool = ProbeForegroundTool.Text;
            colors.StatisticsInactive = StatisticsForegroundInactive.Text;
            colors.StatisticsUp = StatisticsForegroundUp.Text;
            colors.StatisticsDown = StatisticsForegroundDown.Text;
            colors.StatisticsIndeterminate = StatisticsForegroundIndeterminate.Text;
            colors.StatisticsError = StatisticsForegroundError.Text;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideMinimizeAndMaximizeButtons(this);
        }
    }
}
