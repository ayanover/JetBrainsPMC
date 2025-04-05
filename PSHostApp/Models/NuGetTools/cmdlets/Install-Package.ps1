function Install-Package {
    <#
    .SYNOPSIS
    Installs a NuGet package to the current project.
    
    .DESCRIPTION
    This cmdlet installs a specified NuGet package to the current project using the dotnet CLI.
    
    .PARAMETER PackageId
    The ID of the package to install.
    
    .PARAMETER Version
    The specific version of the package to install. If not specified, the latest version will be installed.
    
    .PARAMETER PreRelease
    Whether to include prerelease versions when determining the latest version.
    
    .EXAMPLE
    Install-Package -PackageId "Newtonsoft.Json"
    
    .EXAMPLE
    Install-Package -PackageId "Microsoft.EntityFrameworkCore" -Version "5.0.0"
    
    .EXAMPLE
    Install-Package -PackageId "Microsoft.AspNetCore.Mvc" -PreRelease
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$PackageId,
        
        [Parameter(Mandatory = $false)]
        [string]$Version,
        
        [Parameter(Mandatory = $false)]
        [switch]$PreRelease
    )
    
    process {
        try {
            if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
                throw "dotnet CLI is not installed or not in the PATH. Please install .NET SDK."
            }
            
            $projectFiles = Get-ChildItem -Path . -Filter "*.csproj", "*.fsproj", "*.vbproj" -File
            if ($projectFiles.Count -eq 0) {
                throw "No project file found in the current directory. Please navigate to a directory containing a .NET project file."
            }
            
            $projectFile = $projectFiles[0].FullName
            Write-Verbose "Using project file: $projectFile"
            
            $addPackageArgs = @("add", $projectFile, "package", $PackageId)
            
            if ($Version) {
                $addPackageArgs += "--version", $Version
            }
            
            if ($PreRelease) {
                $addPackageArgs += "--prerelease"
            }
            
            Write-Verbose "Running: dotnet $($addPackageArgs -join ' ')"
            $output = & dotnet $addPackageArgs
            
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to install package: $($output -join "`n")"
            }
            
            Write-Output "Successfully installed package '$PackageId'"
            return $output
        }
        catch {
            Write-Error "Error installing NuGet package: $_"
        }
    }
}