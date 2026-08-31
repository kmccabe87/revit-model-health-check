# Changelog

All notable changes to Revit Model Health Check are recorded here. A version
entry describes the source state; it does not prove release approval. Consult
`RELEASE_CHECKLIST.md` and `update-manifest.json` for verification status.

## [Unreleased]

- No product behavior changes yet.

## [0.6.16] - 2026-08-31

- Added STRATUS mapped-parameter consistency and fabrication publish-readiness checks.
- Added publish-complexity context, nested-family counts, diagnostic Publish Weight,
  slowest-element reporting, and a Top Slowest Elements CSV section.
- Updated the build to select .NET 8 or .NET 10 for Revit 2025/2026 from the
  installed Revit API version; Revit 2027 remains on .NET 10.
- Changed release packaging to capture the exact locally installed/tested payload
  for Revit 2025, 2026, and 2027.
- Preserved the WPF dashboard, active-view centerline logic, manual Performance
  Analyzer, per-user installation, and existing reporting/selection behavior.

## [0.6.13] - 2026-08-26

- Fixed the build preflight so it scans authored C# only and ignores generated
  `obj`/`bin` files such as WPF `HealthDashboard.g.cs`.
- Corrected preflight diagnostics to report the real source path and line.
- Synchronized project, assembly, dashboard, build, installer, package, and
  documentation version metadata to 0.6.13.
- Confirmed that supplied Revit 2025, 2026, and 2027 build outputs match the
  staged Distribution ZIP contents.
- Preserved Health Check, Performance Analyzer, WPF UI, ribbon, installer, and
  deployment behavior.

## [0.6.12] - 2026-08-26

- Renamed the ribbon to **Pre-Publish Checks > Health Check > Scan Model**.
- Rebuilt the combined dashboard in WPF with dark custom chrome and styling.
- Kept Performance Analyzer manual and added safer physical-model filtering and
  breadcrumb diagnostics.
- Scoped centerline visibility to the active view, including effective parent,
  subcategory, and applicable view-template control.
- Added automatic scrolling to overflowing Issue Details content.
- Preserved per-user deployment and the Revit 2025/2026 .NET 8 plus Revit 2027
  .NET 10 framework matrix.
- Corrected Install All so it invokes the proven single-year path sequentially.
- Added repository instructions, reusable project skill, static validation,
  release gates, and an update-manifest contract.

## [0.4.1]

- Added the dedicated Health Check ribbon icon assets.
- This was the previous public GitHub baseline before the v0.6.12 import.
