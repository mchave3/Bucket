function Get-HelloWorld {
    <#
    .SYNOPSIS
    A brief description of the Get-HelloWorld function.

    .DESCRIPTION
    A detailed description of the Get-HelloWorld function, its purpose, and how it works.

    .PARAMETER Name
    Specifies the name to use in the greeting.

    .EXAMPLE
    Get-HelloWorld -Name "John"
    Returns "Hello, John!"

    .NOTES
    Additional information about the function.
    #>
    [CmdletBinding()]
    param (
        [Parameter()]
        [string]$Name = "World"
    )

    return "Hello, $Name!"
}