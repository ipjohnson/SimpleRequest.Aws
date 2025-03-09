using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Runtime.Logging;

public interface ICloudWatchDimensionSetProvider {
    IEnumerable<IReadOnlyList<string>> GetDimensionSets(IReadOnlyList<string> dimensionSetNames);
}

[SingletonService]
public class CloudWatchDimensionSetProvider : ICloudWatchDimensionSetProvider {

    public IEnumerable<IReadOnlyList<string>> GetDimensionSets(IReadOnlyList<string> dimensionSetNames) {
        yield return dimensionSetNames;
    }
}