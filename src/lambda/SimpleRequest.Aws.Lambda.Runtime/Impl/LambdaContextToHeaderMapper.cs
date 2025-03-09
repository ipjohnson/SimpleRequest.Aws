using Amazon.Lambda.Core;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Primitives;

namespace SimpleRequest.Aws.Lambda.Runtime.Impl;

public interface ILambdaContextToHeaderMapper {
    IDictionary<string, StringValues> GetHeaders(ILambdaContext lambdaContext);
}

[SingletonService]
public class LambdaContextToHeaderMapper : ILambdaContextToHeaderMapper {

    public IDictionary<string, StringValues> GetHeaders(ILambdaContext lambdaContext) {
        var dictionary = new Dictionary<string, StringValues>();

        foreach (var kvp in lambdaContext.ClientContext.Custom) {
            dictionary[kvp.Key] = kvp.Value;
        }
        
        foreach (var kvp in lambdaContext.ClientContext.Environment) {
            dictionary[kvp.Key] = kvp.Value;
        }
        
        return dictionary;
    }
}