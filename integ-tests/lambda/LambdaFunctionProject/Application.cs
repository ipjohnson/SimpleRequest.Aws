using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime;

namespace LambdaFunctionProject;

[DependencyModule]
[LambdaHost]
public partial class Application;