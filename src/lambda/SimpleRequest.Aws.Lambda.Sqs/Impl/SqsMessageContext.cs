using Amazon.Lambda.SQSEvents;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Sqs.Impl;

public interface ISqsMessageContext {
    SQSEvent.SQSMessage Message { get; }
}

[CrossWireService]
public class SqsMessageContext : ISqsMessageContext {

    public SQSEvent.SQSMessage Message {
        get;
        set;
    } = default!;
}