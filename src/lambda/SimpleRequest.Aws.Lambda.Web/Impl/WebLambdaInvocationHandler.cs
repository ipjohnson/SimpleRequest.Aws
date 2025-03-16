using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.ContextAccessor;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Lambda.Web.Impl;

[SingletonService]
public class WebLambdaInvocationHandler(
    IApiGatewayEventConverter eventConverter, 
    IServiceProvider serviceProvider,
    IRequestInvocationEngine invocationEngine,
    LambdaContextAccessor lambdaContextAccessor,
    IContextAccessor? contextAccessor = null) : ILambdaInvocationHandler {
    private readonly MemoryStream _inputStream = new();
    private readonly MemoryStream _outputStream = new();
    
    public async Task<InvocationResponse> Invoke(InvocationRequest invocation) {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = await eventConverter.GetContext(invocation, scope.ServiceProvider, _inputStream, _outputStream);
        
        lambdaContextAccessor.LambdaContext = invocation.LambdaContext;
        
        if (contextAccessor != null) {
            contextAccessor.Context = context;
        }
        
        await invocationEngine.Invoke(context);
        
        var response = await eventConverter.GetResponse(context, _outputStream);

        await context.MetricLogger.Flush();
        
        return response;
    }
}