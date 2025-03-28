using System.IO.Pipelines;
using System.Net.Http.Headers;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

public interface ILambdaServerProxy {
    Task<(IRequestContext, IServiceScope)?> GetNextInvokeContext();
    
    Task SendResponse(IRequestContext context, PipeReader reader);
}

[SingletonService]
public class LambdaServerProxy(
    IServiceProvider serviceProvider,
    ILambdaHttpClientProvider provider,
    ILambdaRequestHeaderMapper headerMapper) : ILambdaServerProxy {
    private MediaTypeHeaderValue _headerValue =
        MediaTypeHeaderValue.Parse("application/vnd.awslambda.http-integration-response");
    
    public async Task<(IRequestContext, IServiceScope)?> GetNextInvokeContext() {
        var client = provider.GetClient();

        try {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Get, "/2018-06-01/runtime/invocation/next");

            var response = await client.SendAsync(httpRequest);
            
            var (request, lambdaContext) = await headerMapper.MapRequestData(response);
            
            var scope = serviceProvider.CreateScope();
            
            var requestContext = 
                GetRequestContext(request, response, lambdaContext, scope.ServiceProvider);
            
            return (requestContext, scope);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }

    private IRequestContext GetRequestContext(IRequestData request, HttpResponseMessage response, ILambdaContext? lambdaContext, IServiceProvider scopeServiceProvider) {
        var context = new RequestContext(
            serviceProvider,
            request,
            new ResponseData(new Dictionary<string, StringValues>(), new ResponseCookies()),
            serviceProvider.GetRequiredService<IMetricLogger>(),
            serviceProvider.GetRequiredService<DataServices>(),
            CancellationToken.None,
            serviceProvider.GetRequiredService<IRequestLogger>()
        );
        
        context.Items.Set("AWS_REQUEST_ID", response.Headers.GetValues("Lambda-Runtime-Aws-Request-Id").First());
        
        return context;
    }

    public async Task SendResponse(IRequestContext context, PipeReader reader) {
        var requestId = context.Items.Get("AWS_REQUEST_ID") as string ?? "unknown";

        var streamContent = new ResponsiveStreamContent(reader.AsStream(true)) {
        };
        
        var client = provider.GetClient();
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/2018-06-01/runtime/invocation/{requestId}/response");
        
        request.Content = streamContent;
        request.Content.Headers.ContentType = _headerValue;
        request.Headers.Add("Transfer-Encoding", "chunked");

        try {
            var response = await client.SendAsync(request).ConfigureAwait(false);

            Console.WriteLine("Status code: " +
                response.StatusCode
            );
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }

    }
}