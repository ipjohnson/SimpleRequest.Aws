using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Aws.Lambda.Web;

namespace SimpleRequest.Aws.Lambda.Responsive;

[DependencyModule]
[WebLambda]
public partial class ResponsiveLambdaHost : IServiceCollectionConfiguration {

    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton(new LambdaEnvironment());
    }
}