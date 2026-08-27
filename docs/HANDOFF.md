# Start Here - Personal ChatGPT Handoff

You are taking over development/release management for **Revit Model Health Check**.

## First action every time
Use the newest attached source ZIP as the source of truth. Unpack and inventory it before changing anything. Never reconstruct the project from memory.

## Current baseline at handoff
- Version: **v0.6.13**
- Revit 2025: .NET 8
- Revit 2026: .NET 8
- Revit 2027: .NET 10
- Ribbon tab: **Pre-Publish Checks**
- Panel: **Health Check**
- Button: **Scan Model**
- Dashboard: WPF
- Revit 2025, 2026, and 2027 build outputs and a Distribution ZIP were supplied with the v0.6.13 handoff.

Read `PROJECT_INSTRUCTIONS.md` before making changes and `GITHUB_HANDOFF_WORKFLOW.md` before publishing anything.

## Main GitHub job
Maintain the repository and releases so the work ChatGPT can pull the latest approved source in the morning and end users can eventually update from inside the add-in.

## Do not publish a release merely because it compiles
A release is approved only after the supported Revit versions build and the changed behavior has been exercised in Revit. If test evidence is missing, commit/update the source if appropriate but clearly mark the release as not yet approved rather than pretending it was tested.
