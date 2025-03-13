using Amazon.Lambda.DynamoDBEvents;

namespace SimpleRequest.Aws.Host.DdbStream.Attributes;

public class OldImageAttribute  : BaseDdbEventAttribute {
    protected override object GetValueFromRecord(DynamoDBEvent.DynamodbStreamRecord record) {
        return record.Dynamodb.OldImage;
    }

    protected override string FieldName => "OldImage";
}