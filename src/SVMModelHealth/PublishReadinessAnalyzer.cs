using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SVMModelHealth;

internal sealed class StratusParameterReadinessResult
{
    public int ElementsInspected { get; init; }
    public int StratusAwareElements { get; init; }
    public List<ElementId> AffectedElementIds { get; init; } = new();
    public Dictionary<string, int> MissingByParameter { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public string Details { get; init; } = "";
}

internal sealed class FabricationReadinessResult
{
    public int FabricationPartCount { get; init; }
    public bool ConfigurationAvailable { get; init; }
    public string ConfigurationName { get; init; } = "";
    public string ProfileName { get; init; } = "";
    public List<ElementId> ElementIds { get; init; } = new();
    public string Details { get; init; } = "";
}

internal static class PublishReadinessAnalyzer
{
    internal static readonly string[] StratusMappedParameters =
    {
        "STRATUS Assembly Name",
        "STRATUS Tracking Status",
        "STRATUS Item Number",
        "STRATUS QR Code",
        "STRATUS Package Name"
    };

    public static StratusParameterReadinessResult AnalyzeStratusParameters(Document doc)
    {
        var candidates = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .ToElements()
            .Where(IsLikelyPublishable)
            .ToList();

        // STRATUS mappings are not necessarily bound to every publishable category. To avoid
        // false positives, each property is enforced only inside an API-type/category cohort
        // where that property is already present on at least one peer element. This catches
        // partial binding problems without assuming all five properties belong everywhere.
        var aware = candidates.Where(HasAnyStratusParameter).ToList();
        var missingByParameter = StratusMappedParameters.ToDictionary(x => x, _ => 0, StringComparer.OrdinalIgnoreCase);
        var affected = new HashSet<ElementId>();
        var cohortsChecked = 0;

        var cohorts = candidates.GroupBy(element => new
        {
            ApiType = element.GetType().FullName ?? element.GetType().Name,
            Category = GetCategoryName(element)
        });

        foreach (var cohort in cohorts)
        {
            var peers = cohort.ToList();
            foreach (var parameterName in StratusMappedParameters)
            {
                if (!peers.Any(element => LookupParameterSafe(element, parameterName) != null)) continue;
                cohortsChecked++;
                foreach (var element in peers)
                {
                    if (LookupParameterSafe(element, parameterName) != null) continue;
                    missingByParameter[parameterName]++;
                    affected.Add(element.Id);
                }
            }
        }

        var missingSummary = missingByParameter.Where(x => x.Value > 0).ToList();
        string details;
        if (aware.Count == 0)
        {
            details = $"Inspected {candidates.Count:N0} physical model element(s). No known STRATUS mapped parameters were detected, so parameter consistency could not be validated.";
        }
        else if (missingSummary.Count == 0)
        {
            details = $"Checked {aware.Count:N0} STRATUS-aware element(s) across {cohortsChecked:N0} applicable parameter cohort(s). No partial STRATUS parameter bindings were found.";
        }
        else
        {
            details = $"Checked {aware.Count:N0} STRATUS-aware element(s) across {cohortsChecked:N0} applicable parameter cohort(s). {affected.Count:N0} element(s) are missing a STRATUS property that exists on peer elements of the same API type/category: "
                + string.Join("; ", missingSummary.Select(x => $"{x.Key}: {x.Value:N0}")) + ".";
        }

        return new StratusParameterReadinessResult
        {
            ElementsInspected = candidates.Count,
            StratusAwareElements = aware.Count,
            AffectedElementIds = affected.ToList(),
            MissingByParameter = missingByParameter,
            Details = details
        };
    }

    public static FabricationReadinessResult AnalyzeFabrication(Document doc)
    {
        var parts = new FilteredElementCollector(doc)
            .OfClass(typeof(FabricationPart))
            .WhereElementIsNotElementType()
            .Cast<FabricationPart>()
            .ToList();

        if (parts.Count == 0)
        {
            return new FabricationReadinessResult
            {
                FabricationPartCount = 0,
                ConfigurationAvailable = true,
                Details = "No fabrication parts were found in the model."
            };
        }

        object? configuration = null;
        MethodInfo? method = null;
        try
        {
            method = typeof(FabricationConfiguration).GetMethod(
                "GetFabricationConfiguration",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(Document) },
                modifiers: null);
            configuration = method?.Invoke(null, new object[] { doc });
        }
        catch { }

        var configName = ReadStringProperty(configuration, "Name");
        var profileName = ReadStringProperty(configuration, "ProfileName");
        if (string.IsNullOrWhiteSpace(profileName)) profileName = ReadStringProperty(configuration, "Profile");
        var inspectionSupported = method != null;
        var available = configuration != null || !inspectionSupported;

        var details = configuration != null
            ? $"{parts.Count:N0} fabrication part(s) detected. Loaded fabrication configuration: {(string.IsNullOrWhiteSpace(configName) ? "(name unavailable)" : configName)}"
              + (string.IsNullOrWhiteSpace(profileName) ? "." : $"; profile: {profileName}.")
            : !inspectionSupported
                ? $"{parts.Count:N0} fabrication part(s) detected. This Revit API build does not expose the expected fabrication-configuration inspection method, so configuration availability was not scored as a failure."
                : $"{parts.Count:N0} fabrication part(s) detected, but no loaded fabrication configuration could be resolved through the Revit API.";

        return new FabricationReadinessResult
        {
            FabricationPartCount = parts.Count,
            ConfigurationAvailable = available,
            ConfigurationName = configName,
            ProfileName = profileName,
            ElementIds = parts.Select(x => x.Id).ToList(),
            Details = details
        };
    }

    private static bool IsLikelyPublishable(Element element)
    {
        if (element.ViewSpecific) return false;
        return element is FamilyInstance
            || element is FabricationPart
            || element is MEPCurve
            || element is HostObject
            || element is DirectShape
            || element is AssemblyInstance;
    }

    private static bool HasAnyStratusParameter(Element element)
        => StratusMappedParameters.Any(name => LookupParameterSafe(element, name) != null);

    private static string GetCategoryName(Element element)
    {
        try { return element.Category?.Name ?? "No Category"; }
        catch { return "Unknown Category"; }
    }

    private static Parameter? LookupParameterSafe(Element element, string name)
    {
        try { return element.LookupParameter(name); }
        catch { return null; }
    }

    private static string ReadStringProperty(object? target, string propertyName)
    {
        if (target == null) return "";
        try
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(target)?.ToString() ?? "";
        }
        catch { return ""; }
    }
}
