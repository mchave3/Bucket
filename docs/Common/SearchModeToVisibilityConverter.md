# SearchModeToVisibilityConverter Class Documentation

## Overview
Converts SearchMode enum values to Visibility based on a parameter. Used to show/hide UI panels based on the selected search mode in the Microsoft Update Catalog page.

## Location
- **File**: `src/Common/SearchModeToVisibilityConverter.cs`
- **Namespace**: `Bucket.Common`

## Class Definition
```csharp
public class SearchModeToVisibilityConverter : IValueConverter
```

## Methods

### Convert
```csharp
public object Convert(object value, Type targetType, object parameter, string language)
```

Converts a SearchMode enum value to Visibility.

**Parameters:**
- `value`: The SearchMode enum value
- `targetType`: Target type (should be Visibility)
- `parameter`: Expected mode name as string (e.g., "OperatingSystem" or "SearchQuery")
- `language`: Language parameter (not used)

**Returns:** 
- `Visibility.Visible` if the mode matches the parameter
- `Visibility.Collapsed` otherwise

### ConvertBack
```csharp
public object ConvertBack(object value, Type targetType, object parameter, string language)
```

Not implemented (throws NotImplementedException).

## Usage Examples

### XAML Usage
```xml
<!-- Show panel when SearchMode is OperatingSystem -->
<Grid Visibility="{x:Bind ViewModel.SearchMode, Mode=OneWay, 
                   Converter={StaticResource SearchModeToVisibilityConverter}, 
                   ConverterParameter=OperatingSystem}">
    <!-- OS search controls -->
</Grid>

<!-- Show panel when SearchMode is SearchQuery -->
<TextBox Visibility="{x:Bind ViewModel.SearchMode, Mode=OneWay, 
                      Converter={StaticResource SearchModeToVisibilityConverter}, 
                      ConverterParameter=SearchQuery}"/>
```

### Resource Declaration
```xml
<common:SearchModeToVisibilityConverter x:Key="SearchModeToVisibilityConverter" />
```

## Features
- Type-safe enum to visibility conversion
- Parameter-based mode matching
- One-way conversion only
- Null-safe implementation

## Dependencies
- Microsoft.UI.Xaml for Visibility enum
- Bucket.Models for SearchMode enum

## Related Files
- [`MicrosoftUpdateCatalogPage.md`](../Views/MicrosoftUpdateCatalogPage.md) - Primary usage
- [`MSCatalogSearchRequest.md`](../Models/MSCatalogSearchRequest.md) - SearchMode enum definition

## Best Practices
1. Always provide a valid SearchMode string as parameter
2. Use Mode=OneWay in bindings (ConvertBack not supported)
3. Consider caching converter instances in resources
4. Handle null values gracefully

## Error Handling
- Returns Collapsed for null or invalid values
- Parameter must match enum value name exactly
- Case-sensitive string comparison

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 