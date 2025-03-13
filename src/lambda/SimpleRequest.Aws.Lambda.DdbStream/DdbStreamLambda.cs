using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Aws.Lambda.Runtime.Interfaces;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.DdbStream;

[DependencyModule]
[LambdaHost.Attribute]
public partial class DdbStreamLambda {
    public partial class Attribute : ILambdaHostAttribute, ISimpleRequestEntryAttribute {
        
    }
}