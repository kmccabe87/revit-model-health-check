$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$installer = Join-Path $here "Install.ps1"

if (!(Test-Path $installer)) {
    Write-Host "INSTALL FAILED: scripts\Install.ps1 was not found." -ForegroundColor Red
    Write-Host "Extract the ZIP completely before running INSTALL.cmd." -ForegroundColor Yellow
    exit 2
}

function Get-InstallMark([int]$year) {
    $manifest = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$year\Revit Model Health Check.addin"
    if (Test-Path $manifest) { return "Installed" }
    return "Not installed"
}

function Show-Menu {
    Clear-Host
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host "Revit Model Health Check v0.6.12" -ForegroundColor Cyan
    Write-Host "Choose Revit version(s) to install" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host ("  [1] Revit 2025  [{0}]" -f (Get-InstallMark 2025))
    Write-Host ("  [2] Revit 2026  [{0}]" -f (Get-InstallMark 2026))
    Write-Host ("  [3] Revit 2027  [{0}]" -f (Get-InstallMark 2027))
    Write-Host "  [A] Install All (2025, 2026, 2027)"
    Write-Host ""
    Write-Host "  [Esc] Exit"
    Write-Host ""
    Write-Host "Install one year and this menu will return so you can choose another." -ForegroundColor DarkGray
}

function Invoke-SelectedInstall([int[]]$years) {
    Write-Host ""
    $results = @()
    foreach ($year in $years) {
        Write-Host ("Installing Revit Model Health Check for Revit {0}..." -f $year) -ForegroundColor Cyan
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $installer -Years "$year"
        $code = $LASTEXITCODE
        $results += [pscustomobject]@{ Year = $year; ExitCode = $code }
        if ($code -eq 0) {
            Write-Host ("Revit {0}: INSTALLED" -f $year) -ForegroundColor Green
        } else {
            Write-Host ("Revit {0}: FAILED (exit code {1})" -f $year, $code) -ForegroundColor Red
        }
        Write-Host ""
    }

    $failed = @($results | Where-Object { $_.ExitCode -ne 0 })
    if ($failed.Count -eq 0) {
        Write-Host "All selected versions completed successfully." -ForegroundColor Green
    } else {
        Write-Host ("Installation completed with failures: {0}" -f (($failed | ForEach-Object { $_.Year }) -join ', ')) -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Press Esc to exit, or any other key to return to the install menu..." -ForegroundColor Gray
    $key = [Console]::ReadKey($true)
    if ($key.Key -eq [ConsoleKey]::Escape) {
        Write-Host "Installer closed." -ForegroundColor Gray
        exit 0
    }
    return $true
}

$keepRunning = $true
while ($keepRunning) {
    Show-Menu
    $key = [Console]::ReadKey($true)
    switch ($key.Key) {
        'D1' { $keepRunning = Invoke-SelectedInstall @(2025) }
        'NumPad1' { $keepRunning = Invoke-SelectedInstall @(2025) }
        'D2' { $keepRunning = Invoke-SelectedInstall @(2026) }
        'NumPad2' { $keepRunning = Invoke-SelectedInstall @(2026) }
        'D3' { $keepRunning = Invoke-SelectedInstall @(2027) }
        'NumPad3' { $keepRunning = Invoke-SelectedInstall @(2027) }
        'A' { $keepRunning = Invoke-SelectedInstall @(2025, 2026, 2027) }
        'Escape' { $keepRunning = $false }
        default { }
    }
}

Write-Host "Installer closed." -ForegroundColor Gray
exit 0
