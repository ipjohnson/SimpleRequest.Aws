using System.Text.Json;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Aws.Lambda.CwDashboard.Models;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Aws.Lambda.CwDashboard.Impl;

public interface IJsonDocumentToContextMapper {
    IRequestContext MapToContext(JsonDocument document, IServiceProvider serviceProvider);

    InvocationResponse MapToResponse(IRequestContext context);
}

[SingletonService]
public class JsonDocumentToContextMapper(
    IAwsJsonSerializerOptions serializerOptions,
    DataServices dataServices) : IJsonDocumentToContextMapper {
    private MemoryStream _inbound = new();
    private MemoryStream _outbound = new();

    public IRequestContext MapToContext(JsonDocument document, IServiceProvider serviceProvider) {
        var widgetContext = document.RootElement.GetProperty("widgetContext");

        var request = MapRequest(document, serviceProvider, widgetContext, out var paramsData);

        var response = new ResponseData(
            new Dictionary<string, StringValues>(),
            new ResponseCookies()) {
            Body = _outbound
        };

        var context = new RequestContext(
            serviceProvider,
            request,
            response,
            serviceProvider.GetRequiredService<IMetricLogger>(),
            dataServices,
            CancellationToken.None,
            serviceProvider.GetRequiredService<IRequestLogger>()
        );

        if (paramsData != null) {
            context.Items.Set("ParamData", paramsData);
        }

        if (widgetContext.TryGetProperty("timeRange", out var timeRange)) {
            var range = timeRange.Deserialize<TimeRangeModel>(serializerOptions.Options);
            if (range != null) {
                context.Items.Set("TimeRange", range);
            }
        }
        
        context.Items.Set("DashboardInfo", widgetContext.Deserialize<CwDashboardInfoModel>()!);

        return context;
    }

    public InvocationResponse MapToResponse(IRequestContext context) {
        if (context.ResponseData.ContentType == "text/html") {
            return SerializeHtmlResponse(context);
        }
        
        var response = new InvocationResponse(context.ResponseData.Body ?? _outbound, false);
        
        response.OutputStream.Position = 0;
        
        return response;
    }

    private InvocationResponse SerializeHtmlResponse(IRequestContext context) {
        _outbound.Position = 0;
        
        var streamReader = new StreamReader(context.ResponseData.Body ?? _outbound);

        var stringValue = streamReader.ReadToEnd();
        
        _outbound.Position = 0;
        _outbound.SetLength(0);

        JsonSerializer.Serialize(_outbound, stringValue);
        _outbound.Position = 0;
        
        return new InvocationResponse(_outbound, false);
    }

    private IRequestData MapRequest(JsonDocument document, IServiceProvider serviceProvider, JsonElement widgetContext, out JsonElement? paramsData) {
        var path = "";

        if (widgetContext.TryGetProperty("params", out var value) && 
            value.ValueKind == JsonValueKind.Object) {
            if (value.TryGetProperty("path", out var pathElement)) {
                path = pathElement.GetString();
            }
            if (value.TryGetProperty("data", out var dataElement)) {
                paramsData = dataElement;
            }
            else {
                paramsData = null;
            }
        }
        else {
            paramsData = null;
        }

        if (string.IsNullOrEmpty(path)) {
            path = "/";
        }

        return new RequestData(
            path,
            "POST",
            _inbound,
            "application/json",
            new PathTokenCollection(),
            new Dictionary<string, StringValues>(),
            new QueryParametersCollection(new Dictionary<string, string>()),
            new RequestCookies()
        );
    }
}