# Update Manifest Contract

`update-manifest.json` is intentionally unapproved until Windows/Revit testing
and release packaging finish.

An updater must reject a manifest when any of these conditions is true:

- `releaseApproved` is not `true`.
- The semantic version is not newer than the installed version.
- The current Revit year is absent from `supportedRevitVersions`.
- The requested asset URL or SHA-256 is missing.
- The downloaded asset hash does not exactly match the manifest.

The add-in must not replace its own loaded DLL. It may show a subtle update
state and download a validated payload, but installation must be delegated to
an external updater after the user saves/syncs and closes Revit normally.

Before publishing an approved manifest, update the version, tag, release-notes
URL, asset names, URLs, and SHA-256 values; then complete every gate in
`RELEASE_CHECKLIST.md`.
