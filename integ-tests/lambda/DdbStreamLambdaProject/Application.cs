using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.DdbStream;
using SimpleRequest.Runtime;

namespace DdbStreamLambdaProject;

[DependencyModule]
[DdbStreamLambda]
[EnhancedLoggingSupport]
public partial class Application;