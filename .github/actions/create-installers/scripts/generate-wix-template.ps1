param(
    [Parameter(Mandatory=$true)]
    [string]$AppName,

    [Parameter(Mandatory=$true)]
    [string]$AppVersion,

    [Parameter(Mandatory=$true)]
    [string]$CompanyName,

    [Parameter(Mandatory=$true)]
    [string]$ProductDescription,

    [Parameter(Mandatory=$true)]
    [string]$BuildOutputPath,

    [Parameter(Mandatory=$true)]
    [string]$ProgramFilesFolder,

    [Parameter(Mandatory=$true)]
    [string]$WixArch
)

Write-Host "Generating WiX template with parameters:"
Write-Host "  AppName: $AppName"
Write-Host "  AppVersion: $AppVersion"
Write-Host "  CompanyName: $CompanyName"
Write-Host "  WixArch: $WixArch"

# Get the template file path
$templatePath = Join-Path $PSScriptRoot "..\templates\installer-template.wxs"

if (-not (Test-Path $templatePath)) {
    Write-Error "❌ Template file not found: $templatePath"
    exit 1
}

# Read the template content
$templateContent = Get-Content -Path $templatePath -Raw -Encoding UTF8

# Replace placeholders with actual values
$wxsContent = $templateContent -replace '{{APP_NAME}}', $AppName `
                                -replace '{{APP_VERSION}}', $AppVersion `
                                -replace '{{COMPANY_NAME}}', $CompanyName `
                                -replace '{{PRODUCT_DESCRIPTION}}', $ProductDescription `
                                -replace '{{BUILD_OUTPUT_PATH}}', $BuildOutputPath `
                                -replace '{{PROGRAM_FILES_FOLDER}}', $ProgramFilesFolder

$wxsFile = "bucket-installer.wxs"
Set-Content -Path $wxsFile -Value $wxsContent -Encoding UTF8

Write-Host "WiX source file generated successfully: $wxsFile"
