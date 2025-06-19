# ObjectToBooleanConverter Class Documentation

## Overview

A WinUI 3 value converter that transforms any object into a boolean value based on its null state. This converter is primarily used to enable or disable UI controls based on whether an object (such as a selected item) is null or not. It's commonly used for button enabling/disabling scenarios in MVVM applications.

## Location

- **File**: `src/Common/ObjectToBooleanConverter.cs`
- **Namespace**: `Bucket.Common`

## Class Definition

```csharp
public class ObjectToBooleanConverter : IValueConverter
```

## Methods

### Convert

```csharp
public object Convert(object value, Type targetType, object parameter, string language)
```

Converts an object to a boolean value based on its null state.

**Parameters:**
- `value`: The object to convert
- `targetType`: The target type (not used)
- `parameter`: Optional parameter (not used)
- `language`: The language (not used)

**Returns:**
- `true` if the object is not null
- `false` if the object is null

### ConvertBack

```csharp
public object ConvertBack(object value, Type targetType, object parameter, string language)
```

Converts a boolean value back to an object. This method is not implemented as it's not typically needed for object-to-boolean conversions.

**Throws:** `NotImplementedException`

## Usage Examples

### Basic XAML Usage

```xaml
<!-- Enable button only when an item is selected -->
<Button Content="Delete Selected"
        IsEnabled="{Binding SelectedImage, Converter={StaticResource ObjectToBooleanConverter}}" />

<!-- Enable multiple buttons based on selection -->
<Button Content="Edit"
        IsEnabled="{Binding SelectedItem, Converter={StaticResource ObjectToBooleanConverter}}" />
<Button Content="Remove"
        IsEnabled="{Binding SelectedItem, Converter={StaticResource ObjectToBooleanConverter}}" />
```

### Resource Dictionary Registration

```xaml
<ResourceDictionary xmlns:common="using:Bucket.Common">
    <common:ObjectToBooleanConverter x:Key="ObjectToBooleanConverter" />
</ResourceDictionary>
```

### ViewModel Properties

```csharp
[ObservableProperty]
private WindowsImageInfo selectedImage;

[ObservableProperty]
private object selectedItem;
```

## Features

### Null State Detection
- **Reliable**: Uses simple null checking for consistent behavior
- **Type Agnostic**: Works with any object type
- **Performance**: Minimal overhead with direct null comparison

### Common Use Cases
- **Button Enabling**: Enable/disable buttons based on selection
- **Validation**: Check if required objects are available
- **Conditional Logic**: Simple null-state-based UI decisions

## Dependencies

### External Dependencies
- **Microsoft.UI.Xaml.Data**: For IValueConverter interface
- **System**: For NotImplementedException

### Internal Dependencies
- None (standalone utility class)

## Related Files

- [`BooleanToVisibilityConverter.md`](./BooleanToVisibilityConverter.md): Related converter for boolean-to-visibility conversion
- [`ImageManagementPage.md`](../Views/ImageManagementPage.md): Primary usage example
- [`Converters.xaml`](../Themes/Converters.md): Resource dictionary registration

## Best Practices

### XAML Usage

```xaml
<!-- Recommended: Clear, descriptive bindings -->
<Button Content="Process Selected"
        IsEnabled="{Binding SelectedImage, Converter={StaticResource ObjectToBooleanConverter}}" />

<!-- Avoid: Don't use for complex logic -->
<!-- Use ViewModel properties instead -->
```

### ViewModel Design

```csharp
// Good: Simple object properties
[ObservableProperty]
private WindowsImageInfo selectedImage;

// Better: For complex logic, use computed properties
public bool CanProcessImage => SelectedImage != null && SelectedImage.FileExists;
```

### Performance Considerations
- Use this converter for simple null checks only
- For complex boolean logic, implement computed properties in ViewModels
- Cache converter instances through resource dictionaries

## Error Handling

### Input Validation
- **Null Safety**: Handles null input gracefully
- **Type Safety**: Works with any object type
- **No Exceptions**: Convert method never throws exceptions

### ConvertBack Limitations
- ConvertBack is not implemented by design
- Throws NotImplementedException if called
- One-way conversion is the intended usage pattern

## Performance Notes

### Optimization Characteristics
- **Minimal Overhead**: Simple null comparison
- **No Allocations**: Returns cached boolean values
- **Thread Safe**: No internal state, safe for concurrent use

### Usage Recommendations
- Prefer this converter over code-behind for enabling/disabling controls
- Use for reactive UI updates in MVVM scenarios
- Combine with other converters for complex scenarios if needed

## Security Considerations

- **No Security Implications**: Pure null-state checking
- **Safe with User Data**: No data inspection beyond null checking
- **No Side Effects**: Read-only operation with no external dependencies

## Common Patterns

### Selection-Based Actions

```xaml
<!-- Typical pattern: Actions enabled when item selected -->
<StackPanel Orientation="Horizontal">
    <Button Content="Edit"
            IsEnabled="{Binding SelectedItem, Converter={StaticResource ObjectToBooleanConverter}}" />
    <Button Content="Delete"
            IsEnabled="{Binding SelectedItem, Converter={StaticResource ObjectToBooleanConverter}}" />
    <Button Content="Export"
            IsEnabled="{Binding SelectedItem, Converter={StaticResource ObjectToBooleanConverter}}" />
</StackPanel>
```

### Form Validation

```xaml
<!-- Enable submit when required object is available -->
<Button Content="Submit"
        IsEnabled="{Binding ValidationResult, Converter={StaticResource ObjectToBooleanConverter}}" />
```

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
