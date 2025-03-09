using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Runtime.Logging;

public interface ICloudWatchNamespaceProvider {
    string GetNamespace();
}

[SingletonService]
public class CloudWatchNamespaceProvider : ICloudWatchNamespaceProvider {
    private string? _namespace;
    
    public string GetNamespace() {
        return _namespace ??= Environment.GetEnvironmentVariable("METRICS_NAMESPACE") ?? "LambdaMetrics";
    }
}