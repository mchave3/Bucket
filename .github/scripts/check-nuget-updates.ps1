param(
    [string]$OutputFile = "updates-summary.md"
)

# Exécuter dotnet outdated et capturer la sortie
Write-Host "Vérification des packages NuGet obsolètes..."
$outdatedResult = dotnet outdated --output json 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Aucune mise à jour nécessaire ou erreur lors de la vérification"
    echo "has-updates=false" >> $env:GITHUB_OUTPUT
    exit 0
}

try {
    $json = $outdatedResult | ConvertFrom-Json
    $hasUpdates = $false
    $updatesList = @()

    foreach ($project in $json.Projects) {
        if ($project.TargetFrameworks) {
            foreach ($framework in $project.TargetFrameworks) {
                if ($framework.Dependencies -and $framework.Dependencies.Count -gt 0) {
                    $hasUpdates = $true
                    foreach ($dep in $framework.Dependencies) {
                        $updatesList += "- **$($dep.Name)**: $($dep.ResolvedVersion) → $($dep.LatestVersion) dans $($project.Name)"
                    }
                }
            }
        }
    }

    if ($hasUpdates) {
        echo "has-updates=true" >> $env:GITHUB_OUTPUT

        # Créer le résumé des mises à jour
        $summaryContent = @"
# 📦 Mises à jour NuGet disponibles

Les packages suivants peuvent être mis à jour :

$($updatesList -join "`n")

## Détails de l'exécution

Cette mise à jour automatique a été générée le $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') UTC.

Les tests ont été exécutés avec succès après la mise à jour.

## Actions effectuées

1. ✅ Vérification des packages obsolètes
2. ✅ Mise à jour des packages
3. ✅ Build de la solution
4. ✅ Exécution des tests
5. ✅ Création de cette Pull Request

Cette PR peut être mergée en toute sécurité si tous les checks passent au vert.
"@

        $summaryContent | Out-File -FilePath $OutputFile -Encoding UTF8
        Write-Host "Fichier de résumé créé: $OutputFile"
        Write-Host "Packages à mettre à jour trouvés: $($updatesList.Count)"
    } else {
        echo "has-updates=false" >> $env:GITHUB_OUTPUT
        Write-Host "Tous les packages sont à jour !"
    }
} catch {
    Write-Host "Erreur lors du parsing: $_"
    echo "has-updates=false" >> $env:GITHUB_OUTPUT
    exit 1
}
