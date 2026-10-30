[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if (-not $Force) {
    Write-Host 'This permanently drops the NoorDb database.' -ForegroundColor Yellow
    $answer = Read-Host 'Type YES to continue'
    if ($answer -ne 'YES') {
        Write-Host 'Cancelled.'
        exit 0
    }
}

Push-Location $root
try {
    dotnet tool restore
    dotnet ef database drop `
        --project 'backend\Noor.Infrastructure' `
        --startup-project 'backend\Noor.Api' `
        --force

    if ($LASTEXITCODE -ne 0) { throw 'The database drop failed.' }

    Write-Host 'NoorDb dropped. Start the API to recreate and seed it (Development auto-migrates).' -ForegroundColor Green
}
finally {
    Pop-Location
}
