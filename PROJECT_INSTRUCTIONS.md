# Revit Model Health Check - Project Instructions

## Source of truth
- At the start of every development chat, use the newest attached Revit Model Health Check source ZIP as the implementation source of truth.
- Do not recreate the add-in from memory or from an older package.
- Before editing, unpack and inventory the package: version, Revit targets, project files, commands, ribbon tab/panel/button text, WPF views, icons, settings/config files, build scripts, installers, distribution packaging, and deployment folder names.
- When practical, run the baseline build before editing and record pre-existing failures.

## Change discipline
- Preserve every working feature and unrelated behavior unless explicitly asked to remove or redesign it.
- Make the smallest coherent change.
- Increment the version for every delivered build and synchronize that version across assembly/project metadata, README, build scripts, installer scripts, package scripts, manifests, and release artifacts.
- Never claim a build is accepted from static inspection alone. Build every supported Revit version and exercise the reported case in Revit when possible.

## Supported Revit/framework matrix
- Revit 2025: `net8.0-windows`
- Revit 2026: `net8.0-windows`
- Revit 2027: `net10.0-windows`
- Do not accidentally move Revit 2027 back to .NET 8.

## Current product identity
- Product/install name: **Revit Model Health Check**
- Ribbon tab: **Pre-Publish Checks**
- Ribbon panel: **Health Check**
- Ribbon button: **Scan Model**
- Revit ribbon button text must never be empty; `PushButtonData` rejects empty `Text`.
- Main UI is WPF, not WinForms.
- Tagline: **Less mystery. Better models.**

## Current UI requirements
- Keep the WPF dashboard visually close to the approved dark navy/blue reference design.
- Avoid native white/light chrome or stray borders on any side of the window.
- Keep the header compact and centered.
- Issue Details must scroll vertically when content overflows.
- Performance Analyzer must remain inside the same window and must never auto-run when the dashboard opens.
- Preserve dark custom scrollbars, grid styling, sorting, buttons, metric cards, and transparent icon assets.

## Health Check behavior
- Preserve all existing health rules unless explicitly changed.
- Centerline visibility must answer only whether centerlines are effectively visible in the **current active view**.
- A Centerline subcategory is effectively visible only when it and all required parent categories are visible.
- If a view template controls the relevant visibility settings, evaluate the effective template-controlled state for the active view; do not scan every project view/template.

## Performance Analyzer behavior
- It is opt-in/manual only.
- Preserve filtering that skips style/type/non-model definitions that previously caused crashes (for example line/graphics styles such as `Linear - 3/64" Arial - Baseline`).
- Show enough breadcrumb/status information to identify the current element if a failure occurs.
- Preserve sortable results and element selection/show behavior.

## Installer/distribution requirements
- Normal installation is per-user and must use `%APPDATA%\Autodesk\Revit\Addins\<year>`.
- Never hardcode a Windows username or machine-specific path.
- Distribution users should not need Visual Studio, source code, or the .NET SDK.
- Interactive installer choices: Revit 2025, Revit 2026, Revit 2027, Install All, Esc to exit.
- Install All must call the same proven single-year installation routine sequentially for 2025, 2026, and 2027.
- After an installation, Esc exits immediately; any other key returns to the menu.
- If Revit is running, show a clear human-readable explanation rather than only an internal exit code.
- Installer/update logic must account for unsaved work, normal Revit close, stuck/elevated processes, per-version paths, and accurate success/failure reporting.

## GitHub strategy
- GitHub should become the shared source, release, distribution, and update channel.
- Development may remain local; GitHub is the handoff layer between work and personal ChatGPT environments.
- `main` should represent the latest approved source state.
- Create a formal GitHub Release only after supported builds pass and the release has been exercised in Revit.
- Releases should contain a source ZIP, a ready-to-install Distribution ZIP, changelog/release notes, and a machine-readable update manifest.
- The future in-app updater should check GitHub Releases, compare versions, show a subtle Update Available state, download the correct release asset, and launch an external updater so the running Revit DLL is never overwritten in-process.

## Development loop
1. Inventory the latest source package.
2. Establish/build the baseline.
3. Make the smallest requested change.
4. Build Revit 2025, 2026, and 2027.
5. Try to break the changed behavior and run relevant regressions.
6. Fix failures and repeat.
7. Package source and distribution artifacts.
8. Handoff with baseline version, output version, changes, unchanged features, build results, exact Revit test steps, known limitations, and lessons learned.

## Regressions/lessons that must not repeat
- WPF/Revit namespaces can collide. Explicitly qualify/alias ambiguous types such as `Autodesk.Revit.DB.Color` vs `System.Windows.Media.Color` and WPF `Visibility`.
- Avoid invalid C# backslash escapes in Windows paths; use verbatim strings or escaped backslashes.
- Do not use WinForms for the main dashboard; it caused persistent native white seams/chrome.
- Do not let build scripts print success after a failed compile.
- PowerShell launchers should tolerate normal execution-policy restrictions by using an appropriate process-scoped bypass launcher.
- Keep deployment names consistent: `Revit Model Health Check.addin` and `Revit Model Health Check` folder per Revit year.
- Preserve user-independent `%APPDATA%` paths.
- A compile is not acceptance; test the actual reported Revit failure and relevant regressions.
