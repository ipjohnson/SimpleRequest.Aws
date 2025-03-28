using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime.Host;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

[SingletonService]
public class ResponsiveLambdaHostImpl(ILambdaInvokeEngine engine) : ILambdaHostInstance {

    public void Run() {
        engine.InvokeAsync().Wait();
    }
}