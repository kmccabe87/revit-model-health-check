param([string]$Years = "2025,2026,2027")
$Years = @($Years -split ',' | ForEach-Object { [int]$_.Trim() } | Where-Object { $_ -in @(2025, 2026, 2027) })
if ($Years.Count -eq 0) {
    Write-Host "INSTALL FAILED: no supported Revit years were selected." -ForegroundColor Red
    exit 2
}
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $here
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Revit Model Health Check v0.6.12 - Revit 2025 / 2026 / 2027 Installer" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$running = @(Get-Process Revit -ErrorAction SilentlyContinue)
if ($running.Count -gt 0) {
    Write-Host "Revit is currently running." -ForegroundColor Yellow
    Write-Host "Save/sync all open work and close every Revit session normally, then run INSTALL.cmd again." -ForegroundColor Yellow
    Write-Host "The installer will not force-close Revit." -ForegroundColor Yellow
    exit 3
}

$failed = @()
foreach ($year in $Years) {
    $source = Join-Path $root "build\$year"
    $dll = Join-Path $source "SVMModelHealth.dll"
    if (!(Test-Path $dll)) {
        Write-Host "INSTALL FAILED for Revit ${year}: build output missing. Run BUILD.cmd first, or use the Distribution ZIP produced by PACKAGE.cmd." -ForegroundColor Red
        $failed += $year
        continue
    }

    $addinRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$year"
    $pluginDir = Join-Path $addinRoot "Revit Model Health Check"
    $manifestPath = Join-Path $addinRoot "Revit Model Health Check.addin"

    New-Item -ItemType Directory -Force -Path $addinRoot | Out-Null
    if (Test-Path $pluginDir) { Remove-Item $pluginDir -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
    Copy-Item (Join-Path $source "*") $pluginDir -Recurse -Force

    $assembly = Join-Path $pluginDir "SVMModelHealth.dll"
    $manifest = @"
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>Revit Model Health Check</Name>
    <Assembly>$assembly</Assembly>
    <AddInId>8E4E28AA-1D79-43E6-A44C-3DBA6AA82617</AddInId>
    <FullClassName>SVMModelHealth.App</FullClassName>
    <VendorId>SVM</VendorId>
    <VendorDescription>Silicon Valley Mechanical</VendorDescription>
  </AddIn>
</RevitAddIns>
"@
    Set-Content -Path $manifestPath -Value $manifest -Encoding UTF8

    if (!(Test-Path $assembly) -or !(Test-Path $manifestPath)) {
        Write-Host "INSTALL FAILED for Revit $year" -ForegroundColor Red
        $failed += $year
        continue
    }

    # Remove legacy deployment names only after the new deployment verifies successfully.
    Remove-Item (Join-Path $addinRoot "SVMModelHealth.addin") -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $addinRoot "SVMModelHealth") -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host "INSTALL SUCCEEDED for Revit $year" -ForegroundColor Green
    Write-Host "  Manifest: $manifestPath" -ForegroundColor DarkGreen
    Write-Host "  Add-in folder: $pluginDir" -ForegroundColor DarkGreen
}

if ($failed.Count -gt 0) {
    Write-Host "Installation incomplete. Failed: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}
Write-Host "ALL INSTALLS SUCCEEDED" -ForegroundColor Green
exit 0
