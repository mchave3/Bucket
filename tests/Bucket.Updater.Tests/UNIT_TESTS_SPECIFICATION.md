# Unit Tests Specification for Bucket.Updater

## Overview

This document specifies the complete list of unit tests to be implemented for the **Bucket.Updater** project. The focus is exclusively on **business logic testing**, excluding any WinUI or WinRT related components.

**Target Project**: `Bucket.Updater.Tests`
**Estimated Total**: ~65 unit tests

## Test Framework Requirements

- **Framework**: xUnit
- **Assertions**: FluentAssertions
- **Mocking**: Moq
- **Coverage Target**: 95%+ on business logic

## 1. UpdaterConfiguration Tests (7 tests)

### Test Class: `UpdaterConfigurationTests`

#### InitializeRuntimeProperties() Tests
- ✅ `InitializeRuntimeProperties_ShouldDetectX86Architecture_WhenProcessArchitectureIsX86()`
- ✅ `InitializeRuntimeProperties_ShouldDetectX64Architecture_WhenProcessArchitectureIsX64()`
- ✅ `InitializeRuntimeProperties_ShouldDetectARM64Architecture_WhenProcessArchitectureIsArm64()`
- ✅ `InitializeRuntimeProperties_ShouldDefaultToX64_WhenArchitectureIsUnknown()`

#### GetArchitectureString() Tests
- ✅ `GetArchitectureString_ShouldReturnX86_WhenArchitectureIsX86()`
- ✅ `GetArchitectureString_ShouldReturnX64_WhenArchitectureIsX64()`
- ✅ `GetArchitectureString_ShouldReturnArm64_WhenArchitectureIsARM64()`

## 2. Models Validation Tests (3 tests)

### Test Class: `UpdateInfoTests`
- ✅ `Constructor_ShouldInitializeAllPropertiesWithDefaults()`
- ✅ `Properties_ShouldAllowAssignment_WhenValidValuesProvided()`

### Test Class: `UpdateAssetTests`
- ✅ `Constructor_ShouldInitializeAllPropertiesWithDefaults()`

## 3. GitHubService Tests (20 tests)

### Test Class: `GitHubServiceTests`

#### Version Comparison Logic (via CheckForUpdatesAsync)
- ✅ `CheckForUpdatesAsync_ShouldReturnUpdate_WhenNewVersionIsGreater()`
  - Test data: "1.2.0" > "1.1.0"
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenNewVersionIsLower()`
  - Test data: "1.1.0" < "1.2.0"
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenVersionsAreEqual()`
  - Test data: "1.1.0" == "1.1.0"
- ✅ `CheckForUpdatesAsync_ShouldHandleVersionsWithVPrefix()`
  - Test data: "v1.2.0" > "v1.1.0"
- ✅ `CheckForUpdatesAsync_ShouldIgnorePrereleasesSuffixes()`
  - Test data: "1.2.0-beta" > "1.1.0"
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenVersionParsingFails()`
  - Test data: malformed versions
- ✅ `CheckForUpdatesAsync_ShouldHandleDifferentVersionFormats()`
  - Test data: "1.2.3" vs "1.2.3.0"

#### Release Filtering Logic
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenNoReleaseFoundForReleaseChannel()`
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenNoReleaseFoundForNightlyChannel()`
- ✅ `CheckForUpdatesAsync_ShouldFilterReleasesForReleaseChannel_WhenNonPrereleaseOnly()`
- ✅ `CheckForUpdatesAsync_ShouldFilterReleasesForNightlyChannel_WhenPrereleaseWithNightly()`

#### Asset Selection Logic
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenNoMsiAssetFoundForArchitecture()`
- ✅ `CheckForUpdatesAsync_ShouldSelectCorrectMsiAsset_WhenX86Architecture()`
- ✅ `CheckForUpdatesAsync_ShouldSelectCorrectMsiAsset_WhenX64Architecture()`
- ✅ `CheckForUpdatesAsync_ShouldSelectCorrectMsiAsset_WhenARM64Architecture()`

#### Data Mapping and Error Handling
- ✅ `CheckForUpdatesAsync_ShouldReturnCorrectUpdateInfo_WhenUpdateAvailable()`
- ✅ `CheckForUpdatesAsync_ShouldMapGitHubDataCorrectly_WhenMappingToUpdateInfo()`
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenExceptionOccurs()`

#### GetReleasesAsync Tests
- ✅ `GetReleasesAsync_ShouldReturnEmptyList_WhenApiCallFails()`
- ✅ `GetReleasesAsync_ShouldFilterPrereleases_WhenIncludePrereleaseIsFalse()`
- ✅ `GetReleasesAsync_ShouldIncludeAllReleases_WhenIncludePrereleaseIsTrue()`

## 4. UpdateService Tests (15 tests)

### Test Class: `UpdateServiceTests`

#### CheckForUpdatesAsync Tests
- ✅ `CheckForUpdatesAsync_ShouldLoadConfiguration_WhenCalled()`
- ✅ `CheckForUpdatesAsync_ShouldCallGitHubServiceWithCorrectConfiguration()`
- ✅ `CheckForUpdatesAsync_ShouldReturnUpdateInfo_WhenUpdateAvailable()`
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenNoUpdateAvailable()`
- ✅ `CheckForUpdatesAsync_ShouldReturnNull_WhenExceptionOccurs()`
- ✅ `CheckForUpdatesAsync_ShouldLogOperationsCorrectly()`

#### DownloadUpdateAsync Tests
- ✅ `DownloadUpdateAsync_ShouldCreateCorrectTemporaryPath()`
- ✅ `DownloadUpdateAsync_ShouldCallGitHubServiceWithCorrectParameters()`
- ✅ `DownloadUpdateAsync_ShouldSaveStreamToTemporaryFile()`
- ✅ `DownloadUpdateAsync_ShouldReportProgressCorrectly()`
- ✅ `DownloadUpdateAsync_ShouldHandleCancellation_WhenCancellationTokenTriggered()`
- ✅ `DownloadUpdateAsync_ShouldCleanupOnError_WhenExceptionOccurs()`
- ✅ `DownloadUpdateAsync_ShouldReturnDownloadedFilePath_WhenSuccessful()`

#### Other Methods Tests
- ✅ `InstallUpdateAsync_ShouldCallInstallationServiceWithCorrectParameters()`
- ✅ `GetConfiguration_ShouldReturnConfigurationFromService()`

## 5. ConfigurationService Tests (8 tests)

### Test Class: `ConfigurationServiceTests`

#### GetConfiguration Tests
- ✅ `GetConfiguration_ShouldReturnCachedConfiguration_WhenAlreadyLoaded()`
- ✅ `GetConfiguration_ShouldLoadConfigurationSynchronously_WhenNotCached()`
- ✅ `GetConfiguration_ShouldCacheConfiguration_AfterLoading()`

#### LoadConfigurationAsync Tests
- ✅ `LoadConfigurationAsync_ShouldReturnCachedConfiguration_WhenAlreadyLoaded()`
- ✅ `LoadConfigurationAsync_ShouldLoadFromAppConfigReader_WhenNotCached()`
- ✅ `LoadConfigurationAsync_ShouldCallInitializeRuntimeProperties_WhenConfigurationLoaded()`
- ✅ `LoadConfigurationAsync_ShouldUseDefaultConfiguration_WhenAppConfigReaderReturnsNull()`
- ✅ `LoadConfigurationAsync_ShouldReturnDefaultConfiguration_WhenExceptionOccurs()`

## 6. AppConfigReader Tests (12 tests)

### Test Class: `AppConfigReaderTests`

#### File Existence and Basic Validation
- ✅ `ReadConfiguration_ShouldReturnNull_WhenConfigFileDoesNotExist()`
- ✅ `ReadConfigurationAsync_ShouldReturnNull_WhenConfigFileDoesNotExist()`
- ✅ `ReadConfiguration_ShouldReturnNull_WhenConfigFileIsEmpty()`
- ✅ `ReadConfiguration_ShouldReturnNull_WhenJsonIsMalformed()`

#### JSON Parsing Tests
- ✅ `ReadConfiguration_ShouldReturnCorrectConfiguration_WhenJsonIsValid()`
- ✅ `ReadConfigurationAsync_ShouldReturnCorrectConfiguration_WhenJsonIsValid()`
- ✅ `ReadConfiguration_ShouldParseAllFields_WhenAllFieldsPresent()`
- ✅ `ReadConfiguration_ShouldUseDefaults_WhenFieldsAreMissing()`
- ✅ `ReadConfiguration_ShouldParseUpdateChannelEnum_WhenValidValues()`
- ✅ `ReadConfiguration_ShouldParseSystemArchitectureEnum_WhenValidValues()`
- ✅ `ReadConfiguration_ShouldParseDateTimeFields_WhenValidFormat()`
- ✅ `ReadConfiguration_ShouldReturnNull_WhenParsingExceptionOccurs()`

## 7. InstallationService Tests (8 tests)

### Test Class: `InstallationServiceTests`

#### File Cleanup Logic
- ✅ `CleanupDownloadedFiles_ShouldDeleteFile_WhenFileExists()`
- ✅ `CleanupDownloadedFiles_ShouldNotThrow_WhenFileDoesNotExist()`
- ✅ `CleanupDownloadedFiles_ShouldLogOperation_WhenCalled()`
- ✅ `CleanupDownloadedFiles_ShouldHandleExceptions_WhenFileIsLocked()`

#### Temporary Files Cleanup
- ✅ `CleanupAllTemporaryFiles_ShouldIdentifyAndDeleteBucketTempFiles()`
- ✅ `CleanupAllTemporaryFiles_ShouldCleanSystemTempDirectory()`
- ✅ `CleanupAllTemporaryFiles_ShouldHandleMissingDirectories()`
- ✅ `CleanupAllTemporaryFiles_ShouldLogNumberOfDeletedFiles()`

## Implementation Guidelines

### 1. Test Structure
Each test class should follow this structure:
```csharp
public class [ClassName]Tests
{
    private readonly [ClassName] _sut; // System Under Test
    private readonly Mock<IDependency> _mockDependency;

    public [ClassName]Tests()
    {
        // Setup mocks and SUT
    }

    [Fact]
    public void Method_ShouldExpectedBehavior_WhenCondition()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

### 2. Mock Configuration
- **HttpClient**: Use `MockHttpMessageHandler` for HTTP calls
- **File System**: Mock file operations using `IFileSystem` abstraction or test filesystem
- **Dependencies**: Mock all external dependencies (IConfigurationService, IGitHubService, etc.)

### 3. Test Data
Create test fixtures for:
- Sample GitHub API responses (JSON)
- Sample AppConfig.json files with various configurations
- Version comparison test cases
- Architecture detection scenarios

### 4. Async Testing
Use proper async/await patterns:
```csharp
[Fact]
public async Task MethodAsync_ShouldBehavior_WhenCondition()
{
    // Arrange
    // Act
    var result = await _sut.MethodAsync();
    // Assert
}
```

### 5. Exception Testing
```csharp
[Fact]
public void Method_ShouldThrow_WhenInvalidInput()
{
    // Arrange
    // Act & Assert
    Assert.Throws<SpecificException>(() => _sut.Method(invalidInput));
}
```

### 6. Coverage Exclusions
Exclude from coverage:
- WinUI/WinRT related code
- ViewModels (UI binding logic)
- Views and Pages
- Platform-specific MSI installation logic

## Priority Implementation Order

1. **UpdaterConfiguration** (foundational, simple)
2. **GitHubService** (core business logic)
3. **ConfigurationService** (essential for other services)
4. **AppConfigReader** (configuration parsing)
5. **UpdateService** (orchestration layer)
6. **InstallationService** (file management logic)
7. **Models** (simple validation tests)

## Test Data Examples

### Sample AppConfig.json
```json
{
  "UpdateChannel": "Release",
  "Architecture": "X64",
  "GitHubOwner": "mchave3",
  "GitHubRepository": "Bucket",
  "CurrentVersion": "1.0.0.0",
  "LastUpdateCheck": "2025-09-12T10:00:00Z"
}
```

### Sample GitHub Release Response
```json
{
  "tag_name": "v1.2.0",
  "name": "Release 1.2.0",
  "body": "Release notes...",
  "published_at": "2025-09-12T10:00:00Z",
  "prerelease": false,
  "assets": [
    {
      "name": "Bucket--x64.msi",
      "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket--x64.msi",
      "size": 50000000,
      "content_type": "application/octet-stream"
    }
  ]
}
```

This specification provides a comprehensive roadmap for implementing robust unit tests focusing exclusively on business logic validation and error handling scenarios.