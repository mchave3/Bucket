# Determine root path with fallback mechanism
$rootPath = if ($PSScriptRoot) {
    Split-Path $PSScriptRoot -Parent
} else {
    # Fallback to current directory's parent when running directly in console
    Split-Path (Get-Location) -Parent
}

$functionPath = Join-Path $rootPath "src"

# Verify path exists
if (-not (Test-Path $functionPath)) {
    Write-Error "Functions path not found: $functionPath"
    exit 1
}

# Get all .ps1 files
$files = Get-ChildItem -Path $functionPath -Filter "*.ps1" -Recurse
$totalFiles = $files.Count

if ($totalFiles -eq 0) {
    Write-Warning "No .ps1 files found in $functionPath"
    exit 0
}

$counter = 0

Write-Host "Starting conversion of $totalFiles .ps1 files to UTF-8 BOM..." -ForegroundColor Cyan

foreach ($file in $files) {
    $counter++
    $percentage = ($counter / $totalFiles) * 100

    # Update progress bar
    Write-Progress -Activity "Converting files to UTF-8 BOM" `
                   -Status "Processing: $($file.Name) ($counter / $totalFiles)" `
                   -PercentComplete $percentage

    # Read file content as bytes
    $content = Get-Content $file.FullName -Raw

    # Convert to bytes with UTF-8 BOM encoding without adding extra newlines
    $encoding = New-Object System.Text.UTF8Encoding $true
    $bytes = $encoding.GetBytes($content)

    # Write bytes directly to file to preserve exact content
    [System.IO.File]::WriteAllBytes($file.FullName, $bytes)

    # Log progress
    Write-Host "[$counter/$totalFiles] Converted: $($file.FullName)" -ForegroundColor Green
}

# End of conversion
Write-Progress -Activity "Conversion completed" -Completed
Write-Host "All files have been successfully converted to UTF-8 BOM!" -ForegroundColor Cyan