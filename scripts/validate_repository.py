#!/usr/bin/env python3
"""Cross-platform static repository validation.

This does not compile against Autodesk assemblies or replace in-Revit tests.
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


def require(relative: str) -> Path:
    path = ROOT / relative
    if not path.is_file():
        raise RuntimeError(f"Required file is missing: {relative}")
    return path


def require_text(relative: str, expected: str) -> None:
    text = require(relative).read_text(encoding="utf-8-sig")
    if expected not in text:
        raise RuntimeError(f"Expected content is missing from {relative}: {expected}")


def main() -> int:
    version = require("VERSION").read_text(encoding="utf-8").strip()
    if not re.fullmatch(r"\d+\.\d+\.\d+", version):
        raise RuntimeError(f"VERSION is not semantic: {version}")

    for relative in (
        "README.md",
        "CHANGELOG.md",
        "PROJECT_INSTRUCTIONS.md",
        "RELEASE_CHECKLIST.md",
        "update-manifest.json",
        "src/SVMModelHealth/SVMModelHealth.csproj",
        "src/SVMModelHealth/App.cs",
        "src/SVMModelHealth/HealthDashboard.xaml",
        "config/health-rules.json",
        ".codex/skills/revit-model-health-check-dev/SKILL.md",
    ):
        require(relative)

    require_text("src/SVMModelHealth/SVMModelHealth.csproj", f"<Version>{version}</Version>")
    require_text(
        "src/SVMModelHealth/SVMModelHealth.csproj",
        '<RevitTargetFramework Condition="\'$(RevitTargetFramework)\' == \'\' and \'$(RevitYear)\' == \'2027\'">net10.0-windows</RevitTargetFramework>',
    )
    require_text(
        "src/SVMModelHealth/SVMModelHealth.csproj",
        '<TargetFramework>$(RevitTargetFramework)</TargetFramework>',
    )
    require_text("scripts/Build.ps1", 'return "net10.0-windows"')
    require_text("scripts/Build.ps1", 'return "net8.0-windows"')
    require_text("src/SVMModelHealth/App.cs", 'const string tabName = "Pre-Publish Checks";')
    require_text("src/SVMModelHealth/App.cs", 'const string panelName = "Health Check";')
    require_text("src/SVMModelHealth/App.cs", '"Scan Model"')
    require_text("scripts/Package.ps1", f'$version = "{version}"')

    rules = json.loads(require("config/health-rules.json").read_text(encoding="utf-8-sig"))
    if not rules.get("rules"):
        raise RuntimeError("Health rules are empty")

    manifest = json.loads(require("update-manifest.json").read_text(encoding="utf-8"))
    if manifest.get("version") != version:
        raise RuntimeError("Update manifest version is out of sync")
    if manifest.get("frameworks", {}).get("2027") != "net10.0-windows":
        raise RuntimeError("Manifest maps Revit 2027 to the wrong framework")
    if manifest.get("releaseApproved"):
        for asset_name in ("source", "distribution"):
            asset = manifest.get("assets", {}).get(asset_name, {})
            if not asset.get("url") or not asset.get("sha256"):
                raise RuntimeError(f"Approved manifest is missing {asset_name} URL/SHA-256")

    machine_path = re.compile(r"C:\\Users\\(?![<%$.*])", re.IGNORECASE)
    for path in ROOT.rglob("*"):
        if not path.is_file() or any(part in {"build", "dist", "bin", "obj", ".git"} for part in path.parts):
            continue
        if path.suffix.lower() not in {".cs", ".csproj", ".ps1", ".cmd", ".json", ".md", ".yml", ".yaml"}:
            continue
        if machine_path.search(path.read_text(encoding="utf-8-sig", errors="replace")):
            raise RuntimeError(f"Machine-specific C:\\Users path found in {path.relative_to(ROOT)}")

    print(f"Repository static validation passed for v{version}.")
    print("Windows/Revit compilation and behavior verification remain required.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (OSError, RuntimeError, ValueError, json.JSONDecodeError) as exc:
        print(f"VALIDATION FAILED: {exc}", file=sys.stderr)
        raise SystemExit(1)
