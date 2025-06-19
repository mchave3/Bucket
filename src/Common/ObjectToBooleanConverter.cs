using Microsoft.UI.Xaml.Data;

namespace Bucket.Common;

/// <summary>
/// Converts any object to a boolean value.
/// Returns true if the object is not null, false otherwise.
/// </summary>
public class ObjectToBooleanConverter : IValueConverter
{
    /// <summary>
    /// Converts an object to a boolean value.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="targetType">The target type (not used).</param>
    /// <param name="parameter">Optional parameter (not used).</param>
    /// <param name="language">The language (not used).</param>
    /// <returns>True if the object is not null, false otherwise.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null;
    }

    /// <summary>
    /// Converts a boolean value back to an object.
    /// This method is not implemented as it's not typically needed.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type (not used).</param>
    /// <param name="parameter">Optional parameter (not used).</param>
    /// <param name="language">The language (not used).</param>
    /// <returns>The original value (not converted).</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("ConvertBack is not implemented for ObjectToBooleanConverter");
    }
}
