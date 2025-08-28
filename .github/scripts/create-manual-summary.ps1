param(
    [string]$Packages = "",
    [string]$TargetVersions = "",
    [string]$OutputFile = "manual-updates-summary.md"
)

Write-Host "Création du résumé de mise à jour manuelle..."

$summaryContent = @"
# 🔧 Mise à jour manuelle des packages NuGet

Cette mise à jour a été déclenchée manuellement le $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') UTC.

## Packages ciblés
"@

if ([string]::IsNullOrWhiteSpace($Packages)) {
    $summaryContent += "`n- Tous les packages"
} else {
    $summaryContent += "`n- $Packages"
}

if (![string]::IsNullOrWhiteSpace($TargetVersions)) {
    $summaryContent += "`n`n## Versions cibles`n- $TargetVersions"
}

$summaryContent += @"

## Actions effectuées

1. ✅ Mise à jour des packages demandés
2. ✅ Build de la solution
3. ✅ Exécution des tests
4. ✅ Création de cette Pull Request

Cette PR a été créée suite à une demande manuelle et peut être mergée si tous les checks passent au vert.
"@

$summaryContent | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "Fichier de résumé créé: $OutputFile"
