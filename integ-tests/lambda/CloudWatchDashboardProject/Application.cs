using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.CwDashboard;
using SimpleRequest.RazorBlade;

namespace CloudWatchDashboardProject;

[DependencyModule]
[CloudWatchLambda]
[RazorBladeRuntime]
public partial class Application;