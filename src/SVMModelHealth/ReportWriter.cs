using System.Net;
using System.Text;

namespace SVMModelHealth;

public static class ReportWriter
{
    public static string ToHtml(HealthScanResult scan)
    {
        static string E(string value) => WebUtility.HtmlEncode(value ?? string.Empty);
        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>Revit Model Health Check</title><style>");
        sb.Append("body{font-family:Segoe UI,Arial,sans-serif;background:#091827;color:#e8eef2;margin:0}header{background:#102d43;padding:24px 36px}main{padding:24px 36px}.score{font-size:54px;font-weight:700}.meta{opacity:.82}.card{background:#0e2334;border:1px solid #24465d;border-radius:8px;margin:12px 0;padding:16px}.Pass{border-left:5px solid #76d486}.Review{border-left:5px solid #e0cb6f}.Fail{border-left:5px solid #ef7d73}.count{float:right;font-size:22px;font-weight:700}h3{margin:0 0 8px}small{opacity:.75}</style></head><body>");
        sb.Append($"<header><div>SVM REVIT MODEL HEALTH</div><div class='score'>{scan.Score}</div><div class='meta'>{E(scan.DocumentTitle)} &nbsp; | &nbsp; {scan.TotalElements:N0} elements &nbsp; | &nbsp; {scan.ScannedAt:g}</div></header><main>");
        foreach (var check in scan.Checks)
        {
            sb.Append($"<section class='card {check.Status}'><div class='count'>{check.Count}</div><h3>{E(check.Name)}</h3><small>{E(check.Category)} · {check.Status} · review ≥ {check.ReviewAt} · fail ≥ {check.FailAt}</small><p>{E(check.Details)}</p><p><b>Recommended action:</b> {E(check.Guidance)}</p></section>");
        }
        sb.Append("</main></body></html>");
        return sb.ToString();
    }
}
