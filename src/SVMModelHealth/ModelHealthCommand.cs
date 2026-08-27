using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace SVMModelHealth;

[Transaction(TransactionMode.Manual)]
public sealed class ModelHealthCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            if (uiDoc?.Document == null)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Revit Model Health Check", "Open a Revit project before running Health Check.");
                return Result.Cancelled;
            }

            var config = RuleConfig.Load();
            var healthScan = HealthScanner.Scan(uiDoc.Document, config, uiDoc.ActiveView);

            // The WPF dashboard opens with health results immediately. Performance analysis
            // remains opt-in from the Performance tab so normal Health Check use stays fast and safe.
            var window = new HealthDashboard(uiDoc, healthScan, config);
            window.ShowDialog();
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.ToString();
            Autodesk.Revit.UI.TaskDialog.Show("Revit Model Health Check", "The health check failed:\n\n" + ex.Message);
            return Result.Failed;
        }
    }
}
