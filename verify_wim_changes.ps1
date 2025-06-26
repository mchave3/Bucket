# Script to verify WIM metadata and compare with images.json
param(
    [string]$WimPath = "C:\ProgramData\Bucket\ImportedWIMs\en-us_windows_11_business_editions_version_24h2_updated_june_2025_x64_dvd_4cf132a4_001.wim",
    [int]$IndexToCheck = 3
)

Write-Host "=== WIM File Metadata Verification ===" -ForegroundColor Cyan
Write-Host "WIM File: $WimPath" -ForegroundColor Yellow
Write-Host "Index to check: $IndexToCheck" -ForegroundColor Yellow
Write-Host ""

# Check if WIM file exists
if (-not (Test-Path $WimPath)) {
    Write-Host "ERROR: WIM file not found!" -ForegroundColor Red
    exit 1
}

# Get WIM metadata using Get-WindowsImage
try {
    Write-Host "Extracting WIM metadata..." -ForegroundColor Green
    $wimInfo = Get-WindowsImage -ImagePath $WimPath -Index $IndexToCheck

    Write-Host "=== WIM File Content (Index $IndexToCheck) ===" -ForegroundColor Cyan
    Write-Host "Index: $($wimInfo.ImageIndex)" -ForegroundColor White
    Write-Host "Name: '$($wimInfo.ImageName)'" -ForegroundColor White
    Write-Host "Description: '$($wimInfo.ImageDescription)'" -ForegroundColor White
    Write-Host "Architecture: $($wimInfo.Architecture)" -ForegroundColor White
    Write-Host "Size: $([math]::Round($wimInfo.ImageSize / 1MB, 1)) MB" -ForegroundColor White
    Write-Host ""

} catch {
    Write-Host "ERROR: Failed to read WIM metadata: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Compare with images.json
$jsonPath = "C:\ProgramData\Bucket\ImportedWIMs\images.json"
if (Test-Path $jsonPath) {
    Write-Host "=== Comparing with images.json ===" -ForegroundColor Cyan

    try {
        $jsonContent = Get-Content $jsonPath | ConvertFrom-Json
        $targetImage = $jsonContent | Where-Object { $_.FilePath -eq $WimPath }

        if ($targetImage) {
            $targetIndex = $targetImage.Indices | Where-Object { $_.Index -eq $IndexToCheck }

            if ($targetIndex) {
                Write-Host "JSON Content (Index $IndexToCheck):" -ForegroundColor Green
                Write-Host "Name: '$($targetIndex.Name)'" -ForegroundColor White
                Write-Host "Description: '$($targetIndex.Description)'" -ForegroundColor White
                Write-Host "DisplayText: '$($targetIndex.DisplayText)'" -ForegroundColor White
                Write-Host ""

                # Compare values
                Write-Host "=== Comparison Results ===" -ForegroundColor Cyan
                $nameMatch = $wimInfo.ImageName -eq $targetIndex.Name
                $descMatch = $wimInfo.ImageDescription -eq $targetIndex.Description

                Write-Host "Name matches: $nameMatch" -ForegroundColor $(if ($nameMatch) { "Green" } else { "Red" })
                Write-Host "Description matches: $descMatch" -ForegroundColor $(if ($descMatch) { "Green" } else { "Red" })

                if (-not $nameMatch) {
                    Write-Host "  WIM Name: '$($wimInfo.ImageName)'" -ForegroundColor Yellow
                    Write-Host "  JSON Name: '$($targetIndex.Name)'" -ForegroundColor Yellow
                }

                if (-not $descMatch) {
                    Write-Host "  WIM Description: '$($wimInfo.ImageDescription)'" -ForegroundColor Yellow
                    Write-Host "  JSON Description: '$($targetIndex.Description)'" -ForegroundColor Yellow
                }

            } else {
                Write-Host "ERROR: Index $IndexToCheck not found in JSON" -ForegroundColor Red
            }
        } else {
            Write-Host "ERROR: WIM file not found in images.json" -ForegroundColor Red
        }

    } catch {
        Write-Host "ERROR: Failed to read images.json: $($_.Exception.Message)" -ForegroundColor Red
    }

} else {
    Write-Host "WARNING: images.json not found at $jsonPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Verification Complete ===" -ForegroundColor Cyan
