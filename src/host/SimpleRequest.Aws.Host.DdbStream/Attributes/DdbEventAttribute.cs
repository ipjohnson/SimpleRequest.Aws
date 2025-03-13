using Amazon.Lambda.DynamoDBEvents;
using SimpleRequest.Aws.Host.DdbStream.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Host.DdbStream.Attributes;

public class DdbEventAttribute : Attribute, IInvokeParameterValueProvider {

    public ValueTask<object?> GetParameterValueAsync(IRequestContext context, IInvokeParameterInfo parameter) {
        var record = context.Items.Get(DdbConstants.DdbRecordKey) as DynamoDBEvent.DynamodbStreamRecord;

        if (record == null) {
            throw new Exception("Could not find ddb record: " + DdbConstants.DdbRecordKey);
        }

        return new ValueTask<object?>(record);
    }
}