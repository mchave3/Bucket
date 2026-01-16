function Get-BucketState
{
    <#
      .SYNOPSIS
      Returns the current Bucket application state object.

      .DESCRIPTION
      This function provides access to the script-scoped BucketState object which contains
      paths, configuration, metadata, and session information. It ensures the state has been
      initialized before returning it.

      .EXAMPLE
      $state = Get-BucketState
      Write-Host "Images folder: $($state.Paths.Images)"

      Gets the current state and accesses the Images path.

      .OUTPUTS
      [PSCustomObject] The current Bucket application state object.
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param()

    process
    {
        if ($null -eq $script:BucketState)
        {
            throw 'Bucket state has not been initialized. Call Initialize-BucketState first.'
        }

        return $script:BucketState
    }
}
