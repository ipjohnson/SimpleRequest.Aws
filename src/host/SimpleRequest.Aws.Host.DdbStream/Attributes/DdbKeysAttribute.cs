using Amazon.Lambda.DynamoDBEvents;

namespace SimpleRequest.Aws.Host.DdbStream.Attributes;

public class DdbKeysAttribute : BaseDdbEventAttribute {
    protected override object GetValueFromRecord(DynamoDBEvent.DynamodbStreamRecord record) {
        return record.Dynamodb.Keys;
    }

    protected override string FieldName => "Keys";
}