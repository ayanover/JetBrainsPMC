function Remove-Package {
    <#
    .SYNOPSIS
    Removes a NuGet package from the current project.
    
    .DESCRIPTION
    This cmdlet removes a specified NuGet package from the current project using the dotnet CLI.
    
    .PARAMETER PackageId
    The ID of the package to remove.
    
    .EXAMPLE
    Remove-Package -PackageId "Newtonsoft.Json"
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$PackageId
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
            
            $removePackageArgs = @("remove", $projectFile, "package", $PackageId)
            
            Write-Verbose "Running: dotnet $($removePackageArgs -join ' ')"
            $output = & dotnet $removePackageArgs
            
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to remove package: $($output -join "`n")"
            }
            
            Write-Output "Successfully removed package '$PackageId'"
            return $output
        }
        catch {
            Write-Error "Error removing NuGet package: $_"
        }
    }
}