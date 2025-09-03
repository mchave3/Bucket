# Test Build Script for WiX 6 Setup with Full Automation
# This script tests the WiX 6 setup generation locally with auto-publish and harvest

param(
    [string]$Version = "1.0.0.0",
    [ValidateSet("x64", "x86", "ARM64")]
    [string]$Platform = "x64",
    [string]$Configuration = "Release"
)

Write-Host "=== Testing WiX 6 Setup Build (Fully Automated) ===" -ForegroundColor Cyan
Write-Host "🚀 Using WiX 6 with automatic file collection and variable extraction" -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Platform-specific settings
$ridMap = @{
    "x64" = "win-x64"
    "x86" = "win-x86"
    "ARM64" = "win-arm64"
}

$programFilesMap = @{
    "x64" = "ProgramFiles64Folder"
    "x86" = "ProgramFilesFolder"
    "ARM64" = "ProgramFiles64Folder"
}

$rid = $ridMap[$Platform]
$programFilesFolder = $programFilesMap[$Platform]

Write-Host "Runtime Identifier: $rid" -ForegroundColor Cyan
Write-Host "Program Files Folder: $programFilesFolder" -ForegroundColor Cyan

# Create output directory
$outputDir = "artifacts\installers"
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
Write-Host "Created output directory: $outputDir" -ForegroundColor Green

# Install/Update WiX 6 if needed
Write-Host "`n=== Checking WiX 6 Installation ===" -ForegroundColor Cyan
try {
    $wixVersion = wix --version 2>$null
    if ($wixVersion -and $wixVersion.StartsWith("6.")) {
        Write-Host "✅ WiX 6 is already installed: $wixVersion" -ForegroundColor Green
    } else {
        Write-Host "Updating to WiX 6..." -ForegroundColor Yellow
        dotnet tool update --global wix --version 6.0.2
        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ Failed to install WiX 6"
            exit 1
        }
        Write-Host "✅ WiX 6 installed successfully" -ForegroundColor Green
    }
} catch {
    Write-Host "Installing WiX 6..." -ForegroundColor Yellow
    dotnet tool install --global wix --version 6.0.2
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Failed to install WiX 6"
        exit 1
    }
    Write-Host "✅ WiX 6 installed successfully" -ForegroundColor Green
}

# Verify WiX 6 installation
$wixVersion = wix --version
Write-Host "Using WiX version: $wixVersion" -ForegroundColor Green

# Build the WiX project with full automation
Write-Host "`n=== Building WiX 6 Project (Auto-Publish & Harvest) ===" -ForegroundColor Cyan
Write-Host "🔄 The WiX project will automatically:" -ForegroundColor Yellow
Write-Host "   • Publish the application" -ForegroundColor Gray
Write-Host "   • Harvest all files from publish directory" -ForegroundColor Gray
Write-Host "   • Extract variables from .csproj" -ForegroundColor Gray
Write-Host "   • Generate platform-specific MSI" -ForegroundColor Gray

dotnet build setup\Bucket.Setup\Bucket.Setup.wixproj `
    -c $Configuration `
    -p:Platform=$Platform `
    -p:Version=$Version `
    -p:RuntimeIdentifier=$rid `
    -p:ProgramFilesFolder=$programFilesFolder `
    --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ WiX 6 build completed successfully with full automation!" -ForegroundColor Green

    # Check for output file
    $expectedMsi = "artifacts\installers\Bucket-$Version-$Platform.msi"
    if (Test-Path $expectedMsi) {
        $fileInfo = Get-Item $expectedMsi
        $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        Write-Host "✅ MSI generated: $expectedMsi ($fileSizeMB MB)" -ForegroundColor Green

        # Show what was automatically harvested
        $publishPath = "src\Bucket.App\bin\$Platform\$Configuration\net9.0-windows10.0.26100\$rid\publish"
        if (Test-Path $publishPath) {
            $files = Get-ChildItem -Path $publishPath -Recurse -File
            Write-Host "📊 Auto-harvested $($files.Count) files from publish directory" -ForegroundColor Cyan

            # Show key files that were included
            $keyFiles = $files | Where-Object { $_.Extension -in @('.exe', '.dll', '.json') } | Select-Object -First 5
            Write-Host "🔸 Key files automatically included:" -ForegroundColor Gray
            $keyFiles | ForEach-Object {
                Write-Host "   - $($_.Name)" -ForegroundColor Gray
            }
            if ($files.Count -gt 5) {
                Write-Host "   ... and $($files.Count - 5) more files" -ForegroundColor Gray
            }
        }

        # Show platform-specific details
        Write-Host "🎯 Platform-specific configuration:" -ForegroundColor Cyan
        Write-Host "   - Architecture: $Platform" -ForegroundColor Gray
        Write-Host "   - Runtime ID: $rid" -ForegroundColor Gray
        Write-Host "   - Program Files: $programFilesFolder" -ForegroundColor Gray

    } else {
        Write-Warning "⚠️ Expected MSI not found: $expectedMsi"
        Write-Host "Files in output directory:" -ForegroundColor Yellow
        Get-ChildItem "artifacts\installers" -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Error "❌ WiX 6 build failed"

    # Show build logs if available
    $logPaths = @(
        "setup\Bucket.Setup\obj\$Platform\$Configuration",
        "setup\Bucket.Setup\bin\$Platform\$Configuration"
    )

    foreach ($logPath in $logPaths) {
        if (Test-Path $logPath) {
            Write-Host "Build logs in $logPath :" -ForegroundColor Yellow
            Get-ChildItem $logPath -Recurse -Filter "*.log" -ErrorAction SilentlyContinue | ForEach-Object {
                Write-Host "=== $($_.Name) ===" -ForegroundColor Yellow
                Get-Content $_.FullName -Tail 15 -ErrorAction SilentlyContinue
            }
        }
    }

    exit 1
}

Write-Host "`n=== Test Completed Successfully ===" -ForegroundColor Green
