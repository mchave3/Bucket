# Script de Build Automatisé - Publish + WiX Setup
# Ce script démontre l'approche 100% automatisée

param(
    [string]$Platform = "x64",
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0.0"
)

Write-Host "=== BUILD AUTOMATISÉ BUCKET SETUP ===" -ForegroundColor Green
Write-Host "Platform: $Platform" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Cyan

# Étape 1: Publish de l'application
Write-Host "`n🚀 Étape 1: Publish de l'application..." -ForegroundColor Yellow

$publishPath = "..\..\src\Bucket.App"
$publishCommand = "dotnet publish `"$publishPath\Bucket.App.csproj`" -c $Configuration -p Platform=$Platform --self-contained true"

Write-Host "Commande: $publishCommand" -ForegroundColor Gray
try {
    Invoke-Expression $publishCommand
    Write-Host "✅ Publish réussi!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur lors du publish: $_" -ForegroundColor Red
    exit 1
}

# Étape 2: Vérifier le contenu du publish
Write-Host "`n📁 Étape 2: Vérification du contenu publié..." -ForegroundColor Yellow

$targetDir = "..\..\src\Bucket.App\bin\$Configuration\net6.0-windows10.0.17763.0\$Platform\publish\"
if (Test-Path $targetDir) {
    $files = Get-ChildItem $targetDir -Recurse | Where-Object { -not $_.PSIsContainer }
    Write-Host "✅ Répertoire de publish trouvé: $targetDir" -ForegroundColor Green
    Write-Host "📊 Nombre de fichiers détectés: $($files.Count)" -ForegroundColor Cyan

    # Afficher quelques exemples de fichiers
    Write-Host "📋 Exemples de fichiers qui seront automatiquement inclus:" -ForegroundColor Cyan
    $files | Select-Object -First 10 | ForEach-Object {
        Write-Host "   - $($_.Name)" -ForegroundColor Gray
    }
    if ($files.Count -gt 10) {
        Write-Host "   ... et $($files.Count - 10) autres fichiers" -ForegroundColor Gray
    }
} else {
    Write-Host "❌ Répertoire de publish non trouvé: $targetDir" -ForegroundColor Red
}

# Étape 3: Build du setup WiX
Write-Host "`n🔨 Étape 3: Build du setup WiX..." -ForegroundColor Yellow

$wixCommand = "dotnet build Bucket.Setup.wixproj -c $Configuration -p Platform=$Platform -p Version=$Version"
Write-Host "Commande: $wixCommand" -ForegroundColor Gray

try {
    Invoke-Expression $wixCommand
    Write-Host "✅ Build WiX réussi!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur lors du build WiX: $_" -ForegroundColor Red
    exit 1
}

# Étape 4: Vérifier le résultat
Write-Host "`n📦 Étape 4: Vérification de l'installeur..." -ForegroundColor Yellow

$installerPath = "..\..\artifacts\installers\Bucket-$Version-$Platform.msi"
if (Test-Path $installerPath) {
    $installerInfo = Get-Item $installerPath
    Write-Host "✅ Installeur créé avec succès!" -ForegroundColor Green
    Write-Host "📍 Emplacement: $installerPath" -ForegroundColor Cyan
    Write-Host "📏 Taille: $([math]::Round($installerInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    Write-Host "🕒 Créé le: $($installerInfo.CreationTime)" -ForegroundColor Cyan
} else {
    Write-Host "❌ Installeur non trouvé: $installerPath" -ForegroundColor Red
}

Write-Host "`n🎉 PROCESSUS TERMINÉ!" -ForegroundColor Green
Write-Host "✨ Avantages de cette approche automatisée:" -ForegroundColor Cyan
Write-Host "   • Aucun fichier à lister manuellement dans WiX" -ForegroundColor White
Write-Host "   • Nouvelles dépendances automatiquement détectées" -ForegroundColor White
Write-Host "   • Localisations automatiquement incluses" -ForegroundColor White
Write-Host "   • Runtime .NET automatiquement embarqué" -ForegroundColor White
Write-Host "   • Maintenance minimale du setup" -ForegroundColor White
