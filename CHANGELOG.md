# Changelog

All notable changes to Revit Model Health Check are recorded here. A version
entry describes the source state; it does not prove release approval. Consult
`RELEASE_CHECKLIST.md` and `update-manifest.json` for verification status.

## [Unreleased]

- No product behavior changes yet.

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
