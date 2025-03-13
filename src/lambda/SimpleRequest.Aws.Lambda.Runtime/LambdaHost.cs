using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Aws.Lambda.Runtime.Interfaces;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Runtime;


public partial class LambdaHostAttribute : ILambdaHostAttribute, ISimpleRequestEntryAttribute;

[DependencyModule]
[SimpleRequestRuntime]
public partial class LambdaHost {
    public static void Run<T>() where T : IDependencyModule, new() {
        Run(new T());
    }

    public static void Run(params IDependencyModule[] modules) {
        var asyncResult = new LambdaBootstrap(
            new LambdaBootstrapInstance(modules).Bootstrap).RunAsync();
        
        asyncResult.Wait();
    }
}