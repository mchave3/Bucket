using System;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Formats values using a format string provided as parameter
/// </summary>
public class StringFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string format && !string.IsNullOrEmpty(format))
        {
            return string.Format(format, value);
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
} 