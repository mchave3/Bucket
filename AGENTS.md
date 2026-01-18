# AGENTS.md (coding-agent guide)

This repository is a PowerShell module built with Sampler + Invoke-Build.
- Authoring source lives in `source/`
- Tests live in `tests/`
- Build artifacts go in `output/` (do not hand-edit)

## Repo map

- Module manifest (authoring source): `source/Bucket.psd1`
- Root module stub (generated during build): `source/Bucket.psm1` (intentionally empty)
- Public functions (exported): `source/Public/*.ps1`
- Private helpers (internal): `source/Private/*.ps1`
- Localization: `source/en-US/`
- Build config: `build.yaml`
- Dependency manifests: `RequiredModules.psd1`, `Resolve-Dependency.psd1`

Tests:
- QA gates: `tests/QA/module.tests.ps1`
- Unit tests: `tests/Unit/Public/*.tests.ps1`, `tests/Unit/Private/*.tests.ps1`

## Editor/assistant rules found

- Cursor rules: none found (`.cursor/rules/**` and `.cursorrules` not present)
- Copilot rules: none found (`.github/copilot-instructions.md` not present)

If these appear later, follow them as higher-priority guidance.

## Runtime targets

- PowerShell 7+ (`PowerShellVersion = '7.0'` in `source/Bucket.psd1`)
- Core-only (`CompatiblePSEditions = @('Core')`)
- Runtime dependency: `PwshSpectreConsole` (`source/Bucket.psd1`, `RequiredModules.psd1`)

## Build / lint / test (run from repo root)

### Prereqs

- Recommended shell: PowerShell 7+ (`pwsh`)
- Build runner: Invoke-Build (bootstrapped via `build.ps1`)
- Dependency restore uses `Resolve-Dependency.ps1` (configured in `Resolve-Dependency.psd1`)

### Restore dependencies (no build)

Preferred (fast, mirrors CI bootstrap):
- `pwsh -NoProfile -File .\build.ps1 -ResolveDependency -Tasks noop`

Notes:
- Dependencies are typically saved under `output/RequiredModules/` and prepended to `PSModulePath` by `build.ps1`.
- `Resolve-Dependency.psd1` enables PSResourceGet by default (`UsePSResourceGet = $true`).
- If you see missing modules, rerun restore or use `-AutoRestore` on `build.ps1`.

### List available build tasks

- `pwsh -NoProfile -File .\build.ps1 -Tasks ?`

### Common workflows (Sampler)

Defined in `build.yaml` under `BuildWorkflow`:
- Build + test (default workflow): `pwsh -NoProfile -File .\build.ps1`
- Build only: `pwsh -NoProfile -File .\build.ps1 -Tasks build`
- Test only: `pwsh -NoProfile -File .\build.ps1 -Tasks test`
- Pack: `pwsh -NoProfile -File .\build.ps1 -Tasks pack`

Build output:
- ModuleBuilder output is configured with `BuiltModuleSubdirectory: module` and `VersionedOutputDirectory: true` in `build.yaml`.

### Run a single test file (preferred)

Use the build pipeline so dependencies + `PSModulePath` match CI:
- `pwsh -NoProfile -File .\build.ps1 -Tasks test -PesterScript .\tests\Unit\Public\Start-Bucket.tests.ps1`

Notes:
- `-PesterScript` has alias `-PesterPath` (future-proof for Pester v5+).
- You can pass multiple `-PesterScript` values.

### Run tests directly with Pester (fast iteration)

This works if the module is already importable in your session:
- `pwsh -NoProfile -Command "Invoke-Pester -Path .\\tests\\Unit\\Public\\Start-Bucket.tests.ps1 -Output Detailed"`

Tag filtering (Pester 5 style):
- `pwsh -NoProfile -Command "Invoke-Pester -Path .\\tests -TagFilter helpQuality -ExcludeTagFilter FunctionalQuality"`

### Linting (PSScriptAnalyzer)

QA runs ScriptAnalyzer per-function via `tests/QA/module.tests.ps1`, but you can run it manually:
- `pwsh -NoProfile -Command "Invoke-ScriptAnalyzer -Path .\\source -Recurse"`

## QA expectations (enforced by tests/QA/module.tests.ps1)

- Every function in the module must have a unit test file under `tests/Unit/**/<Function>.tests.ps1`.
- Every function must pass ScriptAnalyzer (no per-file settings file found in this repo).
- Comment-based help is required and validated via AST:
  - `.SYNOPSIS` present
  - `.DESCRIPTION` length > 40 characters
  - At least one `.EXAMPLE` (must include the function name)
  - Every parameter must have `.PARAMETER` help text (length > 25 characters)
- Module must import and remove cleanly.

## Code style guidelines (PowerShell)

### File layout

- One function per file; file name matches function name.
- Public functions: `source/Public/`
- Private helpers: `source/Private/`
- Do not hand-edit `output/**` (generated).
- Avoid editing `source/Bucket.psm1` directly; it is generated during build.

### Advanced function shape

Prefer advanced functions with a consistent layout:
- `function Verb-Noun`
- Opening brace on the next line
- `[CmdletBinding()]` and `[OutputType([type])]` at top
- `param (...)` block with 4-space indentation
- `process { ... }` (use `begin/process/end` only when needed)

### Naming and parameters

- Use approved `Verb-Noun` names.
- Parameters: PascalCase (`$ImagePath`, `$ShowExit`, `$ConfirmPreference`).
- Local variables: meaningful; prefer lower camel-case (`$projectPath`, `$fileName`).
- Prefer `[ValidateSet(...)]` / `[ValidateScript(...)]` for user-facing validation.

### Types and output

- Prefer explicit parameter types (e.g. `[string]`, `[string[]]`, `[hashtable]`, `[switch]`).
- Add `[OutputType([type])]` when practical; use `[OutputType([void])]` for UI-only functions.
- Prefer outputting objects (or `Write-Output`) over `return` for normal pipelines.
- Keep return contracts strict (example: navigation results are hashtables with an `Action` key).

### ShouldProcess / side effects

- For destructive operations, use `SupportsShouldProcess = $true` and call `$PSCmdlet.ShouldProcess(...)`.
- Support `-WhatIf` / `-Confirm` behavior; only suppress confirmation when explicitly requested.

### Error handling

- Do not set `$ErrorActionPreference` globally.
- For catchable failures: use `-ErrorAction Stop` and `try { } catch { }`.
- Throw terminating errors for invalid user input (fail fast).
- Never use empty catch blocks.

### Logging and user-visible output

- Diagnostics: `Write-Verbose -Message '...'`
- Recoverable issues: `Write-Warning -Message '...'`
- Persistent session logging: `Write-BucketLog` (initialized via `Initialize-BucketLogging`).
- Interactive UI output: prefer `Write-SpectreHost` / `Invoke-Spectre*` (PwshSpectreConsole).
- Avoid adding new `Write-Host` usage in module code; prefer Spectre or standard streams.

Log level consistency:
- `Write-BucketLog` validates `Level` as `Info|Warning|Error|Debug`. Keep call sites aligned with that set (avoid `Information`).

### Encoding

- UI uses Spectre markup; treat output strings as markup (escape/untrusted values if needed).
- `Start-Bucket` sets console encoding to UTF-8 for rendering; avoid changing global encoding elsewhere.

## Tooling note

- No LSP is configured for `.ps1` in this environment; rely on Pester + ScriptAnalyzer + QA tests for validation.
