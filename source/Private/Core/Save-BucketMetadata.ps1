function Save-BucketMetadata
{
    <#
      .SYNOPSIS
      Saves the Bucket metadata to the metadata.json file.

      .DESCRIPTION
      This function persists the current metadata (including imported images) to the
      metadata.json file in the Bucket data directory. It converts the metadata object
      to JSON format and writes it to disk with proper error handling.

      .EXAMPLE
      Save-BucketMetadata

      Saves the current BucketState.Metadata to the configured metadata file path.

      .EXAMPLE
      Save-BucketMetadata -Metadata $customMetadata -Path 'C:\custom\metadata.json'

      Saves custom metadata to a specific path.

      .PARAMETER Metadata
      The metadata object to save. Defaults to $script:BucketState.Metadata.

      .PARAMETER Path
      The file path to save the metadata to. Defaults to $script:BucketState.Paths.Metadata.

      .OUTPUTS
      [void] This function does not return output.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '', Justification = 'Metadata is a standard singular noun referring to data about data.')]
    [CmdletBinding()]
    [OutputType([void])]
    param
    (
        [Parameter()]
        [object]
        $Metadata,

        [Parameter()]
        [string]
        $Path
    )

    process
    {
        # Use defaults from BucketState if not provided
        if ($null -eq $Metadata)
        {
            $state = Get-BucketState
            $Metadata = $state.Metadata
        }

        if ([string]::IsNullOrEmpty($Path))
        {
            $state = Get-BucketState
            $Path = $state.Paths.Metadata
        }

        Write-Verbose -Message "Saving metadata to: $Path"

        try
        {
            # Ensure the parent directory exists
            $parentDir = Split-Path -Path $Path -Parent
            if (-not (Test-Path -Path $parentDir -PathType Container))
            {
                New-Item -Path $parentDir -ItemType Directory -Force | Out-Null
            }

            # Convert to JSON and save with UTF8 encoding
            $jsonContent = $Metadata | ConvertTo-Json -Depth 10
            Set-Content -Path $Path -Value $jsonContent -Encoding UTF8 -Force

            Write-Verbose -Message 'Metadata saved successfully.'
        }
        catch
        {
            Write-Error -Message "Failed to save metadata: $_"
            throw
        }
    }
}
