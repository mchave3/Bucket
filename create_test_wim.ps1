# Create a test WIM file for testing our XML editing functionality
# Note: This will create a minimal WIM file that contains basic metadata for testing

$tempDir = "e:\Bucket\test_wim_temp"
$wimPath = "e:\Bucket\test.wim"

# Clean up previous test files
if (Test-Path $wimPath) {
    Remove-Item $wimPath -Force
}
if (Test-Path $tempDir) {
    Remove-Item $tempDir -Recurse -Force
}

# Create a temporary directory with some test content
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
New-Item -ItemType Directory -Path "$tempDir\TestFolder" -Force | Out-Null
"Test content" | Out-File -FilePath "$tempDir\test.txt" -Encoding utf8
"Another test content" | Out-File -FilePath "$tempDir\TestFolder\test2.txt" -Encoding utf8

try {
    # Use DISM to create a test WIM (requires admin privileges)
    Write-Host "Creating test WIM file using DISM..."
    $dismResult = Start-Process -FilePath "dism" -ArgumentList "/Capture-Image", "/ImageFile:`"$wimPath`"", "/CaptureDir:`"$tempDir`"", "/Name:`"Test Image`"", "/Description:`"Test Description`"" -Wait -PassThru -NoNewWindow

    if ($dismResult.ExitCode -eq 0) {
        Write-Host "Test WIM created successfully at: $wimPath"

        # Display WIM info
        Write-Host "`nWIM Information:"
        $wimInfo = Start-Process -FilePath "dism" -ArgumentList "/Get-WimInfo", "/WimFile:`"$wimPath`"" -Wait -PassThru -NoNewWindow -RedirectStandardOutput "wim_info.txt"
        if (Test-Path "wim_info.txt") {
            Get-Content "wim_info.txt"
            Remove-Item "wim_info.txt" -Force
        }
    } else {
        Write-Error "Failed to create test WIM. Exit code: $($dismResult.ExitCode)"
        Write-Host "Note: This command requires administrator privileges."
        Write-Host "Alternative: Use an existing WIM file for testing."
    }
} catch {
    Write-Error "Error creating test WIM: $_"
    Write-Host "Note: DISM operations require administrator privileges."
} finally {
    # Clean up temporary directory
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force
    }
}

Write-Host "`nTest WIM path: $wimPath"
Write-Host "Use this file to test the XML editing functionality."
