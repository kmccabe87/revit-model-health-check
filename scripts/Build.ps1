param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [int[]]$Years = @(2025, 2026, 2027)
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $here
$project = Join-Path $root "src\SVMModelHealth\SVMModelHealth.csproj"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Revit Model Health Check v0.6.13 - Revit 2025 / 2026 / 2027 Build" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

if (!(Test-Path $project)) { throw "Project file not found: $project" }

# Source preflight: catch the recurring CS1009 class of mistake before running three builds.
# This intentionally targets ordinary C# string literals containing Windows-style paths with
# unescaped backslashes. Verbatim strings (@"...") and escaped backslashes are allowed.
$sourceRoot = Join-Path $root "src\SVMModelHealth"
# Only inspect authored source files. WPF/MSBuild generates .cs files under obj\ and bin\
# (for example HealthDashboard.g.cs with #line paths such as "..\..\..\HealthDashboard.xaml").
# Those generated files are valid compiler output and must never fail this source preflight.
$csFiles = Get-ChildItem $sourceRoot -Filter *.cs -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](obj|bin)[\\/]'
}
$badEscapeHits = @()
foreach ($csFile in $csFiles) {
    $lineNo = 0
    foreach ($line in Get-Content $csFile.FullName) {
        $lineNo++
        if ($line -match '"[^"\r\n]*\\(?![\\"''0abfnrtvuxU])' -and $line -notmatch '@"') {
            $badEscapeHits += "$($csFile.FullName):${lineNo}: $line"
        }
    }
}
if ($badEscapeHits.Count -gt 0) {
    Write-Host "SOURCE PREFLIGHT FAILED: possible invalid C# escape sequence(s) found:" -ForegroundColor Red
    $badEscapeHits | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    exit 1
}
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) { throw ".NET SDK was not found. Install the .NET 8 SDK and .NET 10 SDK." }

$sdkLines = @(& dotnet --list-sdks)
$hasNet8Sdk = @($sdkLines | Where-Object { $_ -match '^8\.' }).Count -gt 0
$hasNet10Sdk = @($sdkLines | Where-Object { $_ -match '^10\.' }).Count -gt 0
if (($Years -contains 2025 -or $Years -contains 2026) -and !$hasNet8Sdk) {
    throw ".NET 8 SDK was not found. Revit 2025 and 2026 builds require .NET 8."
}
if (($Years -contains 2027) -and !$hasNet10Sdk) {
    throw ".NET 10 SDK was not found. Revit 2027 builds require .NET 10."
}

Write-Host "Framework matrix: Revit 2025/2026 = .NET 8; Revit 2027 = .NET 10" -ForegroundColor Gray

$failures = @()
foreach ($year in $Years) {
    $revitDir = "C:\Program Files\Autodesk\Revit $year"
    $api = Join-Path $revitDir "RevitAPI.dll"
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor DarkCyan
    Write-Host "Revit $year" -ForegroundColor DarkCyan
    Write-Host "============================================================" -ForegroundColor DarkCyan
    if (!(Test-Path $api)) {
        Write-Host "BUILD FAILED: Revit $year API not found at $revitDir" -ForegroundColor Red
        $failures += $year
        continue
    }

    $outDir = Join-Path $root "build\$year\"
    if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null

    $framework = if ($year -eq 2027) { "net10.0-windows" } else { "net8.0-windows" }
    Write-Host "Target framework: $framework" -ForegroundColor Gray

    & dotnet build $project -c $Configuration -f $framework -p:RevitYear=$year -p:RevitInstallDir="$revitDir" -p:OutputPath="$outDir"
    if ($LASTEXITCODE -ne 0 -or !(Test-Path (Join-Path $outDir "SVMModelHealth.dll"))) {
        Write-Host "BUILD FAILED for Revit $year" -ForegroundColor Red
        $failures += $year
    } else {
        Write-Host "BUILD SUCCEEDED for Revit $year" -ForegroundColor Green
        Write-Host "Output: $outDir" -ForegroundColor Green
    }
}

if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "OVERALL BUILD FAILED. Failed Revit year(s): $($failures -join ', ')" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "ALL BUILDS SUCCEEDED: Revit 2025, 2026, and 2027" -ForegroundColor Green
exit 0
