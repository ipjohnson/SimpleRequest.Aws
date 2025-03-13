using System.Text.Json.Serialization;

namespace WebLambdaProject.Models;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(NotesModel))]
[JsonSerializable(typeof(List<NotesModel>))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
internal partial class ModelSerializers : JsonSerializerContext {
    
}