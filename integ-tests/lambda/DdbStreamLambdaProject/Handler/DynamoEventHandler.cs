using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Logging;
using SimpleRequest.Aws.Host.DdbStream.Attributes;
using SimpleRequest.Models.Attributes;

namespace DdbStreamLambdaProject.Handler;

public class DynamoEventHandler(ILogger<DynamoEventHandler> logger) {
    [Function(Name = "DynamoEventHandler")]
    public async Task ProcessEvent(
        [OldImage]Dictionary<string, DynamoDBEvent.AttributeValue> oldImage,
        [NewImage]Dictionary<string, DynamoDBEvent.AttributeValue> newImage) {
        
        logger.LogInformation("Processing old image: " + oldImage.GetValueOrDefault("PK"));
        logger.LogInformation("Processing new image: " + newImage.GetValueOrDefault("PK"));        
    }
}