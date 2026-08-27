# Source Inventory - v0.6.12

## Identity and compatibility

| Item | Baseline |
| --- | --- |
| Product | Revit Model Health Check |
| Assembly/project | `SVMModelHealth` |
| Revit 2025 | `net8.0-windows` |
| Revit 2026 | `net8.0-windows` |
| Revit 2027 | `net10.0-windows` |
| Ribbon | Pre-Publish Checks > Health Check > Scan Model |
| Dashboard | WPF |
| Deployment | `%APPDATA%\Autodesk\Revit\Addins\<year>` |

## Main components

- `src/SVMModelHealth/App.cs`: Revit application and ribbon registration.
- `src/SVMModelHealth/ModelHealthCommand.cs`: command entry point.
- `src/SVMModelHealth/HealthScanner.cs`: health-check collection, including
  active-view centerline visibility.
- `src/SVMModelHealth/PerformanceAnalyzer.cs`: opt-in performance profiling.
- `src/SVMModelHealth/HealthDashboard.xaml*`: combined WPF dashboard.
- `src/SVMModelHealth/ReportWriter.cs`: HTML report output.
- `config/health-rules.json`: thresholds, weights, severity, and guidance.
- `scripts/Build.ps1`: multi-year framework and Revit API build matrix.
- `scripts/Install*.ps1`: per-user single/all-year installation.
- `scripts/Package.ps1`: ready-to-install Distribution ZIP packaging.
- `assets/`: transparent 16, 32, and 64 pixel ribbon icon assets.

## Baseline environment result

The handoff environment is Linux and does not contain PowerShell, the .NET 8
and .NET 10 SDKs, or Autodesk Revit assemblies. The Windows/Revit build and
runtime acceptance gates therefore remain pending. Repository static checks are
separate and must never be presented as Revit verification.
