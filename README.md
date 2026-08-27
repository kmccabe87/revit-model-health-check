# Revit Model Health Check

Model QA/QC and opt-in performance analysis for Autodesk Revit 2025, 2026, and 2027.

## Download and install

**Most users should download the ready-to-install Distribution ZIP from [GitHub Releases](https://github.com/kmccabe87/revit-model-health-check/releases).**

You do **not** need Visual Studio, the source code, or the .NET SDK.

1. Download `Revit_Model_Health_Check_vX.Y.Z_Distribution.zip` from the newest tested release.
2. Extract the ZIP completely.
3. Save your work and close every Revit session.
4. Double-click `INSTALL.cmd`.
5. Choose Revit 2025, 2026, 2027, or **Install All**.
6. Reopen Revit and use **Pre-Publish Checks > Health Check > Scan Model**.

> The Source ZIP and GitHub's automatic source downloads are for developers. For normal installation, use the asset with **Distribution** in its filename.

## What it does

- Runs model-health checks and presents a compact health score and actionable issue list.
- Checks effective centerline visibility in the active view, including parent-category and applicable view-template control.
- Exports health results to HTML and CSV and helps locate affected elements.
- Provides a manual, opt-in Performance Analyzer with safe filtering for model elements.
- Uses a dark WPF dashboard with sortable, searchable results and scrollable issue details.
- Supports one-click per-user installation for Revit 2025, 2026, 2027, or all supported versions.

**Less mystery. Better models.**

## Supported Revit versions

| Revit | Target framework |
|---|---|
| 2025 | .NET 8 (`net8.0-windows`) |
| 2026 | .NET 8 (`net8.0-windows`) |
| 2027 | .NET 10 (`net10.0-windows`) |

## Uninstall

Close Revit, extract the Distribution ZIP if needed, and run `UNINSTALL.cmd`.

## For developers

The latest imported source is **v0.6.13**. Tested installable versions are published separately under [Releases](https://github.com/kmccabe87/revit-model-health-check/releases).

To build locally:

1. Install the supported Revit versions and the required .NET SDKs.
2. Clone or download the source and run `BUILD.cmd` on Windows.
3. Confirm all three targets succeed.
4. Run `PACKAGE.cmd` to create the Distribution ZIP.
5. Exercise the add-in in each supported Revit version before publishing a release.

Build outputs are written to `build\2025`, `build\2026`, and `build\2027`. See [CHANGELOG.md](CHANGELOG.md) for version history and [PROJECT_INSTRUCTIONS.md](PROJECT_INSTRUCTIONS.md) for contribution and release requirements.

## License

Released under the [MIT License](LICENSE).
