# Custom instructions for Copilot

## Project context
This document is a style and structure guide for you, GitHub Copilot, to follow when generating or refactoring PowerShell code in the Bucket project. You must be able to parse, understand, and apply these rules to all code suggestions and refactorings.

### General Principles
- Write clear, maintainable, and consistent code.
- Use English for all code, comments, and log messages.
- Favor readability and explicitness over cleverness.
- Do not use global-scope variables (avoid $global:). Prefer script-scope ($script:) or local variables.
- Target PowerShell Core 7.4+ exclusively. Do not maintain compatibility with Windows PowerShell 5.1.

### PowerShell Core 7.4+ Specific Guidelines
- Leverage modern PowerShell Core features (parallel processing, improved cmdlets, cross-platform capabilities).
- Use `foreach -Parallel` for CPU-intensive operations when appropriate.
- Take advantage of improved error handling and debugging features in PowerShell Core.
- Use modern .NET APIs available in PowerShell Core.
- Avoid legacy Windows PowerShell ISE-specific constructs.
- Use UTF-8 with BOM encoding by default for file operations.
- Target Windows platform exclusively - do not maintain compatibility with Linux or macOS.

### Logging
- All log messages must be harmonized.
- Use Write-BucketLog for all logging, and choose the appropriate log level (Verbose, Debug, Info, Warning, Error, Fatal).
- Log key actions, errors, and important state changes.
- Follow these guidelines for log levels:
  - **Verbose**: Implementation details and fine-grained execution information (e.g., "Creating temporary variables")
  - **Debug**: Important values and state information (e.g., "Working path: X")
  - **Info**: Main steps and normal application flow (e.g., "Initialization complete", user actions, navigation events)
  - **Warning**: Suboptimal situations that don't stop execution (e.g., "Folder already exists, using existing folder")
  - **Error**: Operation failures and exceptions that impact functionality (e.g., "Unable to create folder")
  - **Fatal**: Critical conditions that prevent the application from functioning (e.g., "Incompatible configuration detected")
- Include relevant variable values in log messages to provide context.
- For UI operations, log both the user action and the outcome.

### XAML & UI
- When loading XAML, always clean up design-time namespaces and x:Class attributes for compatibility.
- Always check for file existence before loading XAML.
- Use FindName to bind named XAML elements to PowerShell variables.
- Organize UI structure following the Page architecture model:
  1. XAML file for the UI definition (`PageNamePage.xaml`)
  2. Initialization script for data and events (`Initialize-BucketPageNamePage.ps1`)
  3. Invocation script for page navigation (`Invoke-BucketPageNamePage.ps1`)
- Follow the recommended UI control naming pattern: `PageName_ControlTypeDescriptor` (e.g., `UserSettings_SaveButton`).
- Use consistent style attributes across UI controls for a unified look.
- When working with data binding, ensure properties are properly defined in the DataContext.

### Error Handling
- Use try/catch for all file, IO, or UI operations that may fail.
- Log all errors with Write-BucketLog at Error level.
- Use exit 1 for unrecoverable errors in entry-point scripts.
- Include specific error messages that describe what failed and why.
- For UI operations, inform the user of errors through appropriate UI mechanisms.
- Handle exceptions at the appropriate level - don't catch exceptions too early if they should be handled by a higher-level function.
- When appropriate, clean up resources in finally blocks to ensure they're properly released regardless of errors.

## Coding style

#### Example (correct):
```powershell
if ($issueButton) {
    $issueButton.Add_Click({
            param($senderObj, $e)
            Write-BucketLog -Data "[About] Report Issue button clicked" -Level Info
            Start-Process "https://github.com/mchave3/Bucket/issues/new"
        })
}
```

#### Example (incorrect):
```powershell
if ($licenseButton) {
    $licenseButton.Add_Click({
        param($senderObj, $e)
        Write-BucketLog -Data "[About] License button clicked" -Level Info
        Start-Process "https://github.com/mchave3/Bucket/blob/main/LICENSE"
    })
}
```

## Coding style

### Indentation
#### WPF Controls Indentation (PowerShell)
- When working with WPF controls and event handlers in PowerShell, always indent the script block passed to Add_Click (or similar event methods) one level further to the right than the surrounding if statement.
- This ensures clear visual separation between the control structure and the event handler logic.

##### Example (correct):
```powershell
if ($issueButton) {
    $issueButton.Add_Click({
            param($senderObj, $e)
            Write-BucketLog -Data "[About] Report Issue button clicked" -Level Info
            Start-Process "https://github.com/mchave3/Bucket/issues/new"
        })
}
```

##### Example (incorrect):
```powershell
if ($licenseButton) {
    $licenseButton.Add_Click({
        param($senderObj, $e)
        Write-BucketLog -Data "[About] License button clicked" -Level Info
        Start-Process "https://github.com/mchave3/Bucket/blob/main/LICENSE"
    })
}
```

### Structure
- Use a clear and consistent order for function blocks: param, begin (if needed), process, end (if needed).
- Separate logical blocks with blank lines for readability.
- Use parameter validation attributes (e.g. [Parameter(Mandatory = $true)]) for all function parameters.
- Use [CmdletBinding()] for advanced functions.
- **For all control structures (e.g., if/else, elseif, try/catch/finally, switch), always place the opening brace on the same line as the statement. The else, elseif, catch, and finally keywords must always appear on a new line, directly after the closing brace of the previous block.**
- Organize code in logical sections using region blocks that match the application's architecture.
- Follow a consistent pattern for function organization:
  1. Function header (with SYNOPSIS, DESCRIPTION, etc.)
  2. Parameter block
  3. Begin block (for initialization, if needed)
  4. Process block (main logic)
  5. End block (cleanup and output handling, if needed)

#### Example:
```powershell
if ($condition) {
    # Do something
}
elseif ($otherCondition) {
    # Do something else
}
else {
    # Default case
}

try {
    # Try block
}
catch {
    # Catch block
}
finally {
    # Finally block
}
```

### Naming
- Use PascalCase for function names (e.g. Invoke-BucketNavigationService).
- Use camelCase for local variables, PascalCase for script/global variables.
- Prefix script-global variables with $script: as appropriate.
- Use descriptive names for all variables and parameters.
- For XAML-related functions, use consistent naming patterns:
  - Initialize-BucketPageNamePage.ps1 for page initialization
  - Invoke-BucketPageNamePage.ps1 for page navigation
- For UI control variables in WPF pages, use the pattern: `$pageName_ControlTypeDescriptor` (e.g., `$aboutPage_ReportIssueButton`).
- For script variables in UI pages, use the prefix `$script:pageNameLower_` to avoid conflicts between pages.

### Comments
- Use concise comments to explain the purpose of each logical block, function, or non-obvious code section.
- Use region blocks (#region ... #endregion) to group major logical sections, but do not overuse them. Only use regions for high-level structure (e.g. Initialization, Validation, Main Logic, Error Handling).
- Do not place a region block immediately above the function definition. Regions should only be used inside the function body to group major logical sections.
- Always add a comment before complex or critical logic.
- If comments are not consistent with the code, you are allowed to modify or rewrite them to ensure they accurately describe the logic.
- For UI event handlers, include a comment describing what the event does and any side effects.
- Use consistent region naming patterns, particularly for UI pages:
  ```powershell
  #region Data Initialization
  # Initialize data and variables
  #endregion Data Initialization

  #region Event Handlers
  # Define UI event handlers
  #endregion Event Handlers

  #region Page Navigation
  # Handle navigation and context
  #endregion Page Navigation
  ```
- Comment any code that works around specific PowerShell version limitations or compatibility issues.

### Function Header Consistency
- Ensure that in the header of each PowerShell function, the SYNOPSIS, DESCRIPTION, and EXAMPLE fields are accurate and consistent with the actual code and usage.
- Do not modify the NOTES or LINK fields.
- Do not remove the NOTES or LINK fields from the function header under any circumstances.
- Add line breaks in these fields as needed to improve readability and clarity of the header.
- Include a proper function header structure for all functions:
```powershell
<#
    .SYNOPSIS
    Brief description of what the function does.

    .DESCRIPTION
    Detailed description of the function's purpose, functionality, and usage patterns.
    Include any important information about how the function works.

    .NOTES
    Name: Function-Name.ps1
    Author: Mickaël CHAVE
    Created: MM/DD/YYYY
    Version: 1.0.0
    Repository: https://github.com/mchave3/Bucket
    License: MIT License

    .LINK
    https://github.com/mchave3/Bucket

    .PARAMETER ParameterName
    Description of the parameter and its purpose.

    .EXAMPLE
    Invoke-BucketFunction -Parameter Value
    Description of what this example does and expected result.
#>
```
- Ensure all parameters are documented in the function header.
- Include practical examples that demonstrate common usage patterns.

### Module Structure and File Organization
- Follow the standard PowerShell module structure with clear separation between Public and Private functions:
  - `Public/` - Contains all functions that are exported by the module
  - `Private/` - Contains all internal helper functions used by the module
  - `Classes/` - Contains all PowerShell class definitions
  - `GUI/` - Contains all XAML files for UI components
  - `Variables/` - Contains scripts that define module variables
  - `en-US/` - Contains localization files and help documentation
- Each function should be in its own file with the same name as the function (e.g., `Get-BucketVersion.ps1` contains the `Get-BucketVersion` function).
- Organize related functions into subdirectories that reflect their purpose (e.g., `Private/Core/`, `Private/GUI/Navigation/`).
- When adding a new feature or component, create a dedicated directory structure following the existing patterns:
  ```
  Private/
    FeatureName/
      Invoke-BucketFeatureFunction.ps1
      Get-BucketFeatureData.ps1
  GUI/
    FeatureName/
      FeatureNamePage.xaml
  ```
- Always include core function components in the following order within each file:
  1. File header comment with filepath
  2. Function header with documentation
  3. Function definition with parameter block
  4. Function implementation with appropriate regions
- When importing modules or adding dependencies:
  - Document all dependencies in the module manifest (.psd1)
  - Use Import-Module with the -Force parameter only when necessary
  - Prefer RequiredModules in the module manifest over explicit imports within scripts
- Maintain a consistent folder depth and naming convention across the project

## Reference
- Reference implementations for testing and best practices can be found in: Initialize-BucketISO_DataSourcePage.ps1, Import-BucketISO.ps1, Start-Bucket.ps1, Invoke-BucketPreFlight.ps1, and Get-BucketVersion.ps1.