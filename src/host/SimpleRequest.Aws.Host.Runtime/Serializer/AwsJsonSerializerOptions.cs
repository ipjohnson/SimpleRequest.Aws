using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.Serialization.SystemTextJson.Converters;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleRequest.Aws.Host.Runtime.Serializer;

public interface IAwsJsonSerializerOptions {
    JsonSerializerOptions Options { get; }
}

[SingletonService]
public class AwsJsonSerializerOptions([FromKeyedServices(SerializerConstants.AwsKey)] IEnumerable<IJsonTypeInfoResolver> providers) : IAwsJsonSerializerOptions {
    private JsonSerializerOptions? _options;
    
    public JsonSerializerOptions Options => _options ??= DefaultOptions(providers);
    
    public static JsonSerializerOptions DefaultOptions(IEnumerable<IJsonTypeInfoResolver> providers) => 
        new JsonSerializerOptions {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = new AwsNamingPolicy(),
        TypeInfoResolver = JsonTypeInfoResolver.Combine(
            providers.ToArray()
            ),
        Converters = {
            new DateTimeConverter(),
            new MemoryStreamConverter(),
            new ConstantClassConverter(),
            new ByteArrayConverter()
        }
    };
}