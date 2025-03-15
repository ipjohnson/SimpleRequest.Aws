using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Aws.Lambda.Runtime.Impl;

public interface ILambdaInvocationHandler {
    Task<InvocationResponse> Invoke(InvocationRequest invocation);
}

[SingletonService]
public class LambdaInvocationHandler(IServiceProvider serviceProvider,
    LambdaContextAccessor lambdaContextAccessor,
    DataServices requestServices,
    IRequestInvocationEngine requestInvocationEngine)
    : ILambdaInvocationHandler {
    private readonly MemoryStream _outputStream = new ();

    public async Task<InvocationResponse> Invoke(InvocationRequest invocation) {
        await using var scoped = serviceProvider.CreateAsyncScope();
        
        lambdaContextAccessor.LambdaContext = invocation.LambdaContext;
    
        var requestData = new RequestData(
            invocation.LambdaContext.FunctionName, 
            "POST",
            invocation.InputStream, 
            "application/json", 
            new PathTokenCollection(),
            new Dictionary<string, StringValues>(),
            new QueryParametersCollection(new Dictionary<string, string>()),
            new RequestCookies());
        
        var responseData = new ResponseData(
            new Dictionary<string, StringValues>(),
            new ResponseCookies()) {
            Body = _outputStream
        };

        var metrics = serviceProvider.GetRequiredService<IMetricLogger>();
        
        var context = new RequestContext(
            scoped.ServiceProvider,
            requestData,
            responseData,
            metrics,
            requestServices,
            CancellationToken.None,
            scoped.ServiceProvider.GetRequiredService<IRequestLogger>());
        
        _outputStream.Position = 0;
        
        await requestInvocationEngine.Invoke(context);
        
        _outputStream.Position = 0;

        await metrics.Flush();
        
        return new InvocationResponse(_outputStream, false);
    }
}