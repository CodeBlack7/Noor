[CmdletBinding()]
param(
    [string]$NodeHome = 'C:\Users\rahul\tools\node22'
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

Write-Host 'Building .NET solution...' -ForegroundColor DarkYellow
dotnet build (Join-Path $root 'Noor.sln') --nologo
if ($LASTEXITCODE -ne 0) { throw 'The .NET build failed.' }

if (-not (Test-Path (Join-Path $web 'node_modules'))) {
    Write-Host 'Installing Angular dependencies...' -ForegroundColor DarkYellow
    Push-Location $web
    try { & npm.cmd install } finally { Pop-Location }
    if ($LASTEXITCODE -ne 0) { throw 'npm install failed.' }
}

Write-Host 'Building Angular production bundle...' -ForegroundColor DarkYellow
Push-Location $web
try { & npm.cmd run build } finally { Pop-Location }
if ($LASTEXITCODE -ne 0) { throw 'The Angular build failed.' }

Write-Host 'Build complete.' -ForegroundColor Green
