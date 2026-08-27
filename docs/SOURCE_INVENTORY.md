# Source Inventory - v0.6.13

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

## Build/package evidence received

- Revit 2025, 2026, and 2027 DLL/PDB/dependency outputs were supplied.
- Each supplied DLL is byte-identical to its matching staged distribution DLL.
- The supplied Distribution ZIP passes archive integrity testing.
- The v0.6.13 delta fixes the build preflight false positive caused by generated
  WPF/MSBuild C# under `obj`/`bin`.

The current handoff environment cannot independently rebuild against Autodesk
Revit assemblies or perform runtime Revit verification. Supplied build evidence
is recorded separately from in-Revit acceptance.
