using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using vmPing.Configuration;
using vmPing.Probes;
using vmPing.Properties;
using vmPing.Services;

namespace vmPing.Converters
{
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool v && v
                ? TrueValue
                : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(TrueValue);
        }
    }

    public class BoolToStringConverter : BoolToValueConverter<string> { }

    public class BooleanToHiddenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool v && v)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool v && v);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class InverseHiddenToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Returns true if visibility is hidden or collapsed.
            if (value is Visibility v)
            {
                return v == Visibility.Hidden || v == Visibility.Collapsed;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    internal static class ProbeBrushHelper
    {
        private static readonly BrushConverter s_brushConverter = new BrushConverter();

        public static Brush FromColorString(string color)
        {
            try
            {
                Brush brush = s_brushConverter.ConvertFromString(color) as Brush;
                return brush ?? Brushes.Transparent;
            }
            catch
            {
                return Brushes.Transparent;
            }
        }
    }

    public class ProbeStatusToBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ProbeStatus))
            {
                return Brushes.Transparent;
            }

            ProbeColors colors = ThemeManager.ProbeColors;
            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Up:
                    return ProbeBrushHelper.FromColorString(colors.BackgroundUp);
                case ProbeStatus.Down:
                    return ProbeBrushHelper.FromColorString(colors.BackgroundDown);
                case ProbeStatus.Error:
                    return ProbeBrushHelper.FromColorString(colors.BackgroundError);
                case ProbeStatus.LatencyHigh:
                case ProbeStatus.Indeterminate:
                    return ProbeBrushHelper.FromColorString(colors.BackgroundIndeterminate);
                case ProbeStatus.Tool:
                    return ProbeBrushHelper.FromColorString(colors.BackgroundTool);
                default:
                    return ProbeBrushHelper.FromColorString(colors.BackgroundInactive);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ProbeStatusToForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ProbeStatus))
            {
                return Brushes.Transparent;
            }

            ProbeColors colors = ThemeManager.ProbeColors;
            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Up:
                    return ProbeBrushHelper.FromColorString(colors.ForegroundUp);
                case ProbeStatus.Down:
                    return ProbeBrushHelper.FromColorString(colors.ForegroundDown);
                case ProbeStatus.Error:
                    return ProbeBrushHelper.FromColorString(colors.ForegroundError);
                case ProbeStatus.LatencyHigh:
                case ProbeStatus.Indeterminate:
                    return ProbeBrushHelper.FromColorString(colors.ForegroundIndeterminate);
                case ProbeStatus.Tool:
                    return ProbeBrushHelper.FromColorString(colors.ForegroundTool);
                default:
                    return ProbeBrushHelper.FromColorString(colors.ForegroundInactive);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ProbeStatusToStatisticsBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ProbeStatus))
            {
                return Brushes.Transparent;
            }

            ProbeColors colors = ThemeManager.ProbeColors;
            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Up:
                    return ProbeBrushHelper.FromColorString(colors.StatisticsUp);
                case ProbeStatus.Down:
                    return ProbeBrushHelper.FromColorString(colors.StatisticsDown);
                case ProbeStatus.Error:
                    return ProbeBrushHelper.FromColorString(colors.StatisticsError);
                case ProbeStatus.LatencyHigh:
                case ProbeStatus.Indeterminate:
                    return ProbeBrushHelper.FromColorString(colors.StatisticsIndeterminate);
                default:
                    return ProbeBrushHelper.FromColorString(colors.StatisticsInactive);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class StringToBrushConverter : IValueConverter
    {
        private static readonly BrushConverter s_brushConverter = new BrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (string.IsNullOrWhiteSpace(str))
            {
                return Binding.DoNothing;
            }

            try
            {
                Brush brush = s_brushConverter.ConvertFromString(str) as Brush;
                return brush ?? Binding.DoNothing;
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class HostnameFontsizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double))
            {
                return 12.5;
            }

            double width = (double)value;

            if (width > 250) return 18;
            if (width > 225) return 17;
            if (width > 200) return 16;
            if (width > 175) return 15;
            if (width > 150) return 14;
            if (width > 125) return 13;
            return 12.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ButtonTextVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is double v && v > 300)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ProbeStatusToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ProbeStatus))
            {
                return string.Empty;
            }

            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Up:
                    return "t";
                case ProbeStatus.Down:
                    return "u";
                case ProbeStatus.LatencyHigh:
                case ProbeStatus.Indeterminate:
                    return "i";
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


    public class ProbeCountToGlobalStartStopText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long count = (value is long v) ? v : 0;
            return count > 0
                ? Strings.Toolbar_StopAll
                : Strings.Toolbar_StartAll;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ProbeTypeToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ProbeType))
            {
                return ApplicationOptions.ProbeFontSize;
            }

            switch ((ProbeType)value)
            {
                case ProbeType.Ping:
                    return ApplicationOptions.ProbeFontSize;
                default:
                    return ApplicationOptions.ToolFontSize;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class StringLengthToBoolConverter : IValueConverter
    {
        // Return true if string is not empty.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
