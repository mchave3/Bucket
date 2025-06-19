using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts boolean values to Visibility enumeration values.
/// Supports reversing the conversion with the "Reverse" parameter.
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type (not used).</param>
    /// <param name="parameter">Optional parameter. Use "Reverse" to invert the conversion.</param>
    /// <param name="language">The language (not used).</param>
    /// <returns>Visibility.Visible if true, Visibility.Collapsed if false. Reversed if parameter is "Reverse".</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var boolValue = value is bool b && b;
        var reverse = parameter is string param && param.Equals("Reverse", StringComparison.OrdinalIgnoreCase);

        if (reverse)
        {
            boolValue = !boolValue;
        }

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a Visibility value back to a boolean value.
    /// </summary>
    /// <param name="value">The Visibility value to convert.</param>
    /// <param name="targetType">The target type (not used).</param>
    /// <param name="parameter">Optional parameter. Use "Reverse" to invert the conversion.</param>
    /// <param name="language">The language (not used).</param>
    /// <returns>True if Visible, false if Collapsed. Reversed if parameter is "Reverse".</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        var isVisible = value is Visibility visibility && visibility == Visibility.Visible;
        var reverse = parameter is string param && param.Equals("Reverse", StringComparison.OrdinalIgnoreCase);

        if (reverse)
        {
            isVisible = !isVisible;
        }

        return isVisible;
    }
}
