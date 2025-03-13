using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime.Serializer;

namespace SimpleRequest.Aws.Host.Sqs.Serializer;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(SQSEvent.SQSMessage))]
[JsonSerializable(typeof(SQSEvent.MessageAttribute))]
[JsonSerializable(typeof(SQSBatchResponse))]
[TransientService(Key = SerializerConstants.AwsKey)]
public partial class SqsSerializerContext : JsonSerializerContext;