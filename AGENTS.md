# AGENTS.md (coding-agent guide)

This is a PowerShell module repo built with Sampler + Invoke-Build. Source lives in `source/`, tests in `tests/`, and generated build artifacts go to `output/`.

## Repo map

- Module manifest: `source/Bucket.psd1`
- Root module stub: `source/Bucket.psm1` (rebuilt/merged during build)
- Public functions: `source/Public/*.ps1` (exported)
- Private helpers: `source/Private/*.ps1` (internal)
- Tests:
  - QA gates: `tests/QA/module.tests.ps1`
  - Unit tests: `tests/Unit/Public/*.tests.ps1`, `tests/Unit/Private/*.tests.ps1`
- Build config: `build.yaml`
- Dependency manifests: `RequiredModules.psd1`, `Resolve-Dependency.psd1`

## Editor/assistant rules found

- Cursor rules: none found (`.cursor/rules/**` and `.cursorrules` not present).
- Copilot rules: none found (`.github/copilot-instructions.md` not present).

If these appear later, follow them as higher-priority guidance.

## Build, lint, test (run from repo root)

### Prereqs

- Recommended shell: PowerShell 7+ (`pwsh`) for dependency restore.
- Target runtime: Windows PowerShell 5.0+ (see `PowerShellVersion = '5.0'` in `source/Bucket.psd1`).
- Git recommended: QA checks validate `CHANGELOG.md` changes via `git diff`.

### Restore dependencies (no build)

- `pwsh -NoProfile -File .\build.ps1 -ResolveDependency -Tasks noop`

Notes:
- Dependencies are typically saved under `output/RequiredModules/` and prepended to `PSModulePath` by `build.ps1`.
- The default resolver is configured in `Resolve-Dependency.psd1` (currently PSResourceGet).

### List available build tasks

- `pwsh -NoProfile -File .\build.ps1 -Tasks ?`

### Common workflows

- Build + test (default): `pwsh -NoProfile -File .\build.ps1`
- Build only: `pwsh -NoProfile -File .\build.ps1 -Tasks build`
- Test only: `pwsh -NoProfile -File .\build.ps1 -Tasks test`
- Pack: `pwsh -NoProfile -File .\build.ps1 -Tasks pack`

### Run a single test file (preferred)

Use the pipeline so the module is built and discoverable the same way CI does:

- `pwsh -NoProfile -File .\build.ps1 -Tasks test -PesterScript .\tests\Unit\Public\Get-Something.tests.ps1`

Other useful filters:

- `pwsh -NoProfile -File .\build.ps1 -Tasks test -PesterTag helpQuality`
- `pwsh -NoProfile -File .\build.ps1 -Tasks test -PesterExcludeTag FunctionalQuality`

### Run tests directly with Pester (fast iteration)

This can work if the module is already importable in your session:

- `pwsh -NoProfile -Command "Invoke-Pester -Path .\\tests\\Unit\\Public\\Get-Something.tests.ps1 -Output Detailed"`

If it fails to import `Bucket`, run via `build.ps1` instead.

### Linting (PSScriptAnalyzer)

ScriptAnalyzer is enforced by QA tests (`tests/QA/module.tests.ps1`) for each function file.

- `pwsh -NoProfile -Command "Invoke-ScriptAnalyzer -Path .\\source -Recurse"`

## Code style guidelines (PowerShell)

Follow existing patterns in `source/Public/Get-Something.ps1` and `source/Private/Get-PrivateFunction.ps1`.

### Layout and file rules

- One function per file; the file name must match the function name.
- Public functions go in `source/Public/`; private helpers go in `source/Private/`.
- Do not hand-edit `output/**` (generated).
- Avoid editing `source/Bucket.psm1` directly unless you know the build merge rules.

### Naming

- Use `Verb-Noun` with approved verbs (e.g. `Get-`, `Set-`, `Test-`).
- Parameters: PascalCase (`-PrivateData`).
- Locals: meaningful names; prefer lower camel-case (`$projectPath`).

### Advanced function shape

- Prefer advanced functions: `function Verb-Noun { [CmdletBinding()] param(...) begin/process/end }`.
- Use `SupportsShouldProcess = $true` for side-effecting commands.
- Support pipeline input when sensible (`ValueFromPipeline`, `ValueFromPipelineByPropertyName`).

### Comment-based help (required)

QA tests enforce help quality for every function:

- Include `.SYNOPSIS`.
- Include `.DESCRIPTION` (must be > ~40 chars).
- Include at least one `.EXAMPLE`.
- Every parameter must have a description (and it must be descriptive; length threshold enforced).

### Imports and dependencies

- Do not `Import-Module` inside function bodies unless unavoidable.
- Prefer declaring dependencies via the module manifest (`.psd1`) / build pipeline.
- Tests should `Import-Module` in `BeforeAll` and `Remove-Module` in `AfterAll`.

### Formatting

- 4-space indentation.
- Opening brace on the next line for `function`, `param`, `process`, etc.
- Prefer `Write-Verbose -Message '...'`; avoid `Write-Host` in module code.

### Types and output

- Prefer explicit parameter types (e.g. `[string]`).
- Add `[OutputType([type])]` when practical.
- Prefer outputting objects / `Write-Output` over `return` for clarity.

### Error handling

- Do not set `$ErrorActionPreference` globally.
- For catchable failures: use `-ErrorAction Stop` and `try { } catch { }`.
- Throw terminating errors for invalid user input (fail fast).

### Logging

- `Write-Verbose` for diagnostics, `Write-Warning` for recoverable issues.
- Avoid `Write-Host` (acceptable in build scripts).

## Testing conventions (Pester 5)

- Every function must have a unit test file:
  - Public: `tests/Unit/Public/<Function>.tests.ps1`
  - Private: `tests/Unit/Private/<Function>.tests.ps1` (use `InModuleScope`)
- Prefer `Describe` per function and focused `Context`/`It` blocks.
- Use `Mock` + `Should -Invoke` to verify internal calls.

## Changelog rule (enforced)

If code changes are made, QA expects `CHANGELOG.md` to be updated with at least one entry under `## [Unreleased]`.
