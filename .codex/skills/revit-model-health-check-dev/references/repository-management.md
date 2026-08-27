# Repository Management

## Repository

- GitHub: `kmccabe87/revit-model-health-check`
- Default branch: `main`
- `main` represents the latest approved source state.
- Use a feature branch and pull request for source imports and behavior changes.

## Before writing to GitHub

1. Read `AGENTS.md`, `PROJECT_INSTRUCTIONS.md`, and `RELEASE_CHECKLIST.md`.
2. Compare the branch or attached package with current `main`; preserve history
   and unrelated repository files.
3. Run `scripts/validate_repository.py` or the PowerShell equivalent.
4. Record unavailable Windows/Revit build gates as pending, never as passed.

## Release boundary

A commit or pull request may contain unverified source when that status is
explicit. A user-facing GitHub Release may not.

Before publishing a release:

- Build Revit 2025/2026 on .NET 8 and Revit 2027 on .NET 10 against installed
  Revit API assemblies.
- Exercise the changed behavior inside each applicable Revit version.
- Build and verify both the Source ZIP and Distribution ZIP.
- Fill asset URLs and SHA-256 values in `update-manifest.json`.
- Change `releaseApproved` to `true` only after every required gate passes.

Never overwrite the running Revit add-in DLL. The future updater must hand off
to an external process after Revit closes normally.
