# Custom instructions for Copilot

## Project context
This document is a style and structure guide for you, GitHub Copilot, to follow when generating or refactoring PowerShell code in the Bucket project. You must be able to parse, understand, and apply these rules to all code suggestions and refactorings.

### General Principles
- Write clear, maintainable, and consistent code.
- Use English for all code, comments, and log messages.
- Favor readability and explicitness over cleverness.
- Do not use global-scope variables (avoid $global:). Prefer script-scope ($script:) or local variables.

### Logging
- All log messages must be harmonized: start with a context tag in square brackets (e.g. [Navigation], [ISO Import], [Pre-Flight], [Bucket]).
- Use Write-BucketLog for all logging, and choose the appropriate log level (Info, Debug, Warning, Error, Verbose).
- Log key actions, errors, and important state changes.

### XAML & UI
- When loading XAML, always clean up design-time namespaces and x:Class attributes for compatibility.
- Always check for file existence before loading XAML.
- Use FindName to bind named XAML elements to PowerShell variables.

### Error Handling
- Use try/catch for all file, IO, or UI operations that may fail.
- Log all errors with Write-BucketLog at Error level.
- Use exit 1 for unrecoverable errors in entry-point scripts.

## Indentation
### WPF Controls Indentation (PowerShell)
- When working with WPF controls and event handlers in PowerShell, always indent the script block passed to Add_Click (or similar event methods) one level further to the right than the surrounding if statement.
- This ensures clear visual separation between the control structure and the event handler logic.

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
### Structure
- Use a clear and consistent order for function blocks: param, begin (if needed), process, end (if needed).
- Separate logical blocks with blank lines for readability.
- Use parameter validation attributes (e.g. [Parameter(Mandatory = $true)]) for all function parameters.
- Use [CmdletBinding()] for advanced functions.
- **For all control structures (e.g., if/else, elseif, try/catch/finally, switch), always place the opening brace on the same line as the statement. The else, elseif, catch, and finally keywords must always appear on a new line, directly after the closing brace of the previous block.**

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

### Comments
- Use concise comments to explain the purpose of each logical block, function, or non-obvious code section.
- Use region blocks (#region ... #endregion) to group major logical sections, but do not overuse them. Only use regions for high-level structure (e.g. Initialization, Validation, Main Logic, Error Handling).
- Do not place a region block immediately above the function definition. Regions should only be used inside the function body to group major logical sections.
- Always add a comment before complex or critical logic.
- If comments are not consistent with the code, you are allowed to modify or rewrite them to ensure they accurately describe the logic.

### Function Header Consistency
- Ensure that in the header of each PowerShell function, the SYNOPSIS, DESCRIPTION, and EXAMPLE fields are accurate and consistent with the actual code and usage.
- Do not modify the NOTES or LINK fields.
- Do not remove the NOTES or LINK fields from the function header under any circumstances.
- Add line breaks in these fields as needed to improve readability and clarity of the header.

## Reference
- Reference implementations for testing and best practices can be found in: Initialize-BucketISO_DataSourcePage.ps1, Import-BucketISO.ps1, Start-Bucket.ps1, Invoke-BucketPreFlight.ps1, and Get-BucketVersion.ps1.