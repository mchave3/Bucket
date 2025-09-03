# Script to automatically generate AutoHarvestFiles.wxs with all published files
param(
    [string]$PublishPath = "..\..\src\Bucket.App\bin\x64\Release\net9.0-windows10.0.26100\win-x64\publish",
    [string]$OutputFile = "AutoHarvestFiles.wxs"
)

Write-Host "🚀 Automatic generation of $OutputFile" -ForegroundColor Green
Write-Host "📁 Source directory: $PublishPath" -ForegroundColor Cyan

# Resolve absolute path
$PublishPathResolved = Resolve-Path $PublishPath
Write-Host "📁 Resolved path: $PublishPathResolved" -ForegroundColor Gray

# Check that directory exists
if (-not (Test-Path $PublishPathResolved)) {
    Write-Error "❌ Publish directory not found: $PublishPathResolved"
    exit 1
}

# Collect all files (including localization files)
try {
    $files = Get-ChildItem -Path $PublishPathResolved -Recurse -File
    Write-Host "📊 Number of files found: $($files.Count)" -ForegroundColor Yellow

    if ($files.Count -eq 0) {
        Write-Error "❌ No files found in publish directory"
        exit 1
    }
}
catch {
    Write-Error "❌ Error enumerating files: $_"
    exit 1
}

# Analyze all necessary directories
$directories = @{}
foreach ($file in $files) {
    $relativeDir = [System.IO.Path]::GetDirectoryName($file.FullName.Replace($PublishPathResolved.Path, "").TrimStart('\'))
    if ($relativeDir -and $relativeDir -ne "") {
        $directories[$relativeDir] = $true
    }
}

# Beginning of WiX file with directory structure
$wixContent = @"
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <?include Variables.wxi ?>
  <Fragment>
    <!-- Directory Structure -->
    <DirectoryRef Id="INSTALLFOLDER">
"@

# Create directory structure hierarchically
$dirIds = @{"" = "INSTALLFOLDER"}  # Root directory
$sortedDirs = $directories.Keys | Sort-Object

# First pass: create all directory IDs
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

# Second pass: create all directories simply
# First create all necessary parent directories
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

# Sort by depth (number of \ in path) filtering null values
$allPaths = $allPaths | Where-Object { $_ -ne $null -and $_ -ne "" -and $_.Trim() -ne "" } | Sort-Object @{ Expression = { $_.Split('\').Length } }, @{ Expression = { $_ } }

# Create all directories with their hierarchical structure
$openTags = @()
$currentDepth = 0

foreach ($path in $allPaths) {
    $pathParts = $path.Split('\')
    $depth = $pathParts.Length
    $dirName = $pathParts[-1]
    $dirId = "Dir_" + ($path -replace '[\\\/\-\.\s]', '_').Replace('___', '_').Replace('__', '_')
    $dirIds[$path] = $dirId

    Write-Host "📁 Processing: $path (depth=$depth, currentDepth=$currentDepth)" -ForegroundColor Cyan

    # Close tags if going up in the tree
    $loopCount = 0
    while ($currentDepth -ge $depth) {
        $loopCount++
        if ($loopCount -gt 100) {
            Write-Host "❌ ERROR: Infinite loop detected! currentDepth=$currentDepth, depth=$depth" -ForegroundColor Red
            Write-Host "   Path: $path" -ForegroundColor Red
            Write-Host "   OpenTags: $($openTags -join ', ')" -ForegroundColor Red
            break
        }

        Write-Host "  🔽 Closing level $currentDepth" -ForegroundColor Yellow
        $wixContent += ("  " * ($currentDepth + 1)) + "</Directory>`n"
        if ($openTags.Length -gt 0) {
            $openTags = $openTags[0..($openTags.Length-2)]
        }
        $currentDepth--
    }

    # Open the new directory
    Write-Host "  🔼 Opening level $depth : $dirName" -ForegroundColor Green
    $indent = "  " * ($depth + 1)
    $wixContent += "$indent<Directory Id=`"$dirId`" Name=`"$dirName`">`n"
    $openTags += $dirId
    $currentDepth = $depth
}

# Close all remaining tags
Write-Host "🔚 Final closing: $($openTags.Length) open tags, currentDepth=$currentDepth" -ForegroundColor Magenta
while ($openTags.Length -gt 0 -and $currentDepth -gt 0) {
    Write-Host "  🔽 Final closing level $currentDepth" -ForegroundColor Yellow
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

# Generate a unique GUID for each component
function New-Guid {
    [System.Guid]::NewGuid().ToString().ToUpper()
}

# Create ComponentGroups for each directory
$componentsByDir = @{}
foreach ($file in $files) {
    $relativeFilePath = $file.FullName.Replace($PublishPathResolved.Path, "").TrimStart('\')
    $relativeDir = [System.IO.Path]::GetDirectoryName($relativeFilePath)

    if (-not $componentsByDir.ContainsKey($relativeDir)) {
        $componentsByDir[$relativeDir] = @()
    }
    $componentsByDir[$relativeDir] += $file
}

# Generate components for each directory
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

        # Create a valid component name (limited to 72 characters)
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

# Create the main ComponentGroup that references all others
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

# Write the file
try {
    Set-Content -Path $OutputFile -Value $wixContent -Encoding UTF8
    Write-Host "✅ File generated: $OutputFile" -ForegroundColor Green
    Write-Host "📊 Components created: $($componentIndex - 1)" -ForegroundColor Cyan

    # Check the size of the generated file
    $fileInfo = Get-Item $OutputFile
    Write-Host "📄 File size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray

    # Explicitly set success exit code
    exit 0
}
catch {
    Write-Error "❌ Error generating file: $_"
    exit 1
}