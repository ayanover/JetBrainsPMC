@{
    RootModule = 'NuGetTools.psm1'
    ModuleVersion = '1.0.0'
    GUID = '12345678-1234-1234-1234-123456789012'
    Author = 'Aver'
    CompanyName = 'JetBrains'
    Copyright = '(c) 2025. All rights reserved.'
    Description = 'A PowerShell module for managing NuGet packages in .NET projects in Rider'
    PowerShellVersion = '5.1'
    
    RequiredModules = @()
    FunctionsToExport = @('Find-NuGetPackage', 'Install-NuGetPackage', 'Remove-NuGetPackage')
    CmdletsToExport = @()
    VariablesToExport = @()
    AliasesToExport = @()
    
    PrivateData = @{
        PSData = @{
            Tags = @('NuGet', 'Package', 'Management', 'dotnet')
            LicenseUri = 'https://example.com/license'
            ProjectUri = 'https://example.com/project'
            ReleaseNotes = 'Initial release of the NuGetTools module'
        }
    }
}