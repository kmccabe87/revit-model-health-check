# Release Checklist

A successful compile is necessary but not sufficient. Keep the release in a
draft or development state until every required item below is complete.

## 1. Source integrity

- [ ] `VERSION`, project metadata, scripts, README, artifact names, changelog,
      and manifest all use the same semantic version.
- [ ] `scripts/Validate-Repository.ps1` passes.
- [ ] No machine-specific username or `C:\Users\...` path was introduced.
- [ ] Ribbon and deployment names match the project instructions.

## 2. Supported builds

- [ ] Revit 2025 builds as `net8.0-windows` before 2025.5 or
      `net10.0-windows` for 2025.5+ against its installed Revit API.
- [ ] Revit 2026 builds as `net8.0-windows` before 2026.5 or
      `net10.0-windows` for 2026.5+ against its installed Revit API.
- [ ] Revit 2027 builds as `net10.0-windows` against its installed Revit API.
- [ ] Build logs are retained as release evidence.

## 3. Revit verification

- [ ] The add-in loads once in each supported Revit year.
- [ ] Ribbon shows **Pre-Publish Checks > Health Check > Scan Model**.
- [ ] Health rules run and reports/export still work.
- [ ] Centerline visibility is verified in the active view, including parent
      category and view-template-controlled cases.
- [ ] Issue Details scrolls when its content overflows.
- [ ] Performance Analyzer does not auto-run and completes manually.
- [ ] Sorting, searching, selection/show behavior, icons, and dark WPF chrome
      pass regression checks.

## 4. Installer and package

- [ ] Single-year installs pass for 2025, 2026, and 2027.
- [ ] Install All calls the same single-year routine for all three years.
- [ ] Revit-running failure messaging protects unsaved work.
- [ ] Per-user files land under `%APPDATA%\Autodesk\Revit\Addins\<year>`.
- [ ] `PACKAGE.cmd` creates the versioned Distribution ZIP.
- [ ] The Distribution ZIP installs on a clean second workstation without a
      source tree, Visual Studio, or .NET SDK.

## 5. GitHub release

- [ ] Source ZIP and Distribution ZIP names match `update-manifest.json`.
- [ ] SHA-256 is calculated for each release asset and inserted in the manifest.
- [ ] Asset URLs and release-notes URL are filled in.
- [ ] `releaseApproved` is changed to `true` only after the gates above pass.
- [ ] Tag, changelog, release title, and manifest version agree.
- [ ] Release remains a draft until all assets and checks are verified.
