using Amazon.Lambda.DynamoDBEvents;

namespace SimpleRequest.Aws.Host.DdbStream.Attributes;

public class NewImageAttribute : BaseDdbEventAttribute {
    protected override object GetValueFromRecord(DynamoDBEvent.DynamodbStreamRecord record) {
        return record.Dynamodb.NewImage;
    }

    protected override string FieldName => "NewImage";
}