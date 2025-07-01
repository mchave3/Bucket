using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts numeric value to Visibility.Visible if zero, Collapsed otherwise
/// </summary>
public class ZeroToVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        if (value is double doubleValue)
        {
            return Math.Abs(doubleValue) < 0.001 ? Visibility.Visible : Visibility.Collapsed;
        }
        if (value is float floatValue)
        {
            return Math.Abs(floatValue) < 0.001f ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
} 