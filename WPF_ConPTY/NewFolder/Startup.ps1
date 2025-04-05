$modulePath = Join-Path -Path $PWD -ChildPath 'Modules\NuGetTools'
$env:PSModulePath = $env:PSModulePath + ";" + $modulePath

Import-Module "$modulePath\NuGetTools.psd1" -Force -Verbose




cls
Write-Host @'
                        _       _____  __  __  _____ 
     /\                ( )     |  __ \|  \/  |/ ____|
    /  \__   _____ _ __|/ ___  | |__) | \  / | |     
   / /\ \ \ / / _ \ '__| / __| |  ___/| |\/| | |     
  / ____ \ V /  __/ |    \__ \ | |    | |  | | |____ 
 /_/    \_\_/ \___|_|    |___/ |_|    |_|  |_|\_____|
                                                     
 ====================================================
           Terminal App v1.0.0
 ====================================================
'@ -ForegroundColor Cyan
