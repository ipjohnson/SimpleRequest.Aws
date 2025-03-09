using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Amazon.Lambda.APIGatewayEvents;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime.Serializer;

namespace SimpleRequest.Aws.Lambda.Web.Serializer;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[TransientService(Key = SerializerConstants.AwsKey, ServiceType = typeof(IJsonTypeInfoResolver))]
internal partial class WebEventTypeResolverContext : JsonSerializerContext {
    
}
