# Script pour générer automatiquement AutoHarvestFiles.wxs avec tous les fichiers publiés
param(
    [string]$PublishPath = "..\..\src\Bucket.App\bin\x64\Release\net9.0-windows10.0.26100\win-x64\publish",
    [string]$OutputFile = "AutoHarvestFiles.wxs"
)

Write-Host "🚀 Génération automatique de $OutputFile" -ForegroundColor Green
Write-Host "📁 Répertoire source: $PublishPath" -ForegroundColor Cyan

# Résoudre le chemin absolu
$PublishPathResolved = Resolve-Path $PublishPath
Write-Host "📁 Chemin résolu: $PublishPathResolved" -ForegroundColor Gray

# Vérifier que le répertoire existe
if (-not (Test-Path $PublishPathResolved)) {
    Write-Error "❌ Répertoire de publication non trouvé: $PublishPathResolved"
    exit 1
}

# Collecter tous les fichiers (y compris les fichiers de localisation)
$files = Get-ChildItem -Path $PublishPathResolved -Recurse -File
Write-Host "📊 Nombre de fichiers trouvés: $($files.Count)" -ForegroundColor Yellow

# Analyser tous les répertoires nécessaires
$directories = @{}
foreach ($file in $files) {
    $relativeDir = [System.IO.Path]::GetDirectoryName($file.FullName.Replace($PublishPathResolved.Path, "").TrimStart('\'))
    if ($relativeDir -and $relativeDir -ne "") {
        $directories[$relativeDir] = $true
    }
}

# Début du fichier WiX avec structure de répertoires
$wixContent = @"
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <?include Variables.wxi ?>
  <Fragment>
    <!-- Directory Structure -->
    <DirectoryRef Id="INSTALLFOLDER">
"@

# Créer la structure de répertoires de manière hiérarchique
$dirIds = @{"" = "INSTALLFOLDER"}  # Répertoire racine
$sortedDirs = $directories.Keys | Sort-Object

# Première passe : créer tous les IDs de répertoires
foreach ($dir in $sortedDirs) {
    $pathParts = $dir.Split('\')
    $currentPath = ""

    for ($i = 0; $i -lt $pathParts.Length; $i++) {
        if ($i -eq 0) {
            $currentPath = $pathParts[$i]
        } else {
            $currentPath = $currentPath + "\" + $pathParts[$i]
        }

        if (-not $dirIds.ContainsKey($currentPath)) {
            $dirId = "Dir_" + ($currentPath -replace '[\\\/\-\.\s]', '_').Replace('___', '_').Replace('__', '_')
            $dirIds[$currentPath] = $dirId
        }
    }
}

# Deuxième passe : créer tous les répertoires de manière simple
# Créer d'abord tous les répertoires parents nécessaires
$allPaths = @()
foreach ($dir in $sortedDirs) {
    $pathParts = $dir.Split('\')
    $currentPath = ""
    for ($i = 0; $i -lt $pathParts.Length; $i++) {
        if ($i -eq 0) {
            $currentPath = $pathParts[$i]
        } else {
            $currentPath = $currentPath + "\" + $pathParts[$i]
        }
        if ($allPaths -notcontains $currentPath) {
            $allPaths += $currentPath
        }
    }
}

# Trier par profondeur (nombre de \ dans le chemin) en filtrant les valeurs nulles
$allPaths = $allPaths | Where-Object { $_ -ne $null -and $_ -ne "" } | Sort-Object { $_.Split('\').Length }, $_

# Créer tous les répertoires avec leur structure hiérarchique
$openTags = @()
$currentDepth = 0

foreach ($path in $allPaths) {
    $pathParts = $path.Split('\')
    $depth = $pathParts.Length
    $dirName = $pathParts[-1]
    $dirId = "Dir_" + ($path -replace '[\\\/\-\.\s]', '_').Replace('___', '_').Replace('__', '_')
    $dirIds[$path] = $dirId

    Write-Host "📁 Traitement: $path (depth=$depth, currentDepth=$currentDepth)" -ForegroundColor Cyan

    # Fermer les tags si on remonte dans l'arborescence
    $loopCount = 0
    while ($currentDepth -ge $depth) {
        $loopCount++
        if ($loopCount -gt 100) {
            Write-Host "❌ ERREUR: Boucle infinie détectée! currentDepth=$currentDepth, depth=$depth" -ForegroundColor Red
            Write-Host "   Path: $path" -ForegroundColor Red
            Write-Host "   OpenTags: $($openTags -join ', ')" -ForegroundColor Red
            break
        }

        Write-Host "  🔽 Fermeture niveau $currentDepth" -ForegroundColor Yellow
        $wixContent += ("  " * ($currentDepth + 1)) + "</Directory>`n"
        if ($openTags.Length -gt 0) {
            $openTags = $openTags[0..($openTags.Length-2)]
        }
        $currentDepth--
    }

    # Ouvrir le nouveau répertoire
    Write-Host "  🔼 Ouverture niveau $depth : $dirName" -ForegroundColor Green
    $indent = "  " * ($depth + 1)
    $wixContent += "$indent<Directory Id=`"$dirId`" Name=`"$dirName`">`n"
    $openTags += $dirId
    $currentDepth = $depth
}

# Fermer tous les tags restants
Write-Host "🔚 Fermeture finale: $($openTags.Length) tags ouverts, currentDepth=$currentDepth" -ForegroundColor Magenta
while ($openTags.Length -gt 0 -and $currentDepth -gt 0) {
    Write-Host "  🔽 Fermeture finale niveau $currentDepth" -ForegroundColor Yellow
    $indent = "  " * ($currentDepth + 1)
    $wixContent += "$indent</Directory>`n"
    if ($openTags.Length -gt 0) {
        $openTags = $openTags[0..($openTags.Length-2)]
    }
    $currentDepth--
}

$wixContent += @"
    </DirectoryRef>

    <!-- Auto-generated components by directory -->
"@

# Générer un GUID unique pour chaque composant
function New-Guid {
    [System.Guid]::NewGuid().ToString().ToUpper()
}

# Créer des ComponentGroups pour chaque répertoire
$componentsByDir = @{}
foreach ($file in $files) {
    $relativeFilePath = $file.FullName.Replace($PublishPathResolved.Path, "").TrimStart('\')
    $relativeDir = [System.IO.Path]::GetDirectoryName($relativeFilePath)

    if (-not $componentsByDir.ContainsKey($relativeDir)) {
        $componentsByDir[$relativeDir] = @()
    }
    $componentsByDir[$relativeDir] += $file
}

# Générer les composants pour chaque répertoire
$componentIndex = 1
$isFirstGroup = $true
foreach ($dir in $componentsByDir.Keys | Sort-Object) {
    $dirId = if ($dir -eq "") { "INSTALLFOLDER" } else { $dirIds[$dir] }

    if (-not $isFirstGroup) {
        $wixContent += "    </ComponentGroup>`n`n"
    }
    $isFirstGroup = $false

    $dirDisplayName = if ($dir -eq "") { "Root" } else { $dir }
    $groupId = "Files_" + ($dirDisplayName -replace '[\\\/\-\.\s]', '_').Replace('___', '_').Replace('__', '_') + "_Group"
    $wixContent += "    <!-- Components for directory: $dirDisplayName -->`n"
    $wixContent += "    <ComponentGroup Id=`"$groupId`" Directory=`"$dirId`">`n"

    foreach ($file in $componentsByDir[$dir]) {
        $relativeFilePath = $file.FullName.Replace($PublishPathResolved.Path, "").TrimStart('\')
        $fileName = $file.Name

        # Créer un nom de composant valide (limité à 72 caractères)
        $baseName = "File_" + ($fileName -replace '[\\\/\-\.\s]', '_').Replace('___', '_').Replace('__', '_')
        if ($baseName.Length -gt 50) {
            $baseName = $baseName.Substring(0, 50)
        }
        $componentName = "${baseName}_$componentIndex"
        $componentGuid = "{" + (New-Guid) + "}"

        $fileId = $componentName
        $sourcePath = "`$(var.PublishOutputPath)$relativeFilePath"

        $wixContent += "      <!-- File: $fileName -->`n"
        $wixContent += "      <Component Id=`"$componentName`" Guid=`"$componentGuid`">`n"
        $wixContent += "        <File Id=`"$fileId`" Source=`"$sourcePath`" KeyPath=`"yes`" />`n"
        $wixContent += "      </Component>`n`n"

        $componentIndex++
    }
}

# Créer le ComponentGroup principal qui référence tous les autres
$wixContent += "    </ComponentGroup>`n`n"
$wixContent += "    <!-- Main ComponentGroup that references all directory groups -->`n"
$wixContent += "    <ComponentGroup Id=`"AutoHarvestedFiles`">`n"

foreach ($dir in $componentsByDir.Keys | Sort-Object) {
    $dirDisplayName = if ($dir -eq "") { "Root" } else { $dir }
    $groupId = "Files_" + ($dirDisplayName -replace '[\\\/\-\.\s]', '_').Replace('___', '_').Replace('__', '_') + "_Group"
    $wixContent += "      <ComponentGroupRef Id=`"$groupId`" />`n"
}

$wixContent += @"
    </ComponentGroup>
  </Fragment>
</Wix>
"@

# Écrire le fichier
Set-Content -Path $OutputFile -Value $wixContent -Encoding UTF8
Write-Host "✅ Fichier généré: $OutputFile" -ForegroundColor Green
Write-Host "📊 Composants créés: $($componentIndex - 1)" -ForegroundColor Cyan

# Vérifier la taille du fichier généré
$fileInfo = Get-Item $OutputFile
Write-Host "📄 Taille du fichier: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
