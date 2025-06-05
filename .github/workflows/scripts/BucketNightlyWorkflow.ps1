<#
    .SYNOPSIS
    Bucket Nightly Build & Statistics Workflow Script

    .DESCRIPTION
    This script handles the complete nightly build process for the Bucket PowerShell module.
    It performs bootstrapping, building, testing, and generates a comprehensive job summary.

    .NOTES
    Name: BucketNightlyWorkflow.ps1
    Author: Mickaël CHAVE
    Created: 06/05/2025
    Version: 25.5.6.27
    Repository: https://github.com/mchave3/Bucket
    License: MIT License

    .LINK
    https://github.com/mchave3/Bucket

    .EXAMPLE
    .\BucketNightlyWorkflow.ps1
    Runs the complete nightly build and test workflow with job summary generation.
#>

[CmdletBinding()]
param()

#region Initialization
Write-Host "🚀 Starting Bucket Nightly Build Workflow" -ForegroundColor Green
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Cyan
Write-Host "OS: $($PSVersionTable.OS)" -ForegroundColor Cyan
Write-Host "Platform: $($PSVersionTable.Platform)" -ForegroundColor Cyan

# Initialize variables for job summary
$script:BuildStartTime = Get-Date

# Define all build tasks in execution order (matching the exact "." task order in Bucket.build.ps1)
$script:BuildTasks = @(
    @{ Name = 'Clean'; Description = 'Clean and reset Artifacts/Archive Directory'; Required = $true },
    @{ Name = 'ValidateRequirements'; Description = 'Validate system requirements are met'; Required = $true },
    @{ Name = 'ImportModuleManifest'; Description = 'Import the current module manifest file'; Required = $true },
    @{ Name = 'FormattingCheck'; Description = 'Analyze scripts for coding format compliance'; Required = $true },
    @{ Name = 'Analyze'; Description = 'PSScriptAnalyzer against Module source path'; Required = $true },
    @{ Name = 'Test'; Description = 'Pester Unit Tests in Tests\Unit folder'; Required = $true },
    @{ Name = 'CreateHelpStart'; Description = 'Initialize help creation process'; Required = $true },
    @{ Name = 'Build'; Description = 'Build the Module to Artifacts folder'; Required = $true },
    @{ Name = 'IntegrationTest'; Description = 'Pester Integration Tests'; Required = $true },
    @{ Name = 'Archive'; Description = 'Create archive of the built Module'; Required = $true }
)

$script:BuildResults = @{
    Bootstrap = @{ Status = 'Pending'; Duration = $null; Error = $null }
    Tasks = @()
    TestResults = @{ Status = 'Pending'; Duration = $null; Error = $null; Details = @{} }
    Artifacts = @{ Status = 'Pending'; Duration = $null; Error = $null; Files = @() }
}

# Initialize task results
foreach ($task in $script:BuildTasks) {
    $script:BuildResults.Tasks += @{
        Name = $task.Name
        Description = $task.Description
        Required = $task.Required
        Status = 'Pending'
        Duration = $null
        Error = $null
        StartTime = $null
        EndTime = $null
    }
}
#endregion Initialization

#region Helper Functions
function Write-StepHeader {
    param([string]$Title)
    Write-Host ""
    Write-Host "---------------------------------------------" -ForegroundColor DarkCyan
    Write-Host "  $Title" -ForegroundColor Yellow
    Write-Host "---------------------------------------------" -ForegroundColor DarkCyan
}

function Write-StepResult {
    param(
        [string]$Step,
        [bool]$Success,
        [timespan]$Duration,
        [string]$Error = $null
    )

    $status = if ($Success) { "✅ SUCCESS" } else { "❌ FAILED" }
    $color = if ($Success) { "Green" } else { "Red" }

    Write-Host "[$status] $Step completed in $($Duration.TotalSeconds.ToString('F2'))s" -ForegroundColor $color

    if ($Error) {
        Write-Host "Error: $Error" -ForegroundColor Red
    }
}

function Add-ToJobSummary {
    param([string]$Content)

    if ($env:GITHUB_STEP_SUMMARY) {
        Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $Content
    }
}


function Invoke-BuildTask {
    param(
        [string]$TaskName,
        [string]$Description,
        [bool]$Required = $true
    )

    $taskResult = $script:BuildResults.Tasks | Where-Object { $_.Name -eq $TaskName }
    $taskResult.StartTime = Get-Date

    Write-Host ""
    Write-Host "🔨 Executing Task: $TaskName" -ForegroundColor Yellow
    Write-Host "   Description: $Description" -ForegroundColor Gray
    try {
        # Execute the specific build task
        Push-Location -Path ".\src"        # Run individual task with Invoke-Build and capture both success and error streams
        Invoke-Build -Task $TaskName -File ".\Bucket.build.ps1" 2>&1 | Out-Null

        # Check if the build was successful by examining the output
        $buildSuccess = $LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq $null

        Pop-Location

        $taskResult.EndTime = Get-Date
        $taskResult.Duration = $taskResult.EndTime - $taskResult.StartTime

        if ($buildSuccess) {
            $taskResult.Status = 'Success'
            Write-Host "   ✅ Task '$TaskName' completed successfully in $($taskResult.Duration.TotalSeconds.ToString('F2'))s" -ForegroundColor Green
            return $true
        } else {
            throw "Build task failed with exit code: $LASTEXITCODE"
        }
    }
    catch {
        Pop-Location -ErrorAction SilentlyContinue

        $taskResult.EndTime = Get-Date
        $taskResult.Duration = if ($taskResult.StartTime) { $taskResult.EndTime - $taskResult.StartTime } else { [TimeSpan]::Zero }
        $taskResult.Status = if ($Required) { 'Failed' } else { 'Skipped' }
        $taskResult.Error = $_.Exception.Message

        if ($Required) {
            Write-Host "   ❌ Task '$TaskName' failed after $($taskResult.Duration.TotalSeconds.ToString('F2'))s" -ForegroundColor Red
            Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        } else {
            Write-Host "   ⚠️  Task '$TaskName' skipped (optional): $($_.Exception.Message)" -ForegroundColor Yellow
            $taskResult.Status = 'Skipped'
            return $true
        }
    }
}
#endregion Helper Functions

#region Bootstrap Process
Write-StepHeader "Bootstrap Dependencies"
$bootstrapStart = Get-Date

try {
    Write-Host "Running bootstrap script..." -ForegroundColor Cyan
    & "./actions_bootstrap.ps1"

    $bootstrapDuration = (Get-Date) - $bootstrapStart
    $script:BuildResults.Bootstrap.Status = 'Success'
    $script:BuildResults.Bootstrap.Duration = $bootstrapDuration

    Write-StepResult -Step "Bootstrap" -Success $true -Duration $bootstrapDuration
}
catch {
    $bootstrapDuration = (Get-Date) - $bootstrapStart
    $script:BuildResults.Bootstrap.Status = 'Failed'
    $script:BuildResults.Bootstrap.Duration = $bootstrapDuration
    $script:BuildResults.Bootstrap.Error = $_.Exception.Message

    Write-StepResult -Step "Bootstrap" -Success $false -Duration $bootstrapDuration -Error $_.Exception.Message
    Write-Host "❌ Bootstrap failed, but continuing to attempt build tasks..." -ForegroundColor Yellow
}
#endregion Bootstrap Process

#region Build Process
Write-StepHeader "Execute Individual Build Tasks"
$buildStart = Get-Date

try {
    Write-Host "Starting individual build task execution..." -ForegroundColor Cyan

    # Debug: Show initial task states
    Write-Host "🔍 Initial task states:" -ForegroundColor DarkGray
    foreach ($taskResult in $script:BuildResults.Tasks) {
        Write-Host "   - $($taskResult.Name): $($taskResult.Status)" -ForegroundColor DarkGray
    }

    # Execute each build task individually
    $overallSuccess = $true
    $executedTasks = 0
    $successfulTasks = 0
    $failedTasks = 0
    $skippedTasks = 0

    foreach ($task in $script:BuildTasks) {
        $executedTasks++

        Write-Host "🔄 Processing task $executedTasks/$($script:BuildTasks.Count): $($task.Name)" -ForegroundColor Cyan

        $taskSuccess = Invoke-BuildTask -TaskName $task.Name -Description $task.Description -Required $task.Required

        # Get the updated task result to check the actual status
        $taskResult = $script:BuildResults.Tasks | Where-Object { $_.Name -eq $task.Name }
        Write-Host "   📊 Task result status: $($taskResult.Status)" -ForegroundColor Gray

        if ($taskSuccess) {
            if ($taskResult.Status -eq 'Success') {
                $successfulTasks++
            } elseif ($taskResult.Status -eq 'Skipped') {
                $skippedTasks++
            }
        } else {
            $failedTasks++
            if ($task.Required) {
                $overallSuccess = $false
                Write-Host "❌ Build process stopped due to required task failure: $($task.Name)" -ForegroundColor Red
                break
            }
        }

        # Debug: Show current statistics
        Write-Host "   📈 Current stats: $successfulTasks✅ $failedTasks❌ $skippedTasks⚠️ $(($script:BuildResults.Tasks | Where-Object { $_.Status -eq 'Pending' }).Count)⏳" -ForegroundColor DarkGray
    }

    $buildDuration = (Get-Date) - $buildStart
    if ($overallSuccess) {
        Write-StepResult -Step "Build Process" -Success $true -Duration $buildDuration
        Write-Host "📊 Task Summary: $successfulTasks successful, $skippedTasks skipped, $failedTasks failed out of $executedTasks total" -ForegroundColor Green
    } else {
        Write-StepResult -Step "Build Process" -Success $false -Duration $buildDuration -Error "One or more required tasks failed"
        Write-Host "📊 Task Summary: $successfulTasks successful, $skippedTasks skipped, $failedTasks failed out of $executedTasks total" -ForegroundColor Red
        Write-Host "❌ Build process failed, but continuing to generate summary..." -ForegroundColor Yellow
    }
}
catch {
    $buildDuration = (Get-Date) - $buildStart
    Write-StepResult -Step "Build Process" -Success $false -Duration $buildDuration -Error $_.Exception.Message
    Write-Host "❌ Build process encountered an exception, but continuing to generate summary..." -ForegroundColor Yellow
}
#endregion Build Process

#region Test Results Analysis
Write-StepHeader "Analyze Test Results"
$testAnalysisStart = Get-Date

try {
    # Look for test output files
    $testOutputPath = ".\src\Artifacts\testOutput"
    $testResults = @{
        UnitTests = @{ Found = $false; Passed = 0; Failed = 0; Total = 0 }
        IntegrationTests = @{ Found = $false; Passed = 0; Failed = 0; Total = 0 }
        CodeCoverage = @{ Found = $false; Percentage = 0 }
    }

    if (Test-Path $testOutputPath) {
        # Parse Pester test results if available
        $pesterFiles = Get-ChildItem -Path $testOutputPath -Filter "*.xml" -ErrorAction SilentlyContinue

        foreach ($file in $pesterFiles) {
            try {
                $xml = [xml](Get-Content $file.FullName)
                $testSuite = $xml.'test-results'

                if ($testSuite) {
                    $testType = if ($file.Name -match "Integration") { "IntegrationTests" } else { "UnitTests" }

                    $testResults[$testType].Found = $true
                    $testResults[$testType].Total = [int]$testSuite.total
                    $testResults[$testType].Passed = [int]$testSuite.total - [int]$testSuite.failures - [int]$testSuite.errors
                    $testResults[$testType].Failed = [int]$testSuite.failures + [int]$testSuite.errors
                }
            }
            catch {
                Write-Warning "Could not parse test file: $($file.Name)"
            }
        }

        # Check for code coverage
        $coverageFile = Get-ChildItem -Path $testOutputPath -Filter "*coverage*" -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($coverageFile) {
            $testResults.CodeCoverage.Found = $true
            # Try to extract coverage percentage (implementation depends on coverage format)
        }
    }

    $testAnalysisDuration = (Get-Date) - $testAnalysisStart
    $script:BuildResults.TestResults.Status = 'Success'
    $script:BuildResults.TestResults.Duration = $testAnalysisDuration
    $script:BuildResults.TestResults.Details = $testResults

    Write-StepResult -Step "Test Analysis" -Success $true -Duration $testAnalysisDuration
}
catch {
    $testAnalysisDuration = (Get-Date) - $testAnalysisStart
    $script:BuildResults.TestResults.Status = 'Failed'
    $script:BuildResults.TestResults.Duration = $testAnalysisDuration
    $script:BuildResults.TestResults.Error = $_.Exception.Message

    Write-StepResult -Step "Test Analysis" -Success $false -Duration $testAnalysisDuration -Error $_.Exception.Message
}
#endregion Test Results Analysis

#region Artifacts Collection
Write-StepHeader "Collect Build Artifacts"
$artifactsStart = Get-Date

try {
    $artifactFiles = @()

    # Collect artifacts from various locations
    $artifactPaths = @(
        ".\src\Artifacts\*.psd1",
        ".\src\Artifacts\*.psm1",
        ".\src\Archive\*.zip",
        ".\src\Artifacts\testOutput\*.xml"
    )

    foreach ($path in $artifactPaths) {
        $files = Get-ChildItem -Path $path -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            $artifactFiles += @{
                Name = $file.Name
                Path = $file.FullName
                Size = $file.Length
                LastModified = $file.LastWriteTime
            }
        }
    }

    $artifactsDuration = (Get-Date) - $artifactsStart
    $script:BuildResults.Artifacts.Status = 'Success'
    $script:BuildResults.Artifacts.Duration = $artifactsDuration
    $script:BuildResults.Artifacts.Files = $artifactFiles

    Write-StepResult -Step "Artifacts Collection" -Success $true -Duration $artifactsDuration
    Write-Host "Found $($artifactFiles.Count) artifact files" -ForegroundColor Green
}
catch {
    $artifactsDuration = (Get-Date) - $artifactsStart
    $script:BuildResults.Artifacts.Status = 'Failed'
    $script:BuildResults.Artifacts.Duration = $artifactsDuration
    $script:BuildResults.Artifacts.Error = $_.Exception.Message

    Write-StepResult -Step "Artifacts Collection" -Success $false -Duration $artifactsDuration -Error $_.Exception.Message
}
#endregion Artifacts Collection

#region Job Summary Generation
Write-StepHeader "Generate Job Summary"

$totalDuration = (Get-Date) - $script:BuildStartTime

# Calculate overall success
$bootstrapSuccess = $script:BuildResults.Bootstrap.Status -eq 'Success'
$tasksSuccess = ($script:BuildResults.Tasks | Where-Object { $_.Required -and $_.Status -eq 'Failed' }).Count -eq 0
$overallSuccess = $bootstrapSuccess -and $tasksSuccess

# Calculate task statistics
$successfulTasks = ($script:BuildResults.Tasks | Where-Object { $_.Status -eq 'Success' }).Count
$failedTasks = ($script:BuildResults.Tasks | Where-Object { $_.Status -eq 'Failed' }).Count
$skippedTasks = ($script:BuildResults.Tasks | Where-Object { $_.Status -eq 'Skipped' }).Count
$pendingTasks = ($script:BuildResults.Tasks | Where-Object { $_.Status -eq 'Pending' }).Count

# Generate GitHub Job Summary
$summary = @"
# 🌙 Bucket Nightly Build Report

**Build Date:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC')
**Total Duration:** $($totalDuration.TotalMinutes.ToString('F2')) minutes
**Overall Status:** $(if ($overallSuccess) { '✅ SUCCESS' } else { '❌ FAILED' })

## 📊 High-Level Summary

| Component | Status | Duration | Details |
|-----------|--------|----------|---------|
| Bootstrap | $(if ($script:BuildResults.Bootstrap.Status -eq 'Success') { '✅' } elseif ($script:BuildResults.Bootstrap.Status -eq 'Failed') { '❌' } else { '⏳' }) $($script:BuildResults.Bootstrap.Status) | $($script:BuildResults.Bootstrap.Duration.TotalSeconds.ToString('F1'))s | Dependencies installation |
| Build Tasks | $(if ($tasksSuccess) { '✅ SUCCESS' } else { '❌ FAILED' }) | $($totalDuration.TotalSeconds.ToString('F1'))s | $successfulTasks✅ $failedTasks❌ $skippedTasks⚠️ $pendingTasks⏳ |
| Test Analysis | $(if ($script:BuildResults.TestResults.Status -eq 'Success') { '✅' } elseif ($script:BuildResults.TestResults.Status -eq 'Failed') { '❌' } else { '⏳' }) $($script:BuildResults.TestResults.Status) | $($script:BuildResults.TestResults.Duration.TotalSeconds.ToString('F1'))s | Test results parsing |
| Artifacts | $(if ($script:BuildResults.Artifacts.Status -eq 'Success') { '✅' } elseif ($script:BuildResults.Artifacts.Status -eq 'Failed') { '❌' } else { '⏳' }) $($script:BuildResults.Artifacts.Status) | $($script:BuildResults.Artifacts.Duration.TotalSeconds.ToString('F1'))s | Build outputs collection |

## 🔧 Detailed Build Tasks Results

| # | Task Name | Status | Duration | Required | Description | Error |
|---|-----------|--------|----------|----------|-------------|-------|

"@

# Add each task to the table
$taskNumber = 1
foreach ($task in $script:BuildResults.Tasks) {
    $statusIcon = switch ($task.Status) {
        'Success' { '✅' }
        'Failed' { '❌' }
        'Skipped' { '⚠️' }
        'Pending' { '⏳' }
        default { '❓' }
    }

    $duration = if ($task.Duration) { "$($task.Duration.TotalSeconds.ToString('F1'))s" } else { '-' }
    $required = if ($task.Required) { '🔴 Yes' } else { '🟡 No' }
    $error = if ($task.Error) { $task.Error.Substring(0, [Math]::Min(50, $task.Error.Length)) + '...' } else { '-' }

    $summary += "| $taskNumber | **$($task.Name)** | $statusIcon $($task.Status) | $duration | $required | $($task.Description) | $error |`n"
    $taskNumber++
}

$summary += @"

## 🧪 Test Results

"@

# Add test results details
$testDetails = $script:BuildResults.TestResults.Details
if ($testDetails.UnitTests.Found) {
    $summary += @"

### Unit Tests
- **Total:** $($testDetails.UnitTests.Total)
- **Passed:** $($testDetails.UnitTests.Passed) ✅
- **Failed:** $($testDetails.UnitTests.Failed) $(if ($testDetails.UnitTests.Failed -gt 0) { '❌' } else { '' })

"@
}

if ($testDetails.IntegrationTests.Found) {
    $summary += @"

### Integration Tests
- **Total:** $($testDetails.IntegrationTests.Total)
- **Passed:** $($testDetails.IntegrationTests.Passed) ✅
- **Failed:** $($testDetails.IntegrationTests.Failed) $(if ($testDetails.IntegrationTests.Failed -gt 0) { '❌' } else { '' })

"@
}

# Add artifacts information
$summary += @"

## 📦 Build Artifacts

Generated $($script:BuildResults.Artifacts.Files.Count) artifact files:

"@

foreach ($artifact in $script:BuildResults.Artifacts.Files) {
    $sizeKB = [math]::Round($artifact.Size / 1KB, 2)
    $summary += "- **$($artifact.Name)** ($($sizeKB) KB)`n"
}

# Add error information if any failures occurred
$allErrors = @()
if ($script:BuildResults.Bootstrap.Error) { $allErrors += "Bootstrap: $($script:BuildResults.Bootstrap.Error)" }
if ($script:BuildResults.TestResults.Error) { $allErrors += "Test Analysis: $($script:BuildResults.TestResults.Error)" }
if ($script:BuildResults.Artifacts.Error) { $allErrors += "Artifacts: $($script:BuildResults.Artifacts.Error)" }

# Add task errors
$taskErrors = $script:BuildResults.Tasks | Where-Object { $_.Error } | ForEach-Object { "$($_.Name): $($_.Error)" }
$allErrors += $taskErrors

if ($allErrors.Count -gt 0) {
    $summary += @"

## ❌ Errors Encountered

"@
    foreach ($error in $allErrors) {
        $summary += "- $error`n"
    }
}

$summary += @"

---
*Generated by Bucket Nightly Build Workflow*
"@

# Output to GitHub Job Summary
Add-ToJobSummary -Content $summary

# Also output to console
Write-Host ""
Write-Host "📋 JOB SUMMARY" -ForegroundColor Magenta
Write-Host "---------------------------------------------" -ForegroundColor DarkMagenta
Write-Host $summary

Write-Host ""
if ($overallSuccess) {
    Write-Host "🎉 Nightly build completed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "💥 Nightly build failed. Check the errors above." -ForegroundColor Red
    exit 1
}
#endregion Job Summary Generation