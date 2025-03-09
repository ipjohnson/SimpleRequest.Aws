using Amazon.Lambda.Core;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Runtime.Context;

public interface ILambdaContextAccessor {
    ILambdaContext? LambdaContext { get; }
}

[CrossWireService]
public class LambdaContextAccessor : ILambdaContextAccessor {
    public ILambdaContext? LambdaContext {
        get;
        set;
    }
}