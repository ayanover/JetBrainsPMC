function Find-NuGetPackage {
    <#
    .SYNOPSIS
    Searches for NuGet packages using the NuGet API.
    
    .DESCRIPTION
    This cmdlet queries the NuGet API to search for packages and displays the results.
    
    .PARAMETER SearchTerm
    The search term to use when searching for packages.
    
    .PARAMETER MaxResults
    The maximum number of results to return. Default is 10.
    
    .PARAMETER IncludePrerelease
    Whether to include prerelease packages in the search results.
    
    .EXAMPLE
    Find-Package -SearchTerm "Newtonsoft.Json"
    
    .EXAMPLE
    Find-Package -SearchTerm "logging" -MaxResults 20 -IncludePrerelease
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$SearchTerm,
        
        [Parameter(Mandatory = $false)]
        [int]$MaxResults = 10,
        
        [Parameter(Mandatory = $false)]
        [switch]$IncludePrerelease
    )
    
    begin {
        $apiUrl = "https://api-v2v3search-0.nuget.org/query"
    }
    
    process {
        try {
            $queryParams = @{
                q = $SearchTerm
                take = $MaxResults
            }
            
            if ($IncludePrerelease) {
                $queryParams.prerelease = "true"
            }

            Add-Type -AssemblyName System.Web
            $queryString = [System.Web.HttpUtility]::ParseQueryString("")
            foreach ($key in $queryParams.Keys) {
                $queryString.Add($key, $queryParams[$key])
            }
            
            $uriBuilder = New-Object System.UriBuilder($apiUrl)
            $uriBuilder.Query = $queryString.ToString()
            
            Write-Verbose "Querying NuGet API: $($uriBuilder.Uri.ToString())"
            $response = Invoke-RestMethod -Uri $uriBuilder.Uri -Method Get
            
            if ($response.data.Count -eq 0) {
                Write-Warning "No packages found matching '$SearchTerm'"
                return
            }
            
            $results = $response.data | ForEach-Object {
                [PSCustomObject]@{
                    Id = $_.id
                    Version = $_.version
                    Description = $_.description
                    Authors = $_.authors
                    Tags = $_.tags
                    TotalDownloads = $_.totalDownloads
                    ProjectUrl = $_.projectUrl
                }
            }
            
            return $results
        }
        catch {
            Write-Error "Error searching NuGet packages: $_"
        }
    }
}