# GitHub Actions Workflows Specification for Bucket Application

## Project Overview

The **Bucket** repository hosts a Visual Studio solution containing a WinUI3 application built on .NET 9. The solution consists of 7 projects organized as follows:

### Projects Structure

- **Bucket.App** (Main WinUI3 application project)
- **Bucket.Core** (Core library)
- **Bucket.App.Tests** (Unit tests for the main application)
- **Bucket.Core.Tests** (Unit tests for the core library)
- **Bucket.Setup_x64** (Advanced Installer project for x64 architecture)
- **Bucket.Setup_x86** (Advanced Installer project for x86 architecture)
- **Bucket.Setup_arm64** (Advanced Installer project for ARM64 architecture)

### Application Characteristics

- **Technology Stack**: WinUI3 with .NET 9
- **Package Type**: Unpackaged application
- **Build Configuration**: Must be built in Release mode for publication
- **Installer Technology**: Advanced Installer for MSI/EXE generation

## Repository Branching Strategy

The repository follows a three-tier branching model:

1. **Top Level**: `main` branch (production-ready code)
2. **Mid Level**: `dev` branch (integration and pre-release testing)
3. **Low Level**: Feature, fix, and development branches

## Required GitHub Actions Workflows

Two main workflows are required with supporting reusable sub-workflows:

### 1. Nightly Build Workflow (`dotnet-nightly.yml`)

**Trigger Conditions:**

- **Schedule**: Automated execution every night at 02:00 UTC
- **Branch**: Only on `dev` branch
- **Condition**: Only execute if new code has been committed since the last build
- **Skip Logic**: Skip execution if no changes detected to avoid unnecessary builds

### 2. Release Workflow (`dotnet-release.yml`)

**Trigger Conditions:**

- **Execution**: Manual trigger only (workflow_dispatch)
- **Branch**: Only on `main` branch
- **Timing**: On-demand execution

## Workflow Requirements and Build Process

Both workflows must execute the following steps in order:

### 1. Unit Testing

- Execute all unit tests from `Bucket.App.Tests` and `Bucket.Core.Tests`
- Ensure all tests pass before proceeding
- Generate test reports and coverage if possible

### 2. Solution Building and Multi-Architecture Publishing

- Use `microsoft/setup-msbuild` action ([Reference](https://github.com/microsoft/setup-msbuild/blob/main/README.md))
- Build the entire solution in Release configuration for ALL target architectures:
  - **x64 architecture**: Build and publish solution targeting x64 platform
  - **x86 architecture**: Build and publish solution targeting x86 platform
  - **ARM64 architecture**: Build and publish solution targeting ARM64 platform
- Each architecture build must be published separately to generate architecture-specific deployable artifacts
- Ensure all projects compile successfully for each target architecture
- Create separate ZIP packages for each architecture for direct user download from GitHub releases

### 3. Setup Projects Generation with Architecture Matching

- **Critical Requirement**: Solution must be built for each architecture BEFORE generating corresponding setup projects
- **Architecture Mapping**: Each setup project must use the build artifacts from its corresponding architecture:
  - `Bucket.Setup_x64` → Uses x64 build artifacts and generates x64 MSI
  - `Bucket.Setup_x86` → Uses x86 build artifacts and generates x86 MSI
  - `Bucket.Setup_arm64` → Uses ARM64 build artifacts and generates ARM64 MSI
- **Advanced Installer Project Structure**:
  - **Source Files**: Only `.aip` (Advanced Installer Project) and `.aiproj` (MSBuild project) files are source controlled
  - **Generated Artifacts**: The following are automatically generated during build and should NOT be committed:
    - `{ProjectName}-cache/` directories (build cache)
    - `{ProjectName}-SetupFiles/` directories (containing final MSI files)
    - All contents within these directories are build artifacts
- **Reason**: Advanced Installer projects need to automatically synchronize files from the architecture-specific built solution
- Use `caphyon/advinst-github-action@main` action ([Reference](https://github.com/Caphyon/advinst-github-action/blob/main/README.md))
- Generate MSI installers maintaining strict architecture correspondence
- **Output Location**: Final MSI files will be generated in respective `{ProjectName}-SetupFiles/` directories

### 4. Artifacts and Release Management

- Generate comprehensive job summary
- Create GitHub release with all artifacts organized by architecture
- Include changelog and release notes
- Upload all generated files:
  - **ZIP packages**: Separate ZIP for each architecture (x64, x86, ARM64)
  - **MSI installers**: Architecture-specific MSI files from `{ProjectName}-SetupFiles/` directories:
    - `setup/Bucket.Setup_x64-SetupFiles/Bucket.Setup_x64.msi`
    - `setup/Bucket.Setup_x86-SetupFiles/Bucket.Setup_x86.msi`
    - `setup/Bucket.Setup_arm64-SetupFiles/Bucket.Setup_arm64.msi`
- **MSI Renaming Convention**: Rename MSI files before upload to include app name, version, and architecture:
  - **Format**: `Bucket-{VERSION}-{ARCHITECTURE}.msi`
  - **Examples**:
    - `Bucket-25.9.20.1-x64.msi`
    - `Bucket-25.9.20.1-x86.msi`
    - `Bucket-25.9.20.1-arm64.msi`
    - `Bucket-25.9.20.1-Nightly-x64.msi` (for nightly builds)
- Ensure proper naming convention that clearly identifies the target architecture
- **Important**: Only upload the final MSI files, not the cache or intermediate build files

## Versioning Strategy

The application uses a date-based versioning scheme:

### Nightly Builds

- **Format**: `YY.MM.DD.BUILD-Nightly`
- **Example**: `25.9.20.1-Nightly`
- **GitHub Release Type**: Pre-release

### Production Releases

- **Format**: `YY.MM.DD.BUILD`
- **Example**: `25.9.20.1`
- **GitHub Release Type**: Latest release

### Version Application Requirements

- Version must be embedded in all artifacts (ZIP packages and MSI files for all architectures)
- GitHub release tag must match the version exactly
- Version should be automatically incremented for builds on the same day
- All architecture-specific artifacts must share the same version number

## Architecture and Reusability Requirements

### Reusable Sub-Workflows

- Create modular, reusable sub-workflows to minimize code duplication
- Enable easy maintenance and updates
- Common sub-workflows should handle:
  - Environment setup and dependencies
  - Testing execution
  - Multi-architecture building and publishing
  - Architecture-specific setup project generation
  - Artifact creation and upload with proper architecture labeling
  - MSI renaming with standardized naming convention

### Workflow Structure

- Main workflows (`dotnet-nightly.yml` and `dotnet-release.yml`) should orchestrate the process
- Sub-workflows should handle specific tasks
- Proper error handling and rollback mechanisms
- Clear logging and progress reporting

## Technical Specifications

### Required GitHub Actions

1. `microsoft/setup-msbuild` for solution building
2. `caphyon/advinst-github-action@main` for setup project generation
3. Standard actions for checkout, .NET setup, artifact upload, and release creation

### Environment Requirements

- Windows-based runners (required for Advanced Installer)
- .NET 9 SDK
- MSBuild tools
- Advanced Installer command-line tools
- **Git Configuration**: Ensure proper `.gitignore` to exclude generated Advanced Installer artifacts:
  - `setup/*-cache/` directories
  - `setup/*-SetupFiles/` directories
  - Only `.aip` and `.aiproj` files should be version controlled in setup projects

### Security and Access

- Proper secrets management for any required tokens
- Secure handling of build artifacts
- Appropriate permissions for GitHub releases

## Success Criteria

1. **Automated Nightly Builds**: Successful daily builds on `dev` branch with change detection
2. **Manual Release Process**: Reliable on-demand releases from `main` branch
3. **Complete Multi-Architecture Support**: All platforms (x64, x86, ARM64) with corresponding MSI and ZIP packages
4. **Architecture Consistency**: Each setup project uses the correct architecture-specific build artifacts
5. **Proper Versioning**: Consistent version application across all architecture-specific artifacts
6. **Code Reusability**: Maintainable sub-workflows with minimal duplication
7. **Robust Error Handling**: Clear failure reporting and recovery mechanisms

## Additional Considerations

- Build caching for improved performance
- Parallel execution where possible (while respecting dependencies)
- Comprehensive logging and monitoring
- Documentation for workflow maintenance and troubleshooting
