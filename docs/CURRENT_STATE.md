# Current State - v0.6.12

## Latest user-facing changes
- Ribbon tab renamed to **Pre-Publish Checks**.
- Ribbon button renamed to **Scan Model**.
- WPF branding white/blue title lines centered relative to each other.
- WPF top area has been tightened compared with earlier builds.
- Issue Details shows a vertical scrollbar automatically when content exceeds the available height.
- Centerline visibility check is scoped to the active view and considers effective parent/subcategory visibility.

## Preserve
- Health Check rules and reports.
- Performance Analyzer and its safeguards.
- Sortable grids and selection behavior.
- WPF dashboard theme and custom chrome.
- Multi-year installer and Install All workflow.
- Distribution-package workflow.
- `%APPDATA%\\Autodesk\\Revit\\Addins\\<year>` per-user deployment.
- Framework matrix: 2025/2026 = .NET 8; 2027 = .NET 10.

## Pending GitHub direction
GitHub Releases should eventually serve both handoff and end-user updates. The add-in may later check a release manifest for a newer version and launch an external updater after Revit closes. Do not overwrite the running add-in DLL from inside Revit.
