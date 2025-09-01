# 📦 Create Installers Action

This reusable GitHub action automatically generates MSI and EXE installers for your WinUI3 application using WiX Toolset and Inno Setup.

## 🎯 Features

- **📦 MSI Installers**: Generated with WiX Toolset v4 for native Windows integration
- **🔧 EXE Installers**: Created with Inno Setup for a modern user interface
- **🏗️ Multi-architecture support**: x86, x64, and ARM64
- **🔐 Digital signing**: Optional support for signing installers
- **📋 Automatic configuration**: Automatic parameter detection based on platform
- **✨ User interface**: Desktop and start menu shortcuts, clean uninstallation

## 📋 Prerequisites

- **Operating System**: Windows (windows-latest)
- **Runtime**: .NET (automatically installed by the action)
- **Required files**:
  - Your application build output
  - Application icon (`Assets/AppIcon.ico`)
  - LICENSE file (for Inno Setup)

## 🔧 Usage

```yaml
- name: 📦 Create Installers
  uses: ./.github/actions/create-installers
  with:
    app-name: 'MyApp'
    app-version: '1.0.0'
    platform: 'x64'
    build-output-path: 'BuildOutput'
    company-name: 'My Company'
    product-description: 'My application description'
    sign-installers: false
```

## 📥 Inputs

| Parameter | Required | Description | Default |
|-----------|----------|-------------|---------|
| `app-name` | ✅ | Application name | - |
| `app-version` | ✅ | Application version | - |
| `platform` | ✅ | Target platform (x86, x64, ARM64) | - |
| `build-output-path` | ✅ | Path to build output | - |
| `company-name` | ❌ | Company name | `Mickaël CHAVE` |
| `product-description` | ❌ | Product description | `Bucket - Modern File Management Application` |
| `sign-installers` | ❌ | Sign installers | `false` |
| `certificate-path` | ❌ | Path to PFX certificate | - |
| `certificate-password` | ❌ | Certificate password | - |

## 📤 Outputs

| Output | Description |
|--------|-------------|
| `msi-installer-path` | Path to the created MSI installer |
| `exe-installer-path` | Path to the created EXE installer |
| `msi-installer-size` | MSI installer size in MB |
| `exe-installer-size` | EXE installer size in MB |

## 🏗️ Installer Architecture

### 📦 MSI Installer (WiX Toolset)

- **Windows Installer Standard**: Native integration with Windows
- **Clean uninstallation**: Via "Programs and Features"
- **Update support**: Automatic detection of existing versions
- **Automatic rollback**: In case of installation failure
- **Silent installation**: Support for `/quiet` and `/passive` parameters

**Installation structure:**
```
%ProgramFiles%\{AppName}\
├── {AppName}.exe
├── Assets\
│   └── AppIcon.ico
├── Strings\
└── [other application files]
```

**Created shortcuts:**
- Start Menu: `{AppName}`
- Desktop: `{AppName}` (optional)

### 🔧 EXE Installer (Inno Setup)

- **Modern interface**: Modern wizard style with multi-language support
- **Automatic detection**: Automatic uninstallation of previous versions
- **Advanced options**: User choice for shortcuts
- **License support**: LICENSE file display
- **Advanced compression**: LZMA for smaller files

**Supported languages:**
- English (default)
- French

**Advanced features:**
- Existing version detection
- Automatic uninstallation of old versions
- Launch after installation option
- Specific architecture support

## 🔐 Digital Signing (Optional)

To sign your installers, configure the following parameters:

```yaml
- name: 📦 Create Signed Installers
  uses: ./.github/actions/create-installers
  with:
    # ... other parameters ...
    sign-installers: true
    certificate-path: 'path/to/certificate.pfx'
    certificate-password: ${{ secrets.CERTIFICATE_PASSWORD }}
```

## 🧰 Tools Used

- **[WiX Toolset v4](https://wixtoolset.org/)**: MSI installer creation
- **[Inno Setup 6](https://jrsoftware.org/isinfo.php)**: EXE installer creation
- **[SignTool](https://docs.microsoft.com/en-us/windows/win32/seccrypto/signtool)**: Digital signing (optional)

## 📁 Action Structure

```text
.github/actions/create-installers/
├── action.yml                        # GitHub Actions definition
├── README.md                         # This documentation
├── scripts/
│   ├── generate-wix-template.ps1     # WiX MSI generation script
│   └── generate-inno-template.ps1    # Inno Setup EXE generation script
└── templates/
    ├── installer-template.wxs        # WiX XML template
    └── installer-template.iss        # Inno Setup script template
```

## 📊 Example Output

```
📦 Installer Creation Summary:
  • Platform: x64
  • Application: Bucket v24.9.1.1-Nightly
  • MSI Installer: Bucket-24.9.1.1-Nightly-x64.msi (15.2 MB)
  • EXE Installer: Bucket-24.9.1.1-Nightly-x64-Setup.exe (12.8 MB)
  • Company: Mickaël CHAVE
  • Signed: false
  • Status: ✅ Installers created successfully
```

## 🛠️ Maintenance

### Tool updates

Tools are automatically downloaded and installed on each execution to ensure the latest versions:

- WiX Toolset: Installed via `dotnet tool install --global wix`
- Inno Setup: Downloaded from the official site

### Template Architecture

This action uses a template-based approach for generating installer configurations:

**Template Files:**

- `templates/installer-template.wxs`: WiX XML template for MSI generation
- `templates/installer-template.iss`: Inno Setup script template for EXE generation

**Placeholder System:**

Templates use `{{PLACEHOLDER}}` syntax for dynamic content replacement:

```xml
<!-- WiX Template Example -->
<Package Name="{{APP_NAME}}"
         Version="{{APP_VERSION}}"
         Manufacturer="{{COMPANY_NAME}}" />
```

```ini
; Inno Setup Template Example
AppName={{APP_NAME}}
AppVersion={{APP_VERSION}}
AppPublisher={{COMPANY_NAME}}
```

**Available Placeholders:**

- `{{APP_NAME}}`: Application name
- `{{APP_VERSION}}`: Application version
- `{{COMPANY_NAME}}`: Company name
- `{{PRODUCT_DESCRIPTION}}`: Product description
- `{{BUILD_OUTPUT_PATH}}`: Build output directory path
- `{{PROGRAM_FILES_FOLDER}}`: Platform-specific Program Files folder

### Customization

To customize installation behavior:

1. **Modify Templates**: Edit `templates/installer-template.wxs` or `templates/installer-template.iss`
2. **Update Scripts**: Modify placeholder replacement logic in:
   - `scripts/generate-wix-template.ps1`: WiX processing
   - `scripts/generate-inno-template.ps1`: Inno Setup processing
3. **Add Placeholders**: Extend the placeholder system by updating both templates and scripts

## 🚨 Troubleshooting

### Common errors

1. **Build file not found**:
   - Check that `build-output-path` points to the correct directory
   - Ensure the build succeeded before this step

2. **Missing icon**:
   - Check for presence of `Assets/AppIcon.ico` in build output
   - Use a valid ICO file (not PNG or other format)

3. **WiX compilation error**:
   - Check that all referenced files exist
   - Consult detailed WiX logs

4. **Inno Setup error**:
   - Check the syntax of the generated script
   - Ensure the LICENSE file is accessible

## 📚 Resources

- [WiX Toolset Documentation](https://wixtoolset.org/docs/)
- [Inno Setup Documentation](https://jrsoftware.org/ishelp/)
- [Windows Code Signing Guide](https://docs.microsoft.com/en-us/windows/win32/seccrypto/signtool)
