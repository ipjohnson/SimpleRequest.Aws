using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.EventBridge;

namespace EventBridgeLambdaProject;

[DependencyModule]
[EventBridgeHost]
public partial class Application;