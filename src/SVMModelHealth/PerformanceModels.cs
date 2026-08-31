using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace SVMModelHealth;

public sealed class ElementPerformanceSample
{
    public ElementId ElementId { get; init; } = ElementId.InvalidElementId;
    public string Group { get; init; } = "";
    public string ApiType { get; init; } = "";
    public string Category { get; init; } = "";
    public long TotalMs { get; init; }
    public long GeometryMs { get; init; }
    public long ParameterMs { get; init; }
    public long ConnectorMs { get; init; }
    public int ParameterCount { get; init; }
    public int SolidCount { get; init; }
    public int FaceCount { get; init; }
    public int EdgeCount { get; init; }
    public int ConnectorCount { get; init; }
    public int NestedInstanceCount { get; init; }
    public long PublishWeight { get; init; }
}

public sealed class PerformanceGroupResult
{
    public string Group { get; init; } = "";
    public string ApiType { get; init; } = "";
    public string Category { get; init; } = "";
    public int Instances { get; init; }
    public long TotalMs { get; init; }
    public double AverageMs { get; init; }
    public long MaxElementMs { get; init; }
    public long GeometryMs { get; init; }
    public long ParameterMs { get; init; }
    public long ConnectorMs { get; init; }
    public int SolidCount { get; init; }
    public int FaceCount { get; init; }
    public int EdgeCount { get; init; }
    public int ParameterCount { get; init; }
    public int ConnectorCount { get; init; }
    public int NestedInstanceCount { get; init; }
    public long PublishWeight { get; init; }
    public string Rating { get; init; } = "Normal";
    public List<ElementId> ElementIds { get; init; } = new();
}

public sealed class PerformanceScanResult
{
    public string DocumentTitle { get; init; } = "";
    public DateTime ScannedAt { get; init; } = DateTime.Now;
    public long TotalMs { get; init; }
    public int ElementsScanned { get; init; }
    public int ElementsSkipped { get; init; }
    public List<PerformanceGroupResult> Groups { get; init; } = new();
    public List<ElementPerformanceSample> SlowestElements { get; init; } = new();
    public int FamilyInstances { get; init; }
    public int FabricationParts { get; init; }
    public int AssemblyInstances { get; init; }
    public int Levels { get; init; }
    public int Grids { get; init; }
    public int Rooms { get; init; }
    public int Spaces { get; init; }
    public int NestedInstances { get; init; }
    public long ModelFileBytes { get; init; }
}
