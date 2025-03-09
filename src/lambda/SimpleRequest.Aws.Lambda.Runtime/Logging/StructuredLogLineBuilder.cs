using System.Text;
using CompiledTemplateEngine.Runtime.Utilities;
using Microsoft.Extensions.Logging;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Aws.Lambda.Runtime.Serializer;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Aws.Lambda.Runtime.Logging;

public class StructuredLogLineBuilder(ILambdaContextAccessor lambdaContextAccessor,
    ILoggingContextAccessor? loggingContextAccessor,
    IJsonSerializer serializer,
    IMemoryStreamPool memoryStreamPool,
    IUtf8WriterPool utf8WriterPool,
    string logger) {
    
    public string Build<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) {
        using var memoryStream = memoryStreamPool.Get();
        using var utf8Writer = utf8WriterPool.Get();

        var jsonWriter = utf8Writer.Item;
        
        jsonWriter.Reset(memoryStream.Item);

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("logger", logger);
        jsonWriter.WriteString("logLevel", logLevel.ToString());
        jsonWriter.WriteString("eventId", eventId.ToString());
        jsonWriter.WriteString("awsRequestId", 
            lambdaContextAccessor.LambdaContext?.AwsRequestId ?? string.Empty);
        
        if (exception != null) {
            jsonWriter.WriteString("exception", exception.ToString());
            jsonWriter.WriteString("exceptionType", exception.GetType().Name);
            jsonWriter.WriteString("stackTrace", exception.StackTrace);
            jsonWriter.WriteString("exceptionMessage", exception.Message);
        }

        var message = formatter(state, exception);
        
        jsonWriter.WriteString("msg", message);

        if (loggingContextAccessor is { LogData.Count: > 0 }) {
            jsonWriter.WritePropertyName("context");
            jsonWriter.WriteStartObject();
            foreach (var pair in loggingContextAccessor.LogData) {
                if ((pair.Feature & LoggingDataFeature.LogData) != LoggingDataFeature.LogData) {
                    continue;
                }
                
                jsonWriter.WritePropertyValue(pair.Key, pair.Value);
            }
            jsonWriter.WriteEndObject(); 
        }
        
        if (state is IEnumerable<KeyValuePair<string, object>> listData) {
            jsonWriter.WritePropertyName("state");
            jsonWriter.WriteStartObject();
            foreach (var pair in listData) {
                if (pair.Key == "{OriginalFormat}") {
                    continue;
                }
                
                jsonWriter.WriteKeyValuePair(pair);
            }
            jsonWriter.WriteEndObject();
        }
        else if (state != null) {
            var jsonString = serializer.Serialize(state);
            
            jsonWriter.WritePropertyName("state");
            jsonWriter.WriteRawValue(jsonString);
        }
        
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();
        
        return Encoding.UTF8.GetString(memoryStream.Item.ToArray());
    }
}