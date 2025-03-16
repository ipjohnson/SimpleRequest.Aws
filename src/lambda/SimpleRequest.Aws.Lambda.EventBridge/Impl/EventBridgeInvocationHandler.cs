using System.Text.Json;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Lambda.EventBridge.Impl;

[SingletonService]
public class EventBridgeInvocationHandler(
    LambdaContextAccessor lambdaContextAccessor,
    IServiceProvider serviceProvider,
    IRequestInvocationEngine requestInvocationEngine,
    IEventBridgeJsonDocumentToContextMapper mapper) : ILambdaInvocationHandler  {

    public async Task<InvocationResponse> Invoke(InvocationRequest invocation) {
        lambdaContextAccessor.LambdaContext = invocation.LambdaContext;

        await using var scope = serviceProvider.CreateAsyncScope();
        
        var context = await mapper.MapToContext(scope.ServiceProvider, invocation);
        
        await requestInvocationEngine.Invoke(context);
        
        return await mapper.MapToResponse(context);
    }
}