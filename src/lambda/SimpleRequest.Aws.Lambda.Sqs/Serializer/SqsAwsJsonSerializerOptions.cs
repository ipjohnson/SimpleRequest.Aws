using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Amazon.Lambda.SQSEvents;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime.Serializer;

namespace SimpleRequest.Aws.Lambda.Sqs.Serializer;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(SQSEvent.SQSMessage))]
[JsonSerializable(typeof(SQSEvent.MessageAttribute))]
[TransientService(Key = SerializerConstants.AwsKey)]
internal partial class SqsTypesContext : JsonSerializerContext {
    
}
