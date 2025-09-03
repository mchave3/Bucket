# Script de test pour valider le workflow de génération de harvest
Write-Host "=== Test du Workflow de Génération de Harvest ===" -ForegroundColor Cyan

# Configuration
$configuration = "Release"
$version = "25.1.15.1"
$platforms = @(
    @{ Name = "x64"; RID = "win-x64" }
)

# Étape 1: Publier l'application
Write-Host "`n🚀 Étape 1: Publication de l'application" -ForegroundColor Green
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

# Étape 2: Générer le fichier harvest
Write-Host "`n🔄 Étape 2: Génération du fichier AutoHarvestFiles.wxs" -ForegroundColor Green
Push-Location "setup/Bucket.Setup"
try {
    $publishPath = "..\..\src\Bucket.App\bin\x64\$configuration\net9.0-windows10.0.26100\win-x64\publish"
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

# Étape 3: Construire le MSI
Write-Host "`n🏗️ Étape 3: Construction du MSI" -ForegroundColor Green
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

Write-Host "`n🎉 Test du workflow terminé avec succès!" -ForegroundColor Green
