using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

public interface ILambdaRequestHeaderMapper {
    Task<(IRequestData, ILambdaContext?)> MapRequestData(HttpResponseMessage response);

    void MapResponse(IRequestContext context, HttpResponseMessage response);
}

[SingletonService]
public class LambdaRequestHeaderMapper(
    IApiGwV2HeaderMapper apiGwV2HeaderMapper,
    IAwsJsonSerializerOptions options) : ILambdaRequestHeaderMapper {
    private MemoryStream _memoryStream = new ();
    
    public async Task<(IRequestData, ILambdaContext?)> MapRequestData(HttpResponseMessage response) {
        var body = response.Content.ReadAsStream();
        var request = await JsonSerializer.DeserializeAsync<APIGatewayHttpApiV2ProxyRequest>(
            response.Content.ReadAsStream(), options.Options);

        return (apiGwV2HeaderMapper.MapRequestData(request, _memoryStream), CreateLambdaContext(response));
    }

    private ILambdaContext? CreateLambdaContext(HttpResponseMessage response) {
        return null;
    }

    public void MapResponse(IRequestContext context, HttpResponseMessage response) {
        
    }
}