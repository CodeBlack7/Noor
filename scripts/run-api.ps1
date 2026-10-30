[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Push-Location $root
try {
    Write-Host 'Starting Noor API on http://localhost:5080 (Swagger: /swagger)' -ForegroundColor DarkYellow
    dotnet run --project 'backend\Noor.Api' --launch-profile http
}
finally {
    Pop-Location
}
