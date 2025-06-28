# MountedImageInfo Class Documentation

## Overview
Represents information about a mounted Windows image, including mount path, image details, and status information.

## Location
- **File**: `src/Models/MountedImageInfo.cs`
- **Namespace**: `Bucket.Models`

## Class Definition
```csharp
public partial class MountedImageInfo : ObservableObject
```

## Properties

### Id
```csharp
[ObservableProperty]
private string id = string.Empty;
```
Gets or sets the unique identifier for this mount.

### ImagePath
```csharp
[ObservableProperty]
private string imagePath = string.Empty;
```
Gets or sets the path to the WIM/ESD file.

### Index
```csharp
[ObservableProperty]
private int index;
```
Gets or sets the index number that is mounted.

### MountPath
```csharp
[ObservableProperty]
private string mountPath = string.Empty;
```
Gets or sets the mount directory path.

### ImageName
```csharp
[ObservableProperty]
private string imageName = string.Empty;
```
Gets or sets the friendly name of the image.

### EditionName
```csharp
[ObservableProperty]
private string editionName = string.Empty;
```
Gets or sets the name of the Windows edition.

### MountedAt
```csharp
[ObservableProperty]
private DateTime mountedAt = DateTime.Now;
```
Gets or sets the mount timestamp.

### Status
```csharp
[ObservableProperty]
private MountStatus status = MountStatus.Mounted;
```
Gets or sets the mount status.

### DisplayText
```csharp
public string DisplayText => $"{ImageName} - Index {Index} ({EditionName})";
```
Gets the display text for the mount.

### FormattedMountTime
```csharp
public string FormattedMountTime => MountedAt.ToString("yyyy-MM-dd HH:mm:ss");
```
Gets the formatted mount time.

## Usage Examples

### Creating a New Mounted Image Info
```csharp
var mountedImage = new MountedImageInfo
{
    Id = Guid.NewGuid().ToString(),
    ImagePath = @"C:\Images\install.wim",
    Index = 1,
    MountPath = @"C:\ProgramData\Bucket\Mounts\install_1",
    ImageName = "Windows 11 Pro",
    EditionName = "Windows 11 Pro",
    MountedAt = DateTime.Now,
    Status = MountStatus.Mounted
};
```

### Displaying Mount Information
```csharp
Console.WriteLine($"Mount: {mountedImage.DisplayText}");
Console.WriteLine($"Path: {mountedImage.MountPath}");
Console.WriteLine($"Mounted: {mountedImage.FormattedMountTime}");
```

## Features
- **MVVM Support**: Inherits from `ObservableObject` for data binding
- **Automatic Properties**: Uses `[ObservableProperty]` for simplified property implementation
- **Display Formatting**: Provides formatted display text and timestamps
- **Status Tracking**: Tracks mount status with `MountStatus` enum

## Dependencies
- `CommunityToolkit.Mvvm.ComponentModel` for MVVM support

## Related Files
- [`MountStatus.md`](./MountStatus.md) - Enumeration for mount status values
- [`IWindowsImageMountService.md`](../Services/WindowsImage/IWindowsImageMountService.md) - Service interface for mount operations
- [`WindowsImageMountService.md`](../Services/WindowsImage/WindowsImageMountService.md) - Service implementation

## Best Practices
- Always set a unique `Id` when creating new instances
- Update `Status` to reflect current mount state
- Use `DisplayText` for user-friendly display in UI
- Validate paths before setting `ImagePath` and `MountPath`

## Error Handling
- Properties are initialized with safe default values
- String properties default to empty strings to prevent null reference exceptions
- DateTime properties default to current time

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 