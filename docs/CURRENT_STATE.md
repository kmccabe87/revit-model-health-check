# Current State - v0.6.16

## v0.6.16 source and distribution
- Added STRATUS mapped-parameter consistency and fabrication publish-readiness checks.
- Added publish-complexity context, nested-family counts, diagnostic Publish Weight,
  slowest-element reporting, and expanded performance CSV output.
- Revit 2025/2026 builds select .NET 8 or .NET 10 from the installed Revit API
  update; Revit 2027 remains on .NET 10.
- Distribution packaging captures and verifies the exact installed v0.6.16 payload
  for Revit 2025, 2026, and 2027.
- A ready-to-install v0.6.16 Distribution ZIP was supplied with all three compiled payloads.

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
- Framework selection: Revit 2025/2026 before update 5 = .NET 8;
  Revit 2025.5+/2026.5+ and Revit 2027 = .NET 10.

## GitHub direction
The public GitHub repository is the source, handoff, and future release/update channel. The add-in may later check a release manifest for a newer version and launch an external updater after Revit closes. Do not overwrite the running add-in DLL from inside Revit.
