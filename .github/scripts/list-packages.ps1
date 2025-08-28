# Script pour lister toutes les versions actuelles des packages NuGet
Write-Host "📦 Inventaire des packages NuGet du projet Bucket" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Fonction pour extraire les packages d'un fichier csproj
function Get-PackagesFromProject {
    param([string]$ProjectPath)

    if (Test-Path $ProjectPath) {
        Write-Host "🔍 Analyse de: $ProjectPath" -ForegroundColor Yellow
        [xml]$project = Get-Content $ProjectPath

        $packages = $project.Project.ItemGroup.PackageReference
        if ($packages) {
            foreach ($package in $packages) {
                if ($package.Include -and $package.Version) {
                    Write-Host "  ├─ $($package.Include): $($package.Version)" -ForegroundColor Green
                }
            }
        } else {
            Write-Host "  └─ Aucun package NuGet trouvé" -ForegroundColor Gray
        }
        Write-Host ""
    }
}

# Analyser tous les projets
$projects = @(
    "src\Bucket.App\Bucket.App.csproj",
    "src\Bucket.Core\Bucket.Core.csproj",
    "tests\Bucket.App.Tests\Bucket.App.Tests.csproj",
    "tests\Bucket.Core.Tests\Bucket.Core.Tests.csproj"
)

foreach ($project in $projects) {
    Get-PackagesFromProject $project
}

# Vérifier les packages obsolètes si dotnet-outdated est installé
Write-Host "🔄 Vérification des packages obsolètes..." -ForegroundColor Cyan
try {
    $outdatedCheck = dotnet outdated --output json 2>&1
    if ($LASTEXITCODE -eq 0) {
        $json = $outdatedCheck | ConvertFrom-Json
        $hasOutdated = $false

        foreach ($project in $json.Projects) {
            if ($project.TargetFrameworks) {
                foreach ($framework in $project.TargetFrameworks) {
                    if ($framework.Dependencies -and $framework.Dependencies.Count -gt 0) {
                        if (-not $hasOutdated) {
                            Write-Host "⚠️  Packages obsolètes détectés:" -ForegroundColor Red
                            $hasOutdated = $true
                        }
                        foreach ($dep in $framework.Dependencies) {
                            Write-Host "  ├─ $($dep.Name): $($dep.ResolvedVersion) → $($dep.LatestVersion)" -ForegroundColor Red
                        }
                    }
                }
            }
        }

        if (-not $hasOutdated) {
            Write-Host "✅ Tous les packages sont à jour !" -ForegroundColor Green
        }
    } else {
        Write-Host "ℹ️  dotnet-outdated-tool n'est pas installé" -ForegroundColor Yellow
        Write-Host "   Installez-le avec: dotnet tool install --global dotnet-outdated-tool" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Erreur lors de la vérification: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "📊 Résumé généré le $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
