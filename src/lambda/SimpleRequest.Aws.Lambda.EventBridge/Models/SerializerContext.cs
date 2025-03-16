using System.Text.Json.Serialization;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime.Serializer;

namespace SimpleRequest.Aws.Lambda.EventBridge.Models;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(EventBridgeInfoModel))]
[SingletonService(Key = SerializerConstants.AwsKey)]
public partial class SerializerContext : JsonSerializerContext {
    
}