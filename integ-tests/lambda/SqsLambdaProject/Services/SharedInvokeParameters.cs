using DependencyModules.Runtime.Attributes;

namespace SqsLambdaProject.Services;

public interface ISharedInvokeParameters {
    
    void Add(params object[] parameters);
    
    IReadOnlyList<object[]> ParameterSets { get; }
}

[SingletonService]
public class SharedInvokeParameters : ISharedInvokeParameters {
    private List<object[]> _parameterSets = new ();

    public void Add(params object[] parameters) {
        _parameterSets.Add(parameters);
    }

    public IReadOnlyList<object[]> ParameterSets => _parameterSets;
}