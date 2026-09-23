$ErrorActionPreference = "Stop"

function Test-Command {
    param(
        [Parameter(Mandatory)]
        [string] $Name
    )

    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if (-not $command) {
        throw "Required command '$Name' was not found."
    }

    Write-Host "[OK] $Name -> $($command.Source)"
}

Test-Command dotnet
Test-Command az
Test-Command azd

$dotnetVersion = dotnet --version
if ([version]$dotnetVersion -lt [version]"10.0.0") {
    throw ".NET 10 or later is required. Found $dotnetVersion."
}

Write-Host "[OK] dotnet version $dotnetVersion"

$account = az account show --output json 2>$null | ConvertFrom-Json
if (-not $account) {
    throw "Azure CLI is not signed in. Run 'az login'."
}

Write-Host "[OK] Azure subscription: $($account.name)"
Write-Host "[OK] Azure tenant: $($account.tenantId)"

az account get-access-token `
    --resource https://ai.azure.com `
    --query expiresOn `
    --output tsv | Out-Null

Write-Host "[OK] Foundry access token request succeeded."
Write-Host "Prerequisite checks completed."
