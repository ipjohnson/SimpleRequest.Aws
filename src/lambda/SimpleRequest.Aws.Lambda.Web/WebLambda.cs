using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime;

namespace SimpleRequest.Aws.Lambda.Web;

[DependencyModule]
[LambdaHost.Attribute]
public partial class WebLambda {
    
}