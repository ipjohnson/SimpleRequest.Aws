using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Aws.Lambda.Web.Impl;

public interface IApiGatewayEventConverter {
    Task<IRequestContext> GetContext(
        InvocationRequest request, 
        IServiceProvider serviceProvider,
        MemoryStream inputStream,
        MemoryStream outputStream);
    
    Task<InvocationResponse> GetResponse(IRequestContext context,
        MemoryStream outputStream);
}

[SingletonService]
public class ApiGatewayV2ProxyConverter(
    IAwsJsonSerializerOptions options ,
    DataServices requestServices) : IApiGatewayEventConverter {

    public Task<IRequestContext> GetContext(
        InvocationRequest request,
        IServiceProvider serviceProvider,
        MemoryStream inputStream,
        MemoryStream outputStream) {
        return CreateRequestContext(request, serviceProvider, inputStream, outputStream);
    }

    public async Task<InvocationResponse> GetResponse(IRequestContext context,
        MemoryStream outputStream) {
        var response = await CreateResponse(context, outputStream);
        
        outputStream.Position = 0;
        await JsonSerializer.SerializeAsync(outputStream, response, options.Options);
        outputStream.Position = 0;

        return new InvocationResponse(outputStream, false);
    }

    protected virtual async Task<IRequestContext> CreateRequestContext(
        InvocationRequest request, 
        IServiceProvider serviceProvider,
        MemoryStream inputStream,
        MemoryStream outputStream) {
        var proxyRequest = 
            await JsonSerializer.DeserializeAsync<APIGatewayHttpApiV2ProxyRequest>(request.InputStream, options.Options);
        
        return MapProxyRequestToContext(proxyRequest,serviceProvider, inputStream, outputStream);
    }

    private IRequestContext MapProxyRequestToContext(
        APIGatewayHttpApiV2ProxyRequest? proxyRequest, 
        IServiceProvider serviceProvider, 
        MemoryStream inputStream,
        MemoryStream outputStream) {
        
        if (proxyRequest == null) {
            throw new Exception("ProxyRequest is null");
        }
        
        var requestData = MapRequestData(proxyRequest, inputStream);
        
        outputStream.SetLength(0);
        outputStream.Position = 0;
        
        return new RequestContext(
            serviceProvider,
            requestData,
            new ResponseData (new Dictionary<string, StringValues>(), new ResponseCookies()) {
                Body = outputStream
            },
            serviceProvider.GetRequiredService<IMetricLogger>(),
            requestServices,
            CancellationToken.None, 
            serviceProvider.GetRequiredService<IRequestLogger>()
        );
    }

    private IRequestData MapRequestData(APIGatewayHttpApiV2ProxyRequest proxyRequest, MemoryStream memoryStream) {
        
        memoryStream.SetLength(0);
        
        if (!string.IsNullOrEmpty(proxyRequest.Body)) {
            memoryStream.Write(Encoding.UTF8.GetBytes(proxyRequest.Body));
        }
        
        memoryStream.Position = 0;
        
        var headers = MapRequestHeaders(proxyRequest);

        return new RequestData(
            proxyRequest.RawPath,
            proxyRequest.RequestContext.Http.Method,
            memoryStream,
            proxyRequest.Headers.TryGetValue("Content-Type", out var contentTypeHeader) ? contentTypeHeader : "application/json",
            new PathTokenCollection(),
            headers,
            new QueryParametersCollection(proxyRequest.QueryStringParameters),
            new RequestCookies(GetCookies(proxyRequest.Cookies))
        );
    }

    private IEnumerable<KeyValuePair<string,string>> GetCookies(string[] proxyRequestCookies) {
        foreach (var cookieString in proxyRequestCookies) {
            var splitCookie = cookieString.Split(';');
            var cookie = splitCookie[0].Split('=');
            yield return new KeyValuePair<string, string>(cookie[0], cookie[1]);
        }
    }

    private IDictionary<string,StringValues> MapRequestHeaders(APIGatewayHttpApiV2ProxyRequest proxyRequest) {
        var headers = new Dictionary<string, StringValues>();

        foreach (var header in proxyRequest.Headers) {
            headers.Add(header.Key, new StringValues(header.Value.Split(',')));
        }
        
        return headers;
    }

    protected virtual async Task<APIGatewayHttpApiV2ProxyResponse> CreateResponse(IRequestContext context,
        MemoryStream outputStream) {
        string bodyString = "";
        
        if (outputStream.Length > 0) {
            var outputBytes = outputStream.ToArray();
            if (context.ResponseData.IsBinary) {
                bodyString = Convert.ToBase64String(outputBytes);
            }
            else {
                bodyString = Encoding.UTF8.GetString(outputBytes);
            }
        }
        
        return new APIGatewayHttpApiV2ProxyResponse {
            Body = bodyString,
            StatusCode = context.ResponseData.Status ?? 
                         GetStatusCode(context),
            Headers = MapResponseHeaders(context),
            IsBase64Encoded = context.ResponseData.IsBinary
        };
    }

    private int GetStatusCode(IRequestContext context) {
        if (context.ResponseData.ExceptionValue != null) {
            return context.RequestHandlerInfo?.FailureStatus ?? 500;
        }
        
        return context.RequestHandlerInfo?.SuccessStatus ?? 200;
    }

    private IDictionary<string,string> MapResponseHeaders(IRequestContext context) {
        var headers = new Dictionary<string, string>();

        foreach (var kvp in context.ResponseData.Headers) {
            if (kvp.Value.Count > 0) {
                headers.Add(kvp.Key, kvp.Value!);
            }
        }
        
        return headers;
    }
}