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

$wxsContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Package Name="$AppName"
           Version="$AppVersion"
           Manufacturer="$CompanyName"
           UpgradeCode="12345678-1234-1234-1234-123456789012"
           Language="1033"
           Codepage="1252"
           InstallerVersion="500">

    <SummaryInformation Description="$ProductDescription"
                        Comments="Installer for $AppName $AppVersion"
                        Manufacturer="$CompanyName" />

    <MajorUpgrade DowngradeErrorMessage="A newer version of $AppName is already installed." />

    <MediaTemplate EmbedCab="yes" />

    <Feature Id="ProductFeature" Title="$AppName" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
      <ComponentRef Id="ApplicationShortcut" />
      <ComponentRef Id="DesktopShortcut" />
    </Feature>

    <Icon Id="AppIcon" SourceFile="$BuildOutputPath\Assets\AppIcon.ico" />
    <Property Id="ARPPRODUCTICON" Value="AppIcon" />
    <Property Id="ARPURLINFOABOUT" Value="https://github.com/mchave3/Bucket" />

    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="$ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="$AppName" />
      </Directory>
      <Directory Id="ProgramMenuFolder">
        <Directory Id="ApplicationProgramsFolder" Name="$AppName" />
      </Directory>
      <Directory Id="DesktopFolder" />
    </Directory>

    <ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER">
      <Component Id="MainExecutable">
        <File Id="BucketExe"
              Source="$BuildOutputPath\$AppName.exe"
              KeyPath="yes" />
      </Component>

      <Component Id="AppFiles">
        <File Id="AllFiles" Source="$BuildOutputPath\*" />
      </Component>
    </ComponentGroup>

    <Component Id="ApplicationShortcut" Directory="ApplicationProgramsFolder" Guid="*">
      <Shortcut Id="ApplicationStartMenuShortcut"
                Name="$AppName"
                Description="$ProductDescription"
                Target="[INSTALLFOLDER]$AppName.exe"
                WorkingDirectory="INSTALLFOLDER"
                Icon="AppIcon" />
      <RemoveFolder Id="CleanUpShortCut" Directory="ApplicationProgramsFolder" On="uninstall" />
      <RegistryValue Root="HKCU"
                     Key="Software\$CompanyName\$AppName"
                     Name="installed"
                     Type="integer"
                     Value="1"
                     KeyPath="yes" />
    </Component>

    <Component Id="DesktopShortcut" Directory="DesktopFolder" Guid="*">
      <Shortcut Id="ApplicationDesktopShortcut"
                Name="$AppName"
                Description="$ProductDescription"
                Target="[INSTALLFOLDER]$AppName.exe"
                WorkingDirectory="INSTALLFOLDER"
                Icon="AppIcon" />
      <RegistryValue Root="HKCU"
                     Key="Software\$CompanyName\$AppName"
                     Name="desktop_shortcut"
                     Type="integer"
                     Value="1"
                     KeyPath="yes" />
    </Component>
  </Package>
</Wix>
"@

$wxsFile = "bucket-installer.wxs"
Set-Content -Path $wxsFile -Value $wxsContent -Encoding UTF8

Write-Host "WiX source file generated successfully: $wxsFile"
