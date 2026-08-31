using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SVMModelHealth;

public static class PerformanceAnalyzer
{
    public static PerformanceScanResult Scan(Document doc, Action<int, int, string>? progress = null)
    {
        var overall = Stopwatch.StartNew();
        var options = new Options
        {
            ComputeReferences = false,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Fine
        };

        var allInstances = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .ToElements();

        // Do not profile arbitrary database objects. GraphicsStyle, line styles, annotation
        // definitions, materials, view definitions, etc. are not physical model content and
        // have caused Revit-native crashes when treated as geometry-bearing elements.
        var candidates = allInstances.Where(IsPerformanceCandidate).ToList();
        var filteredOut = allInstances.Count - candidates.Count;
        var samples = new List<ElementPerformanceSample>();
        var failedCandidates = 0;
        var totalCandidates = candidates.Count;

        using var breadcrumbs = TryOpenBreadcrumbLog(doc, totalCandidates, filteredOut);
        for (var index = 0; index < candidates.Count; index++)
        {
            var element = candidates[index];
            var current = index + 1;
            var label = GetSafeLabel(element);

            // Report BEFORE touching parameters/geometry/connectors. If a native Revit crash
            // occurs, the last visible status and breadcrumb identify the element being entered.
            progress?.Invoke(current, totalCandidates, label);
            WriteBreadcrumb(breadcrumbs, $"START|{current}|{FormatBreadcrumb(element)}");

            try
            {
                samples.Add(ProfileElement(element, options));
                WriteBreadcrumb(breadcrumbs, $"OK|{current}|{element.Id.Value}");
            }
            catch (Exception ex)
            {
                failedCandidates++;
                WriteBreadcrumb(breadcrumbs, $"SKIP|{current}|{element.Id.Value}|{ex.GetType().Name}|{OneLine(ex.Message)}");
            }
        }

        overall.Stop();
        var groups = samples
            .GroupBy(s => new { s.Group, s.ApiType, s.Category })
            .Select(g =>
            {
                var total = g.Sum(x => x.TotalMs);
                var avg = g.Any() ? g.Average(x => (double)x.TotalMs) : 0.0;
                return new PerformanceGroupResult
                {
                    Group = g.Key.Group,
                    ApiType = g.Key.ApiType,
                    Category = g.Key.Category,
                    Instances = g.Count(),
                    TotalMs = total,
                    AverageMs = avg,
                    MaxElementMs = g.Max(x => x.TotalMs),
                    GeometryMs = g.Sum(x => x.GeometryMs),
                    ParameterMs = g.Sum(x => x.ParameterMs),
                    ConnectorMs = g.Sum(x => x.ConnectorMs),
                    SolidCount = g.Sum(x => x.SolidCount),
                    FaceCount = g.Sum(x => x.FaceCount),
                    EdgeCount = g.Sum(x => x.EdgeCount),
                    ParameterCount = g.Sum(x => x.ParameterCount),
                    ConnectorCount = g.Sum(x => x.ConnectorCount),
                    NestedInstanceCount = g.Sum(x => x.NestedInstanceCount),
                    PublishWeight = g.Sum(x => x.PublishWeight),
                    Rating = Rate(avg, total),
                    ElementIds = g.Select(x => x.ElementId).Distinct().ToList()
                };
            })
            .OrderByDescending(g => g.TotalMs)
            .ThenByDescending(g => g.AverageMs)
            .ToList();

        WriteBreadcrumb(breadcrumbs, $"COMPLETE|scanned={samples.Count}|failed={failedCandidates}|filtered={filteredOut}|ms={overall.ElapsedMilliseconds}");

        var familyInstances = candidates.Count(x => x is FamilyInstance);
        var fabricationParts = candidates.Count(x => x is FabricationPart);
        var assemblyInstances = new FilteredElementCollector(doc).OfClass(typeof(AssemblyInstance)).WhereElementIsNotElementType().GetElementCount();
        var levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).WhereElementIsNotElementType().GetElementCount();
        var grids = new FilteredElementCollector(doc).OfClass(typeof(Grid)).WhereElementIsNotElementType().GetElementCount();
        var rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().GetElementCount();
        var spaces = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces).WhereElementIsNotElementType().GetElementCount();
        long modelFileBytes = 0;
        try { if (!string.IsNullOrWhiteSpace(doc.PathName) && File.Exists(doc.PathName)) modelFileBytes = new FileInfo(doc.PathName).Length; } catch { }

        return new PerformanceScanResult
        {
            DocumentTitle = doc.Title,
            ScannedAt = DateTime.Now,
            TotalMs = overall.ElapsedMilliseconds,
            ElementsScanned = samples.Count,
            ElementsSkipped = failedCandidates + filteredOut,
            Groups = groups,
            SlowestElements = samples.OrderByDescending(s => s.TotalMs).Take(100).ToList(),
            FamilyInstances = familyInstances,
            FabricationParts = fabricationParts,
            AssemblyInstances = assemblyInstances,
            Levels = levels,
            Grids = grids,
            Rooms = rooms,
            Spaces = spaces,
            NestedInstances = samples.Sum(x => x.NestedInstanceCount),
            ModelFileBytes = modelFileBytes
        };
    }

    private static bool IsPerformanceCandidate(Element element)
    {
        // Explicit exclusions first. Some of these are not ElementType objects and therefore
        // survive WhereElementIsNotElementType().
        if (element is GraphicsStyle ||
            element is View ||
            element is Material ||
            element is LinePatternElement ||
            element is FillPatternElement ||
            element is Phase ||
            element is Level ||
            element is Grid)
            return false;

        if (element.ViewSpecific) return false;

        // Positive whitelist: actual model content that a publisher commonly extracts.
        if (element is FamilyInstance) return true;
        if (element is FabricationPart) return true;
        if (element is MEPCurve) return true;
        if (element is HostObject) return true;
        if (element is DirectShape) return true;
        if (element is ImportInstance) return true;

        return false;
    }

    private static ElementPerformanceSample ProfileElement(Element element, Options options)
    {
        long geometryMs = 0, parameterMs = 0, connectorMs = 0;
        int parameters = 0, solids = 0, faces = 0, edges = 0, connectors = 0, nestedInstances = 0;
        var total = Stopwatch.StartNew();

        try
        {
            var sw = Stopwatch.StartNew();
            foreach (Parameter parameter in element.Parameters)
            {
                parameters++;
                try
                {
                    _ = parameter.StorageType switch
                    {
                        StorageType.String => parameter.AsString(),
                        StorageType.Integer => parameter.AsInteger().ToString(),
                        StorageType.Double => parameter.AsDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                        StorageType.ElementId => parameter.AsElementId().Value.ToString(),
                        _ => ""
                    };
                }
                catch { }
            }
            sw.Stop();
            parameterMs = sw.ElapsedMilliseconds;
        }
        catch { }

        try
        {
            var sw = Stopwatch.StartNew();
            var geometry = element.get_Geometry(options);
            if (geometry != null) CountGeometry(geometry, ref solids, ref faces, ref edges);
            sw.Stop();
            geometryMs = sw.ElapsedMilliseconds;
        }
        catch { }

        try
        {
            var sw = Stopwatch.StartNew();
            connectors = TouchConnectors(element);
            sw.Stop();
            connectorMs = sw.ElapsedMilliseconds;
        }
        catch { }

        if (element is FamilyInstance familyInstance)
        {
            try { nestedInstances = familyInstance.GetSubComponentIds().Count; } catch { }
        }

        total.Stop();
        var publishWeight = CalculatePublishWeight(parameters, connectors, nestedInstances, solids, faces, edges);
        return new ElementPerformanceSample
        {
            ElementId = element.Id,
            Group = GetGroupName(element),
            ApiType = element.GetType().FullName ?? element.GetType().Name,
            Category = element.Category?.Name ?? "No Category",
            TotalMs = total.ElapsedMilliseconds,
            GeometryMs = geometryMs,
            ParameterMs = parameterMs,
            ConnectorMs = connectorMs,
            ParameterCount = parameters,
            SolidCount = solids,
            FaceCount = faces,
            EdgeCount = edges,
            ConnectorCount = connectors,
            NestedInstanceCount = nestedInstances,
            PublishWeight = publishWeight
        };
    }

    private static string GetGroupName(Element element)
    {
        if (element is FamilyInstance fi)
        {
            var family = fi.Symbol?.Family?.Name ?? "Family";
            var type = fi.Symbol?.Name ?? element.Name;
            return $"{family} : {type}";
        }

        var name = string.IsNullOrWhiteSpace(element.Name) ? element.GetType().Name : element.Name;
        return name;
    }

    private static string GetSafeLabel(Element element)
    {
        string category;
        string name;
        try { category = element.Category?.Name ?? "No Category"; } catch { category = "Unknown Category"; }
        try { name = GetGroupName(element); } catch { name = element.GetType().Name; }
        return $"{element.GetType().Name} | {category} | {name} | Id {element.Id.Value}";
    }

    private static void CountGeometry(IEnumerable geometry, ref int solids, ref int faces, ref int edges)
    {
        foreach (var item in geometry)
        {
            if (item is Solid solid)
            {
                if (solid.Faces.Size == 0 && solid.Edges.Size == 0) continue;
                solids++;
                faces += solid.Faces.Size;
                edges += solid.Edges.Size;
            }
            else if (item is GeometryInstance instance)
            {
                try { CountGeometry(instance.GetInstanceGeometry(), ref solids, ref faces, ref edges); } catch { }
            }
        }
    }

    private static int TouchConnectors(Element element)
    {
        var count = 0;
        object? manager = null;
        if (element is FamilyInstance fi)
            manager = fi.MEPModel?.ConnectorManager;

        if (manager == null)
        {
            var property = element.GetType().GetProperty("ConnectorManager", BindingFlags.Public | BindingFlags.Instance);
            manager = property?.GetValue(element);
        }

        if (manager == null) return count;
        var connectorsProperty = manager.GetType().GetProperty("Connectors", BindingFlags.Public | BindingFlags.Instance);
        var connectors = connectorsProperty?.GetValue(manager) as IEnumerable;
        if (connectors == null) return count;
        foreach (var connector in connectors)
        {
            if (connector == null) continue;
            count++;
            _ = connector.GetType().GetProperty("Id")?.GetValue(connector);
            _ = connector.GetType().GetProperty("Origin")?.GetValue(connector);
        }
        return count;
    }

    private static long CalculatePublishWeight(int parameters, int connectors, int nestedInstances, int solids, int faces, int edges)
    {
        // Diagnostic heuristic only; this is not a STRATUS formula. Geometry and nesting carry
        // more weight because they tend to increase extraction work more than simple metadata.
        return parameters
            + connectors * 8L
            + nestedInstances * 25L
            + solids * 12L
            + faces * 2L
            + Math.Min(edges, 5000);
    }

    private static StreamWriter? TryOpenBreadcrumbLog(Document doc, int candidates, int filteredOut)
    {
        try
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RevitModelHealthCheck");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, "PerformanceAnalysis.log");
            var writer = new StreamWriter(path, false, new UTF8Encoding(false)) { AutoFlush = true };
            writer.WriteLine($"Revit Model Health Check performance breadcrumb log");
            writer.WriteLine($"Started={DateTime.Now:O}");
            writer.WriteLine($"Document={OneLine(doc.Title)}");
            writer.WriteLine($"Candidates={candidates}|FilteredNonModel={filteredOut}");
            return writer;
        }
        catch
        {
            return null;
        }
    }

    private static void WriteBreadcrumb(StreamWriter? writer, string line)
    {
        try { writer?.WriteLine($"{DateTime.Now:O}|{line}"); } catch { }
    }

    private static string FormatBreadcrumb(Element element)
    {
        string category;
        string name;
        try { category = element.Category?.Name ?? "No Category"; } catch { category = "Unknown Category"; }
        try { name = GetGroupName(element); } catch { name = element.GetType().Name; }
        return $"id={element.Id.Value}|class={element.GetType().FullName}|category={OneLine(category)}|name={OneLine(name)}";
    }

    private static string OneLine(string? value) => (value ?? "").Replace("\r", " ").Replace("\n", " ").Replace("|", "/");

    private static string Rate(double averageMs, long totalMs)
    {
        if (averageMs >= 500 || totalMs >= 10000) return "Critical";
        if (averageMs >= 150 || totalMs >= 5000) return "Very Slow";
        if (averageMs >= 50 || totalMs >= 2000) return "Slow";
        if (averageMs >= 15 || totalMs >= 750) return "Moderate";
        return "Normal";
    }
}
