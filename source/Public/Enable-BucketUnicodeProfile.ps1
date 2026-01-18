function Enable-BucketUnicodeProfile
{
    <#
      .SYNOPSIS
      Ensures your PowerShell profile starts with UTF-8 console encoding configuration.

      .DESCRIPTION
      Adds (or moves) the PwshSpectreConsole-recommended UTF-8 encoding statement to the
      first line of the specified PowerShell profile file. This enables full Unicode
      rendering for Spectre-based terminal UI, and the operation is idempotent.

      .EXAMPLE
      Enable-BucketUnicodeProfile

      Ensures the current user's PowerShell profile starts with the UTF-8 encoding line.

      .EXAMPLE
      Enable-BucketUnicodeProfile -ProfilePath "$PROFILE" -WhatIf

      Shows what would change without modifying the profile file.

      .PARAMETER ProfilePath
      The profile file path to update. Defaults to $PROFILE (CurrentUserCurrentHost).
      Use this when you want to update a different profile file path than the current host.

      .PARAMETER Force
      Creates the parent directory if needed and overwrites read-only file attributes when writing.
      Use this only when you understand the implications of updating the profile on disk.

      .PARAMETER PassThru
      Returns a status object describing whether the profile was modified and what action was taken.
      By default, this function produces no output to keep interactive usage clean.

      .OUTPUTS
      [pscustomobject] when -PassThru is specified, otherwise no output.
    #>
    [CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
    [OutputType([pscustomobject])]
    param
    (
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $ProfilePath = $PROFILE,

        [Parameter()]
        [switch]
        $Force,

        [Parameter()]
        [switch]
        $PassThru
    )

    process
    {
        $encodingLine = '$OutputEncoding = [Console]::InputEncoding = [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()'

        $action = 'Ensure UTF-8 encoding statement is first line'
        if (-not $PSCmdlet.ShouldProcess($ProfilePath, $action))
        {
            return
        }

        $profileDir = Split-Path -Path $ProfilePath -Parent
        if (-not [string]::IsNullOrEmpty($profileDir) -and -not (Test-Path -Path $profileDir -PathType Container))
        {
            New-Item -Path $profileDir -ItemType Directory -Force:$Force | Out-Null
        }

        $originalExists = Test-Path -Path $ProfilePath -PathType Leaf
        $raw = ''
        if ($originalExists)
        {
            $raw = Get-Content -Path $ProfilePath -Raw -ErrorAction Stop
        }

        $newline = "`n"
        if ($raw -match "`r`n")
        {
            $newline = "`r`n"
        }

        $hadTrailingNewline = $false
        if (-not [string]::IsNullOrEmpty($raw))
        {
            $hadTrailingNewline = $raw.EndsWith("`n")
        }

        $lines = @()
        if (-not [string]::IsNullOrEmpty($raw))
        {
            $lines = $raw -split "\r?\n"
        }

        $normalizedLines = @($lines | Where-Object { $_.Trim() -ne $encodingLine })

        $alreadyFirst = $false
        if ($lines.Count -gt 0)
        {
            $alreadyFirst = ($lines[0].Trim() -eq $encodingLine)
        }

        $encodingLineOccurrences = @($lines | Where-Object { $_.Trim() -eq $encodingLine }).Count
        $noChange = $alreadyFirst -and ($encodingLineOccurrences -eq 1)

        $resultAction = 'None'
        $changed = -not $noChange

        if ($changed)
        {
            $newLines = @($encodingLine) + $normalizedLines

            $newContent = ($newLines -join $newline)
            if ($hadTrailingNewline -or (-not $originalExists))
            {
                $newContent += $newline
            }

            if ($originalExists)
            {
                if ($encodingLineOccurrences -gt 0)
                {
                    $resultAction = 'Moved'
                }
                else
                {
                    $resultAction = 'Inserted'
                }
            }
            else
            {
                $resultAction = 'Created'
            }

            $fileItem = Get-Item -LiteralPath $ProfilePath -ErrorAction SilentlyContinue
            $hadReadOnly = $false
            if ($null -ne $fileItem)
            {
                $hadReadOnly = $fileItem.IsReadOnly
            }

            if ($hadReadOnly -and $Force)
            {
                Set-ItemProperty -LiteralPath $ProfilePath -Name IsReadOnly -Value $false -ErrorAction Stop
            }

            Set-Content -Path $ProfilePath -Value $newContent -Encoding utf8 -NoNewline -Force:$Force -ErrorAction Stop

            if ($hadReadOnly -and $Force)
            {
                Set-ItemProperty -LiteralPath $ProfilePath -Name IsReadOnly -Value $true -ErrorAction SilentlyContinue
            }
        }

        if ($PassThru)
        {
            [pscustomobject]@{
                ProfilePath  = $ProfilePath
                Changed      = $changed
                Action       = $resultAction
                EncodingLine = $encodingLine
            }
        }
    }
}
