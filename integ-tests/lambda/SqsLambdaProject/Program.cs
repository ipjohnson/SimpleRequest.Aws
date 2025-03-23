using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Aws.Lambda.Sqs;
using SimpleRequest.Runtime;

[assembly: SqsLambda]
[assembly: EnhancedLoggingSupport]

LambdaHost.Run<SqsLambdaProject.ApplicationModule>();