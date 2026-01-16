function Test-BucketAdminPrivilege
{
    <#
      .SYNOPSIS
      Tests whether the current PowerShell session is running with administrative privileges.

      .DESCRIPTION
      This function checks if the current user is a member of the Administrators group
      and has elevated privileges. This is required for WIM image operations that need
      system-level access.

      .EXAMPLE
      if (Test-BucketAdminPrivilege) {
          Write-Host 'Running as administrator'
      }

      Checks if the session is elevated and outputs a message if true.

      .OUTPUTS
      [bool] True if running with administrative privileges, false otherwise.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param()

    process
    {
        $currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = [Security.Principal.WindowsPrincipal]$currentIdentity
        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
}
