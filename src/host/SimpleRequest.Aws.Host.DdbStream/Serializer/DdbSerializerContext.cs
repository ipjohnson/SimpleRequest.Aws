using System.Text.Json.Serialization;
using Amazon.Lambda.DynamoDBEvents;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime.Serializer;

namespace SimpleRequest.Aws.Host.DdbStream.Serializer;


[JsonSourceGenerationOptions]
[JsonSerializable(typeof(DynamoDBEvent))]
[JsonSerializable(typeof(DynamoDBEvent.AttributeValue))]
[JsonSerializable(typeof(DynamoDBEvent.Identity))]
[JsonSerializable(typeof(DynamoDBEvent.DynamodbStreamRecord))]
[JsonSerializable(typeof(DynamoDBEvent.StreamRecord))]
[JsonSerializable(typeof(DynamoDBTimeWindowEvent))]
[JsonSerializable(typeof(DynamoDBTimeWindowResponse))]
[JsonSerializable(typeof(StreamsEventResponse))]
[JsonSerializable(typeof(Dictionary<string,string>))]
[JsonSerializable(typeof(Dictionary<string,DynamoDBEvent.AttributeValue>))]
[TransientService(Key = SerializerConstants.AwsKey)]
public partial class DdbSerializerContext : JsonSerializerContext;