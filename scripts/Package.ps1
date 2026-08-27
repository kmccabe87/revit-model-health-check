$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $here
$version = "0.6.13"
$years = @(2025, 2026, 2027)

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Revit Model Health Check v$version - Distribution Packager" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$missing = @()
foreach ($year in $years) {
    $dll = Join-Path $root "build\$year\SVMModelHealth.dll"
    if (!(Test-Path $dll)) { $missing += $year }
}
if ($missing.Count -gt 0) {
    Write-Host "PACKAGE FAILED: missing successful build output for Revit year(s): $($missing -join ', ')" -ForegroundColor Red
    Write-Host "Run BUILD.cmd successfully before PACKAGE.cmd." -ForegroundColor Yellow
    exit 1
}

$distRoot = Join-Path $root "dist"
$stage = Join-Path $distRoot "Revit_Model_Health_Check_v${version}_Distribution"
$zip = Join-Path $distRoot "Revit_Model_Health_Check_v${version}_Distribution.zip"

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
if (Test-Path $zip) { Remove-Item $zip -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null

Copy-Item (Join-Path $root "INSTALL.cmd") $stage -Force
Copy-Item (Join-Path $root "UNINSTALL.cmd") $stage -Force
Copy-Item (Join-Path $root "README.md") $stage -Force
New-Item -ItemType Directory -Force -Path (Join-Path $stage "scripts") | Out-Null
Copy-Item (Join-Path $root "scripts\Install.ps1") (Join-Path $stage "scripts\Install.ps1") -Force
Copy-Item (Join-Path $root "scripts\InstallMenu.ps1") (Join-Path $stage "scripts\InstallMenu.ps1") -Force
Copy-Item (Join-Path $root "scripts\Uninstall.ps1") (Join-Path $stage "scripts\Uninstall.ps1") -Force

foreach ($year in $years) {
    $target = Join-Path $stage "build\$year"
    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Copy-Item (Join-Path $root "build\$year\*") $target -Recurse -Force
}

Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $zip -CompressionLevel Optimal
Write-Host "DISTRIBUTION PACKAGE CREATED:" -ForegroundColor Green
Write-Host $zip -ForegroundColor Green
Write-Host "Recipients only need to extract this ZIP, close Revit, and double-click INSTALL.cmd. They can install 2025, 2026, 2027, or all three." -ForegroundColor Gray
exit 0
