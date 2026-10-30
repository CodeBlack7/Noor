[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Push-Location $root
try {
    Write-Host 'Running Noor test suite...' -ForegroundColor DarkYellow
    dotnet test (Join-Path $root 'Noor.sln') --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
}
finally {
    Pop-Location
}
