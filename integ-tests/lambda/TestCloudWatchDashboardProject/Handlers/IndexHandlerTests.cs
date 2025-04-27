using AngleSharp.Html.Parser;
using DependencyModules.xUnit.Attributes;
using SimpleRequest.Aws.Lambda.Testing;

namespace TestCloudWatchDashboardProject.Handlers;

public class IndexHandlerTests {
    [ModuleTest]
    public async Task IndexHandlerTest(LambdaHarness harness) {
        var response = await harness.Invoke(_testPayload,"handler");

        var streamReader = new StreamReader(response.Body);
        var result = await streamReader.ReadToEndAsync();
        
    }

    private readonly string _testPayload =
        "{\"widgetContext\":{\"dashboardName\":\"custom-dashboard\",\"widgetId\":\"widget-9\",\"domain\":\"https://us-east-1.console.aws.amazon.com\",\"accountId\":\"20391886789\",\"locale\":\"en\",\"timezone\":{\"label\":\"UTC\",\"offsetISO\":\"+00:00\",\"offsetInMinutes\":0},\"period\":300,\"isAutoPeriod\":true,\"timeRange\":{\"mode\":\"relative\",\"start\":1741989935213,\"end\":1742000735213,\"relativeStart\":10800002},\"theme\":\"light\",\"linkCharts\":true,\"title\":null,\"params\":null,\"forms\":{\"all\":{}},\"width\":344,\"height\":234}}";
}