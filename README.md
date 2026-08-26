# Revit Model Health Check v0.4.1

Distribution-ready Revit add-in for **Revit 2025, 2026, and 2027**.

## Ribbon
**STRATUS Model Check** > **Model Health** > **Health Check**

The Health Check button now uses a dedicated clipboard + magnifier + MEP model health icon.

The single Health Check command opens the combined dashboard:
- **Health Check** tab: QA/QC checks, health score, element selection, HTML/CSV export.
- **Performance** tab: on-demand performance profiling. It does not run automatically.

## Framework matrix
- Revit 2025: .NET 8 (`net8.0-windows`)
- Revit 2026: .NET 8 (`net8.0-windows`)
- Revit 2027: .NET 10 (`net10.0-windows`)

## Build for all supported Revit versions
1. Extract the source ZIP completely.
2. Double-click `BUILD.cmd`.
3. All three installed Revit targets must build successfully.
4. Outputs are written to `build\2025`, `build\2026`, and `build\2027`.

## Create a package you can give to other people
After `BUILD.cmd` succeeds for all three versions, double-click **`PACKAGE.cmd`**.

It creates:

`dist\Revit_Model_Health_Check_v0.4.1_Distribution.zip`

Give that Distribution ZIP to another user. They do **not** need the source code or .NET SDK to install it. They only need to:
1. Extract the Distribution ZIP completely.
2. Save/sync and close every Revit session.
3. Double-click `INSTALL.cmd`.
4. Start the Revit version they use.

## Installed names
Every supported Revit year now uses the same clean deployment names inside its own year-specific folder:

- Manifest: `Revit Model Health Check.addin`
- Add-in folder: `Revit Model Health Check`
- User-facing add-in name: `Revit Model Health Check`
- Ribbon tab: `STRATUS Model Check`
- Ribbon button: `Health Check`

Typical locations are:

`%APPDATA%\Autodesk\Revit\Addins\2025\Revit Model Health Check.addin`

`%APPDATA%\Autodesk\Revit\Addins\2026\Revit Model Health Check.addin`

`%APPDATA%\Autodesk\Revit\Addins\2027\Revit Model Health Check.addin`

The installer also removes the legacy `SVMModelHealth.addin` and `SVMModelHealth` deployment folder after the new installation verifies successfully, preventing duplicate ribbon loads.

## Uninstall
Close Revit and run `UNINSTALL.cmd`. It removes both the current deployment names and any legacy pre-v0.4.0 names.

## Version handoff
- Baseline: **v0.4.0**
- Output: **v0.4.1**
- Added a dedicated Health Check ribbon icon in 16 px, 32 px, and 64 px assets.
- Health checks, centerline check, performance analyzer, sorting, responsive UI, reports, deployment naming, ribbon behavior, and profiling logic remain unchanged.

## Test checklist
- `BUILD.cmd` succeeds for Revit 2025, 2026, and 2027.
- Revit 2025/2026 build as .NET 8; Revit 2027 builds as .NET 10.
- `INSTALL.cmd` creates `Revit Model Health Check.addin` in all three year folders.
- `INSTALL.cmd` creates the `Revit Model Health Check` add-in folder in all three year folders.
- Legacy `SVMModelHealth.addin` / `SVMModelHealth` deployments are removed after successful install.
- Revit shows exactly one **STRATUS Model Check** ribbon tab and one **Health Check** button.
- Health and Performance tabs still work and sortable columns remain sortable.
- `PACKAGE.cmd` creates the Distribution ZIP.
- On a second workstation, the Distribution ZIP installs without source code or a build step.

## Known limitation
The source package cannot be compiled in this Linux-based handoff environment because the Autodesk Revit assemblies are not installed here. Final acceptance requires running `BUILD.cmd` against installed Revit 2025, 2026, and 2027 and then testing the generated Distribution ZIP on Windows/Revit.
