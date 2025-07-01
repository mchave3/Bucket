using System;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts numeric value to true if zero, false otherwise
/// </summary>
public class ZeroToTrueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue == 0;
        }
        if (value is double doubleValue)
        {
            return Math.Abs(doubleValue) < 0.001;
        }
        if (value is float floatValue)
        {
            return Math.Abs(floatValue) < 0.001f;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
} 