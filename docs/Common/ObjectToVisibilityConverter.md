# ObjectToVisibilityConverter Class Documentation

## Overview
The ObjectToVisibilityConverter is a value converter that converts any object to a Visibility enumeration value. It returns Visible if the object is not null, and Collapsed if the object is null. This converter is particularly useful for showing or hiding UI elements based on whether a bound object has a value.

## Location
- **File**: `src/Common/ObjectToVisibilityConverter.cs`
- **Namespace**: `Bucket.Common`

## Class Definition
```csharp
public class ObjectToVisibilityConverter : IValueConverter
```

## Methods

### Convert
```csharp
public object Convert(object value, Type targetType, object parameter, string language)
```
Converts an object to a Visibility value.

- **Parameters**:
  - `value`: The object to convert
  - `targetType`: The target type (not used)
  - `parameter`: Optional parameter. Use "Reverse" to invert the conversion
  - `language`: The language (not used)
- **Returns**: `Visibility.Visible` if object is not null, `Visibility.Collapsed` if null. Reversed if parameter is "Reverse"

### ConvertBack
```csharp
public object ConvertBack(object value, Type targetType, object parameter, string language)
```
Converts a Visibility value back to a boolean value indicating object presence.

- **Parameters**:
  - `value`: The Visibility value to convert
  - `targetType`: The target type (not used)
  - `parameter`: Optional parameter. Use "Reverse" to invert the conversion
  - `language`: The language (not used)
- **Returns**: True if Visible, false if Collapsed. Reversed if parameter is "Reverse"

## Usage Examples

### Basic Usage
```xml
<StackPanel Visibility="{Binding SelectedImage, Converter={StaticResource ObjectToVisibilityConverter}}">
    <!-- Content shown when SelectedImage is not null -->
</StackPanel>
```

### Reverse Usage
```xml
<TextBlock
    Text="No image selected"
    Visibility="{Binding SelectedImage, Converter={StaticResource ObjectToVisibilityConverter}, ConverterParameter=Reverse}" />
```

### Resource Definition
```xml
<ResourceDictionary xmlns:common="using:Bucket.Common">
    <common:ObjectToVisibilityConverter x:Key="ObjectToVisibilityConverter" />
</ResourceDictionary>
```

## Features
- **Null Check**: Automatically converts null objects to Collapsed visibility
- **Non-null Objects**: Converts any non-null object to Visible
- **Reverse Parameter**: Supports inverting the logic with "Reverse" parameter
- **Bidirectional**: Supports both Convert and ConvertBack operations

## Dependencies
- **Microsoft.UI.Xaml**: For Visibility enumeration and IValueConverter interface
- **Microsoft.UI.Xaml.Data**: For IValueConverter interface

## Related Files
- [`BooleanToVisibilityConverter.md`](./BooleanToVisibilityConverter.md) - For boolean-based visibility conversion
- [`ObjectToBooleanConverter.md`](./ObjectToBooleanConverter.md) - For object-to-boolean conversion
- [`../Themes/Converters.xaml`](../Themes/Converters.md) - Resource definitions

## Best Practices
- Use this converter when you need to show/hide UI elements based on object existence
- Use the "Reverse" parameter for inverse logic (showing messages when objects are null)
- Prefer this over chaining ObjectToBooleanConverter + BooleanToVisibilityConverter
- Register as a static resource in Converters.xaml for reuse across the application

## Error Handling
- Handles null values gracefully by returning Collapsed visibility
- Safely handles any object type without throwing exceptions
- Parameter parsing is case-insensitive for "Reverse"

## Use Cases in Bucket Application
- **Image Selection**: Show/hide image details panel based on selected image
- **Quick Details**: Display quick details only when an image is selected
- **Context Actions**: Enable/disable actions based on selection state
- **Placeholder Content**: Show "no selection" messages when nothing is selected

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
