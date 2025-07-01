using System;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts numeric value to true if greater than zero, false otherwise
/// </summary>
public class GreaterThanZeroToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue > 0;
        }
        if (value is double doubleValue)
        {
            return doubleValue > 0;
        }
        if (value is float floatValue)
        {
            return floatValue > 0;
        }
        if (value is long longValue)
        {
            return longValue > 0;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
} 