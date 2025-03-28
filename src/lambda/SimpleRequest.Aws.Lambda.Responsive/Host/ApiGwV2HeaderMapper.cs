using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

public interface IApiGwV2HeaderMapper {
    IRequestData MapRequestData(APIGatewayHttpApiV2ProxyRequest proxyRequest, MemoryStream memoryStream);
}

[SingletonService]
public class ApiGwV2HeaderMapper : IApiGwV2HeaderMapper {

    public IRequestData MapRequestData(APIGatewayHttpApiV2ProxyRequest proxyRequest, MemoryStream memoryStream) {
        
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

    private IEnumerable<KeyValuePair<string,string>> GetCookies(string[]? proxyRequestCookies) {
        if (proxyRequestCookies == null) {
            yield break;
        }
        
        foreach (var cookieString in proxyRequestCookies) {
            var splitCookie = cookieString.Split(';');
            var cookie = splitCookie[0].Split('=');

            if (cookie.Length == 2) {
                yield return new KeyValuePair<string, string>(cookie[0], cookie[1]);
            }
            else {
                yield return new KeyValuePair<string, string>(cookie[0], "");
            }
        }
    }

    private IDictionary<string,StringValues> MapRequestHeaders(APIGatewayHttpApiV2ProxyRequest proxyRequest) {
        var headers = new Dictionary<string, StringValues>();

        foreach (var header in proxyRequest.Headers) {
            headers.Add(header.Key, new StringValues(header.Value.Split(',')));
        }
        
        return headers;
    }
}