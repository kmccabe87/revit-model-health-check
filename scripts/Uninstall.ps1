param([int[]]$Years = @(2025, 2026, 2027))
$ErrorActionPreference = "Stop"

$running = @(Get-Process Revit -ErrorAction SilentlyContinue)
if ($running.Count -gt 0) {
    Write-Host "Revit is currently running. Close Revit before uninstalling." -ForegroundColor Yellow
    exit 3
}

foreach ($year in $Years) {
    $addinRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$year"

    # Current distribution names.
    Remove-Item (Join-Path $addinRoot "Revit Model Health Check.addin") -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $addinRoot "Revit Model Health Check") -Recurse -Force -ErrorAction SilentlyContinue

    # Legacy names from pre-v0.4.0 packages.
    Remove-Item (Join-Path $addinRoot "SVMModelHealth.addin") -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $addinRoot "SVMModelHealth") -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host "Removed Revit Model Health Check from Revit $year" -ForegroundColor Green
}
exit 0
