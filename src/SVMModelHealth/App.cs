using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace SVMModelHealth;

public sealed class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        const string tabName = "Pre-Publish Checks";
        const string panelName = "Health Check";

        try { application.CreateRibbonTab(tabName); } catch { }

        var panel = application.GetRibbonPanels(tabName).FirstOrDefault(p => p.Name == panelName)
                    ?? application.CreateRibbonPanel(tabName, panelName);

        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var installDir = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
        var data = new PushButtonData(
            "SVMModelHealthCommand",
            "Scan Model",
            assemblyPath,
            typeof(ModelHealthCommand).FullName!);

        data.ToolTip = "Open Revit Model Health Check. Performance profiling is available on demand in the dashboard.";
        data.LongDescription = "Opens one Revit Model Health Check dashboard. Health checks run when the command opens; performance profiling is optional and runs only when you click Run Performance Analysis.";
        data.Image = TryLoadImage(Path.Combine(installDir, "assets", "model-health-16.png"));
        data.LargeImage = TryLoadImage(Path.Combine(installDir, "assets", "model-health-32.png"));
        panel.AddItem(data);

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

    private static BitmapImage? TryLoadImage(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }
}
