# Test Build Script for WiX Setup
# This script tests the WiX setup generation locally

param(
    [string]$Version = "1.0.0.0",
    [string]$Platform = "x64",
    [string]$Configuration = "Release"
)

Write-Host "=== Testing WiX Setup Build ===" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Create output directory
$outputDir = "artifacts\installers"
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
Write-Host "Created output directory: $outputDir" -ForegroundColor Green

# First, publish the application
Write-Host "`n=== Publishing Application ===" -ForegroundColor Cyan
$ridMap = @{
    "x64" = "win-x64"
    "x86" = "win-x86"
    "ARM64" = "win-arm64"
}

$rid = $ridMap[$Platform]
Write-Host "Publishing for RID: $rid" -ForegroundColor Yellow

dotnet publish src\Bucket.App\Bucket.App.csproj `
    -c $Configuration `
    -r $rid `
    -p:Platform=$Platform `
    -p:Version=$Version `
    -p:PublishSingleFile=false `
    -p:SelfContained=true `
    --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Failed to publish application"
    exit 1
}

$publishPath = "src\Bucket.App\bin\$Platform\$Configuration\net9.0-windows10.0.26100\$rid\publish"
if (Test-Path $publishPath) {
    $fileCount = (Get-ChildItem -Path $publishPath -Recurse -File).Count
    Write-Host "✅ Published successfully: $fileCount files in $publishPath" -ForegroundColor Green
} else {
    Write-Error "❌ Publish directory not found: $publishPath"
    exit 1
}

# Install WiX if not already installed
Write-Host "`n=== Checking WiX Installation ===" -ForegroundColor Cyan
try {
    wix --version | Out-Null
    Write-Host "✅ WiX is already installed" -ForegroundColor Green
} catch {
    Write-Host "Installing WiX Toolset..." -ForegroundColor Yellow
    dotnet tool install --global wix --version 5.0.1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Failed to install WiX"
        exit 1
    }
    Write-Host "✅ WiX installed successfully" -ForegroundColor Green
}

# Build the WiX project
Write-Host "`n=== Building WiX Project ===" -ForegroundColor Cyan
dotnet build setup\Bucket.Setup\Bucket.Setup.wixproj `
    -c $Configuration `
    -p:Platform=$Platform `
    -p:Version=$Version `
    --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ WiX build completed successfully" -ForegroundColor Green

    # Check for output file
    $expectedMsi = "artifacts\installers\Bucket-$Version-$Platform.msi"
    if (Test-Path $expectedMsi) {
        $fileInfo = Get-Item $expectedMsi
        $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        Write-Host "✅ MSI generated: $expectedMsi ($fileSizeMB MB)" -ForegroundColor Green
    } else {
        Write-Warning "⚠️ Expected MSI not found: $expectedMsi"
        Write-Host "Files in output directory:" -ForegroundColor Yellow
        Get-ChildItem "artifacts\installers" -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Error "❌ WiX build failed"

    # Show build logs if available
    $logPath = "setup\Bucket.Setup\obj\$Platform\$Configuration"
    if (Test-Path $logPath) {
        Write-Host "Build logs:" -ForegroundColor Yellow
        Get-ChildItem $logPath -Recurse -Filter "*.log" | ForEach-Object {
            Write-Host "=== $($_.Name) ===" -ForegroundColor Yellow
            Get-Content $_.FullName -Tail 20
        }
    }

    exit 1
}

Write-Host "`n=== Test Completed Successfully ===" -ForegroundColor Green
