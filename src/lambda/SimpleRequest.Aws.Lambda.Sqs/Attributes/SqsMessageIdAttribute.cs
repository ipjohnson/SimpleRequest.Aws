using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Aws.Lambda.Sqs.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Lambda.Sqs.Attributes;

public class SqsMessageIdAttribute : Attribute, IInvokeParameterValueProvider {
    
    public ValueTask<object?> GetParameterValueAsync(IRequestContext context, IInvokeParameterInfo parameter) {
        var messageContext = context.ServiceProvider.GetRequiredService<ISqsMessageContext>();

        return new ValueTask<object?>(messageContext.Message.MessageId);
    }
}