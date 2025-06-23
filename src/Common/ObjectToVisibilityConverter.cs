using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts any object to a Visibility enumeration value.
/// Returns Visible if the object is not null, Collapsed otherwise.
/// Supports reversing the conversion with the "Reverse" parameter.
/// </summary>
public class ObjectToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts an object to a Visibility value.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="targetType">The target type (not used).</param>
    /// <param name="parameter">Optional parameter. Use "Reverse" to invert the conversion.</param>
    /// <param name="language">The language (not used).</param>
    /// <returns>Visibility.Visible if object is not null, Visibility.Collapsed if null. Reversed if parameter is "Reverse".</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var hasValue = value != null;
        var reverse = parameter is string param && param.Equals("Reverse", StringComparison.OrdinalIgnoreCase);

        if (reverse)
        {
            hasValue = !hasValue;
        }

        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a Visibility value back to a boolean value indicating object presence.
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
