using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Logging;
using SimpleRequest.Aws.Host.DdbStream.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Host.DdbStream.Attributes;

public abstract class BaseDdbEventAttribute : Attribute, IInvokeParameterValueProvider {
    public ValueTask<object?> GetParameterValueAsync(IRequestContext context, IInvokeParameterInfo parameter) {
        var record = context.Items.Get(DdbConstants.DdbRecordKey) as DynamoDBEvent.DynamodbStreamRecord;

        if (record == null) {
            throw new Exception("Could not find ddb record: " + DdbConstants.DdbRecordKey);
        }
        
        var value = GetValueFromRecord(record);

        if (value == null && parameter.IsRequired) {
            throw new Exception($"{FieldName} not present in DynamoDB record ");
        }

        if (parameter.Type != typeof(Dictionary<string, DynamoDBEvent.AttributeValue>)) {
            throw new Exception($"{parameter.Name} must be Dictionary<string, DynamoDBEvent.AttributeValue>");
        }

        context.RequestLogger.Instance.LogInformation("Got here: " + value);
        
        return new ValueTask<object?>(value);
    }

    protected abstract object GetValueFromRecord(DynamoDBEvent.DynamodbStreamRecord record);

    protected abstract string FieldName { get; }
}