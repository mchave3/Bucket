using System;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts DateTime values to formatted strings
/// </summary>
public class DateFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            // Use parameter as format string if provided, otherwise use short date format
            var format = parameter as string ?? "d";
            return dateTime.ToString(format);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string dateString && DateTime.TryParse(dateString, out var result))
        {
            return result;
        }
        return DateTime.MinValue;
    }
} 