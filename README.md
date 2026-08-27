# Revit Model Health Check v0.6.13

## v0.6.13 build preflight fix
- Fixed the recurring false-positive C# escape preflight caused by WPF/MSBuild-generated `obj`/`bin` `.cs` files.
- The preflight now scans authored C# source only and ignores generated files such as `HealthDashboard.g.cs` whose `#line` directives legitimately contain Windows-style relative paths.
- Corrected preflight diagnostics so any future hit includes the actual source file path and line number.
- No Health Check, Performance Analyzer, WPF UI, ribbon, installer, or deployment behavior changed.

## v0.6.13 branding alignment

- Centered the white HEALTH CHECK title and blue Revit model QA / QC subtitle relative to one another in the WPF brand header.
- Removed the small left offset on the subtitle so both lines share the same visual center.
- No health-check, performance, installer, or deployment behavior changed.

Distribution-ready Revit add-in for **Revit 2025, 2026, and 2027**.

## Ribbon
**Pre-Publish Checks** > **Health Check** panel > **Scan Model** button

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

`dist\Revit_Model_Health_Check_v0.6.13_Distribution.zip`

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
- Ribbon tab: `Pre-Publish Checks`
- Ribbon button: `Scan Model`

Typical locations are:

`%APPDATA%\Autodesk\Revit\Addins\2025\Revit Model Health Check.addin`

`%APPDATA%\Autodesk\Revit\Addins\2026\Revit Model Health Check.addin`

`%APPDATA%\Autodesk\Revit\Addins\2027\Revit Model Health Check.addin`

The installer also removes the legacy `SVMModelHealth.addin` and `SVMModelHealth` deployment folder after the new installation verifies successfully, preventing duplicate ribbon loads.

## Uninstall
Close Revit and run `UNINSTALL.cmd`. It removes both the current deployment names and any legacy pre-v0.4.0 names.

## Version handoff
- Baseline: **v0.5.1**
- Output: **v0.6.13**
- Replaced the WinForms dashboard with a WPF dashboard so the UI can closely match the approved Revit Model Health Check reference without native white WinForms chrome.
- Added a custom borderless dark title bar, Revit Model Health Check brand header, blue Health/Performance tabs, dark metric cards, issue-category rail, searchable sortable WPF DataGrids, custom dark scrollbars, dark progress bar, and the **Less mystery. Better models.** footer.
- Preserved the single Health Check ribbon command, all health checks including centerline visibility, on-demand Performance Analyzer, element selection, CSV/HTML exports, sorting, performance safety filtering, deployment naming, installer menu, and share-package workflow.
- Revit framework matrix remains **2025/2026 = .NET 8** and **2027 = .NET 10**.
- Removed the WinForms dependency from the project, eliminating the .NET 10 WinForms designer/WFO warning path from the dashboard.

## Test checklist
- `BUILD.cmd` succeeds for Revit 2025, 2026, and 2027.
- Revit 2025/2026 build as .NET 8; Revit 2027 builds as .NET 10.
- `INSTALL.cmd` creates `Revit Model Health Check.addin` in all three year folders.
- `INSTALL.cmd` creates the `Revit Model Health Check` add-in folder in all three year folders.
- Legacy `SVMModelHealth.addin` / `SVMModelHealth` deployments are removed after successful install.
- Revit shows exactly one **Pre-Publish Checks** ribbon tab, one **Health Check** panel, and one **Scan Model** button.
- Health and Performance tabs are legible, use the same dark/blue theme, and sortable columns remain sortable.
- Grid scrollbars use dark-mode theming and detail panes have no white native scrollbar gutter.
- No white strip is visible above, behind, or immediately below the Health Check / Performance tabs.
- The Performance progress bar uses a dark track with blue progress fill; no white bar remains.
- Button labels are fully visible without clipping.
- Health Check ribbon icon has no white square background.
- Footer displays **Less mystery. Better models.**
- `PACKAGE.cmd` creates the Distribution ZIP.
- On a second workstation, the Distribution ZIP installs without source code or a build step.

## Known limitation
The source package cannot be compiled in this Linux-based handoff environment because the Autodesk Revit assemblies are not installed here. Final acceptance requires running `BUILD.cmd` against installed Revit 2025, 2026, and 2027 and then testing the generated Distribution ZIP on Windows/Revit.

## Distribution installer menu

The shareable Distribution ZIP produced by `PACKAGE.cmd` now contains a double-click `INSTALL.cmd` installer menu. Recipients do not need the source code or a .NET SDK.

Installer choices:

- `1` - Install Revit 2025 only
- `2` - Install Revit 2026 only
- `3` - Install Revit 2027 only
- `A` - Install all three supported Revit versions
- `Esc` - Exit

After installing a single Revit version, the installer returns to the menu so another year can be installed without reopening it. After any install, pressing `Esc` exits.

For sharing through GitHub, run `BUILD.cmd` successfully for all supported years, then run `CREATE SHARE PACKAGE.cmd` (or `PACKAGE.cmd`) and upload the generated `dist\Revit_Model_Health_Check_v0.6.13_Distribution.zip` as the release download.

## v0.6.13 WPF dashboard rebuild
- The dashboard presentation layer is now WPF rather than WinForms.
- The approved screenshot is the visual acceptance reference: dark navy shell, Revit Model Health Check header, blue active tab, dark cards, readable grids, dark scrollbars, and no white native control chrome.
- Health issues can be filtered by category and searched while retaining sortable columns.
- Performance results can be searched and sorted without rerunning the scan.
- Performance analysis remains manual/opt-in and continues to use the existing safe physical-model filtering and breadcrumb diagnostics.

## v0.6.13 WPF compile fix

- Qualifies WPF `Color` so it cannot collide with `Autodesk.Revit.DB.Color`.
- Qualifies WPF `Visibility` so the `Window.Visibility` instance property cannot shadow the enum.
- No health-check, performance-analysis, installer, or UI behavior was intentionally changed from v0.6.0.

## v0.6.13
- Removed the bright/white top edge from the custom WPF window by using `WindowChrome` with zero glass frame and by not painting a top border.
- Fixed **Install All** to run the same proven single-year installer sequentially for 2025, 2026, and 2027 and report each result independently.

## v0.6.13
- Renamed the remaining user-facing **STRATUS Model Check** branding to **Revit Model Health Check**.
- The Revit ribbon tab is now **Revit Model Health Check**.
- The WPF dashboard brand header now displays **REVIT MODEL / HEALTH CHECK** while preserving the existing visual layout and theme.
- No health-check, performance-analysis, installer, or deployment behavior was intentionally changed.

## v0.6.13 naming cleanup
- Shortened the Revit ribbon tab to **Health Check**.
- Renamed the ribbon command button to **Model Health**.
- Simplified the WPF title-bar and brand header to **Health Check** while keeping the existing model-health logo.
- Deployment/add-in folder and manifest names remain **Revit Model Health Check** so upgrades continue to replace the existing installation cleanly.

## v0.6.13 installer UX fix
After any install completes, pressing Esc now exits the installer immediately. Any other key returns to the year-selection menu. This removes the previous two-Escape behavior.

## v0.6.13 title-bar cleanup

- Removed the redundant `Health Check` text from the custom WPF title bar.
- Kept the app icon and version indicator in the title bar.
- The branded `HEALTH CHECK` header remains the single visual title inside the dashboard.
- No health-check, performance, installer, deployment, or Revit-version behavior changed.

## v0.6.13

- Baseline: v0.6.7.
- Corrected inherited version metadata inconsistencies so assembly, build, installer, package, and README all report v0.6.13.
- Tightened the WPF title/header area and centered the Health Check brand/logo group.
- Made the Health Issue Details pane vertically scrollable.
- Changed Centerlines Visible to inspect only the current active view.
- Centerline visibility is now effective visibility: a centerline subcategory is not reported visible when its parent category is hidden.
- When the active view has a template and that template controls the applicable Model/Annotation/Analytical V/G setting, the template's category visibility is used.
- No Performance Analyzer, report, selection, installer-menu, distribution, or unrelated health-check behavior was intentionally changed.

## v0.6.13 compact header and ribbon cleanup
- Reduced the custom WPF title bar, branding header, and tab strip heights to reclaim unused vertical space.
- Kept the centered HEALTH CHECK brand block while shrinking its icon/title footprint proportionally.
- Removed the visible `Model Health` text from the ribbon pushbutton so the button is icon-only; the ribbon panel title remains `Model Health`.
- No health-check, performance-analysis, centerline, reporting, or installer behavior was changed.


## v0.6.13 ribbon naming fix
- Changed the ribbon tab to **Tools for STRATUS**.
- Changed the ribbon panel to **Health Check**.
- Restored a non-empty pushbutton label and named it **Check Model for Publish** so Revit can create the button without an `ArgumentException`.
- No dashboard, scanner, performance, installer, or distribution behavior changed.

## v0.6.13 ribbon naming
- Renamed the Revit ribbon tab to **Pre-Publish Checks**.
- Renamed the ribbon pushbutton to **Scan Model**.
- Kept the **Health Check** panel and all command behavior unchanged.
