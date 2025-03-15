using System.Text.Json;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Lambda.CwDashboard.Impl;

[SingletonService]
public class DashboardInvocationHandler(
    LambdaContextAccessor lambdaContextAccessor,
    IServiceProvider serviceProvider,
    IRequestInvocationEngine requestInvocationEngine,
    IJsonDocumentToContextMapper contextMapper) : ILambdaInvocationHandler {
    
    public async Task<InvocationResponse> Invoke(InvocationRequest invocation) {
        lambdaContextAccessor.LambdaContext = invocation.LambdaContext;
        
        var document = await JsonDocument.ParseAsync(invocation.InputStream);

        await using var scope = serviceProvider.CreateAsyncScope();
        var context = contextMapper.MapToContext(document, scope.ServiceProvider);
        
        await requestInvocationEngine.Invoke(context);
        
        return contextMapper.MapToResponse(context);
    }
}