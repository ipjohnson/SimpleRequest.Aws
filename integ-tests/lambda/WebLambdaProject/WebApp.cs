using System.Text.Json.Serialization.Metadata;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Aws.Lambda.Web;
using SimpleRequest.Runtime;
using SimpleRequest.Web.Runtime;

namespace WebLambdaProject;

[DependencyModule]
[SimpleRequestWeb.Attribute]
[WebLambda.Attribute]
[EnhancedLoggingSupport.Attribute]
public partial class WebApp {
    public static Task RunAsync() {
        return new LambdaBootstrap(
            new LambdaBootstrapInstance(new WebApp()).Bootstrap).RunAsync();
    }
}