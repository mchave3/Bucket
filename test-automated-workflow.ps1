# Test du Workflow Automatisé Complet
# Ce script teste l'approche 100% automatisée : Publish → Harvest → MSI

param(
    [string]$Version = "1.0.0.0",
    [string]$Platform = "x64",
    [string]$Configuration = "Release"
)

Write-Host "=== TEST WORKFLOW AUTOMATISÉ BUCKET ===" -ForegroundColor Green
Write-Host "🎯 Approche 100% automatisée : Publish → Harvest → MSI" -ForegroundColor Cyan
Write-Host ""
Write-Host "Paramètres:" -ForegroundColor Yellow
Write-Host "  Version: $Version" -ForegroundColor White
Write-Host "  Platform: $Platform" -ForegroundColor White
Write-Host "  Configuration: $Configuration" -ForegroundColor White

# Mapping Platform → RID
$ridMap = @{
    "x64" = "win-x64"
    "x86" = "win-x86"
    "ARM64" = "win-arm64"
}
$rid = $ridMap[$Platform]

Write-Host "`n🚀 ÉTAPE 1: Publish de l'application" -ForegroundColor Cyan
Write-Host "RID: $rid" -ForegroundColor Gray

# Publish avec tous les runtimes inclus
$publishArgs = @(
    "publish", "src\Bucket.App\Bucket.App.csproj"
    "-c", $Configuration
    "-r", $rid
    "-p:Platform=$Platform"
    "-p:Version=$Version"
    "-p:PublishSingleFile=false"
    "-p:SelfContained=true"
    "-p:PublishReadyToRun=false"
    "-p:PublishTrimmed=false"
    "--verbosity", "minimal"
)

Write-Host "Commande: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray
dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Échec du publish" -ForegroundColor Red
    exit 1
}

# Vérifier le répertoire de publish
$publishPath = "src\Bucket.App\bin\$Platform\$Configuration\net9.0-windows10.0.26100\$rid\publish"
if (-not (Test-Path $publishPath)) {
    # Essayer le chemin alternatif
    $publishPath = "src\Bucket.App\bin\$Platform\$Configuration\net9.0-windows10.0.26100\$rid"
}

if (Test-Path $publishPath) {
    $files = Get-ChildItem -Path $publishPath -Recurse -File
    Write-Host "✅ Publish réussi!" -ForegroundColor Green
    Write-Host "📁 Répertoire: $publishPath" -ForegroundColor Cyan
    Write-Host "📊 Fichiers générés: $($files.Count)" -ForegroundColor Cyan

    # Analyse des types de fichiers
    $fileTypes = $files | Group-Object Extension | Sort-Object Count -Descending
    Write-Host "📋 Types de fichiers qui seront automatiquement harvestés:" -ForegroundColor Yellow
    $fileTypes | Select-Object -First 8 | ForEach-Object {
        $ext = if ($_.Name) { $_.Name } else { "(sans extension)" }
        Write-Host "   $ext : $($_.Count) fichier(s)" -ForegroundColor Gray
    }

    # Exemples de fichiers critiques
    Write-Host "🔍 Fichiers critiques détectés:" -ForegroundColor Yellow
    $criticalFiles = @("*.exe", "*.dll", "*.json", "*.config")
    foreach ($pattern in $criticalFiles) {
        $matching = $files | Where-Object { $_.Name -like $pattern } | Select-Object -First 3
        if ($matching) {
            Write-Host "   $pattern :" -ForegroundColor Gray
            $matching | ForEach-Object { Write-Host "     - $($_.Name)" -ForegroundColor DarkGray }
        }
    }
} else {
    Write-Host "❌ Répertoire de publish non trouvé: $publishPath" -ForegroundColor Red
    exit 1
}

Write-Host "`n🔨 ÉTAPE 2: Build du setup WiX avec harvest automatique" -ForegroundColor Cyan

# Build WiX avec les nouvelles variables
$wixArgs = @(
    "build", "setup\Bucket.Setup\Bucket.Setup.wixproj"
    "-c", $Configuration
    "-p:Platform=$Platform"
    "-p:Version=$Version"
    "--verbosity", "normal"
)

Write-Host "Commande: dotnet $($wixArgs -join ' ')" -ForegroundColor Gray
dotnet @wixArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build WiX réussi!" -ForegroundColor Green
} else {
    Write-Host "❌ Échec du build WiX" -ForegroundColor Red

    # Afficher les logs d'erreur
    $logPath = "setup\Bucket.Setup\obj\$Platform\$Configuration"
    if (Test-Path $logPath) {
        Write-Host "📋 Logs de build:" -ForegroundColor Yellow
        Get-ChildItem $logPath -Recurse -Filter "*.log" -ErrorAction SilentlyContinue |
            Select-Object -First 2 | ForEach-Object {
            Write-Host "=== $($_.Name) ===" -ForegroundColor Yellow
            Get-Content $_.FullName -Tail 15 | ForEach-Object {
                Write-Host "  $_" -ForegroundColor Gray
            }
        }
    }
    exit 1
}

Write-Host "`n📦 ÉTAPE 3: Vérification du MSI généré" -ForegroundColor Cyan

$expectedMsi = "artifacts\installers\Bucket-$Version-$Platform.msi"
if (Test-Path $expectedMsi) {
    $msiInfo = Get-Item $expectedMsi
    $sizeMB = [math]::Round($msiInfo.Length / 1MB, 2)

    Write-Host "✅ MSI généré avec succès!" -ForegroundColor Green
    Write-Host "📍 Fichier: $expectedMsi" -ForegroundColor Cyan
    Write-Host "📏 Taille: $sizeMB MB" -ForegroundColor Cyan
    Write-Host "🕒 Créé: $($msiInfo.CreationTime)" -ForegroundColor Cyan

    # Comparaison avec la taille du publish
    if (Test-Path $publishPath) {
        $publishSize = (Get-ChildItem $publishPath -Recurse -File | Measure-Object Length -Sum).Sum / 1MB
        $compressionRatio = [math]::Round((1 - ($sizeMB / $publishSize)) * 100, 1)
        Write-Host "📊 Compression: $([math]::Round($publishSize, 2)) MB → $sizeMB MB ($compressionRatio% de réduction)" -ForegroundColor Cyan
    }
} else {
    Write-Host "❌ MSI non trouvé: $expectedMsi" -ForegroundColor Red

    # Lister ce qui existe dans le répertoire
    $installersDir = "artifacts\installers"
    if (Test-Path $installersDir) {
        Write-Host "📁 Contenu du répertoire installers:" -ForegroundColor Yellow
        Get-ChildItem $installersDir | ForEach-Object {
            Write-Host "   - $($_.Name)" -ForegroundColor Gray
        }
    }
    exit 1
}

Write-Host "`n🎉 TEST TERMINÉ AVEC SUCCÈS!" -ForegroundColor Green
Write-Host ""
Write-Host "✨ Résumé de l'automatisation:" -ForegroundColor Cyan
Write-Host "  • Publish: $($files.Count) fichiers collectés automatiquement" -ForegroundColor White
Write-Host "  • Harvest: Aucun fichier à lister manuellement dans WiX" -ForegroundColor White
Write-Host "  • MSI: Généré automatiquement ($sizeMB MB)" -ForegroundColor White
Write-Host "  • Maintenance: Zéro intervention manuelle requise" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Votre setup est maintenant 100% automatisé!" -ForegroundColor Green
