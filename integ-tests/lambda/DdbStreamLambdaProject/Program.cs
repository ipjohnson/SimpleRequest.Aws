using DdbStreamLambdaProject;
using SimpleRequest.Aws.Lambda.DdbStream;
using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Runtime;

[assembly: DdbStreamLambda]
[assembly: EnhancedLoggingSupport]

LambdaHost.Run<ApplicationModule>();