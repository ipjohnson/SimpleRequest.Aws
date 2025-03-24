using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime;
using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Aws.Host.Runtime;
using SimpleRequest.Aws.Lambda.Runtime.Host;
using SimpleRequest.Aws.Lambda.Runtime.Interfaces;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Runtime;


public partial class LambdaHostAttribute : ILambdaHostAttribute, ISimpleRequestEntryAttribute;

[DependencyModule]
[SimpleRequestRuntime]
[AwsHostRuntime]
public partial class LambdaHost {
    public static void Run<T>() where T : IDependencyModule, new() {
        Run(new T());
    }

    public static void Run(params IDependencyModule[] modules) {
        var serviceProvider = CreateServiceProvider(modules);

        var host = serviceProvider.GetRequiredService<ILambdaHostInstance>();
        
        host.Run();
    }

    private static ServiceProvider CreateServiceProvider(IDependencyModule[] modules) {
        var collection = new ServiceCollection();
        
        collection.AddModules(modules);
        
        var serviceProvider = collection.BuildServiceProvider();
        
        return serviceProvider;
    }
}