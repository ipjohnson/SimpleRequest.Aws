using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Aws.Lambda.Sqs.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Lambda.Sqs.Attributes;

public class SqsMessageAttributeValueAttribute(string attributeName) : Attribute, IInvokeParameterValueProvider {

    public ValueTask<object?> GetParameterValueAsync(IRequestContext context, IInvokeParameterInfo parameter) {
        var messageContext = context.ServiceProvider.GetRequiredService<ISqsMessageContext>();

        if (messageContext.Message.MessageAttributes.TryGetValue(attributeName, out var value)) {
            return new ValueTask<object?>(value.StringValue);
        }
        
        messageContext.Message.Attributes.TryGetValue(parameter.Name, out var attributeValue);

        return new ValueTask<object?>(attributeValue);
    }
}