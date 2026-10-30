[CmdletBinding()]
param(
    [string]$NodeHome = 'C:\Users\rahul\tools\node22'
)

$ErrorActionPreference = 'Stop'
$apiScript = Join-Path $PSScriptRoot 'run-api.ps1'
$webScript = Join-Path $PSScriptRoot 'run-web.ps1'

Write-Host 'Opening the API and web servers in separate windows...' -ForegroundColor DarkYellow
Start-Process powershell -ArgumentList @(
    '-NoExit', '-ExecutionPolicy', 'Bypass', '-File', "`"$apiScript`""
)
Start-Sleep -Seconds 4
Start-Process powershell -ArgumentList @(
    '-NoExit', '-ExecutionPolicy', 'Bypass', '-File', "`"$webScript`"",
    '-NodeHome', "`"$NodeHome`""
)

Write-Host 'API:  http://localhost:5080' -ForegroundColor Green
Write-Host 'Web:  http://localhost:4200' -ForegroundColor Green
Write-Host 'Close the two PowerShell windows to stop the servers.'
