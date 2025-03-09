using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Aws.Lambda.Runtime.Interfaces;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Sqs;

[DependencyModule]
[LambdaHost.Attribute]
public partial class SqsLambda {
    public partial class Attribute : ILambdaHostAttribute, ISimpleRequestEntryAttribute {
        
    }
}