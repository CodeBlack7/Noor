[CmdletBinding()]
param(
    [string]$NodeHome = 'C:\Users\rahul\tools\node22',
    [int]$Port = 4200
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$web = Join-Path $root 'frontend\noor-web'

if ($NodeHome -and (Test-Path (Join-Path $NodeHome 'npm.cmd'))) {
    $env:Path = "$NodeHome;$env:Path"
}

if (-not (Get-Command npm.cmd -ErrorAction SilentlyContinue)) {
    throw "npm.cmd was not found. Pass -NodeHome with the folder that contains Node.js."
}

Push-Location $web
try {
    Write-Host "Starting Noor web on http://localhost:$Port" -ForegroundColor DarkYellow
    & npm.cmd start -- --port $Port
}
finally {
    Pop-Location
}
