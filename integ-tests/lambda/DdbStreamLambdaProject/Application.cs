using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.DdbStream;
using SimpleRequest.Runtime;

namespace DdbStreamLambdaProject;

[DependencyModule]
[DdbStreamLambda.Attribute]
[EnhancedLoggingSupport.Attribute]
public partial class Application;