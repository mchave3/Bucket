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

### Testing
```powershell
# Run all tests
dotnet test

# Run tests with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### Code Analysis
- .NET analyzers are enabled globally with `latest` analysis level
- Code style enforcement is enabled in builds via `EnforceCodeStyleInBuild`
- Nullable reference types are enforced project-wide

## Architecture

### Project Structure
- **`src/Bucket.App/`** - Main WinUI 3 application (net9.0-windows10.0.26100)
  - Uses MVVM pattern with CommunityToolkit.Mvvm
  - WinUI 3 with DevWinUI component library
  - Dependency injection via Microsoft.Extensions.DependencyInjection
  - Multi-language support with WinUI3Localizer (English/French)
  - T4 templates for code generation
- **`src/Bucket.Core/`** - Core business logic library (net9.0)
  - Platform-agnostic business logic
  - Models, Services, Helpers

### Key Technologies
- **UI Framework**: WinUI 3 with DevWinUI components
- **Architecture**: MVVM with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging**: Serilog (Console, Debug, File sinks)
- **Settings**: nucs.JsonSettings with auto-save
- **Localization**: WinUI3Localizer (en-US, fr-FR)

### Build Configuration
- Multi-platform targeting: x86, x64, ARM64
- Self-contained Windows App SDK deployment
- Trimming enabled for release builds (disabled in CI/CD)
- T4 template transformations for code generation
- MSI packaging via EnableMsixTooling

### CI/CD Architecture
- **Release Workflow**: `dotnet-release.yml` - Manual releases from main branch
- **Nightly Builds**: `dotnet-nightly.yml` - Automated builds from dev branch
- **Testing**: Modular test workflow via `action_run-tests.yml`
- **Packaging**: Multi-architecture MSI generation via `action_build-package-installers.yml`

### Version Management
- Release format: YY.M.D (e.g., 25.1.15) → MSI gets .1 suffix automatically
- Hotfix format: YY.M.D.BUILD (e.g., 25.1.15.2) → MSI uses exact version
- Git tags use format: v{version}