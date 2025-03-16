using System.Text;
using System.Text.Json;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Aws.Lambda.EventBridge.Models;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Pools;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Aws.Lambda.EventBridge.Impl;

public interface IEventBridgeJsonDocumentToContextMapper {
    Task<IRequestContext> MapToContext(IServiceProvider serviceProvider, InvocationRequest invocation);
    
    Task<InvocationResponse> MapToResponse(IRequestContext context);
}

[SingletonService]
public class EventBridgeJsonDocumentToContextMapper(
    IAwsJsonSerializerOptions awsJsonSerializerOptions) : IEventBridgeJsonDocumentToContextMapper {
    private readonly MemoryStream _outputStream = new ();
    private readonly MemoryStream _inputStream = new ();
    
    public async Task<IRequestContext> MapToContext(
        IServiceProvider serviceProvider, InvocationRequest invocation) {
        _inputStream.SetLength(0);
        _outputStream.SetLength(0);
        
        var document = await JsonDocument.ParseAsync(invocation.InputStream);
        
        var eventBridgeInfoModel = 
            document.RootElement.Deserialize<EventBridgeInfoModel>(awsJsonSerializerOptions.Options);

        var request = CreateRequestData(invocation, document,eventBridgeInfoModel);
        var response = new ResponseData(
            new Dictionary<string, StringValues>(),
            new ResponseCookies()) {
            Body = _outputStream
        };

        return new RequestContext(
            serviceProvider,
            request,
            response,
            serviceProvider.GetRequiredService<IMetricLogger>(),
            serviceProvider.GetRequiredService<DataServices>(),
            CancellationToken.None,
            serviceProvider.GetRequiredService<IRequestLogger>()
        );
    }

    private IRequestData CreateRequestData(InvocationRequest invocation, JsonDocument document, EventBridgeInfoModel? eventBridgeInfoModel) {
        
        if (document.RootElement.TryGetProperty("detail", out var detail)) {
            detail.WriteTo(new Utf8JsonWriter(_inputStream));
        }
        
        _inputStream.Position = 0;

        return new RequestData(
             eventBridgeInfoModel?.Source ?? invocation.LambdaContext.FunctionName,
            "POST",
            _inputStream,
            "application/json",
            new EmptyPathTokenCollection(),
            new Dictionary<string, StringValues>(),
            new QueryParametersCollection(new Dictionary<string, string>()),
            new RequestCookies()
        );
    }

    public Task<InvocationResponse> MapToResponse(IRequestContext context) {
        if (_outputStream.Length == 0) {
            _outputStream.Write("{}"u8);
        }
        _outputStream.Position = 0;
        
        return Task.FromResult(new InvocationResponse(_outputStream, false));
    }
}