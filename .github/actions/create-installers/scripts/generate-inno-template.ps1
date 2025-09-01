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
    [string]$Platform,

    [Parameter(Mandatory=$true)]
    [string]$ExeName
)

Write-Host "Generating Inno Setup template with parameters:"
Write-Host "  AppName: $AppName"
Write-Host "  AppVersion: $AppVersion"
Write-Host "  CompanyName: $CompanyName"
Write-Host "  Platform: $Platform"
Write-Host "  ExeName: $ExeName"

# Determine architecture settings
$archAllowed = switch ($Platform) {
    "x86" { "x86" }
    "x64" { "x64" }
    "ARM64" { "arm64" }
}

$archInstallIn64Bit = if ($Platform -ne "x86") { "x64 arm64" } else { "" }

$issContent = @"
[Setup]
AppId={{12345678-1234-1234-1234-123456789012}
AppName=$AppName
AppVersion=$AppVersion
AppPublisher=$CompanyName
AppPublisherURL=https://github.com/mchave3/Bucket
AppSupportURL=https://github.com/mchave3/Bucket/issues
AppUpdatesURL=https://github.com/mchave3/Bucket/releases
DefaultDirName={autopf}\$AppName
DefaultGroupName=$AppName
AllowNoIcons=yes
LicenseFile=..\..\..\..\LICENSE
OutputDir=.
OutputBaseFilename=$($ExeName -replace '\.exe$', '')
SetupIconFile=$BuildOutputPath\Assets\AppIcon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=$archAllowed
ArchitecturesInstallIn64BitMode=$archInstallIn64Bit

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "$BuildOutputPath\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\$AppName"; Filename: "{app}\$AppName.exe"
Name: "{group}\{cm:UninstallProgram,$AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\$AppName"; Filename: "{app}\$AppName.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\$AppName.exe"; Description: "{cm:LaunchProgram,$AppName}"; Flags: nowait postinstall skipifsilent

[Code]
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#StringChange(AppId, "{{", "").Replace("}", "")}}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  Result := 0;
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
  end;
end;
"@

$issFile = "bucket-setup.iss"
Set-Content -Path $issFile -Value $issContent -Encoding UTF8

Write-Host "Inno Setup script generated successfully: $issFile"
