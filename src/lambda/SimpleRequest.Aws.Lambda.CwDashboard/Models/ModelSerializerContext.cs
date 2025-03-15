using System.Text.Json.Serialization;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime.Serializer;

namespace SimpleRequest.Aws.Lambda.CwDashboard.Models;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(TimeRangeModel))]
[JsonSerializable(typeof(TimeZoneModel))]
[JsonSerializable(typeof(CwDashboardInfoModel))]
[SingletonService(Key = SerializerConstants.AwsKey)]
public partial class ModelSerializerContext : JsonSerializerContext {
    
}