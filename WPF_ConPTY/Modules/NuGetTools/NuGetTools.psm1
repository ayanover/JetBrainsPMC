$PSModuleRoot = $PSScriptRoot

$PrivateFunctionsPath = Join-Path -Path $PSModuleRoot -ChildPath 'Private'
if (Test-Path -Path $PrivateFunctionsPath) {
    $PrivateFunctions = Get-ChildItem -Path $PrivateFunctionsPath -Filter '*.ps1'
    foreach ($PrivateFunction in $PrivateFunctions) {
        . $PrivateFunction.FullName
    }
}

$FunctionsPath = Join-Path -Path $PSModuleRoot -ChildPath 'Functions'
$Functions = Get-ChildItem -Path $FunctionsPath -Filter '*.ps1'

foreach ($Function in $Functions) {
    . $Function.FullName
}

Export-ModuleMember -Function $Functions.BaseName