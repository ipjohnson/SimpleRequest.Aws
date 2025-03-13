using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Sqs;
using SimpleRequest.Runtime;

namespace SqsLambdaProject;

[DependencyModule]
[SqsLambda]
[EnhancedLoggingSupport]
public partial class Application;