using System;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts boolean values to their negation
/// </summary>
public class BooleanNegationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
} 