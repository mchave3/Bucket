# Test script to validate the harvest generation workflow
Write-Host "=== Test of Harvest Generation Workflow ===" -ForegroundColor Cyan

# Configuration
$configuration = "Release"
$version = "25.1.15.1"
$platforms = @(
    @{ Name = "x64"; RID = "win-x64" }
)

# Step 1: Publish the application
Write-Host "`n🚀 Step 1: Publishing the application" -ForegroundColor Green
foreach ($platform in $platforms) {
    Write-Host "Publishing for $($platform.Name)..." -ForegroundColor Yellow

    $publishArgs = @(
        "publish",
        "src/Bucket.App/Bucket.App.csproj",
        "-c", $configuration,
        "-r", "$($platform.RID)",
        "-p:Platform=$($platform.Name)",
        "-p:Version=$version",
        "--self-contained", "true",
        "--verbosity", "minimal"
    )

    dotnet @publishArgs

    if ($LASTEXITCODE -eq 0) {
        $publishPath = "src/Bucket.App/bin/$($platform.Name)/$configuration/net9.0-windows10.0.26100/$($platform.RID)/publish"
        if (Test-Path $publishPath) {
            $files = Get-ChildItem -Path $publishPath -Recurse -File
            Write-Host "✅ Published $($files.Count) files for $($platform.Name)" -ForegroundColor Green
        } else {
            Write-Error "❌ Publish directory not found: $publishPath"
            exit 1
        }
    } else {
        Write-Error "❌ Failed to publish for $($platform.Name)"
        exit 1
    }
}

# Step 2: Generate the harvest file
Write-Host "`n🔄 Step 2: Generating AutoHarvestFiles.wxs file" -ForegroundColor Green
Push-Location "setup/Bucket.Setup"
try {
    # Use the same platform as in Step 1
    $platform = $platforms[0]  # Use first platform (x64)
    $publishPath = "..\..\src\Bucket.App\bin\$($platform.Name)\$configuration\net9.0-windows10.0.26100\$($platform.RID)\publish"
    Write-Host "Looking for publish directory: $publishPath" -ForegroundColor Gray
    .\Generate-AutoHarvestFiles.ps1 -PublishPath $publishPath

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ AutoHarvestFiles.wxs generated successfully" -ForegroundColor Green

        # Show statistics
        if (Test-Path "AutoHarvestFiles.wxs") {
            $content = Get-Content "AutoHarvestFiles.wxs" -Raw
            $componentCount = ($content | Select-String '<Component ' -AllMatches).Matches.Count
            $fileCount = ($content | Select-String '<File ' -AllMatches).Matches.Count
            Write-Host "   📊 Generated $componentCount components with $fileCount files" -ForegroundColor Cyan
        }
    } else {
        Write-Error "❌ Failed to generate AutoHarvestFiles.wxs"
        exit 1
    }
} finally {
    Pop-Location
}

# Step 3: Build the MSI
Write-Host "`n🏗️ Step 3: Building the MSI" -ForegroundColor Green
$buildArgs = @(
    "build",
    "setup/Bucket.Setup/Bucket.Setup.wixproj",
    "-c", $configuration,
    "-p:Platform=x64",
    "-p:Version=$version",
    "-p:RuntimeIdentifier=win-x64",
    "-p:ProgramFilesFolder=ProgramFiles64Folder",
    "--verbosity", "normal"
)

Write-Host "Build command: dotnet $($buildArgs -join ' ')" -ForegroundColor Gray
dotnet @buildArgs

if ($LASTEXITCODE -eq 0) {
    $expectedMsi = "artifacts/installers/Bucket-$version-x64.msi"

    if (Test-Path $expectedMsi) {
        $fileInfo = Get-Item $expectedMsi
        $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        Write-Host "✅ Successfully built MSI: $fileSizeMB MB" -ForegroundColor Green
        Write-Host "📦 MSI Location: $expectedMsi" -ForegroundColor Cyan
    } else {
        Write-Error "❌ Expected MSI file not found: $expectedMsi"
        exit 1
    }
} else {
    Write-Error "❌ Failed to build MSI"
    exit 1
}

Write-Host "`n🎉 Workflow test completed successfully!" -ForegroundColor Green
