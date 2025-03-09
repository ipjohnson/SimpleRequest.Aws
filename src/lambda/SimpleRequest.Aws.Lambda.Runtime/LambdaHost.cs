using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Features;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Aws.Lambda.Runtime.Interfaces;
using SimpleRequest.Aws.Lambda.Runtime.Serializer;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Runtime;



[DependencyModule]
[SimpleRequestRuntime.Attribute]
public partial class LambdaHost {
    public partial class Attribute : ILambdaHostAttribute, ISimpleRequestEntryAttribute {
        
    }

}