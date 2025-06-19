# BooleanToVisibilityConverter Class Documentation

## Overview
A WinUI 3 value converter that transforms boolean values into Visibility enumeration values. This converter enables XAML data binding scenarios where UI element visibility needs to be controlled by boolean properties in ViewModels. It supports both normal and reverse conversion modes.

## Location
- **File**: `src/Common/BooleanToVisibilityConverter.cs`
- **Namespace**: `Bucket.Common`

## Class Definition
```csharp
public class BooleanToVisibilityConverter : IValueConverter
```

## Methods

### Convert
```csharp
public object Convert(object value, Type targetType, object parameter, string language)
```
Converts a boolean value to a Visibility value.

**Parameters:**
- `value`: The boolean value to convert
- `targetType`: The target type (not used)
- `parameter`: Optional parameter. Use "Reverse" to invert the conversion
- `language`: The language (not used)

**Returns:**
- `Visibility.Visible` if true
- `Visibility.Collapsed` if false
- Reversed if parameter is "Reverse"

### ConvertBack
```csharp
public object ConvertBack(object value, Type targetType, object parameter, string language)
```
Converts a Visibility value back to a boolean value.

**Parameters:**
- `value`: The Visibility value to convert
- `targetType`: The target type (not used)
- `parameter`: Optional parameter. Use "Reverse" to invert the conversion
- `language`: The language (not used)

**Returns:**
- `true` if Visible
- `false` if Collapsed
- Reversed if parameter is "Reverse"

## Usage Examples

### Basic XAML Usage
```xaml
<!-- Show element when IsLoading is true -->
<ProgressRing Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}" />

<!-- Hide element when HasData is true (using reverse) -->
<TextBlock Text="No data available"
           Visibility="{Binding HasData, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Reverse}" />
```

### Resource Dictionary Registration
```xaml
<ResourceDictionary xmlns:common="using:Bucket.Common">
    <common:BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />
</ResourceDictionary>
```

### ViewModel Properties
```csharp
[ObservableProperty]
private bool isLoading;

[ObservableProperty]
private bool hasImages;
```

## Features

### Conversion Modes
- **Normal Mode**: `true` → `Visible`, `false` → `Collapsed`
- **Reverse Mode**: `true` → `Collapsed`, `false` → `Visible`

### Parameter Support
- Uses `ConverterParameter="Reverse"` to invert the conversion logic
- Case-insensitive parameter matching
- Graceful handling of null or invalid parameters

### Null Safety
- Handles null input values gracefully
- Treats null as false for boolean conversion
- Returns appropriate default values

## Dependencies

### External Dependencies
- **Microsoft.UI.Xaml**: For Visibility enumeration and IValueConverter interface
- **Microsoft.UI.Xaml.Data**: For data binding infrastructure

### Internal Dependencies
- None (standalone utility class)

## Related Files
- [`ObjectToBooleanConverter.md`](./ObjectToBooleanConverter.md): Related converter for object-to-boolean conversion
- [`ImageManagementPage.md`](../Views/ImageManagementPage.md): Primary usage example
- [`Converters.xaml`](../Themes/Converters.md): Resource dictionary registration

## Best Practices

### XAML Usage
- Always register the converter in a resource dictionary
- Use meaningful resource keys for better maintainability
- Prefer StaticResource over DynamicResource for performance

### Parameter Usage
```xaml
<!-- Correct parameter usage -->
<Grid Visibility="{Binding HasData, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Reverse}" />

<!-- Avoid unnecessary parameters -->
<ProgressRing Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}" />
```

### ViewModel Design
- Use boolean properties for simple show/hide logic
- Consider computed properties for complex visibility rules
- Implement INotifyPropertyChanged for reactive UI updates

## Error Handling

### Invalid Input Types
- Non-boolean values are treated as false
- Null values are treated as false
- No exceptions thrown for type mismatches

### Parameter Validation
- Invalid parameters are ignored (treated as normal mode)
- Case-insensitive "Reverse" parameter matching
- Graceful fallback to default behavior

## Performance Notes

### Optimization Considerations
- Lightweight conversion with minimal overhead
- No memory allocations during conversion
- Efficient string comparison for parameter checking

### Usage Recommendations
- Prefer this converter over code-behind visibility management
- Cache converter instances through resource dictionaries
- Use for reactive UI updates in MVVM scenarios

## Security Considerations
- No security implications (pure data transformation)
- Safe for use with user-provided data
- No external dependencies or side effects

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
