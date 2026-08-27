param([string]$Root)

$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($Root)) {
    $Root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
}
$Root = (Resolve-Path $Root).Path

function Require-File([string]$RelativePath) {
    $path = Join-Path $Root $RelativePath
    if (!(Test-Path $path -PathType Leaf)) {
        throw "Required file is missing: $RelativePath"
    }
    return $path
}

function Require-Match([string]$RelativePath, [string]$Pattern, [string]$Message) {
    $path = Require-File $RelativePath
    $text = Get-Content $path -Raw
    if ($text -notmatch $Pattern) { throw "$Message ($RelativePath)" }
}

$version = (Get-Content (Require-File "VERSION") -Raw).Trim()
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "VERSION is not semantic: $version" }
$escapedVersion = [regex]::Escape($version)

$required = @(
    "README.md",
    "CHANGELOG.md",
    "PROJECT_INSTRUCTIONS.md",
    "RELEASE_CHECKLIST.md",
    "update-manifest.json",
    "src/SVMModelHealth/SVMModelHealth.csproj",
    "src/SVMModelHealth/App.cs",
    "src/SVMModelHealth/HealthDashboard.xaml",
    "config/health-rules.json",
    ".codex/skills/revit-model-health-check-dev/SKILL.md"
)
$required | ForEach-Object { [void](Require-File $_) }

Require-Match "src/SVMModelHealth/SVMModelHealth.csproj" "<Version>$escapedVersion</Version>" "Project version is out of sync"
Require-Match "src/SVMModelHealth/SVMModelHealth.csproj" '<TargetFramework Condition="''\$\(RevitYear\)'' != ''2027''">net8\.0-windows</TargetFramework>' "Revit 2025/2026 framework mapping changed"
Require-Match "src/SVMModelHealth/SVMModelHealth.csproj" '<TargetFramework Condition="''\$\(RevitYear\)'' == ''2027''">net10\.0-windows</TargetFramework>' "Revit 2027 framework mapping changed"
Require-Match "src/SVMModelHealth/App.cs" 'const string tabName = "Pre-Publish Checks";' "Ribbon tab identity changed"
Require-Match "src/SVMModelHealth/App.cs" 'const string panelName = "Health Check";' "Ribbon panel identity changed"
Require-Match "src/SVMModelHealth/App.cs" '"Scan Model"' "Ribbon button identity changed"
Require-Match "scripts/Package.ps1" ('\$version = "' + $escapedVersion + '"') "Package version is out of sync"

$manifest = Get-Content (Require-File "update-manifest.json") -Raw | ConvertFrom-Json
if ($manifest.version -ne $version) { throw "Update manifest version is out of sync" }
if ($manifest.frameworks.'2027' -ne 'net10.0-windows') { throw "Manifest maps Revit 2027 to the wrong framework" }
if ($manifest.releaseApproved -and (!$manifest.assets.source.url -or !$manifest.assets.source.sha256 -or !$manifest.assets.distribution.url -or !$manifest.assets.distribution.sha256)) {
    throw "An approved manifest must include URLs and SHA-256 values for both assets"
}

Get-Content (Require-File "config/health-rules.json") -Raw | ConvertFrom-Json | Out-Null

$sourceText = Get-ChildItem $Root -Recurse -File -Include *.cs,*.csproj,*.ps1,*.cmd,*.json,*.md |
    Where-Object { $_.FullName -notmatch '[\\/](build|dist|bin|obj)[\\/]' } |
    ForEach-Object { Get-Content $_.FullName -Raw }
if (($sourceText -join "`n") -match 'C:\\Users\\[^<%$.*]') {
    throw "A machine-specific C:\Users path was detected"
}

Write-Host "Repository validation passed for v$version." -ForegroundColor Green
Write-Host "This is static validation only; Windows/Revit build and behavior gates remain required." -ForegroundColor Yellow
