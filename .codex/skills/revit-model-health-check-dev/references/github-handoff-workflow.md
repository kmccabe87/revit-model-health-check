# Revit Model Health Check - GitHub Handoff Workflow

## Goal
Use GitHub as the shared source of truth between the work environment, the personal ChatGPT environment, and end users who need updates.

## End-of-day workflow at work
1. Finish the day's Revit testing.
2. Confirm the current source version is internally consistent.
3. Create/export the complete source ZIP.
4. Move the source ZIP plus this handoff pack to the personal computer.

## At home / personal ChatGPT
Attach the newest source ZIP and say:

> Use this source ZIP as the current implementation source of truth. Inventory it before editing. Update the Revit Model Health Check GitHub repository to this approved source state. Preserve history, update the changelog/README, commit the source, and create a GitHub Release only if the release is verified. Attach both the Source ZIP and Distribution ZIP and publish/update the machine-readable update manifest. Select .NET 8 or .NET 10 for Revit 2025/2026 from the installed Revit API update and keep Revit 2027 on .NET 10. Do not introduce machine-specific paths or usernames.

The personal ChatGPT should handle Git/GitHub mechanics when its GitHub connection supports them.

## Next morning at work
- Pull/download the latest approved source from GitHub.
- Treat that package as the new development baseline.
- Do not mix it with an older local source tree.

## Recommended repository policy
- Repository can be private while company/IP permissions are being confirmed.
- `main`: latest approved source.
- Release tags: `v0.6.10`, `v0.6.11`, etc.
- Releases: only tested builds intended for distribution/update.

## Recommended release assets
- `Revit_Model_Health_Check_Revit_2025_2027_vX.Y.Z_Source.zip`
- `Revit_Model_Health_Check_vX.Y.Z_Distribution.zip`
- `update-manifest.json`
- Optional `CHANGELOG.md` / release notes

## Future updater
The app should eventually:
- Check the latest approved GitHub Release.
- Compare semantic versions.
- Show `Update available` without intrusive startup popups.
- Download a validated release payload.
- Verify SHA-256 if provided.
- Launch a separate updater process.
- Ask the user to save/close Revit normally.
- Update the current Revit year by default; optionally update all installed supported years.
- Verify copied files and report exact success/failure.
