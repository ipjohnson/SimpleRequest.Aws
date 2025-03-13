using Amazon.Lambda.SQSEvents;
using SimpleRequest.Aws.Host.Sqs.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Host.Sqs.Attributes;

public class SqsMessageIdAttribute : Attribute, IInvokeParameterValueProvider {
    
    public ValueTask<object?> GetParameterValueAsync(IRequestContext context, IInvokeParameterInfo parameter) {
        var message = context.Items.Get(SqsConstants.ContextItem) as SQSEvent.SQSMessage;

        if (message == null) {
            throw new Exception("Could not find SQSEvent.SQSMessage");
        }
        
        return new ValueTask<object?>(message.MessageId);
    }
}