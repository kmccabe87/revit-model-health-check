using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WpfColor = System.Windows.Media.Color;
using WpfVisibility = System.Windows.Visibility;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SVMModelHealth;

public partial class HealthDashboard : Window
{
    private static readonly Brush AccentBrush = new SolidColorBrush(WpfColor.FromRgb(0, 139, 244));
    private static readonly Brush PanelBrush = new SolidColorBrush(WpfColor.FromRgb(8, 31, 50));
    private static readonly Brush PassBrush = new SolidColorBrush(WpfColor.FromRgb(109, 225, 140));
    private static readonly Brush WarningBrush = new SolidColorBrush(WpfColor.FromRgb(255, 212, 90));
    private static readonly Brush CriticalBrush = new SolidColorBrush(WpfColor.FromRgb(255, 107, 107));
    private static readonly Brush NormalTextBrush = new SolidColorBrush(WpfColor.FromRgb(230, 238, 245));

    private readonly UIDocument _uiDoc;
    private readonly RuleConfig _config;
    private HealthScanResult _scan;
    private PerformanceScanResult? _performanceScan;
    private bool _performanceRunning;
    private List<HealthRow> _healthRows = new();
    private List<PerformanceRow> _performanceRows = new();

    public HealthDashboard(UIDocument uiDoc, HealthScanResult scan, RuleConfig config)
    {
        _uiDoc = uiDoc;
        _scan = scan;
        _config = config;
        InitializeComponent();

        var version = typeof(HealthDashboard).Assembly.GetName().Version?.ToString(3) ?? "0.6.13";
        VersionText.Text = $"v{version}";
        FooterVersionText.Text = $"v{version}";
        RevitContextText.Text = $"Revit {_uiDoc.Application.Application.VersionNumber}   |   Active View: {_uiDoc.ActiveView?.Name ?? "Unknown"}";

        var image = LoadImage(Path.Combine(AppContext.BaseDirectory, "assets", "model-health-64.png"));
        BrandIcon.Source = image;
        TitleIcon.Source = image;

        Loaded += (_, _) =>
        {
            try
            {
                new WindowInteropHelper(this).Owner = _uiDoc.Application.MainWindowHandle;
            }
            catch { }
        };

        BindScan();
        ShowHealthTab();
    }

    private static BitmapImage? LoadImage(string path)
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
        catch { return null; }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object sender, RoutedEventArgs e) => ToggleMaximize();
    private void ToggleMaximize() => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void HealthTabButton_Click(object sender, RoutedEventArgs e) => ShowHealthTab();
    private void PerformanceTabButton_Click(object sender, RoutedEventArgs e) => ShowPerformanceTab();

    private void ShowHealthTab()
    {
        HealthView.Visibility = WpfVisibility.Visible;
        PerformanceView.Visibility = WpfVisibility.Collapsed;
        HealthTabButton.Background = AccentBrush;
        PerformanceTabButton.Background = PanelBrush;
    }

    private void ShowPerformanceTab()
    {
        HealthView.Visibility = WpfVisibility.Collapsed;
        PerformanceView.Visibility = WpfVisibility.Visible;
        HealthTabButton.Background = PanelBrush;
        PerformanceTabButton.Background = AccentBrush;
    }

    private void RunHealth_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _scan = HealthScanner.Scan(_uiDoc.Document, _config, _uiDoc.ActiveView);
            BindScan();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Revit Model Health Check", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void RunPerformance_Click(object sender, RoutedEventArgs e)
    {
        if (_performanceRunning) return;
        try
        {
            _performanceRunning = true;
            RunPerformanceButton.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            PerformanceProgress.Value = 0;
            PerformanceStatusText.Text = "Building safe physical-model candidate list...";
            PerformanceTabButton.Content = "▥  Performance (Analyzing...)";
            RenderUi();

            _performanceScan = PerformanceAnalyzer.Scan(_uiDoc.Document, (current, total, label) =>
            {
                var percent = total <= 0 ? 0 : Math.Min(100, (int)Math.Round(current * 100.0 / total));
                PerformanceProgress.Value = percent;
                PerformanceStatusText.Text = $"Profiling {current:N0} of {total:N0}: {label}";
                RenderUi();
            });

            BindPerformance();
            PerformanceProgress.Value = 100;
            PerformanceStatusText.Text = "Performance analysis complete.";
            PerformanceTabButton.Content = "▥  Performance";
        }
        catch (Exception ex)
        {
            PerformanceStatusText.Text = "Performance analysis stopped. Health Check results are still available.";
            PerformanceTabButton.Content = "▥  Performance (Error)";
            MessageBox.Show(this,
                "Performance analysis could not finish, but the Health Check remains usable.\n\n" + ex.Message,
                "Revit Model Health Check",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        finally
        {
            _performanceRunning = false;
            RunPerformanceButton.IsEnabled = true;
            Mouse.OverrideCursor = null;
        }
    }

    private void RenderUi()
    {
        try
        {
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        }
        catch { }
    }

    private void BindScan()
    {
        var failed = _scan.Checks.Count(c => c.Status == HealthStatus.Fail);
        var review = _scan.Checks.Count(c => c.Status == HealthStatus.Review);
        var passed = _scan.Checks.Count(c => c.Status == HealthStatus.Pass);

        ScoreValue.Text = _scan.Score.ToString();
        ScoreValue.Foreground = _scan.Score >= 85 ? PassBrush : _scan.Score >= 70 ? WarningBrush : CriticalBrush;
        TotalChecksValue.Text = _scan.Checks.Count.ToString("N0");
        FailedValue.Text = failed.ToString("N0");
        ReviewValue.Text = review.ToString("N0");
        PassedValue.Text = passed.ToString("N0");
        HealthSummaryText.Text = $"{_scan.DocumentTitle}   |   {_scan.TotalElements:N0} model elements   |   {passed} passed   |   {review} review   |   {failed} failed";

        _healthRows = _scan.Checks.Select((c, i) => new HealthRow
        {
            ModelIndex = i,
            Status = c.Status.ToString(),
            Check = c.Name,
            Category = c.Category,
            Count = c.Count,
            Review = c.ReviewAt,
            Fail = c.FailAt,
            Severity = c.Severity.ToString()
        }).ToList();

        var previous = HealthCategoryList.SelectedItem?.ToString();
        var categories = new List<string> { $"All Issues   {failed + review}" };
        categories.AddRange(_scan.Checks.Where(c => c.Status != HealthStatus.Pass)
            .GroupBy(c => c.Category)
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}   {g.Count()}"));
        HealthCategoryList.ItemsSource = categories;
        HealthCategoryList.SelectedIndex = Math.Max(0, previous == null ? 0 : categories.IndexOf(previous));
        ApplyHealthFilter();
    }

    private void BindPerformance()
    {
        if (_performanceScan == null) return;
        PerfTimeValue.Text = $"{_performanceScan.TotalMs / 1000.0:N2}s";
        PerfScannedValue.Text = _performanceScan.ElementsScanned.ToString("N0");
        PerfSkippedValue.Text = _performanceScan.ElementsSkipped.ToString("N0");
        PerfGroupsValue.Text = _performanceScan.Groups.Count.ToString("N0");
        var slowestAverage = _performanceScan.Groups.Count == 0 ? 0 : _performanceScan.Groups.Max(g => g.AverageMs);
        PerfSlowValue.Text = $"{slowestAverage:N1} ms";
        PerformanceSummaryText.Text = $"{_performanceScan.DocumentTitle}   |   {_performanceScan.ElementsScanned:N0} elements profiled   |   {_performanceScan.TotalMs / 1000.0:N2} sec total   |   {_performanceScan.ElementsSkipped:N0} skipped";
        PerformanceGridTitle.Text = $"Performance Results ({_performanceScan.Groups.Count:N0})";

        _performanceRows = _performanceScan.Groups.Select((g, i) => new PerformanceRow
        {
            ModelIndex = i,
            Rating = g.Rating,
            Content = g.Group,
            Category = g.Category,
            Instances = g.Instances,
            TotalMs = g.TotalMs,
            AvgMs = Math.Round(g.AverageMs, 2),
            MaxMs = g.MaxElementMs,
            GeometryMs = g.GeometryMs,
            ParametersMs = g.ParameterMs,
            ConnectorsMs = g.ConnectorMs,
            Solids = g.SolidCount,
            Faces = g.FaceCount
        }).ToList();
        ApplyPerformanceFilter();
    }

    private void HealthCategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyHealthFilter();
    private void HealthSearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyHealthFilter();

    private void ApplyHealthFilter()
    {
        if (_healthRows.Count == 0) { HealthGrid.ItemsSource = null; return; }
        var search = (HealthSearchBox.Text ?? string.Empty).Trim();
        var selected = HealthCategoryList.SelectedItem?.ToString() ?? "All Issues";
        var isAll = selected.StartsWith("All Issues", StringComparison.OrdinalIgnoreCase);
        var selectedCategory = isAll ? string.Empty : ExtractCategoryName(selected);

        var filtered = _healthRows.Where(r =>
            (isAll ? r.Status != nameof(HealthStatus.Pass) : r.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(search) || r.Check.Contains(search, StringComparison.OrdinalIgnoreCase) || r.Category.Contains(search, StringComparison.OrdinalIgnoreCase) || r.Status.Contains(search, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        HealthGrid.ItemsSource = filtered;
        HealthGridTitle.Text = $"Issues ({filtered.Count:N0})";
        if (filtered.Count > 0) HealthGrid.SelectedIndex = 0; else HealthDetailsText.Text = "No matching issues.";
    }

    private static string ExtractCategoryName(string display)
    {
        var split = display.LastIndexOf("   ", StringComparison.Ordinal);
        return split > 0 ? display[..split] : display;
    }

    private void PerformanceSearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyPerformanceFilter();

    private void ApplyPerformanceFilter()
    {
        var search = (PerformanceSearchBox.Text ?? string.Empty).Trim();
        var filtered = _performanceRows.Where(r => string.IsNullOrWhiteSpace(search)
            || r.Content.Contains(search, StringComparison.OrdinalIgnoreCase)
            || r.Category.Contains(search, StringComparison.OrdinalIgnoreCase)
            || r.Rating.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        PerformanceGrid.ItemsSource = filtered;
        PerformanceGridTitle.Text = $"Performance Results ({filtered.Count:N0})";
        if (filtered.Count > 0) PerformanceGrid.SelectedIndex = 0; else PerformanceDetailsText.Text = _performanceScan == null ? "Run the performance analysis to populate results." : "No matching performance results.";
    }

    private void HealthGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) => ShowCurrentHealthDetails();
    private void PerformanceGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) => ShowCurrentPerformanceDetails();
    private void HealthGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => SelectCurrentHealth();
    private void PerformanceGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => SelectCurrentPerformance();

    private HealthCheckResult? CurrentCheck()
    {
        if (HealthGrid.SelectedItem is not HealthRow row) return null;
        return row.ModelIndex >= 0 && row.ModelIndex < _scan.Checks.Count ? _scan.Checks[row.ModelIndex] : null;
    }

    private PerformanceGroupResult? CurrentPerformanceGroup()
    {
        if (_performanceScan == null || PerformanceGrid.SelectedItem is not PerformanceRow row) return null;
        return row.ModelIndex >= 0 && row.ModelIndex < _performanceScan.Groups.Count ? _performanceScan.Groups[row.ModelIndex] : null;
    }

    private void ShowCurrentHealthDetails()
    {
        var check = CurrentCheck();
        HealthDetailsText.Text = check == null ? string.Empty : $"{check.Name} — {check.Details}  Recommended action: {check.Guidance}";
    }

    private void HealthGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (e.Row.Item is not HealthRow row) return;
        e.Row.Foreground = row.Status switch
        {
            nameof(HealthStatus.Fail) => CriticalBrush,
            nameof(HealthStatus.Review) => WarningBrush,
            _ => NormalTextBrush
        };
    }

    private void PerformanceGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (e.Row.Item is not PerformanceRow row) return;
        e.Row.Foreground = row.Rating switch
        {
            "Critical" => CriticalBrush,
            "Very Slow" => new SolidColorBrush(WpfColor.FromRgb(255, 165, 72)),
            "Slow" => WarningBrush,
            "Moderate" => new SolidColorBrush(WpfColor.FromRgb(105, 190, 255)),
            _ => NormalTextBrush
        };
    }

    private void ShowCurrentPerformanceDetails()
    {
        var g = CurrentPerformanceGroup();
        if (g == null || _performanceScan == null) { PerformanceDetailsText.Text = string.Empty; return; }
        var slow = _performanceScan.SlowestElements.Where(x => g.ElementIds.Contains(x.ElementId)).OrderByDescending(x => x.TotalMs).Take(10).ToList();
        var ids = string.Join(", ", slow.Select(x => $"{x.ElementId.Value} ({x.TotalMs} ms)"));
        PerformanceDetailsText.Text = $"{g.Group} | API: {g.ApiType} | Instances: {g.Instances:N0} | Total: {g.TotalMs:N0} ms | Average: {g.AverageMs:N2} ms | Max: {g.MaxElementMs:N0} ms | Geometry: {g.GeometryMs:N0} ms | Parameters: {g.ParameterMs:N0} ms | Connectors: {g.ConnectorMs:N0} ms | Solids/Faces: {g.SolidCount:N0}/{g.FaceCount:N0} | Slowest IDs: {ids}";
    }

    private void SelectHealth_Click(object sender, RoutedEventArgs e) => SelectCurrentHealth();
    private void SelectPerformance_Click(object sender, RoutedEventArgs e) => SelectCurrentPerformance();

    private void SelectCurrentHealth()
    {
        var check = CurrentCheck();
        if (check == null || check.ElementIds.Count == 0)
        {
            MessageBox.Show(this, "This check has no selectable model elements.", "Revit Model Health Check", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        SelectAndShow(check.ElementIds);
    }

    private void SelectCurrentPerformance()
    {
        var group = CurrentPerformanceGroup();
        if (group == null || group.ElementIds.Count == 0)
        {
            MessageBox.Show(this, "This performance row has no selectable model elements.", "Revit Model Health Check", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        SelectAndShow(group.ElementIds);
    }

    private void SelectAndShow(ICollection<ElementId> ids)
    {
        _uiDoc.Selection.SetElementIds(ids);
        try { _uiDoc.ShowElements(ids); } catch { }
        Close();
    }

    private void ExportHealthCsv_Click(object sender, RoutedEventArgs e) => ExportCsv();
    private void ExportPerformanceCsv_Click(object sender, RoutedEventArgs e) => ExportPerformanceCsv();
    private void ExportHtml_Click(object sender, RoutedEventArgs e) => ExportHtml();

    private string AskPath(string filter, string extension, string suffix)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            DefaultExt = extension,
            FileName = $"{Sanitize(_scan.DocumentTitle)}_{suffix}_{DateTime.Now:yyyyMMdd_HHmm}.{extension}"
        };
        return dialog.ShowDialog(this) == true ? dialog.FileName : string.Empty;
    }

    private void ExportCsv()
    {
        var path = AskPath("CSV report (*.csv)|*.csv", "csv", "ModelHealth");
        if (string.IsNullOrEmpty(path)) return;
        var sb = new StringBuilder();
        sb.AppendLine("Status,Check,Category,Count,ReviewAt,FailAt,Severity,SelectableElements,Guidance");
        foreach (var check in _scan.Checks)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                check.Status.ToString(), check.Name, check.Category, check.Count.ToString(), check.ReviewAt.ToString(),
                check.FailAt.ToString(), check.Severity.ToString(), check.ElementIds.Count.ToString(), check.Guidance
            }.Select(Csv)));
        }
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        MessageBox.Show(this, "Health Check CSV report exported.", "Revit Model Health Check");
    }

    private void ExportPerformanceCsv()
    {
        if (_performanceScan == null)
        {
            MessageBox.Show(this, "Run the performance analysis first.", "Revit Model Health Check");
            return;
        }
        var path = AskPath("CSV report (*.csv)|*.csv", "csv", "Performance");
        if (string.IsNullOrEmpty(path)) return;
        var sb = new StringBuilder();
        sb.AppendLine("Rating,Content,ApiType,Category,Instances,TotalMs,AverageMs,MaxElementMs,GeometryMs,ParameterMs,ConnectorMs,Solids,Faces,ElementIds");
        foreach (var g in _performanceScan.Groups)
            sb.AppendLine(string.Join(",", new[] { g.Rating, g.Group, g.ApiType, g.Category, g.Instances.ToString(), g.TotalMs.ToString(), g.AverageMs.ToString("0.00"), g.MaxElementMs.ToString(), g.GeometryMs.ToString(), g.ParameterMs.ToString(), g.ConnectorMs.ToString(), g.SolidCount.ToString(), g.FaceCount.ToString(), string.Join(";", g.ElementIds.Select(x => x.Value)) }.Select(Csv)));
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        MessageBox.Show(this, "Performance CSV report exported.", "Revit Model Health Check");
    }

    private void ExportHtml()
    {
        var path = AskPath("HTML report (*.html)|*.html", "html", "ModelHealth");
        if (string.IsNullOrEmpty(path)) return;
        File.WriteAllText(path, ReportWriter.ToHtml(_scan), Encoding.UTF8);
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch { }
    }

    private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    private static string Sanitize(string value) => string.Concat(value.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));

    private sealed class HealthRow
    {
        public int ModelIndex { get; init; }
        public string Status { get; init; } = "";
        public string Check { get; init; } = "";
        public string Category { get; init; } = "";
        public int Count { get; init; }
        public int Review { get; init; }
        public int Fail { get; init; }
        public string Severity { get; init; } = "";
    }

    private sealed class PerformanceRow
    {
        public int ModelIndex { get; init; }
        public string Rating { get; init; } = "";
        public string Content { get; init; } = "";
        public string Category { get; init; } = "";
        public int Instances { get; init; }
        public long TotalMs { get; init; }
        public double AvgMs { get; init; }
        public long MaxMs { get; init; }
        public long GeometryMs { get; init; }
        public long ParametersMs { get; init; }
        public long ConnectorsMs { get; init; }
        public int Solids { get; init; }
        public int Faces { get; init; }
    }
}
