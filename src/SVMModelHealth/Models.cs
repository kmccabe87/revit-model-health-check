using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVMModelHealth;

public enum HealthSeverity { Info, Warning, Critical }
public enum HealthStatus { Pass, Review, Fail }

public sealed class RuleThreshold
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "General";
    public HealthSeverity Severity { get; set; } = HealthSeverity.Warning;
    public int ReviewAt { get; set; } = 1;
    public int FailAt { get; set; } = 10;
    public int Weight { get; set; } = 5;
    public string Guidance { get; set; } = "";
}

public sealed class RuleConfig
{
    public List<RuleThreshold> Rules { get; set; } = new();

    public RuleThreshold Get(string key) => Rules.FirstOrDefault(r => r.Key == key) ?? new RuleThreshold
    {
        Key = key,
        Name = key,
        ReviewAt = 1,
        FailAt = 10,
        Weight = 5
    };

    public static RuleConfig Load()
    {
        var fallback = CreateDefault();
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "health-rules.json");
            if (!File.Exists(path)) return fallback;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Deserialize<RuleConfig>(File.ReadAllText(path), options) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    public static RuleConfig CreateDefault() => new()
    {
        Rules = new List<RuleThreshold>
        {
            new() { Key="warnings", Name="Revit Warnings", Category="Stability", Severity=HealthSeverity.Warning, ReviewAt=25, FailAt=100, Weight=12, Guidance="Review and resolve warnings, prioritizing repeated and geometry-related warnings." },
            new() { Key="imported_cad", Name="Imported CAD (not linked)", Category="Links & Imports", Severity=HealthSeverity.Critical, ReviewAt=1, FailAt=3, Weight=12, Guidance="Replace imported CAD with links where practical, then remove unnecessary imports." },
            new() { Key="linked_cad", Name="Linked CAD", Category="Links & Imports", Severity=HealthSeverity.Info, ReviewAt=6, FailAt=15, Weight=4, Guidance="Confirm each CAD link is required and correctly positioned." },
            new() { Key="inplace_families", Name="In-Place Families", Category="Families", Severity=HealthSeverity.Warning, ReviewAt=3, FailAt=10, Weight=8, Guidance="Replace repeated or complex in-place content with loadable families." },
            new() { Key="model_groups", Name="Model Groups", Category="Modeling", Severity=HealthSeverity.Info, ReviewAt=15, FailAt=40, Weight=3, Guidance="Review model groups for unnecessary nesting or duplication." },
            new() { Key="detail_groups", Name="Detail Groups", Category="Documentation", Severity=HealthSeverity.Info, ReviewAt=25, FailAt=75, Weight=2, Guidance="Review large quantities of detail groups for maintainability." },
            new() { Key="unpinned_rvt_links", Name="Unpinned Revit Links", Category="Links & Imports", Severity=HealthSeverity.Critical, ReviewAt=1, FailAt=2, Weight=10, Guidance="Pin Revit links after confirming coordinates and placement." },
            new() { Key="unloaded_rvt_links", Name="Unloaded Revit Links", Category="Links & Imports", Severity=HealthSeverity.Warning, ReviewAt=1, FailAt=3, Weight=7, Guidance="Confirm unloaded links are intentional before model exchange." },
            new() { Key="unplaced_rooms", Name="Unplaced Rooms", Category="Spatial", Severity=HealthSeverity.Warning, ReviewAt=1, FailAt=10, Weight=4, Guidance="Place or delete rooms that are no longer needed." },
            new() { Key="unenclosed_rooms", Name="Unenclosed Rooms", Category="Spatial", Severity=HealthSeverity.Warning, ReviewAt=1, FailAt=10, Weight=5, Guidance="Resolve room-bounding conditions or delete obsolete rooms." },
            new() { Key="unplaced_spaces", Name="Unplaced Spaces", Category="Spatial", Severity=HealthSeverity.Warning, ReviewAt=1, FailAt=10, Weight=4, Guidance="Place or delete MEP spaces that are no longer needed." },
            new() { Key="unenclosed_spaces", Name="Unenclosed Spaces", Category="Spatial", Severity=HealthSeverity.Warning, ReviewAt=1, FailAt=10, Weight=5, Guidance="Resolve space-bounding conditions or delete obsolete spaces." },
            new() { Key="unused_view_templates", Name="Unused View Templates", Category="Documentation", Severity=HealthSeverity.Info, ReviewAt=10, FailAt=30, Weight=2, Guidance="Remove obsolete templates after confirming they are not required for future deliverables." },
            new() { Key="centerlines_visible", Name="Centerlines Visible", Category="Graphics & Visibility", Severity=HealthSeverity.Warning, ReviewAt=1, FailAt=5, Weight=6, Guidance="Checks only the current active view. Centerlines count as visible only when the centerline category/subcategory and every parent category are visible. If the active view template controls the applicable V/G category setting, the template-controlled state is used." },
            new() { Key="stratus_publish_parameters", Name="STRATUS Publish Parameters", Category="Publish Readiness", Severity=HealthSeverity.Warning, ReviewAt=1, FailAt=10, Weight=9, Guidance="Review STRATUS-aware elements that are missing one or more mapped publish properties: Assembly Name, Tracking Status, Item Number, QR Code, or Package Name. The check only enforces consistency on elements that already expose at least one known STRATUS mapped property." },
            new() { Key="fabrication_publish_readiness", Name="Fabrication Publish Readiness", Category="Publish Readiness", Severity=HealthSeverity.Warning, ReviewAt=1, FailAt=2, Weight=8, Guidance="When fabrication parts are present, confirm Revit can resolve the loaded fabrication configuration before publishing." },
            new() { Key="user_worksets", Name="User Worksets", Category="Worksharing", Severity=HealthSeverity.Info, ReviewAt=20, FailAt=50, Weight=2, Guidance="Confirm the workset structure remains intentional and manageable." }
        }
    };
}

public sealed class HealthCheckResult
{
    public string Key { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public HealthSeverity Severity { get; init; }
    public HealthStatus Status { get; init; }
    public int Count { get; init; }
    public int ReviewAt { get; init; }
    public int FailAt { get; init; }
    public int Weight { get; init; }
    public string Guidance { get; init; } = "";
    public string Details { get; init; } = "";
    public List<ElementId> ElementIds { get; init; } = new();
}

public sealed class HealthScanResult
{
    public string DocumentTitle { get; init; } = "";
    public string DocumentPath { get; init; } = "";
    public DateTime ScannedAt { get; init; } = DateTime.Now;
    public int Score { get; init; }
    public int TotalElements { get; init; }
    public List<HealthCheckResult> Checks { get; init; } = new();
}
