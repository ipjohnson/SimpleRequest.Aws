using CloudWatchDashboardProject;
using SimpleRequest.Aws.Lambda.CwDashboard;
using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.RazorBlade;

[assembly: CloudWatchLambda]
[assembly: RazorBladeRuntime]

LambdaHost.Run<ApplicationModule>();