using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Bucket.Models;

namespace Bucket.Common;

/// <summary>
/// Converts SearchMode enum values to Visibility based on a parameter
/// </summary>
public class SearchModeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SearchMode mode && parameter is string expectedMode)
        {
            return mode.ToString() == expectedMode ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
} 