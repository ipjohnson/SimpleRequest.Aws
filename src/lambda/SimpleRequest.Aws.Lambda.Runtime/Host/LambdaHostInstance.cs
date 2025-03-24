using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime.Impl;

namespace SimpleRequest.Aws.Lambda.Runtime.Host;

public interface ILambdaHostInstance {
    void Run();
}

[SingletonService]
public class LambdaHostInstance(ILambdaInvocationHandler invocationHandler) : ILambdaHostInstance{
    
    public void Run() {
        var asyncResult = 
            new LambdaBootstrap(invocationHandler.Invoke).RunAsync();
        
        asyncResult.Wait();
    }
}