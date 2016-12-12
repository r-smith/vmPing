using System;
using System.Collections.Generic;
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

    public class PingStatusToBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((PingStatus)value)
            {
                case PingStatus.Up:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_UP);
                case PingStatus.Down:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_DOWN);
                case PingStatus.Error:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_ERROR);
                default:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_BACKCOLOR_INACTIVE);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PingStatusToForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((PingStatus)value)
            {
                case PingStatus.Inactive:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_INACTIVE);
                case PingStatus.Error:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ERROR);
                default:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.TXTOUTPUT_FORECOLOR_ACTIVE);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PingStatusToStatisticsBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((PingStatus)value)
            {
                case PingStatus.Up:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_UP);
                case PingStatus.Down:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_DOWN);
                case PingStatus.Error:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_ERROR);
                default:
                    return (Brush)new BrushConverter().ConvertFromString(Constants.LBLSTATS_FORECOLOR_INACTIVE);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
