using Amazon.Lambda.SQSEvents;
using SimpleRequest.Aws.Host.Sqs.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Host.Sqs.Attributes;

public class SqsMessageAttributeValueAttribute(string attributeName) : Attribute, IInvokeParameterValueProvider {

    public ValueTask<object?> GetParameterValueAsync(IRequestContext context, IInvokeParameterInfo parameter) {
        var message = context.Items.Get(SqsConstants.ContextItem) as SQSEvent.SQSMessage;

        if (message == null) {
            throw new Exception("Could not find SQSEvent.SQSMessage");
        }
        
        if (message.MessageAttributes.TryGetValue(attributeName, out var value)) {
            return new ValueTask<object?>(value.StringValue);
        }
        
        message.Attributes.TryGetValue(parameter.Name, out var attributeValue);

        return new ValueTask<object?>(attributeValue);
    }
}