using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Aws.Lambda.Sqs;
using SimpleRequest.Runtime;

namespace SqsLambdaProject;

[DependencyModule]
[SqsLambda.Attribute]
[EnhancedLoggingSupport.Attribute]
public partial class SqsLambdaApp {
    public static Task RunAsync() {
        return new LambdaBootstrap(
            new LambdaBootstrapInstance(new SqsLambdaApp()).Bootstrap).RunAsync();
    }
}