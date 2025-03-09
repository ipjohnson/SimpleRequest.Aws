namespace SqsLambdaProject.Model;

public interface IIdAware {
    string Id { get; }
}

public record SimpleRecordInput(string Id, string Name) : IIdAware;