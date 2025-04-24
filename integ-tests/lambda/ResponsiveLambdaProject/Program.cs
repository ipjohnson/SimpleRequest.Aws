using ResponsiveLambdaProject;
using SimpleRequest.Aws.Lambda.Responsive;
using SimpleRequest.Aws.Lambda.Runtime;

[assembly: ResponsiveLambdaHost]

LambdaHost.Run<ApplicationModule>();