using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime;

namespace SimpleRequest.Aws.Lambda.Web;

[DependencyModule(GenerateFactories = true)]
[LambdaHost]
public partial class WebLambda;