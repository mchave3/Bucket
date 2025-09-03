# Script de test pour vérifier les variables WiX et les références de projet

Write-Host "=== Test des Variables WiX et Références de Projet ===" -ForegroundColor Green

# Vérifier que les projets existent
$bucketAppProject = "..\..\src\Bucket.App\Bucket.App.csproj"
$bucketCoreProject = "..\..\src\Bucket.Core\Bucket.Core.csproj"
$wixProject = ".\Bucket.Setup.wixproj"

Write-Host "Vérification des fichiers de projet..." -ForegroundColor Yellow

if (Test-Path $bucketAppProject) {
    Write-Host "✓ Bucket.App.csproj trouvé" -ForegroundColor Green
} else {
    Write-Host "✗ Bucket.App.csproj manquant" -ForegroundColor Red
}

if (Test-Path $bucketCoreProject) {
    Write-Host "✓ Bucket.Core.csproj trouvé" -ForegroundColor Green
} else {
    Write-Host "✗ Bucket.Core.csproj manquant" -ForegroundColor Red
}

if (Test-Path $wixProject) {
    Write-Host "✓ Bucket.Setup.wixproj trouvé" -ForegroundColor Green
} else {
    Write-Host "✗ Bucket.Setup.wixproj manquant" -ForegroundColor Red
}

# Vérifier les fichiers WiX
$wixFiles = @("Package.wxs", "Components.wxs", "HarvestContent.wxs")

Write-Host "`nVérification des fichiers WiX..." -ForegroundColor Yellow

foreach ($file in $wixFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file trouvé" -ForegroundColor Green
    } else {
        Write-Host "✗ $file manquant" -ForegroundColor Red
    }
}

# Vérifier le contenu des références de projet dans le .wixproj
Write-Host "`nVérification des références de projet..." -ForegroundColor Yellow

$wixContent = Get-Content $wixProject -Raw

if ($wixContent -match "Bucket\.App\.csproj") {
    Write-Host "✓ Référence à Bucket.App trouvée" -ForegroundColor Green
} else {
    Write-Host "✗ Référence à Bucket.App manquante" -ForegroundColor Red
}

if ($wixContent -match "Bucket\.Core\.csproj") {
    Write-Host "✓ Référence à Bucket.Core trouvée" -ForegroundColor Green
} else {
    Write-Host "✗ Référence à Bucket.Core manquante" -ForegroundColor Red
}

# Vérifier l'utilisation des variables dans les fichiers WiX
Write-Host "`nVérification de l'utilisation des variables WiX..." -ForegroundColor Yellow

$componentsContent = Get-Content "Components.wxs" -Raw

if ($componentsContent -match '\$\(var\.Bucket\.App\.TargetFileName\)') {
    Write-Host "✓ Variable TargetFileName utilisée" -ForegroundColor Green
} else {
    Write-Host "✗ Variable TargetFileName non utilisée" -ForegroundColor Red
}

if ($componentsContent -match '\$\(var\.Bucket\.App\.TargetPath\)') {
    Write-Host "✓ Variable TargetPath utilisée" -ForegroundColor Green
} else {
    Write-Host "✗ Variable TargetPath non utilisée" -ForegroundColor Red
}

# Vérifier HarvestFolder
Write-Host "`nVérification de HarvestFolder..." -ForegroundColor Yellow

$harvestContent = Get-Content "HarvestContent.wxs" -Raw

if ($harvestContent -match 'fg:HarvestFolder') {
    Write-Host "✓ HarvestFolder utilisé" -ForegroundColor Green
} else {
    Write-Host "✗ HarvestFolder non utilisé" -ForegroundColor Red
}

if ($harvestContent -match 'xmlns:fg="http://www\.firegiant\.com/schemas/v4/wxs/heatwave/buildtools"') {
    Write-Host "✓ Namespace HeatWave configuré" -ForegroundColor Green
} else {
    Write-Host "✗ Namespace HeatWave manquant" -ForegroundColor Red
}

Write-Host "`n=== Test Terminé ===" -ForegroundColor Green
Write-Host "Vous pouvez maintenant construire le projet avec:" -ForegroundColor Cyan
Write-Host "dotnet build Bucket.Setup.wixproj -c Release -p Platform=x64" -ForegroundColor White
