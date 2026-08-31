---
name: revit-model-health-check-dev
description: Maintain, modify, build, package, hand off, and release the Revit Model Health Check add-in. Use when ChatGPT is working on the Revit Model Health Check source package, its WPF dashboard, Revit 2025-2027 builds, installer/distribution flow, GitHub repository/releases, or future in-app updater. Always treat the newest attached source ZIP as the implementation source of truth and preserve existing behavior unless explicitly changed.
---

# Revit Model Health Check Development

Read `references/project-instructions.md` before making any source change.

When the task involves moving work between machines, GitHub publishing, releases, or the updater, also read `references/github-handoff-workflow.md`.

When starting from a handoff on a new ChatGPT environment, read `references/current-handoff.md`.

## Required workflow

1. Treat the newest attached source ZIP as the source of truth.
2. Unpack and inventory version, Revit targets/frameworks, commands, ribbon UI, WPF files, icons, config, build/install/package scripts, and deployment names.
3. Build the baseline when the required Revit SDK/API environment is available and record pre-existing failures.
4. Preserve unrelated behavior and make the smallest coherent change.
5. Increment and synchronize the version across all relevant files.
6. Build every supported target using the installed Revit API: Revit 2025/2026
   use .NET 8 before update 5 and .NET 10 at update 5 or later; Revit 2027 uses .NET 10.
7. Test the reported Revit failure case and relevant regressions; compilation alone is not acceptance.
8. Package source/distribution artifacts and provide a handoff with baseline, output version, changes, unchanged behavior, build/test results, exact test steps, limitations, and new lessons.

## Non-negotiable compatibility rules

- Keep the main dashboard in WPF.
- Never use an empty Revit `PushButtonData` text value.
- Never hardcode a Windows username. Use `%APPDATA%\\Autodesk\\Revit\\Addins\\<year>` for normal per-user deployment.
- Preserve user-independent installer behavior and the interactive 2025/2026/2027/Install All menu.
- Keep Performance Analyzer manual/opt-in and preserve non-model/style/type filtering.
- Keep the centerline check scoped to the active view and evaluate effective parent/subcategory visibility plus relevant template control.
- Explicitly qualify ambiguous Revit/WPF types when namespaces collide.

## GitHub/release behavior

Use GitHub as the shared handoff and release channel, not as a substitute for testing. Keep `main` at the latest approved source state. Only create a public/user-facing release after supported builds and Revit verification are complete. Release assets should include source ZIP, distribution ZIP, and update manifest when available.
