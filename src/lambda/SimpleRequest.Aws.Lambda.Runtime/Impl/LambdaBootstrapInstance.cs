using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime;
using DependencyModules.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleRequest.Aws.Lambda.Runtime.Impl;

public class LambdaBootstrapInstance {
    private readonly ILambdaInvocationHandler _handler;

    public LambdaBootstrapInstance(params IDependencyModule[] modules) {
        var collection = new ServiceCollection();
        
        collection.AddModules(modules);
        
        var serviceProvider = collection.BuildServiceProvider();
        _handler = serviceProvider.GetRequiredService<ILambdaInvocationHandler>();
    }

    public LambdaBootstrapHandler Bootstrap => _handler.Invoke;
}