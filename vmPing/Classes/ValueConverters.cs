using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace vmPing.Classes
{
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    public class BoolToStringConverter : BoolToValueConverter<string> { }

    public class BooleanToHiddenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false)
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value == true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false)
                return new BitmapImage(new Uri(@"/Resources/play-16.png", UriKind.Relative));
            else
                return new BitmapImage(new Uri(@"/Resources/stopCircle-16.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProbeStatusToBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Up:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.BackgroundColor_Probe_Up);
                case ProbeStatus.Down:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.BackgroundColor_Probe_Down);
                case ProbeStatus.Error:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.BackgroundColor_Probe_Error);
                case ProbeStatus.Indeterminate:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.BackgroundColor_Probe_Indeterminate);
                default:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.BackgroundColor_Probe_Inactive);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProbeStatusToForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Up:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Probe_Up);
                case ProbeStatus.Down:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Probe_Down);
                case ProbeStatus.Error:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Probe_Error);
                case ProbeStatus.Indeterminate:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Probe_Indeterminate);
                default:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Probe_Inactive);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProbeStatusToStatisticsBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Up:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Stats_Up);
                case ProbeStatus.Down:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Stats_Down);
                case ProbeStatus.Error:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Stats_Error);
                case ProbeStatus.Indeterminate:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Stats_Indeterminate);
                default:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Stats_Inactive);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProbeStatusToAliasBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Up:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Alias_Up);
                case ProbeStatus.Down:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Alias_Down);
                case ProbeStatus.Error:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Alias_Error);
                case ProbeStatus.Indeterminate:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Alias_Indeterminate);
                default:
                    return (Brush)new BrushConverter().ConvertFromString(ApplicationOptions.ForegroundColor_Alias_Inactive);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (Brush)new BrushConverter().ConvertFromString((string)value);
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HostnameFontsizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double)value > 250)
                return 18;
            else if ((double)value > 225)
                return 17;
            else if ((double)value > 200)
                return 16;
            else if ((double)value > 175)
                return 15;
            else if ((double)value > 150)
                return 14;
            else if ((double)value > 125)
                return 13;
            else return 12.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ButtonTextVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double)value > 300)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProbeStatusToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var returnString = string.Empty;
            switch ((ProbeStatus)value)
            {
                case ProbeStatus.Error:
                    //returnString = "r";
                    break;
                case ProbeStatus.Down:
                    returnString = "u";
                    break;
                case ProbeStatus.Indeterminate:
                    returnString = "i";
                    break;
                case ProbeStatus.Up:
                    returnString = "t";
                    break;
            }
            return returnString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ProbeCountToGlobalStartStopText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value > 0)
                return "_Stop All (F5)";
            else
                return "_Start All (F5)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProbeCountToGlobalStartStopIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value > 0)
                return new BitmapImage(new Uri(@"/Resources/stopCircle-16.png", UriKind.Relative));
            else
                return new BitmapImage(new Uri(@"/Resources/play-16.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
