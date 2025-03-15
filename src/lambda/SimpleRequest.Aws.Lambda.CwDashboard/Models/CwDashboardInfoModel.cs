namespace SimpleRequest.Aws.Lambda.CwDashboard.Models;

public record CwDashboardInfoModel(
    string DashboardName, 
    string WidgetId,
    string AccountId,
    string Locale,
    int Period,
    bool IsAutoPeriod,
    string Theme);