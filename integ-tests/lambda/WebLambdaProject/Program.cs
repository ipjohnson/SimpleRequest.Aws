using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Aws.Lambda.Web;
using SimpleRequest.Runtime;
using WebLambdaProject;

[assembly: WebLambda]
[assembly: EnhancedLoggingSupport]

LambdaHost.Run<ApplicationModule>();