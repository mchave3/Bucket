function Show-BucketHeader
{
    <#
      .SYNOPSIS
      Displays the Bucket application header using Spectre Figlet text.

      .DESCRIPTION
      This function renders the Bucket logo/header at the top of the terminal using
      Write-SpectreFigletText from PwshSpectreConsole. It provides consistent branding
      across all screens in the application.

      .EXAMPLE
      Show-BucketHeader

      Displays the default Bucket header with cyan color.

      .EXAMPLE
      Show-BucketHeader -Color 'Green'

      Displays the Bucket header with green color.

      .PARAMETER Color
      The color to use for the Figlet text. Defaults to Cyan1.

      .OUTPUTS
      [void] This function outputs directly to the console.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param
    (
        [Parameter()]
        [string]
        $Color = 'Cyan1'
    )

    process
    {
        Write-SpectreFigletText -Text 'Bucket' -Color $Color
    }
}
