# Revit Model Health Check

A modern QA/QC dashboard for Autodesk Revit that helps teams find model-health problems before publishing or exchanging a model.

Current version: **v0.6.13**

## Highlights

- Dark WPF dashboard designed to feel native alongside Revit.
- One-click model scan with a weighted health score and clear issue guidance.
- Searchable, sortable results with category filters and element selection.
- CSV and HTML report exports for coordination and documentation.
- Manual, opt-in Performance Analyzer for finding slow model elements without running automatically.
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

Results are grouped by category and severity, with recommended actions for each issue.

## Performance Analyzer

The Performance tab profiles physical model elements and reports expensive regeneration behavior. It runs only when requested and includes safeguards that skip styles, types, and other non-model definitions that can produce misleading results or Revit API errors.

## Ribbon location

**Pre-Publish Checks** > **Health Check** > **Scan Model**

## Supported versions

| Revit | Framework |
| --- | --- |
| 2025 | .NET 8 |
| 2026 | .NET 8 |
| 2027 | .NET 10 |

## Installation

1. Download and extract the versioned Distribution ZIP.
2. Save or sync your work and close every Revit session.
3. Double-click `INSTALL.cmd`.
4. Choose Revit 2025, 2026, 2027, or **Install All**.
5. Start Revit and open **Pre-Publish Checks**.

Installation is per user and does not require administrator access, Visual Studio, or the .NET SDK.

Files are installed under:

```text
%APPDATA%\Autodesk\Revit\Addins\<year>
```

To remove the add-in, close Revit and run `UNINSTALL.cmd`.

## Building from source

The corresponding Revit versions must be installed so their API assemblies are available.

1. Extract or clone the repository.
2. Run `BUILD.cmd` to build Revit 2025, 2026, and 2027.
3. Run `PACKAGE.cmd` to create the shareable Distribution ZIP.

Build outputs are written to `build\2025`, `build\2026`, and `build\2027`.

## License

Released under the [MIT License](LICENSE).
