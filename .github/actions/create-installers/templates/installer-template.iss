[Setup]
AppId={{D0F5B5D0-1234-4567-8901-123456789012}
AppName={{APP_NAME}}
AppVersion={{APP_VERSION}}
AppPublisher={{COMPANY_NAME}}
AppPublisherURL=https://github.com/mchave3/Bucket
AppSupportURL=https://github.com/mchave3/Bucket/issues
AppUpdatesURL=https://github.com/mchave3/Bucket/releases
DefaultDirName={autopf}\{{APP_NAME}}
DefaultGroupName={{APP_NAME}}
AllowNoIcons=yes
LicenseFile=
PrivilegesRequired=lowest
OutputDir=.
OutputBaseFilename={{EXE_NAME}}
SetupIconFile={{BUILD_OUTPUT_PATH}}\Assets\AppIcon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode={{ARCH_MODE}}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{{BUILD_OUTPUT_PATH}}\{{APP_NAME}}.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{{BUILD_OUTPUT_PATH}}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{{APP_NAME}}"; Filename: "{app}\{{APP_NAME}}.exe"
Name: "{group}\{cm:UninstallProgram,{{APP_NAME}}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{{APP_NAME}}"; Filename: "{app}\{{APP_NAME}}.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\{{APP_NAME}}.exe"; Description: "{cm:LaunchProgram,{{APP_NAME}}}"; Flags: nowait postinstall skipifsilent
