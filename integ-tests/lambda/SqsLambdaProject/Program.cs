using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Aws.Lambda.Sqs;
using SimpleRequest.Runtime;
using SqsLambdaProject;

[assembly: SqsLambda]
[assembly: EnhancedLoggingSupport]

LambdaHost.Run<ApplicationModule>();