function Initialize-BucketState
{
    <#
      .SYNOPSIS
      Initializes the global Bucket application state including paths, configuration, and metadata.

      .DESCRIPTION
      This function creates and validates the Bucket working directory structure under %ProgramData%\Bucket.
      It initializes the script-scoped BucketState object containing paths, configuration, metadata,
      and session information. The function also loads any existing configuration and image metadata
      from JSON files if they exist.

      .EXAMPLE
      Initialize-BucketState

      Initializes the Bucket state, creating necessary directories and loading configuration.

      .OUTPUTS
      [void] This function does not return output but initializes the script-scoped $script:BucketState variable.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param()

    process
    {
        Write-Verbose -Message 'Initializing Bucket application state...'

        # Define the base path and all required subdirectories
        $basePath = Join-Path -Path $env:ProgramData -ChildPath 'Bucket'

        $requiredFolders = @(
            'Logs'
            'Mount'
            'Images'
            'Updates'
        )

        # Initialize the global state object
        $script:BucketState = [PSCustomObject]@{
            Paths    = [PSCustomObject]@{
                Root    = $basePath
                Logs    = Join-Path -Path $basePath -ChildPath 'Logs'
                Mount   = Join-Path -Path $basePath -ChildPath 'Mount'
                Images  = Join-Path -Path $basePath -ChildPath 'Images'
                Updates = Join-Path -Path $basePath -ChildPath 'Updates'
                Config  = Join-Path -Path $basePath -ChildPath 'config.json'
            }
            Config   = [PSCustomObject]@{
                Version = '1.0'
            }
            Metadata = @{
                Images = @()
            }
            Session  = [PSCustomObject]@{
                StartTime    = Get-Date
                IsElevated   = $false
                CurrentImage = $null
            }
        }

        # Create base directory if it doesn't exist
        if (-not (Test-Path -Path $basePath -PathType Container))
        {
            Write-Verbose -Message "Creating base directory: $basePath"
            New-Item -Path $basePath -ItemType Directory -Force | Out-Null
        }

        # Create each required subdirectory
        foreach ($folder in $requiredFolders)
        {
            $folderPath = Join-Path -Path $basePath -ChildPath $folder
            if (-not (Test-Path -Path $folderPath -PathType Container))
            {
                Write-Verbose -Message "Creating directory: $folderPath"
                New-Item -Path $folderPath -ItemType Directory -Force | Out-Null
            }
        }

        # Load existing configuration if present
        $configPath = $script:BucketState.Paths.Config
        if (Test-Path -Path $configPath -PathType Leaf)
        {
            Write-Verbose -Message "Loading configuration from: $configPath"
            try
            {
                $loadedConfig = Get-Content -Path $configPath -Raw | ConvertFrom-Json
                $script:BucketState.Config = $loadedConfig
            }
            catch
            {
                Write-Warning -Message "Failed to load configuration: $_"
            }
        }

        # Load existing image metadata if present
        $metadataPath = Join-Path -Path $basePath -ChildPath 'metadata.json'
        if (Test-Path -Path $metadataPath -PathType Leaf)
        {
            Write-Verbose -Message "Loading metadata from: $metadataPath"
            try
            {
                $loadedMetadata = Get-Content -Path $metadataPath -Raw | ConvertFrom-Json
                $script:BucketState.Metadata = $loadedMetadata
            }
            catch
            {
                Write-Warning -Message "Failed to load metadata: $_"
            }
        }

        Write-Verbose -Message 'Bucket state initialization complete.'
    }
}
