function Test-DotNetProject {
    <#
    .SYNOPSIS
    Tests if the current directory contains a .NET project file.
    
    .DESCRIPTION
    Checks if the current directory contains a .csproj, .fsproj, or .vbproj file.
    
    .OUTPUTS
    System.Boolean. Returns $true if a project file is found, $false otherwise.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param()
    
    $projectFiles = Get-ChildItem -Path . -Filter "*.csproj", "*.fsproj", "*.vbproj" -File
    return $projectFiles.Count -gt 0
}

function Get-ProjectFile {
    <#
    .SYNOPSIS
    Gets the .NET project file in the current directory.
    
    .DESCRIPTION
    Finds and returns the path to a .NET project file (.csproj, .fsproj, or .vbproj) in the current directory.
    
    .OUTPUTS
    System.String. The full path to the project file.
    
    .NOTES
    Throws an exception if no project file is found.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()
    
    $projectFiles = Get-ChildItem -Path . -Filter "*.csproj", "*.fsproj", "*.vbproj" -File
    if ($projectFiles.Count -eq 0) {
        throw "No project file found in the current directory. Please navigate to a directory containing a .NET project file."
    }
    
    if ($projectFiles.Count -gt 1) {
        Write-Warning "Multiple project files found. Using: $($projectFiles[0].Name)"
    }
    
    return $projectFiles[0].FullName
}

function Test-DotNetCLI {
    <#
    .SYNOPSIS
    Tests if the dotnet CLI is available.
    
    .DESCRIPTION
    Checks if the dotnet command is available in the PATH.
    
    .OUTPUTS
    System.Boolean. Returns $true if the dotnet CLI is available, $false otherwise.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param()
    
    return $null -ne (Get-Command "dotnet" -ErrorAction SilentlyContinue)
}