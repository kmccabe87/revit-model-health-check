using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SVMModelHealth;

public static class HealthScanner
{
    public static HealthScanResult Scan(Document doc, RuleConfig config, View? activeView = null)
    {
        var checks = new List<HealthCheckResult>();

        var warnings = doc.GetWarnings();
        var warningIds = warnings.SelectMany(w => w.GetFailingElements()).Distinct().ToList();
        checks.Add(Make(config.Get("warnings"), warnings.Count, warningIds,
            warnings.Count == 0 ? "No Revit warnings found." : $"{warnings.Count} warning(s) affect {warningIds.Count} unique element(s)."));

        var imports = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)).Cast<ImportInstance>().ToList();
        var imported = imports.Where(x => !x.IsLinked).ToList();
        var linkedCad = imports.Where(x => x.IsLinked).ToList();
        checks.Add(Make(config.Get("imported_cad"), imported.Count, imported.Select(x => x.Id), "CAD instances embedded in the RVT."));
        checks.Add(Make(config.Get("linked_cad"), linkedCad.Count, linkedCad.Select(x => x.Id), "CAD instances linked into the RVT."));

        var inPlace = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>()
            .Where(fi => fi.Symbol?.Family?.IsInPlace == true)
            .ToList();
        checks.Add(Make(config.Get("inplace_families"), inPlace.Count, inPlace.Select(x => x.Id), "Instances whose family is modeled in-place."));

        var modelGroups = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_IOSModelGroups)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Group)).Cast<Group>().ToList();
        var detailGroups = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Group)).Cast<Group>().ToList();
        checks.Add(Make(config.Get("model_groups"), modelGroups.Count, modelGroups.Select(x => x.Id), "Placed model-group instances."));
        checks.Add(Make(config.Get("detail_groups"), detailGroups.Count, detailGroups.Select(x => x.Id), "Placed detail-group instances."));

        var rvtLinks = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();
        var unpinned = rvtLinks.Where(x => !x.Pinned).ToList();
        var unloaded = rvtLinks.Where(x => x.GetLinkDocument() == null).ToList();
        checks.Add(Make(config.Get("unpinned_rvt_links"), unpinned.Count, unpinned.Select(x => x.Id), "Revit link instances that are not pinned."));
        checks.Add(Make(config.Get("unloaded_rvt_links"), unloaded.Count, unloaded.Select(x => x.Id), "Revit link instances with no loaded link document."));

        var rooms = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Room>()
            .ToList();
        var unplacedRooms = rooms.Where(r => r.Location == null).ToList();
        var unenclosedRooms = rooms.Where(r => r.Location != null && r.Area <= 0).ToList();
        checks.Add(Make(config.Get("unplaced_rooms"), unplacedRooms.Count, unplacedRooms.Select(x => x.Id), "Rooms with no placement location."));
        checks.Add(Make(config.Get("unenclosed_rooms"), unenclosedRooms.Count, unenclosedRooms.Select(x => x.Id), "Placed rooms reporting zero area."));

        var spaces = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_MEPSpaces)
            .WhereElementIsNotElementType()
            .Cast<Space>()
            .ToList();
        var unplacedSpaces = spaces.Where(s => s.Location == null).ToList();
        var unenclosedSpaces = spaces.Where(s => s.Location != null && s.Area <= 0).ToList();
        checks.Add(Make(config.Get("unplaced_spaces"), unplacedSpaces.Count, unplacedSpaces.Select(x => x.Id), "MEP spaces with no placement location."));
        checks.Add(Make(config.Get("unenclosed_spaces"), unenclosedSpaces.Count, unenclosedSpaces.Select(x => x.Id), "Placed MEP spaces reporting zero area."));

        var views = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().ToList();
        var templates = views.Where(v => v.IsTemplate).ToList();
        var usedTemplateIds = views.Where(v => !v.IsTemplate && v.ViewTemplateId != ElementId.InvalidElementId)
            .Select(v => v.ViewTemplateId)
            .ToHashSet();
        var unusedTemplates = templates.Where(t => !usedTemplateIds.Contains(t.Id)).ToList();
        checks.Add(Make(config.Get("unused_view_templates"), unusedTemplates.Count, unusedTemplates.Select(x => x.Id), "View templates not assigned to any current view."));

        var centerlineVisibility = FindVisibleCenterlines(doc, activeView);
        var centerlineDetails = activeView == null
            ? "No active view was available for the centerline visibility check."
            : centerlineVisibility.Count == 0
                ? $"Current view '{activeView.Name}': no centerline-named category or subcategory is effectively visible. A centerline subcategory is treated as hidden when its parent category is hidden. If a view template controls the applicable V/G category setting, the template-controlled visibility is used."
                : $"Current view '{activeView.Name}': centerlines are effectively visible in "
                  + string.Join("; ", centerlineVisibility.Take(25).Select(x => x.CategoryName))
                  + (centerlineVisibility.Count > 25 ? $"; ... and {centerlineVisibility.Count - 25} more." : ".");
        checks.Add(Make(config.Get("centerlines_visible"), centerlineVisibility.Count, Array.Empty<ElementId>(), centerlineDetails));

        var stratusParameters = PublishReadinessAnalyzer.AnalyzeStratusParameters(doc);
        checks.Add(Make(
            config.Get("stratus_publish_parameters"),
            stratusParameters.AffectedElementIds.Count,
            stratusParameters.AffectedElementIds,
            stratusParameters.Details));

        var fabricationReadiness = PublishReadinessAnalyzer.AnalyzeFabrication(doc);
        var fabricationIssueCount = fabricationReadiness.FabricationPartCount > 0 && !fabricationReadiness.ConfigurationAvailable ? 1 : 0;
        checks.Add(Make(
            config.Get("fabrication_publish_readiness"),
            fabricationIssueCount,
            fabricationIssueCount > 0 ? fabricationReadiness.ElementIds : Array.Empty<ElementId>(),
            fabricationReadiness.Details));

        var worksets = doc.IsWorkshared
            ? new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets().ToList()
            : new List<Workset>();
        checks.Add(Make(config.Get("user_worksets"), worksets.Count, Array.Empty<ElementId>(),
            doc.IsWorkshared ? "User-created worksets in the model." : "Model is not workshared."));

        var totalElements = new FilteredElementCollector(doc).WhereElementIsNotElementType().GetElementCount();
        return new HealthScanResult
        {
            DocumentTitle = doc.Title,
            DocumentPath = string.IsNullOrWhiteSpace(doc.PathName) ? "Unsaved / cloud model" : doc.PathName,
            ScannedAt = DateTime.Now,
            Score = CalculateScore(checks),
            TotalElements = totalElements,
            Checks = checks
        };
    }

    private sealed class CenterlineVisibilityHit
    {
        public ElementId ViewId { get; init; } = ElementId.InvalidElementId;
        public string ViewName { get; init; } = "";
        public string CategoryName { get; init; } = "";
    }

    private sealed class CenterlineCategory
    {
        public Category Category { get; init; } = null!;
        public string Path { get; init; } = "";
    }

    private static List<CenterlineVisibilityHit> FindVisibleCenterlines(Document doc, View? activeView)
    {
        var hits = new List<CenterlineVisibilityHit>();
        if (activeView == null || activeView.IsTemplate) return hits;

        var centerlineCategories = new List<CenterlineCategory>();
        foreach (Category category in doc.Settings.Categories)
            CollectCenterlineCategories(category, category.Name, centerlineCategories);

        var uniqueCategories = centerlineCategories
            .GroupBy(c => c.Category.Id.Value)
            .Select(g => g.First())
            .ToList();

        foreach (var centerlineCategory in uniqueCategories)
        {
            try
            {
                var category = centerlineCategory.Category;
                var visibilityView = GetEffectiveVisibilityView(doc, activeView, category);
                if (IsCategoryClassHidden(visibilityView, category)) continue;
                if (IsCategoryOrParentHidden(visibilityView, category)) continue;

                hits.Add(new CenterlineVisibilityHit
                {
                    ViewId = activeView.Id,
                    ViewName = $"View: {activeView.Name}",
                    CategoryName = centerlineCategory.Path
                });
            }
            catch
            {
                // Some category/view combinations are not valid for visibility queries.
            }
        }

        return hits;
    }

    private static View GetEffectiveVisibilityView(Document doc, View activeView, Category category)
    {
        if (activeView.ViewTemplateId == ElementId.InvalidElementId) return activeView;
        if (doc.GetElement(activeView.ViewTemplateId) is not View template) return activeView;

        var parameter = category.CategoryType switch
        {
            CategoryType.Model => BuiltInParameter.VIS_GRAPHICS_MODEL,
            CategoryType.Annotation => BuiltInParameter.VIS_GRAPHICS_ANNOTATION,
            CategoryType.AnalyticalModel => BuiltInParameter.VIS_GRAPHICS_ANALYTICAL_MODEL,
            _ => BuiltInParameter.INVALID
        };
        if (parameter == BuiltInParameter.INVALID) return activeView;

        var parameterId = new ElementId((long)parameter);
        var templateParameterIds = template.GetTemplateParameterIds();
        if (!templateParameterIds.Any(id => id.Value == parameterId.Value)) return activeView;

        var nonControlledIds = template.GetNonControlledTemplateParameterIds();
        var controlsVisibility = !nonControlledIds.Any(id => id.Value == parameterId.Value);
        return controlsVisibility ? template : activeView;
    }

    private static bool IsCategoryOrParentHidden(View view, Category category)
    {
        Category? current = category;
        while (current != null)
        {
            if (view.CanCategoryBeHidden(current.Id) && view.GetCategoryHidden(current.Id)) return true;
            current = current.Parent;
        }
        return false;
    }

    private static bool IsCategoryClassHidden(View view, Category category)
    {
        return category.CategoryType switch
        {
            CategoryType.Model => view.AreModelCategoriesHidden,
            CategoryType.Annotation => view.AreAnnotationCategoriesHidden,
            CategoryType.AnalyticalModel => view.AreAnalyticalModelCategoriesHidden,
            _ => false
        };
    }

    private static void CollectCenterlineCategories(Category category, string path, ICollection<CenterlineCategory> results)
    {
        if (IsCenterlineName(category.Name))
            results.Add(new CenterlineCategory { Category = category, Path = path });

        try
        {
            foreach (Category subcategory in category.SubCategories)
                CollectCenterlineCategories(subcategory, $"{path} > {subcategory.Name}", results);
        }
        catch
        {
            // Some categories do not expose subcategories in every document context.
        }
    }

    private static bool IsCenterlineName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var normalized = new string(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
        return normalized.Contains("centerline");
    }

    private static HealthCheckResult Make(RuleThreshold rule, int count, IEnumerable<ElementId> ids, string details)
    {
        var reviewAt = Math.Max(0, rule.ReviewAt);
        var failAt = Math.Max(reviewAt, rule.FailAt);
        var status = count >= failAt ? HealthStatus.Fail : count >= reviewAt ? HealthStatus.Review : HealthStatus.Pass;

        // A zero threshold means the rule intentionally triggers at zero. Current defaults are all positive.
        if (reviewAt == 0 && failAt == 0 && count == 0)
            status = HealthStatus.Pass;

        return new HealthCheckResult
        {
            Key = rule.Key,
            Name = rule.Name,
            Category = rule.Category,
            Severity = rule.Severity,
            Status = status,
            Count = count,
            ReviewAt = reviewAt,
            FailAt = failAt,
            Weight = Math.Max(0, rule.Weight),
            Guidance = rule.Guidance,
            Details = details,
            ElementIds = ids.Where(id => id != ElementId.InvalidElementId).Distinct().ToList()
        };
    }

    private static int CalculateScore(IEnumerable<HealthCheckResult> checks)
    {
        double penalty = 0;
        foreach (var check in checks)
        {
            if (check.Status == HealthStatus.Pass) continue;
            penalty += check.Status == HealthStatus.Fail ? check.Weight : check.Weight * 0.45;

            if (check.FailAt > 0 && check.Count > check.FailAt)
            {
                var excessRatio = (double)(check.Count - check.FailAt) / Math.Max(1, check.FailAt);
                penalty += Math.Min(check.Weight, check.Weight * excessRatio * 0.25);
            }
        }
        return Math.Clamp((int)Math.Round(100 - penalty), 0, 100);
    }
}
