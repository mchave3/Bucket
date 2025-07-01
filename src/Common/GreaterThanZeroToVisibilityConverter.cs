using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts numeric value to Visibility.Visible if greater than zero, Collapsed otherwise
/// </summary>
public class GreaterThanZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        if (value is double doubleValue)
        {
            return doubleValue > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        if (value is float floatValue)
        {
            return floatValue > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        if (value is long longValue)
        {
            return longValue > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
} 