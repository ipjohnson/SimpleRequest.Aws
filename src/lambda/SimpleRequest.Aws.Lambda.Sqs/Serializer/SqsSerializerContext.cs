using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime.Serializer;

namespace SimpleRequest.Aws.Lambda.Sqs.Serializer;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(SQSEvent.SQSMessage))]
[JsonSerializable(typeof(SQSEvent.MessageAttribute))]
[JsonSerializable(typeof(SQSBatchResponse))]
[JsonSerializable(typeof(SQSBatchResponse.BatchItemFailure))]
[TransientService(Key = SerializerConstants.AwsKey)]
public partial class SqsSerializerContext : JsonSerializerContext;