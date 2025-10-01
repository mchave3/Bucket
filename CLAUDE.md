# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Bucket** is a modern Windows desktop application built with WinUI 3 and .NET 9, following a clean architecture pattern with MVVM.

## Development Commands

### Building
```powershell
# Restore dependencies
dotnet restore

# Build in Release configuration
dotnet build --configuration Release

# Build for specific platform
dotnet build --configuration Release --runtime win-x64
```

### Code Analysis
- .NET analyzers are enabled globally with `latest` analysis level
- Code style enforcement is enabled in builds via `EnforceCodeStyleInBuild`
- Nullable reference types are enforced project-wide

## Architecture

### Project Structure
- **`src/Bucket.App/`** - Main WinUI 3 application (net9.0-windows10.0.26100)
  - Uses MVVM pattern with CommunityToolkit.Mvvm 8.4.0
  - WinUI 3 with DevWinUI 9.1.0 component library
  - Dependency injection via Microsoft.Extensions.DependencyInjection 9.0.9
  - Multi-language support with WinUI ResourceLoader (.resw files: en-US, fr-FR)
  - T4 templates for code generation (NavigationPageMappings, BreadcrumbPageMappings)
  - Settings management via nucs.JsonSettings 2.0.2 with AutoSaveGenerator
  - Services: UpdateService, SafeShutdownService
  - PRI file generation enabled for localization
- **`src/Bucket.Core/`** - Core business logic library (net9.0-windows10.0.26100)
  - Shared WinUI components and helpers
  - Uses DevWinUI 9.1.0 for cross-project UI components
  - PRI file generation enabled for WinUI APIs
- **`src/Bucket.Updater/`** - Application updater component (net9.0-windows10.0.26100)
  - Handles application updates and versioning
  - Single-file publish enabled for standalone deployment
  - Self-contained deployment with trimming support
  - PRI generation disabled (no localized resources)
- **`setup/Bucket.Setup/`** - WiX Toolset 6.0.2 installer project
  - MSI package generation for x86, x64, and ARM64
  - Platform-specific GUIDs for side-by-side installation

### Key Technologies
- **UI Framework**: WinUI 3 (Microsoft.WindowsAppSDK 1.8.250907003)
- **UI Components**: DevWinUI 9.1.0, DevWinUI.Controls 9.1.0, DevWinUI.ContextMenu 9.0.0
- **Architecture**: MVVM with CommunityToolkit.Mvvm 8.4.0
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection 9.0.9
- **Logging**: Serilog 4.3.0 with Sinks.Console 6.0.0, Sinks.Debug 3.0.0, Sinks.File 7.0.0
- **Settings**: nucs.JsonSettings 2.0.2 with AutoSaveGenerator 2.0.4 (auto-save on property change)
- **Localization**: WinUI ResourceLoader with .resw files (en-US, fr-FR)
- **HTML Parsing**: HtmlAgilityPack 1.12.3
- **Code Generation**: T4 templates for navigation and breadcrumb mappings
- **Build Tools**: Microsoft.Windows.SDK.BuildTools 10.0.26100.4948, Microsoft.NET.ILLink.Tasks 9.0.9

### Build Configuration
- **Platforms**: x86, x64, ARM64 (RuntimeIdentifiers: win-x86, win-x64, win-arm64)
- **Target Framework**: net9.0-windows10.0.26100 (MinVersion: 10.0.17763.0)
- **Windows App SDK**: Self-contained deployment (WindowsAppSDKSelfContained: true)
- **Trimming**:
  - Bucket.App: Partial trimming, disabled in CI/CD (PublishTrimmed)
  - Bucket.Updater: Full trimming in Release, single-file publish
- **Ready-to-Run**: Enabled in local Release builds, disabled in CI/CD
- **Code Analysis**:
  - EnableNETAnalyzers: true, AnalysisLevel: latest, AnalysisMode: AllEnabledByDefault
  - EnforceCodeStyleInBuild: true
  - Nullable: enabled globally (disabled in Bucket.App/Updater)
  - TreatWarningsAsErrors: false
- **Documentation**: GenerateDocumentationFile: true (suppresses CS1591)
- **Localization**:
  - PRI file generation enabled (GeneratePriFile: true)
  - Excluded languages managed via RemoveLanguageFolders.targets
- **T4 Templates**: Auto-transformation before build (NavigationPageMappings, BreadcrumbPageMappings)
- **MSI Packaging**: EnableMsixTooling: true, WiX Toolset SDK 6.0.2

### CI/CD Architecture
- **Release Workflow**: `bucket-release.yml` - Manual releases from main branch with dry-run support
- **Nightly Builds**: `bucket-nightly.yml` - Automated builds from dev branch at 2:00 AM UTC
  - Only builds when PRs merged since last nightly
  - Automatically cleans up old nightly releases (keeps last 7)
  - Creates GitHub issues on build failures
- **Subworkflows**:
  - `subflow_build-msi.yml` - Multi-architecture MSI generation (x64, x86, ARM64)
  - `subflow_release-management.yml` - GitHub release creation and artifact upload
- **CI/CD Fixes**: Special handling for GitHub Actions (EnableWindowsTargeting, disabled trimming/R2R)

### Version Management
- **Release format**: YY.M.D (e.g., 25.1.15) → MSI version: YY.M.D.1
- **Hotfix format**: YY.M.D.BUILD (e.g., 25.1.15.2) → MSI version: YY.M.D.BUILD (exact match)
- **Nightly format**: YY.M.D-Nightly → MSI version: YY.M.D.1
- **Nightly hotfix**: YY.M.D.BUILD-Nightly → MSI version: YY.M.D.BUILD
- **Git tags**: v{version} (e.g., v25.1.15, v25.1.15.2, v25.1.15-Nightly)
- **Version detection**: Automatic channel detection (Release vs Nightly) in AppConfig
- **Architecture detection**: Automatic detection (x86, x64, arm64) at runtime