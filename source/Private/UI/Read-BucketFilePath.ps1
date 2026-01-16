function Read-BucketFilePath
{
    <#
      .SYNOPSIS
      Displays a file picker dialog and returns the selected file path.

      .DESCRIPTION
      This function wraps System.Windows.Forms.OpenFileDialog to provide a user-friendly
      file selection experience. It displays a native Windows file picker dialog with
      configurable title and file type filter. Returns the selected file path or $null
      if the user cancels.

      .EXAMPLE
      $wimPath = Read-BucketFilePath -Title 'Select WIM Image' -Filter 'WIM files (*.wim)|*.wim'

      Displays a file picker for WIM files and returns the selected path.

      .EXAMPLE
      $isoPath = Read-BucketFilePath -Title 'Select ISO' -Filter 'ISO files (*.iso)|*.iso|All files (*.*)|*.*'

      Displays a file picker with multiple filter options.

      .PARAMETER Title
      The title to display in the file picker dialog window.

      .PARAMETER Filter
      The file type filter string in Windows dialog format.
      Example: 'WIM files (*.wim)|*.wim|All files (*.*)|*.*'

      .PARAMETER InitialDirectory
      Optional. The initial directory to open the dialog in.

      .OUTPUTS
      [string] The selected file path, or $null if cancelled.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param
    (
        [Parameter()]
        [string]
        $Title = 'Select File',

        [Parameter()]
        [string]
        $Filter = 'All files (*.*)|*.*',

        [Parameter()]
        [string]
        $InitialDirectory
    )

    process
    {
        Write-Verbose -Message "Opening file picker: $Title"

        # Load Windows Forms assembly
        [void][System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms')

        # Create and configure the dialog
        $dialogProperties = @{
            Title  = $Title
            Filter = $Filter
        }

        if (-not [string]::IsNullOrEmpty($InitialDirectory))
        {
            $dialogProperties['InitialDirectory'] = $InitialDirectory
        }

        $openFileDialog = New-Object System.Windows.Forms.OpenFileDialog -Property $dialogProperties

        # Show dialog and return result
        $result = $openFileDialog.ShowDialog()

        if ($result -eq [System.Windows.Forms.DialogResult]::OK)
        {
            Write-Verbose -Message "File selected: $($openFileDialog.FileName)"
            return $openFileDialog.FileName
        }
        else
        {
            Write-Verbose -Message 'File selection cancelled.'
            return $null
        }
    }
}
