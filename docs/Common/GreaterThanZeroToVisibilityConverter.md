# GreaterThanZeroToVisibilityConverter Class Documentation

## Overview
A value converter that converts numeric values to WinUI Visibility enumeration values. Returns `Visibility.Visible` if the input value is greater than zero, otherwise returns `Visibility.Collapsed`.

## Location
- **File**: `src/Common/GreaterThanZeroToVisibilityConverter.cs`
- **Namespace**: `Bucket.Common`

## Class Definition
```csharp
public class GreaterThanZeroToVisibilityConverter : IValueConverter
```

## Methods

### Convert
```csharp
public object Convert(object value, Type targetType, object parameter, string language)
```
Converts numeric values to Visibility enumeration values.

**Parameters:**
- `value`: The numeric value to convert (supports int, double, float, long)
- `targetType`: The target type (should be Visibility)
- `parameter`: Not used in this converter
- `language`: Not used in this converter

**Returns:**
- `Visibility.Visible` if the value is greater than zero
- `Visibility.Collapsed` if the value is zero or less, or if the type is not supported

### ConvertBack
```csharp
public object ConvertBack(object value, Type targetType, object parameter, string language)
```
Not implemented. Throws `NotImplementedException`.

## Usage Examples

### XAML Binding
```xaml
<!-- Show element only when count is greater than zero -->
<TableView Visibility="{x:Bind ViewModel.Items.Count, Mode=OneWay, Converter={StaticResource GreaterThanZeroToVisibilityConverter}}" />

<!-- Show button only when selected items exist -->
<Button Visibility="{x:Bind ViewModel.SelectedItems.Count, Mode=OneWay, Converter={StaticResource GreaterThanZeroToVisibilityConverter}}" />
```

### Resource Declaration
```xaml
<ResourceDictionary>
    <common:GreaterThanZeroToVisibilityConverter x:Key="GreaterThanZeroToVisibilityConverter" />
</ResourceDictionary>
```

## Features
- **Type Safety**: Handles multiple numeric types (int, double, float, long)
- **Null Safety**: Returns `Visibility.Collapsed` for unsupported types
- **Performance**: Lightweight conversion with no complex logic
- **WinUI Integration**: Directly returns WinUI Visibility enum values

## Supported Types
- `int` - Integer values
- `double` - Double-precision floating-point values
- `float` - Single-precision floating-point values  
- `long` - Long integer values

## Dependencies
- `Microsoft.UI.Xaml` - For Visibility enumeration
- `Microsoft.UI.Xaml.Data` - For IValueConverter interface

## Related Files
- [`BooleanToVisibilityConverter.md`](./BooleanToVisibilityConverter.md) - Boolean to Visibility conversion
- [`ZeroToVisibleConverter.md`](./ZeroToVisibleConverter.md) - Inverse visibility conversion (visible when zero)
- [`GreaterThanZeroToBoolConverter.md`](./GreaterThanZeroToBoolConverter.md) - Similar converter for boolean output

## Best Practices
- Use this converter when you need to show/hide UI elements based on numeric values
- Combine with other converters if more complex logic is needed
- Prefer this over boolean converters when binding directly to Visibility properties

## Common Use Cases
- Hide empty state messages when items are present
- Show action buttons only when items are selected
- Display progress indicators when operations are in progress
- Show/hide sections based on data availability

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 