using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime.Serializer;

namespace SimpleRequest.Aws.Lambda.Responsive;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[TransientService(Key = SerializerConstants.AwsKey)]
internal partial class WebEventTypeResolverContext : JsonSerializerContext;