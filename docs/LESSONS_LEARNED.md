**REVIT PROJECTS**

**Lessons Learned**

Continuity rules, regression history, and project instructions for
Kyle's Revit add-ins

*First edition \| August 24, 2026*

> **Primary purpose:** Prevent a new chat from reintroducing behavior
> that was already corrected. The latest source package is the
> implementation source of truth; this document is the continuity and
> acceptance-criteria source of truth.
>
> **Evidence note:** This edition is based on the project history
> retained in the current workspace, including the recent Kyle's Tools,
> Revit add-in, family-upgrade, ITM, and section-box threads. Full
> transcript retrieval was unavailable, so this document intentionally
> distinguishes confirmed requirements from items that still need
> source-code or model verification.

# How to use this document

Read Section 1 before changing code in a new chat. Use Sections 3–8 as
acceptance criteria. Add a new entry to the regression ledger whenever
Kyle reports a behavior that must be corrected. If the source package
conflicts with an old note, inspect the code and ask; do not silently
discard either source.

# 1. Start-of-chat handoff protocol

> **Non-negotiable:** Do not begin by recreating the add-in from memory.
> Unpack and inspect the newest package first, establish its version and
> feature inventory, and make changes on top of that exact baseline.

1.  Treat the newest source package attached by Kyle as the current code
    source of truth unless he explicitly names a different baseline.

2.  Record the package filename, internal version, supported Revit
    versions, add-in folder name, project/solution files, and installer
    entry points before editing.

3.  Inventory existing commands, ribbon panels, button names, icons,
    settings, profiles, family/ITM resources, and version-specific
    projects. This protects completed features from accidental removal.

4.  Search the code for the feature being changed and for adjacent
    shared logic. Placement, sizing, geometry, selection, transactions,
    settings, and packaging frequently affect more than one command.

5.  Build the unmodified baseline first when practical. Separate
    pre-existing failures from new failures.

6.  Implement the smallest coherent change. Preserve all unrelated
    behavior and user-facing naming unless Kyle requested a redesign.

7.  Build every supported Revit target that the package contains. Do not
    assume a successful build for one year proves compatibility for the
    others.

8.  Package the result using the same expected folder structure and a
    clearly incremented version. For browser extensions, keep the
    unpacked extension folder name exactly unchanged so it can be
    overwritten and refreshed in place.

9.  Provide test steps that exercise the corrected real-world case, not
    only a happy-path synthetic case.

10. When Kyle returns a log, screenshot, model symptom, or pasted error,
    treat it as new evidence. Trace the exact failing path before making
    another broad change.

## Required handoff summary in every completed build

- Baseline package/version used.

- New package/version produced.

- Files and behaviors changed.

- Features deliberately left unchanged.

- Build results by Revit version.

- Exact in-Revit test checklist and any known limitations.

# 2. Core development principles learned

**Geometry must be derived from the actual host and accessory.** Do not
use project ground, global axes, bounding-box centers, or a generic
offset when the requirement depends on a structural member's local
coordinate system, face, web, flange, connector, or actual solid
intersection.

**Size semantics differ by content type.** A Revit family, fabrication
ITM, pipe, insulation layer, CalSil shield, and U-bolt may expose
different nominal/product/physical dimensions. Identify what each number
means before matching or placing.

**Placement and void creation are one geometry problem.** Orient and
seat the accessory against the structural family first; only then derive
bolt/penetration locations and create voids from the confirmed overlap.

**Service membership should improve placement, not block it.** Use a
matching loaded fabrication service when the ITM exists there. If an
accessory legitimately lives outside the active service, allow placement
without forcing a misleading service warning.

**Persistent options beat repeated prompts.** For commands used
repeatedly, store defaults in an Options interface. Kyle specifically
preferred pre-set section-box extension and view-template settings
instead of a prompt every run.

**A successful compile is not a successful feature.** The expected cycle
is: make it, try to break it, fix it, and repeat. Test placement
visually in Revit and validate generated geometry, selection behavior,
transactions, and packaging.

**Do not invent terminology.** Use clear user-facing names. Avoid
unexplained abbreviations such as SVMI unless the project already
defines them and they add value.

# 3. Regression and correction ledger

These are confirmed or strongly evidenced problems Kyle had to report
after an earlier implementation. They should become regression tests.

| **Regression**                                                   | **Observed symptom**                                                                                                                  | **Permanent rule / acceptance test**                                                                                                                                                                    |
|------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **U-bolt voids placed at ground/project coordinates**            | Voids appeared on the ground instead of inside the structural families.                                                               | Transform all accessory/bolt geometry into the host family instance's local coordinate system and place/cut the voids in the intersected structural host.                                               |
| **Void creation before reliable U-bolt orientation**             | A void can be mathematically valid but spatially wrong when the fabrication U-bolt is not seated against the member.                  | Rotate/orient the U-bolt so it touches the structural family, confirm the geometry relationship, then derive the two bolt penetration paths.                                                            |
| **Cal shield sized from insulated outside diameter**             | A fabrication CalSil/cal-shield selection used the insulation OD and selected an 8×2 item where a 4×… item was expected.              | Use the pipe's product/nominal entry for the first CalShield product-entry number. Use the service insulation thickness/wall thickness for the second dimension.                                        |
| **Generic closest-size logic for CalShield ITMs**                | Matching by a generic overall diameter selected the wrong catalog item.                                                               | Parse the CalShield product entry as first-number × thickness. Match the pipe product entry to the closest valid first number, then match the service insulation thickness.                             |
| **Assuming every ITM must belong to the active service**         | Trapeze and accessory ITMs may live in their own service or outside the current pipe service, causing unnecessary warnings or blocks. | Inspect loaded configuration/services. Prefer a service containing the ITM; otherwise allow the legitimate external ITM and suppress only the irrelevant service mismatch warning.                      |
| **New 3D view opened too far away**                              | The section-box view was created but looked extremely zoomed out.                                                                     | After creating and activating the view, request Zoom Extents at the correct UI/application timing so the selected geometry fills the view.                                                              |
| **Runtime prompt used for repeatable settings**                  | Section-box extension distance was requested interactively each run.                                                                  | Expose saved Options for section-box offset and created-view template; use those values when the command runs.                                                                                          |
| **Icons added only to new buttons**                              | The toolset remained visually inconsistent.                                                                                           | Create and assign icons for every ribbon button, using consistent sizing and visual language.                                                                                                           |
| **Installer relied on blocked PowerShell execution**             | Windows reported that install-user.ps1 could not run because script execution was disabled.                                           | Provide a launcher/command that works with the organization's execution-policy constraints, or invoke PowerShell with an appropriate process-scoped bypass while keeping the install explicit and safe. |
| **Installer could not close remaining Revit process**            | Normal close waited, then force-close failed and required Task Manager.                                                               | Detect all Revit processes, communicate save/sync risk, wait for normal close, handle elevated/child processes cleanly, and stop with a precise recovery step instead of looping or claiming success.   |
| **Family upgrade assumed file-version compatibility was enough** | A 2025 family reported: base sketch for extrusion is invalid; Revit deleted instances.                                                | Open and audit the failing family geometry, constraints, profiles, nested content, and formulas in the target Revit version. A successful save/upgrade is not sufficient; review automatic resolutions. |

# 4. Fabrication support accessories

## Command organization

- Prefer one combined command named Place Support Accessories (or a
  shorter name only if it stays immediately understandable).

- The combined workflow may include Revit-family accessories and
  fabrication accessories, but the sizing rules must remain
  content-specific.

- Do not collapse distinct internal placement algorithms merely because
  the ribbon exposes one button.

## CalSil / CalShield sizing rules

- For family-based shields, use the pipe size—not the insulation outside
  diameter—unless the family definition explicitly says otherwise.

- For fabrication CalShield ITMs, the ITM already accounts for outside
  diameter. Match catalog/product-entry semantics instead of adding
  insulation OD again.

- Interpret a product entry such as 8×2 as a first catalog/pipe-size
  value and a second insulation/wall-thickness value. The problematic
  example should have resolved from an 8×2 selection to a 4×\[service
  insulation thickness\] selection.

- Normalize numeric strings and units before comparison. Match exact
  normalized values first; only use closest-value fallback within a
  documented tolerance and report when fallback was used.

- When multiple candidates tie, choose deterministically and log the
  candidate set and selected reason.

## Loaded configuration and service resolution

1.  Read the active fabrication configuration and loaded services
    available through the supported Revit Fabrication API.

2.  Locate services containing the chosen ITM/button when that
    information is exposed.

3.  If the ITM belongs to an available service, place it through that
    service so behavior and metadata match.

4.  If the item is intentionally external or lives in an
    accessory/trapeze service, allow placement and bypass only the
    irrelevant different-service warning.

5.  Never silently substitute a different ITM merely because it is
    easier to access from the active service.

# 5. U-bolt placement and structural voids

> **Geometry sequence:** Select/identify host → resolve member local
> axes and section shape → size U-bolt → orient and seat accessory →
> verify overlap → locate bolt legs/connectors → create host-local voids
> → cut host → validate result.

## Structural member rules retained from the project

- HSS members: center the U-bolt on the relevant face/section unless
  another face is intentionally selected.

- Unequal angles: center on the selected side/leg, not the overall
  bounding-box center.

- W-shapes: position relative to the web while centering within the
  flange/wings as previously specified; do not center blindly on the
  family bounding box.

- Channels and angle families: derive orientation from section geometry
  and the family instance transform, including mirrored and rotated
  instances.

- Columns, beams, and hanger variants may share a profile name but
  differ in local-axis conventions. Verify each supplied family rather
  than assuming a universal axis mapping.

## Void requirements

- Voids belong in, or must cut, the actual intersected structural
  family—not an unrelated work plane at project elevation zero.

- Create one penetration path per bolt leg using confirmed
  bolt/connector geometry when available. If connectors are
  insufficient, derive paths from the placed U-bolt solid only after
  orientation is correct.

- Convert project-space points and directions through the host instance
  inverse transform before authoring host-local geometry.

- Account for host rotation, slope, mirror state, nested transforms, and
  non-axis-aligned framing.

- Use a small, explicit clearance parameter rather than an unexplained
  hard-coded oversize.

- After cutting, verify that each void intersects the host and that the
  cut relationship exists. Roll back or warn if either bolt void misses.

## Minimum regression model

| **Test dimension**   | **Required coverage**                                                                                                                                |
|----------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Host orientation** | Horizontal, vertical, sloped, rotated in plan, mirrored.                                                                                             |
| **Sections**         | HSS square/rectangular, equal angle, unequal angle, W-shape, C/MC channel, beam and column variants.                                                 |
| **Accessory**        | At least two U-bolt sizes; connector-rich and connector-poor content if both exist.                                                                  |
| **Checks**           | Accessory touches the intended face; bolt legs land correctly; voids occur in the host; no geometry appears at project origin/ground; undo is clean. |

# 6. Section Box Toolkit requirements

- Create a new 3D view from the current selection and apply a section
  box around the selected geometry.

- The section-box extension distance is a saved option, not a per-run
  prompt.

- Allow a saved view-template choice for the generated 3D view.

- After creation, activate the view and perform Zoom Extents so the
  selected geometry is framed usefully.

- Create consistent icons for all buttons, not just newly added
  commands.

- Keep naming user-facing and self-explanatory; remove unexplained
  internal abbreviations.

## Zoom Extents implementation caution

Zoom commands often require the newly created view to be active and the
UI to have finished switching views. If a direct post-transaction call
is unreliable, defer the UI action through the supported Revit
external-event/idling mechanism. Test the first run in a fresh Revit
session; do not accept a result that works only after the view was
opened manually.

# 7. Revit family upgrade and compatibility

- Support the explicit project targets: Revit 2025, 2026, and 2027 where
  the source package includes those targets.

- A family opening or saving in a newer Revit version does not prove it
  is healthy. Review every automatic resolution message.

- For the P2558_w_Hardware error—‘Base sketch for extrusion is invalid’
  with Delete Instance(s)—inspect the extrusion profile, sketch-plane
  references, dimensions/constraints, formulas, visibility states,
  nested families, and type-driven degenerate geometry.

- Test every family type that can drive geometry to zero, invert
  dimensions, create self-intersections, or collapse a profile.

- Retain original family files and produce upgraded copies unless Kyle
  explicitly requests in-place replacement.

- Document any geometry Revit deleted or regenerated. Never hide an
  automatic repair warning in the handoff.

# 8. Installer, packaging, and deployment

- Installer messaging must protect unsaved Revit work. Ask the user to
  save/sync, request normal closure first, and clearly label any
  force-close risk.

- Handle script-execution restrictions. A double-clickable launcher
  should invoke the PowerShell installer in a compatible process context
  or provide a clear alternate command.

- Detect all relevant Revit processes and distinguish a normal close
  delay from a process that cannot be terminated due to permissions or
  child dialogs.

- Never report installation success until files were copied to the
  correct per-version add-in locations and expected manifests/assemblies
  exist.

- Preserve resource files, icons, ITMs, families, settings defaults,
  profiles, and installer scripts in the final package.

- Use a stable root folder name when overwrite-in-place deployment
  depends on it. This is mandatory for the STRATUS Viewer Tools browser
  extension.

# 9. Build, test, break, fix, repeat

Kyle's preferred development rhythm is explicit: make it, attempt to
break it, fix it, and repeat. The following gates convert that
preference into a repeatable definition of done.

| **Gate**            | **Pass condition**                                                                                                                        |
|---------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| **Baseline gate**   | Package unpacks; solution structure is understood; unmodified build status is recorded.                                                   |
| **Compile gate**    | All intended Revit targets compile with no new errors. Warnings are reviewed, not ignored wholesale.                                      |
| **Ribbon gate**     | Commands load, button names are correct, and all icons render at expected sizes.                                                          |
| **Behavior gate**   | The exact scenario Kyle reported now behaves correctly in Revit.                                                                          |
| **Regression gate** | Adjacent completed features still work; settings and profiles survive upgrade.                                                            |
| **Geometry gate**   | Rotated, mirrored, sloped, and non-origin hosts are tested for placement/void features.                                                   |
| **Data gate**       | Catalog/product-entry parsing is tested with representative and malformed values.                                                         |
| **Installer gate**  | Install/update/overwrite flow is tested with Revit open and closed, including failure messaging.                                          |
| **Package gate**    | Final zip contains source, binaries/resources expected by the project, installer, version identity, and no accidental intermediate files. |
| **Handoff gate**    | Change summary, build results, exact test steps, known limitations, and new regression rules are supplied.                                |

# 10. Copy-ready project instructions

> **Use:** Paste the following instructions into the Revit Projects
> project instructions. Revise them when a later source package or
> confirmed test result supersedes a rule.

- When Kyle starts a new chat and attaches the latest add-in source
  package, treat that package as the current implementation source of
  truth. Do not rebuild from memory or an older package.

- Before editing, unpack and inventory the package: version, supported
  Revit targets, commands, ribbon UI, icons, settings, profiles,
  families, ITMs, installers, and deployment folder names. Build the
  baseline when practical and record pre-existing failures.

- Preserve every existing feature and unrelated behavior unless Kyle
  explicitly requests its removal or redesign. Make the smallest
  coherent change and increment the version clearly.

- For geometry work, use actual Revit solids, connectors, faces,
  transforms, and member-local axes. Never place host cuts or accessory
  geometry from project-origin, ground-plane, generic bounding-box, or
  global-axis assumptions.

- For fabrication U-bolts: resolve the structural section and local
  orientation, rotate/seat the U-bolt against the intended member face,
  confirm overlap, then derive bolt-leg paths and create/cut host-local
  voids. Test rotated, mirrored, sloped, beam, and column cases. Reject
  any result that places geometry at project origin or ground level.

- For CalSil/CalShield sizing: do not size a fabrication shield from
  insulated outside diameter. Parse product entries by their real
  semantics. Match the pipe product/nominal size to the first CalShield
  number and the service insulation thickness/wall thickness to the
  second number. Use deterministic exact-first matching and a documented
  closest-size fallback.

- For fabrication services: inspect the loaded configuration. Use a
  service containing the chosen ITM when available. If the accessory
  legitimately lives in another service or outside the active service,
  allow placement and bypass only the irrelevant mismatch warning; never
  silently substitute a different ITM.

- For repeated command settings, prefer saved Options over per-run
  prompts. Section Box Toolkit must support saved section-box extension
  distance, saved view-template selection, and Zoom Extents after the
  new 3D view becomes active.

- Use clear names and consistent icons for all ribbon buttons. Do not
  introduce unexplained abbreviations.

- Build and test every Revit version included in the package, normally
  2025–2027. A compile alone is not acceptance: test in Revit, exercise
  the reported failure case, and run relevant regressions.

- Treat Revit family automatic repair warnings as failures requiring
  review. Inspect invalid sketches, constraints, formulas, nested
  content, and every geometry-driving type before declaring an upgrade
  successful.

- Installer/update flows must account for PowerShell execution policy,
  unsaved Revit work, normal close, stuck/elevated processes,
  per-version paths, and accurate success/failure reporting.

- Use the development loop: make it, try to break it, fix it, and
  repeat. In every handoff, state baseline version, output version,
  changes, unchanged features, build results, exact test steps, known
  limitations, and any new lesson that belongs in Lessons Learned.

# 11. Open items for future expansion

- Import additional regression details from older full chat transcripts
  when they are available.

- Add exact package filenames and version lineage for Kyle's Tool Kit
  after the next source package is attached.

- Convert the U-bolt/void and CalShield rules into automated unit tests
  where the Revit API boundary permits, plus repeatable journal/manual
  test models for geometry behavior.

- Add screenshots of correct results and known-failure examples to
  create a visual acceptance catalog.

- Maintain a per-release change log that links each fix to its
  regression test and this ledger entry.

> **Maintenance rule:** This is a living project artifact. Update it at
> the same time a repeated mistake is fixed—not weeks later—so a future
> chat inherits the correction.
