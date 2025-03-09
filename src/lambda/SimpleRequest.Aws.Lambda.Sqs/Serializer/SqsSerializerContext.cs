using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace SimpleRequest.Aws.Lambda.Sqs.Serializer;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(SQSEvent.SQSMessage))]
[JsonSerializable(typeof(SQSEvent.MessageAttribute))]
[JsonSerializable(typeof(SQSBatchResponse))]
[JsonSerializable(typeof(SQSBatchResponse.BatchItemFailure))]
public partial class SqsSerializerContext : JsonSerializerContext {
    
}