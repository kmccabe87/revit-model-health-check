# Revit Model Health Check Repository Instructions

Read `PROJECT_INSTRUCTIONS.md` before changing source. For reusable Codex
workflow guidance, use `.codex/skills/revit-model-health-check-dev/SKILL.md`
and load only the references it routes to.

## Baseline

- Current imported baseline: v0.6.13.
- Revit 2025 and 2026 target `net8.0-windows`.
- Revit 2027 targets `net10.0-windows`.
- Product: Revit Model Health Check.
- Ribbon: Pre-Publish Checks > Health Check > Scan Model.
- Main dashboard: WPF.

## Working rules

- Inventory a newly supplied source package before replacing this tree.
- Preserve working and unrelated behavior; make the smallest coherent change.
- Increment and synchronize every delivered version across the project,
  scripts, documentation, manifests, and artifact names.
- Run `scripts/validate_repository.py` or `scripts/Validate-Repository.ps1`
  before every commit.
- Build all supported Revit targets on Windows with installed Revit API
  assemblies. Linux/static validation is not a substitute for that build.
- Exercise changed behavior inside Revit before marking a release approved.
- Never publish an end-user release when `update-manifest.json` has
  `releaseApproved: false` or test evidence is incomplete.
- Use a feature branch and pull request for source imports or behavior changes.
- Keep `main` as the latest approved source state.

## GitHub and releases

- Repository: `kmccabe87/revit-model-health-check`.
- Preserve history and the existing MIT license.
- Release assets must include the versioned Source ZIP and Distribution ZIP.
- Publish `update-manifest.json` only after filling URLs and SHA-256 values and
  changing `releaseApproved` to `true` after Windows/Revit verification.
- Do not overwrite the running add-in DLL from Revit. A future updater must run
  out of process after Revit closes normally.

## Required handoff

Report baseline and output versions, files and behavior changed, preserved
features, validation/build results per year, exact Revit test steps, known
limitations, and any new regression lesson.
