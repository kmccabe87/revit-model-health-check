# Revit Model Health Check

A modern QA/QC dashboard for Autodesk Revit that helps teams find model-health problems before publishing or exchanging a model.

Current version: **v0.6.16**

## Download and install

**[Download v0.6.16 — ready-to-install Distribution ZIP](https://github.com/kmccabe87/revit-model-health-check/releases/download/v0.6.16/Revit_Model_Health_Check_v0.6.16_Distribution.zip)**

You do **not** need Visual Studio, source code, or a .NET SDK.

1. Download and extract the Distribution ZIP.
2. Save or sync your work and close every Revit session.
3. Double-click `INSTALL.cmd`.
4. Choose Revit 2025, 2026, 2027, or **Install All**.
5. Start Revit and open **Pre-Publish Checks > Health Check > Scan Model**.

The Source ZIP and GitHub's automatic source downloads are developer assets. Previous versions are under [Releases](https://github.com/kmccabe87/revit-model-health-check/releases).

## Highlights

- Dark WPF dashboard designed to feel native alongside Revit.
- One-click model scan with a weighted health score and clear issue guidance.
- Searchable, sortable results with category filters and element selection.
- CSV and HTML report exports for coordination and documentation.
- STRATUS mapped-parameter and fabrication publish-readiness checks.
- Manual, opt-in Performance Analyzer with publish-complexity context and slowest-element reporting.
- Per-user installation with support for Revit 2025, 2026, and 2027.

## Model checks

The Health Check scans for major model-quality concerns, including:

- Revit warnings
- Imported and linked CAD
- In-place families
- Model and detail groups
- Unpinned or unloaded Revit links
- Unplaced or unenclosed rooms and MEP spaces
- Unused view templates
- Visible centerlines in the active view
- Excessive user worksets
- STRATUS mapped-parameter consistency
- Fabrication publish readiness

Results are grouped by category and severity, with recommended actions for each issue.

## Performance Analyzer

The Performance tab profiles physical model elements and reports expensive regeneration behavior. It runs only when requested, skips styles/types/non-model definitions, and reports publish-complexity context, nested families, diagnostic Publish Weight, and the slowest elements.

## Supported versions

| Revit | Framework |
| --- | --- |
| 2025 | .NET 8 before 2025.5; .NET 10 for 2025.5+ |
| 2026 | .NET 8 before 2026.5; .NET 10 for 2026.5+ |
| 2027 | .NET 10 |

Installation is per user under `%APPDATA%\Autodesk\Revit\Addins\<year>` and does not require administrator access. To remove the add-in, close Revit and run `UNINSTALL.cmd`.

## Building from source

The corresponding Revit versions must be installed so their API assemblies are available.

1. Extract or clone the repository.
2. Run `BUILD.cmd` to build Revit 2025, 2026, and 2027.
3. Run `INSTALL.cmd` and test the add-in in each supported Revit version.
4. Close Revit and run `CREATE DISTRIBUTION FROM INSTALLED.cmd` to capture the tested payload.

Build outputs are written to `build\2025`, `build\2026`, and `build\2027`.

## License

Released under the [MIT License](LICENSE).
