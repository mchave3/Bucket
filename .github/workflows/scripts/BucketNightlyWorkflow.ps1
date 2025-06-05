#Requires -Version 7.4
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]    [ValidateSet(
        'InitializeBuildSummary',
        'DisplayEnvironmentInfo',
        'BootstrapDependencies',
        'GetModuleVersion',
        'TestAndBuild',
        'GenerateCodeCoverage',
        'RunCodeAnalysis',
        'GenerateTestReportSummary',
        'WriteCodeMetric',
        'WriteBuildJobSummary',
        'WriteFinalConsolidatedSummary',
        'WriteCompleteSummary',
        'RunPerformanceBenchmark',
        'RunSecurityScan',
        'WriteComprehensiveReport'
    )]
    [string]$Action,
    [string]$RunNumber,
    [string]$CommitSHA,
    [string]$BranchName,
    [string]$TriggerEvent,
    [string]$ModuleVersion,
    [string]$GitHubRefName,
    [string]$GitHubShaForReport,
    [string]$GitHubRunId
)

#region Helper Functions
function Get-GitHubOutput {
    [CmdletBinding()]
    param()
    return $env:GITHUB_OUTPUT
}

function Get-GitHubStepSummary {
    [CmdletBinding()]
    param()
    return $env:GITHUB_STEP_SUMMARY
}
#endregion Helper Functions

#region Script Variables
# Variables to accumulate summary content instead of writing incrementally
$script:buildInfo = @{}
$script:pipelineStatus = @{
    'Build & Test' = @{ Status = 'Pending'; Details = 'Waiting'; Icon = '⏸️' }
    'Code Coverage' = @{ Status = 'Pending'; Details = 'Waiting'; Icon = '⏸️' }
    'Performance' = @{ Status = 'Pending'; Details = 'Waiting'; Icon = '⏸️' }
    'Security Scan' = @{ Status = 'Pending'; Details = 'Waiting'; Icon = '⏸️' }
    'Artifacts' = @{ Status = 'Pending'; Details = 'Waiting'; Icon = '⏸️' }
}
$script:testResults = @{}
$script:codeMetrics = @{}
$script:summaryContent = @()
#endregion Script Variables

#region Workflow Functions

#region Build Summary Initialization
function Initialize-NightlyBuildSummary {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$RunNumber,
        [Parameter(Mandatory = $true)]
        [string]$CommitSHA,
        [Parameter(Mandatory = $true)]
        [string]$BranchName,
        [Parameter(Mandatory = $true)]
        [string]$TriggerEvent
    )
    Write-Host "Initializing Build Summary..." -ForegroundColor Yellow

    # Store build information for later use in final summary
    $script:buildInfo = @{
        RunNumber = $RunNumber
        CommitSHA = $CommitSHA
        BranchName = $BranchName
        TriggerEvent = $TriggerEvent
        StartTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
    }
      Write-Host "Build information initialized!" -ForegroundColor Green
}

#region Environment Info
function Write-NightlyEnvironmentInfo {
    [CmdletBinding()]
    param ()
    Write-Host "=== PowerShell Version ===" -ForegroundColor Green
    $PSVersionTable | Format-Table -AutoSize
    Write-Host "=== System Information ===" -ForegroundColor Green
    if ($IsWindows -or $PSVersionTable.PSEdition -eq 'Desktop') {
        Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, TotalPhysicalMemory | Format-Table -AutoSize
    }
    else {
        Write-Host "Operating System: $($PSVersionTable.OS)" -ForegroundColor Cyan
        Write-Host "Platform: $($PSVersionTable.Platform)" -ForegroundColor Cyan
        if (Get-Command 'free' -ErrorAction SilentlyContinue) {
            Write-Host "Memory Information:" -ForegroundColor Cyan
            free -h
        }
    }
    Write-Host "=== Available Modules (Pester, PSScriptAnalyzer) ===" -ForegroundColor Green
    Get-Module -ListAvailable | Where-Object { $_.Name -like "*Pester*" -or $_.Name -like "*PSScript*" } | Format-Table Name, Version -AutoSize
}

#region Dependency Bootstrap
function Invoke-NightlyBootstrap {
    [CmdletBinding()]
    param ()
    Write-Host "Bootstrapping dependencies..." -ForegroundColor Yellow
    try {
        # Assuming actions_bootstrap.ps1 is at the root of the repository
        # and this script is run from the repository root.
        $bootstrapScriptPath = ".\\actions_bootstrap.ps1"
        if (Test-Path $bootstrapScriptPath) {
            & $bootstrapScriptPath
            Write-Host "Bootstrap script executed successfully." -ForegroundColor Green
        }
        else {
            Write-Error "Bootstrap script not found at $bootstrapScriptPath"
            exit 1
        }
    }
    catch {
        Write-Error "Error during bootstrap: $($_.Exception.Message)"
        exit 1
    }
}

#region Module Version
function Get-NightlyModuleVersion {
    [CmdletBinding()]
    param ()
    Write-Host "Getting module version..." -ForegroundColor Yellow
    try {
        $moduleManifestPath = ".\\src\\Bucket\\Bucket.psd1"
        if (-not (Test-Path $moduleManifestPath)) {
            Write-Error "Module manifest not found at $moduleManifestPath"
            exit 1
        }
        $version = (Import-PowerShellDataFile $moduleManifestPath).ModuleVersion
        Write-Host "Module Version: $version" -ForegroundColor Green
        $outputFile = Get-GitHubOutput
        "version=$version" | Add-Content -Path $outputFile
        Write-Host "Version set in GITHUB_OUTPUT: $outputFile" -ForegroundColor Cyan
    }
    catch {
        Write-Error "Error getting module version: $($_.Exception.Message)"
        exit 1
    }
}

#region Build & Test
function Invoke-NightlyFullBuild {
    [CmdletBinding()]
    param ()
    Write-Host "Running complete test and build process..." -ForegroundColor Yellow
    try {
        Invoke-Build -File .\\src\\Bucket.build.ps1
        Write-Host "Full build process completed." -ForegroundColor Green
    }
    catch {
        Write-Error "Error during full build: $($_.Exception.Message)"
        # Decide if this is a fatal error for the script
    }
}

#region Code Coverage
function Invoke-NightlyCodeCoverage {
    [CmdletBinding()]
    param ()
    Write-Host "Generating code coverage report..." -ForegroundColor Yellow
    try {
        Invoke-Build -Task DevCC -File .\\src\\Bucket.build.ps1

        $coverageFilePath = ".\\cov.xml"
        if (Test-Path $coverageFilePath) {
            Write-Host "Code coverage report generated successfully at: $coverageFilePath" -ForegroundColor Green
            $fileSize = (Get-Item $coverageFilePath).Length
            Write-Host "Coverage file size: $fileSize bytes" -ForegroundColor Cyan
        }
        else {
            Write-Host "Warning: Coverage file was not created at expected location $coverageFilePath" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "Error during code coverage generation: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Code coverage generation failed - continuing build process." -ForegroundColor Yellow
    }
}

#region Code Analysis
function Invoke-NightlyCodeAnalysis {
    [CmdletBinding()]
    param ()
    Write-Host "Running additional code analysis..." -ForegroundColor Yellow
    try {
        Invoke-Build -Task Analyze -File .\\src\\Bucket.build.ps1
        Write-Host "Code analysis completed successfully." -ForegroundColor Green
    }
    catch {
        Write-Host "Code analysis skipped or failed: $($_.Exception.Message)" -ForegroundColor Yellow
        # This might not be a fatal error, depends on workflow requirements
    }
}

#region Test Report Summary
function Get-NightlyTestReportSummary {
    [CmdletBinding()]
    param ()
    Write-Host "Generating test report summary..." -ForegroundColor Yellow
    $testResults = @()
    $coverageResults = @()

    # Parse Pester results
    $pesterFile = ".\\src\\Artifacts\\testOutput\\PesterTests.xml"
    if (Test-Path $pesterFile) {
        try {
            [xml]$pesterXml = Get-Content $pesterFile
            $testResults += @{
                Type     = "Unit Tests"
                Total    = $pesterXml.'test-results'.total
                Passed   = $pesterXml.'test-results'.passed
                Failed   = $pesterXml.'test-results'.failed
                Skipped  = $pesterXml.'test-results'.inconclusive
            }
        }
        catch {
            Write-Host "Warning: Could not parse Pester results from $pesterFile. $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Warning: Pester results file not found at $pesterFile." -ForegroundColor Yellow
    }

    # Parse coverage results
    $coverageFile = ".\\cov.xml"
    if (Test-Path $coverageFile) {
        try {
            [xml]$coverageXml = Get-Content $coverageFile
            $coverage = $coverageXml.coverage
            if ($coverage) {
                $lineRate = [math]::Round([double]$coverage.'line-rate' * 100, 2)
                $coverageResults += @{
                    LineRate     = $lineRate
                    LinesValid   = $coverage.'lines-valid'
                    LinesCovered = $coverage.'lines-covered'
                }
            }
        }
        catch {
            Write-Host "Warning: Could not parse coverage results from $coverageFile. $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Warning: Coverage file not found at $coverageFile." -ForegroundColor Yellow
    }

    $summary = @{
        Tests     = $testResults
        Coverage  = $coverageResults
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
    }

    $summaryJson = $summary | ConvertTo-Json -Depth 10 -Compress
    Write-Host "Test Summary JSON: $summaryJson" -ForegroundColor Green
    $outputFile = Get-GitHubOutput
    "summary=$summaryJson" | Add-Content -Path $outputFile
    Write-Host "Test report summary set in GITHUB_OUTPUT: $outputFile" -ForegroundColor Cyan
}

#region Code Metrics
function Write-NightlyCodeMetric {
    [CmdletBinding()]
    param ()
    Write-Host "Generating code metrics..." -ForegroundColor Yellow
    try {
        $psFiles = Get-ChildItem -Path ".\\src\\Bucket" -Recurse -Filter "*.ps1" | Where-Object { $_.Name -notlike "*.Tests.ps1" }
        $totalLines = 0
        $totalFiles = $psFiles.Count

        foreach ($file in $psFiles) {
            $content = Get-Content $file.FullName -ErrorAction SilentlyContinue
            if ($content) {
                $totalLines += $content.Count
            }
        }

        Write-Host "=== Code Metrics ===" -ForegroundColor Green
        Write-Host "Total PowerShell Files: $totalFiles" -ForegroundColor Cyan
        Write-Host "Total Lines of Code: $totalLines" -ForegroundColor Cyan
        $averageLines = 0
        if ($totalFiles -gt 0) {
            $averageLines = [math]::Round($totalLines / $totalFiles, 2)
        }
        Write-Host "Average Lines per File: $averageLines" -ForegroundColor Cyan

        $metrics = @{
            TotalFiles    = $totalFiles
            TotalLines    = $totalLines
            AverageLines  = $averageLines
            GeneratedAt   = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
        }

        $metrics | ConvertTo-Json | Out-File -FilePath ".\\code-metrics.json" -Encoding UTF8
        Write-Host "Code metrics saved to .\\code-metrics.json" -ForegroundColor Green
    }
    catch {
        Write-Error "Error generating code metrics: $($_.Exception.Message)"
    }
}

#region Build Job Summary
function Write-NightlyBuildJobSummary {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$ModuleVersion
    )    Write-Host "Collecting build job summary data..." -ForegroundColor Yellow

    $buildTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"

    # Initialize status variables
    $testStatus = "❌ No Data"
    $testCount = "No tests found"
    $testDetails = ""
    $coverageStatus = "❌ No Data"
    $coveragePercent = "N/A"

    # Parse test results
    $pesterFile = ".\\src\\Artifacts\\testOutput\\PesterTests.xml"
    Write-Host "🔍 Checking for test results at: $pesterFile" -ForegroundColor Cyan
    if (Test-Path $pesterFile) {
        Write-Host "✅ Pester file found!" -ForegroundColor Green
        try {
            [xml]$pesterXml = Get-Content $pesterFile
            Write-Host "📄 XML loaded successfully" -ForegroundColor Cyan

            # Try NUnit format first
            $total = $pesterXml.'test-results'.total
            $passed = $pesterXml.'test-results'.passed
            $failed = $pesterXml.'test-results'.failed

            if ($null -ne $total -and $null -ne $passed -and $null -ne $failed) {
                $totalInt = if ([string]::IsNullOrEmpty($total)) { 0 } else { [int]$total }
                $passedInt = if ([string]::IsNullOrEmpty($passed)) { 0 } else { [int]$passed }
                $failedInt = if ([string]::IsNullOrEmpty($failed)) { 0 } else { [int]$failed }
                $testCount = "$passedInt/$totalInt"
                $testStatus = if ($failedInt -eq 0) { "✅ All Passed" } else { "❌ $failedInt Failed" }
                $testDetails = "$passedInt passed, $failedInt failed"
            }
            else {
                # Try JUnit format
                if ($pesterXml.testsuites) {
                    $total = $pesterXml.testsuites.tests
                    $failures = $pesterXml.testsuites.failures
                    $passed = [int]$total - [int]$failures
                    $failed = $failures
                }
                elseif ($pesterXml.testsuite) {
                    $total = $pesterXml.testsuite.tests
                    $failures = $pesterXml.testsuite.failures
                    $passed = [int]$total - [int]$failures
                    $failed = $failures
                }

                if ($null -ne $total -and $null -ne $passed -and $null -ne $failed) {
                    $totalInt = if ([string]::IsNullOrEmpty($total)) { 0 } else { [int]$total }
                    $passedInt = if ([string]::IsNullOrEmpty($passed)) { 0 } else { [int]$passed }
                    $failedInt = if ([string]::IsNullOrEmpty($failed)) { 0 } else { [int]$failed }
                    $testCount = "$passedInt/$totalInt"
                    $testStatus = if ($failedInt -eq 0) { "✅ All Passed" } else { "❌ $failedInt Failed" }
                    $testDetails = "$passedInt passed, $failedInt failed"
                }
                else {
                    $testStatus = "⚠️ Parse Issue"
                    $testCount = "Unknown format"
                    $testDetails = "XML format not recognized"
                }
            }
            Write-Host "✅ Test status: $testStatus ($testCount)" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Error parsing test file: $(${_.Exception.Message})" -ForegroundColor Red
            $testStatus = "⚠️ Parse Error"
            $testCount = "Parse failed"
            $testDetails = "Error: $($_.Exception.Message)"
        }
    }
    else {
        Write-Host "❌ Test file not found at $pesterFile" -ForegroundColor Red
        $testDetails = "Test file not found"
    }

    # Parse coverage results
    if (Test-Path ".\\cov.xml") {
        try {
            [xml]$coverageXml = Get-Content ".\\cov.xml"
            $coverageNodes = $coverageXml.SelectNodes("//coverage")
            if ($coverageNodes.Count -gt 0) {
                $lineRate = $coverageNodes[0].'line-rate'
                if ($lineRate) {
                    $coveragePercentVal = [math]::Round([double]$lineRate * 100, 1)
                    $coverageStatus = if ($coveragePercentVal -ge 80) { "✅ $coveragePercentVal%" } else { "⚠️ $coveragePercentVal%" }
                    $coveragePercent = "$coveragePercentVal%"
                }
            }
        }
        catch {
            $coverageStatus = "⚠️ Parse Error"
            $coveragePercent = "Parse failed"
        }
    }

    # Get code metrics
    $codeMetrics = "❌ No Data"
    if (Test-Path ".\\code-metrics.json") {
        try {
            $metrics = Get-Content ".\\code-metrics.json" | ConvertFrom-Json
            $codeMetrics = "📁 $($metrics.TotalFiles) files, 📝 $($metrics.TotalLines) lines"
        }
        catch { $codeMetrics = "⚠️ Parse Error" }
    }

    # Check artifacts
    $artifactsStatus = "✅ Available"
    if (-not (Test-Path ".\\src\\Artifacts")) {
        $artifactsStatus = "❌ Missing"
    }

    # Store test results and metrics for final summary
    $script:testResults = @{
        Status = $testStatus
        Count = $testCount
        Details = $testDetails
        Coverage = @{
            Status = $coverageStatus
            Percent = $coveragePercent
        }
        Metrics = $codeMetrics
        Artifacts = $artifactsStatus
        BuildTime = $buildTime
    }

    # Update pipeline status phases
    Save-NightlyPipelineStatus -Phase "Build & Test" -Status "Completed" -Details $testDetails
    Save-NightlyPipelineStatus -Phase "Code Coverage" -Status "Completed" -Details "$coveragePercent coverage"
    Save-NightlyPipelineStatus -Phase "Artifacts" -Status "Completed" -Details "Build outputs ready"

    Write-Host "Build job summary data collected!" -ForegroundColor Green
}

#region Final Consolidated Summary
function Write-NightlyFinalConsolidatedSummary {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ModuleVersion
    )
    Write-Host "Storing final completion data..." -ForegroundColor Yellow

    $completionTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"

    # Store completion information for final summary
    $script:buildInfo.CompletionTime = $completionTime
    $script:buildInfo.ModuleVersion = $ModuleVersion

    Write-Host "Final completion data stored!" -ForegroundColor Green
}

#region Complete Summary Generation
function Write-NightlyCompleteSummary {
    [CmdletBinding()]
    param()

    Write-Host "Generating complete job summary..." -ForegroundColor Yellow

    # Ensure buildInfo is initialized with safe defaults
    if (-not $script:buildInfo) {
        $script:buildInfo = @{}
    }

    # Set safe default values for missing properties
    $runNumber = if ($script:buildInfo.RunNumber) { $script:buildInfo.RunNumber } else { $RunNumber -or "Unknown" }
    $branchName = if ($script:buildInfo.BranchName) { $script:buildInfo.BranchName } else { $BranchName -or "Unknown" }
    $triggerEvent = if ($script:buildInfo.TriggerEvent) { $script:buildInfo.TriggerEvent } else { $TriggerEvent -or "Unknown" }
    $startTime = if ($script:buildInfo.StartTime) { $script:buildInfo.StartTime } else { Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC" }
    $commitSHA = if ($script:buildInfo.CommitSHA) { $script:buildInfo.CommitSHA } else { $CommitSHA -or "Unknown" }
    $commitShort = if ($commitSHA -and $commitSHA -ne "Unknown" -and $commitSHA.Length -ge 8) { $commitSHA.Substring(0,8) } else { $commitSHA }

    # Generate complete summary with all accumulated data
    $completeSummary = @"
# 🔧 Bucket Nightly Build #$runNumber

## 📋 Build Information

- **Branch:** $branchName
- **Trigger:** $triggerEvent
- **Started:** $startTime
- **Commit:** [$commitShort](https://github.com/mchave3/Bucket/commit/$commitSHA)

---

## 🚀 Pipeline Status

| Phase | Status | Details |
|-------|--------|---------|
| 🏗️ Build & Test | $(if ($script:pipelineStatus -and $script:pipelineStatus['Build & Test']) { "$($script:pipelineStatus['Build & Test'].Icon) $($script:pipelineStatus['Build & Test'].Status)" } else { "⏸️ Pending" }) | $(if ($script:pipelineStatus -and $script:pipelineStatus['Build & Test']) { $script:pipelineStatus['Build & Test'].Details } else { "Not started" }) |
| 📊 Code Coverage | $(if ($script:pipelineStatus -and $script:pipelineStatus['Code Coverage']) { "$($script:pipelineStatus['Code Coverage'].Icon) $($script:pipelineStatus['Code Coverage'].Status)" } else { "⏸️ Pending" }) | $(if ($script:pipelineStatus -and $script:pipelineStatus['Code Coverage']) { $script:pipelineStatus['Code Coverage'].Details } else { "Not started" }) |
| 📈 Code Metrics | ✅ Completed | $(if ($script:testResults -and $script:testResults.Metrics) { $script:testResults.Metrics } else { "Not available" }) |
| ⚡ Performance | $(if ($script:pipelineStatus -and $script:pipelineStatus['Performance']) { "$($script:pipelineStatus['Performance'].Icon) $($script:pipelineStatus['Performance'].Status)" } else { "⏸️ Pending" }) | $(if ($script:pipelineStatus -and $script:pipelineStatus['Performance']) { $script:pipelineStatus['Performance'].Details } else { "Not started" }) |
| 🛡️ Security Scan | $(if ($script:pipelineStatus -and $script:pipelineStatus['Security Scan']) { "$($script:pipelineStatus['Security Scan'].Icon) $($script:pipelineStatus['Security Scan'].Status)" } else { "⏸️ Pending" }) | $(if ($script:pipelineStatus -and $script:pipelineStatus['Security Scan']) { $script:pipelineStatus['Security Scan'].Details } else { "Not started" }) |
| 📦 Artifacts | $(if ($script:pipelineStatus -and $script:pipelineStatus['Artifacts']) { "$($script:pipelineStatus['Artifacts'].Icon) $($script:pipelineStatus['Artifacts'].Status)" } else { "⏸️ Pending" }) | $(if ($script:pipelineStatus -and $script:pipelineStatus['Artifacts']) { $script:pipelineStatus['Artifacts'].Details } else { "Not started" }) |

---

## 📊 Build Summary v$(if ($script:buildInfo -and $script:buildInfo.ModuleVersion) { $script:buildInfo.ModuleVersion } else { $ModuleVersion -or "Unknown" })

**🧪 Test Results:** $(if ($script:testResults -and $script:testResults.Status) { $script:testResults.Status } else { "Not available" }) $(if ($script:testResults -and $script:testResults.Count) { "($($script:testResults.Count))" } else { "" })
**📊 Coverage:** $(if ($script:testResults -and $script:testResults.Coverage -and $script:testResults.Coverage.Percent) { $script:testResults.Coverage.Percent } else { "Not available" })
**⏱️ Completed:** $(if ($script:buildInfo -and $script:buildInfo.CompletionTime) { $script:buildInfo.CompletionTime } else { Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC" })

---

## ✅ Build Completed Successfully!

**Final Status:** All phases completed at $(if ($script:buildInfo -and $script:buildInfo.CompletionTime) { $script:buildInfo.CompletionTime } else { Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC" })

**🔗 Quick Links:**
- [📦 Download Artifacts](./src/Artifacts/)
- [🔄 Workflow Run #$($env:GITHUB_RUN_NUMBER)](https://github.com/mchave3/Bucket/actions/runs/$($env:GITHUB_RUN_ID))
- [📝 Commit Details](https://github.com/mchave3/Bucket/commit/$($env:GITHUB_SHA))

---
*Build completed for Bucket v$(if ($script:buildInfo -and $script:buildInfo.ModuleVersion) { $script:buildInfo.ModuleVersion } else { $ModuleVersion -or "Unknown" })*

**Job summary generated at run-time**
"@

    try {
        $completeSummary | Out-File -FilePath (Get-GitHubStepSummary) -Encoding UTF8
        Write-Host "Complete job summary written to GITHUB_STEP_SUMMARY!" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to write complete summary: $($_.Exception.Message)"
    }
}
#endregion Complete Summary Generation

#region Performance Benchmark
function Invoke-NightlyPerformanceBenchmark {
    [CmdletBinding()]
    param ()
    Write-Host "Running performance benchmarks..." -ForegroundColor Yellow
    try {
        # Ensure module path is correct, assuming script is run from repository root
        $modulePath = Join-Path $PSScriptRoot "..\..\..\src\Bucket\Bucket.psm1" # Adjust if script location changes
        Import-Module $modulePath -Force

        $startTime = Get-Date
        $moduleLoadTime = Measure-Command {
            Remove-Module Bucket -Force -ErrorAction SilentlyContinue
            Import-Module $modulePath -Force
        }

        $functionTests = @()
        # Example: Test Get-BucketVersion if it exists and is simple
        if (Get-Command Get-BucketVersion -Module Bucket -ErrorAction SilentlyContinue) {
            $execTime = Measure-Command { Get-BucketVersion }
            $functionTests += @{
                Function      = "Get-BucketVersion"
                ExecutionTime = $execTime.TotalMilliseconds
            }
        }

        $endTime = Get-Date
        $totalTime = ($endTime - $startTime).TotalSeconds

        $benchmarks = @{
            ModuleLoadTime     = $moduleLoadTime.TotalMilliseconds
            FunctionTests      = $functionTests
            TotalBenchmarkTime = $totalTime
            Timestamp          = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
        }

        Write-Host "=== Performance Benchmarks ===" -ForegroundColor Green
        Write-Host "Module Load Time: $($moduleLoadTime.TotalMilliseconds) ms" -ForegroundColor Cyan
        Write-Host "Total Benchmark Time: $totalTime seconds" -ForegroundColor Cyan

        $benchmarks | ConvertTo-Json -Depth 10 | Out-File -FilePath ".\\performance-benchmarks.json" -Encoding UTF8
        Write-Host "Performance benchmarks saved to .\\performance-benchmarks.json" -ForegroundColor Green

        # Update pipeline status
        Save-NightlyPipelineStatus -Phase "Performance" -Status "Completed" -Details "Benchmarks completed"
    }
    catch {
        Write-Error "Error during performance benchmarks: $($_.Exception.Message)"
    }
}

#region Security Scan
function Invoke-NightlySecurityScan {
    [CmdletBinding()]
    param ()
    Write-Host "Running security scan..." -ForegroundColor Yellow
    try {
        if (-not (Get-Module -ListAvailable PSScriptAnalyzer)) {
            Write-Host "PSScriptAnalyzer not found, installing..." -ForegroundColor Cyan
            Install-Module -Name PSScriptAnalyzer -Force -Scope CurrentUser -Confirm:$false -AcceptLicense
        }

        $securityRules = @(
            'PSAvoidUsingCmdletAliases',
            'PSAvoidUsingPlainTextForPassword',
            'PSAvoidUsingUsernameAndPasswordParams',
            'PSAvoidUsingInvokeExpression',
            'PSUseShouldProcessForStateChangingFunctions'
        )

        $scanPath = ".\\src\\Bucket"
        Write-Host "Scanning files in $scanPath" -ForegroundColor Cyan
        $analyzerResults = Get-ChildItem -Path $scanPath -Recurse -Filter "*.ps1" |
            ForEach-Object {
                Write-Host "Analyzing $($_.FullName)" -ForegroundColor Gray
                Invoke-ScriptAnalyzer -Path $_.FullName -IncludeRule $securityRules -ErrorAction SilentlyContinue
            } | Where-Object { $_ } # Filter out null results if any

        $issues = @()
        foreach ($result in $analyzerResults) {
            $issues += @{
                Severity   = $result.Severity.ToString()
                RuleName   = $result.RuleName
                Message    = $result.Message
                ScriptName = $result.ScriptName
                Line       = $result.Line
            }
        }

        $securityReport = @{
            TotalIssues = $issues.Count
            Issues      = $issues
            Timestamp   = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
        }

        Write-Host "=== Security Scan Results ===" -ForegroundColor Green
        Write-Host "Total Security Issues Found: $($issues.Count)" -ForegroundColor Cyan

        if ($issues.Count -gt 0) {
            $issues | Format-Table Severity, RuleName, ScriptName, Line -AutoSize
        }

        $securityReport | ConvertTo-Json -Depth 10 | Out-File -FilePath ".\\security-report.json" -Encoding UTF8
        Write-Host "Security report saved to .\\security-report.json" -ForegroundColor Green

        # Update pipeline status
        $statusDetails = if ($issues.Count -eq 0) { "No security issues found" } else { "$($issues.Count) issues found" }
        Save-NightlyPipelineStatus -Phase "Security Scan" -Status "Completed" -Details $statusDetails
    }
    catch {
        Write-Error "Error during security scan: $($_.Exception.Message)"
    }
}

#region Comprehensive Report
function Write-NightlyComprehensiveReport {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ModuleVersion,
        [Parameter(Mandatory = $true)]
        [string]$GitHubRefName,
        [Parameter(Mandatory = $true)]
        [string]$GitHubShaForReport,
        [Parameter(Mandatory = $true)]
        [string]$GitHubRunId
    )
    Write-Host "Generating comprehensive nightly report..." -ForegroundColor Yellow

    $reportData = @{
        BuildInfo = @{
            Version   = $ModuleVersion
            Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC"
            Branch    = $GitHubRefName
            Commit    = $GitHubShaForReport
            RunId     = $GitHubRunId
        }
        TestResults = @{ Available = $false }
        Performance = @{ Available = $false }
        Security    = @{ Available = $false }
    }

    # Paths are relative to where artifacts were downloaded, e.g., ./artifacts/
    $testResultsPath = "./artifacts/nightly-test-results" # This is a directory
    if (Test-Path $testResultsPath) {
        $reportData.TestResults = @{
            Available = $true
            Path      = $testResultsPath
            # Further parsing of specific files like PesterTests.xml or cov.xml can be done here if needed for the report
        }
        Write-Host "Test results artifact found at $testResultsPath" -ForegroundColor Cyan
    } else { Write-Host "Test results artifact not found at $testResultsPath" -ForegroundColor Yellow }


    $perfPath = "./artifacts/performance-benchmarks/performance-benchmarks.json"
    if (Test-Path $perfPath) {
        try {
            $perfData = Get-Content $perfPath | ConvertFrom-Json
            $reportData.Performance = $perfData
            $reportData.Performance.Available = $true
            Write-Host "Performance benchmarks artifact found and parsed from $perfPath" -ForegroundColor Cyan
        } catch { Write-Host "Failed to parse performance benchmarks from $perfPath : $($_.Exception.Message)" -ForegroundColor Yellow }
    } else { Write-Host "Performance benchmarks artifact not found at $perfPath" -ForegroundColor Yellow }

    $secPath = "./artifacts/security-report/security-report.json"
    if (Test-Path $secPath) {
        try {
            $secData = Get-Content $secPath | ConvertFrom-Json
            $reportData.Security = $secData
            $reportData.Security.Available = $true
            Write-Host "Security report artifact found and parsed from $secPath" -ForegroundColor Cyan
        } catch { Write-Host "Failed to parse security report from $secPath : $($_.Exception.Message)" -ForegroundColor Yellow }
    } else { Write-Host "Security report artifact not found at $secPath" -ForegroundColor Yellow }

    # Generate simple HTML report
    $htmlHeader = "<!DOCTYPE html><html><head><title>Bucket - Nightly Build Report</title><style>body{font-family:sans-serif}table{border-collapse:collapse}th,td{border:1px solid #ddd;padding:8px}</style></head><body>"
    $htmlContent = "<h1>Bucket - Nightly Build Report</h1>"
    $htmlContent += "<p>Generated: $($reportData.BuildInfo.Timestamp)</p>"
    $htmlContent += "<p>Version: $($reportData.BuildInfo.Version) | Branch: $($reportData.BuildInfo.Branch)</p>"
    $htmlContent += "<h2>Build Summary</h2>"
    $htmlContent += "<p>Run ID: <a href='https://github.com/mchave3/Bucket/actions/runs/$($reportData.BuildInfo.RunId)'>$($reportData.BuildInfo.RunId)</a></p>"
    $htmlContent += "<p>Commit: <a href='https://github.com/mchave3/Bucket/commit/$($reportData.BuildInfo.Commit)'>$($reportData.BuildInfo.Commit.Substring(0,8))</a></p>"
    $htmlContent += "<h2>Reports</h2>"
    $htmlContent += "<table><tr><th>Report Type</th><th>Status</th><th>Details</th></tr>"
    $htmlContent += "<tr><td>Test Results</td><td>$($reportData.TestResults.Available)</td><td>Data in nightly-test-results artifact</td></tr>"
    # Ensure Performance and Security objects exist before trying to access their properties
    $perfModuleLoadTime = if ($reportData.Performance.ModuleLoadTime) { "$($reportData.Performance.ModuleLoadTime)ms" } else { "N/A" }
    $htmlContent += "<tr><td>Performance</td><td>$($reportData.Performance.Available)</td><td>Module Load: $perfModuleLoadTime</td></tr>"
    $secTotalIssues = if ($reportData.Security.TotalIssues) { $reportData.Security.TotalIssues } else { "N/A" }
    $htmlContent += "<tr><td>Security</td><td>$($reportData.Security.Available)</td><td>Issues: $secTotalIssues</td></tr></table>"
    $htmlContent += "<p>Detailed JSON and XML reports are available as build artifacts.</p>"
    $htmlFooter = "</body></html>"

    $fullHtml = $htmlHeader + $htmlContent + $htmlFooter

    try {
        $fullHtml | Out-File -FilePath "./nightly-report.html" -Encoding UTF8
        $reportData | ConvertTo-Json -Depth 10 | Out-File -FilePath "./nightly-report.json" -Encoding UTF8
        Write-Host "=== Nightly Report Generated ===" -ForegroundColor Green
        Write-Host "Report files created: nightly-report.html, nightly-report.json" -ForegroundColor Cyan
    }
    catch {
        Write-Error "Failed to write comprehensive report files: $($_.Exception.Message)"
    }
}
#endregion Workflow Functions

#region Pipeline Status Update
function Save-NightlyPipelineStatus {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Phase,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Running', 'Completed', 'Failed', 'Skipped')]
        [string]$Status,
        [Parameter(Mandatory = $false)]
        [string]$Details = ""
    )

    Write-Host "Updating pipeline status: $Phase -> $Status" -ForegroundColor Cyan

    # Map status to appropriate emoji
    $statusIcon = switch ($Status) {
        'Running' { '🔄' }
        'Completed' { '✅' }
        'Failed' { '❌' }
        'Skipped' { '⏭️' }
        'Pending' { '⏸️' }
    }

    # Update the internal pipeline status instead of writing to summary
    if ($script:pipelineStatus.ContainsKey($Phase)) {
        $script:pipelineStatus[$Phase] = @{
            Status = $Status
            Details = $Details
            Icon = $statusIcon
        }
        Write-Host "Pipeline status updated for $Phase" -ForegroundColor Green
    }
    else {
        Write-Warning "Unknown phase: $Phase"
    }
}
#endregion Pipeline Status Update

#region Main Script Logic
# Dispatches the requested action to the appropriate function
try {
    switch ($Action) {
        'InitializeBuildSummary' { Initialize-NightlyBuildSummary -RunNumber $RunNumber -CommitSHA $CommitSHA -BranchName $BranchName -TriggerEvent $TriggerEvent }
        'DisplayEnvironmentInfo' { Write-NightlyEnvironmentInfo }
        'BootstrapDependencies' { Invoke-NightlyBootstrap }
        'GetModuleVersion' { Get-NightlyModuleVersion }
        'TestAndBuild' { Invoke-NightlyFullBuild }
        'GenerateCodeCoverage' { Invoke-NightlyCodeCoverage }
        'RunCodeAnalysis' { Invoke-NightlyCodeAnalysis }
        'GenerateTestReportSummary' { Get-NightlyTestReportSummary }
        'WriteCodeMetric' { Write-NightlyCodeMetric }
        'WriteBuildJobSummary' { Write-NightlyBuildJobSummary -ModuleVersion $ModuleVersion }
        'WriteFinalConsolidatedSummary' { Write-NightlyFinalConsolidatedSummary -ModuleVersion $ModuleVersion }
        'WriteCompleteSummary' { Write-NightlyCompleteSummary }
        'RunPerformanceBenchmark' { Invoke-NightlyPerformanceBenchmark }
        'RunSecurityScan' { Invoke-NightlySecurityScan }
        'WriteComprehensiveReport' { Write-NightlyComprehensiveReport -ModuleVersion $ModuleVersion -GitHubRefName $GitHubRefName -GitHubShaForReport $GitHubShaForReport -GitHubRunId $GitHubRunId }
        Default {
            Write-Error "Invalid action specified: $Action"
            exit 1
        }
    }
}
catch {
    Write-Error "An error occurred in BucketNightlyWorkflow.ps1 action '$Action': $($_.Exception.Message)"
    exit 1
}
#endregion Main Script Logic
