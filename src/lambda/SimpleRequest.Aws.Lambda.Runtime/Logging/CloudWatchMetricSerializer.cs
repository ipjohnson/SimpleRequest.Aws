using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using CompiledTemplateEngine.Runtime.Utilities;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Runtime.Logging;

namespace SimpleRequest.Aws.Lambda.Runtime.Logging;

public interface ICloudWatchMetricSerializer {
    IReadOnlyList<string> GetSerializedLines(
        ImmutableList<KeyValuePair<string, object>> tags,
        ImmutableList<KeyValuePair<string, object>> data,
        List<CloudWatchMetrics.MetricValue> values);
}

[SingletonService]
public class CloudWatchMetricSerializer(
    LambdaContextAccessor lambdaContextAccessor,
    ICloudWatchNamespaceProvider namespaceProvider,
    ICloudWatchDimensionSetProvider dimensionSetProvider,
    IMemoryStreamPool memoryStreamPool,
    ILoggingContextAccessor? contextAccessor) : ICloudWatchMetricSerializer {
    private const int MaxValuesPerString = 100;

    public IReadOnlyList<string> GetSerializedLines(
        ImmutableList<KeyValuePair<string, object>> tags,
        ImmutableList<KeyValuePair<string, object>> data,
        List<CloudWatchMetrics.MetricValue> values) {
        using var memoryStream = memoryStreamPool.Get();
        var utfWriter = new Utf8JsonWriter(memoryStream.Item);
        var returnList = new List<string>();

        var (allTags, allData, allValues) = AddContextValues(tags,data,values);
        
        if (values.Count > 100) {

            var index = 0;
            while (index <= data.Count) {

                List<CloudWatchMetrics.MetricValue> list;
                if (index + MaxValuesPerString <= data.Count) {
                    list = values.Slice(index, MaxValuesPerString);
                }
                else {
                    list = values.Slice(index, data.Count - index);
                }
                GetSerializedLine(utfWriter, list, tags, data);
                returnList.Add(Encoding.UTF8.GetString(memoryStream.Item.ToArray()));
                memoryStream.Item.Position = 0;
                utfWriter.Reset(memoryStream.Item);
            }
        }
        else if (values.Count > 0) {
            GetSerializedLine(utfWriter, values, tags, data);
            returnList.Add(Encoding.UTF8.GetString(memoryStream.Item.ToArray()));
            memoryStream.Item.Position = 0;
            utfWriter.Reset(memoryStream.Item);
        }

        return returnList;
    }

    private (ImmutableList<KeyValuePair<string, object>> allTags, 
        ImmutableList<KeyValuePair<string, object>>  allData, 
        List<CloudWatchMetrics.MetricValue> allValues) AddContextValues(
        ImmutableList<KeyValuePair<string, object>> tags,
        ImmutableList<KeyValuePair<string, object>> data,
        List<CloudWatchMetrics.MetricValue> values) {

        if (contextAccessor?.LogData != null) {
            foreach (var loggingData in contextAccessor.LogData) {
                if ((loggingData.Feature | LoggingDataFeature.MetricData) == LoggingDataFeature.MetricData) {
                    data = data.Add(new KeyValuePair<string, object>(loggingData.Key, loggingData.Value));
                }
                else if((loggingData.Feature | LoggingDataFeature.MetricTag) == LoggingDataFeature.MetricTag) {
                    tags = tags.Add(new KeyValuePair<string, object>(loggingData.Key, loggingData.Value));
                }
            }
        }

        return (tags, data, values);
    }

    private void GetSerializedLine(Utf8JsonWriter utfWriter, List<CloudWatchMetrics.MetricValue> values,
        ImmutableList<KeyValuePair<string, object>> tags,
        ImmutableList<KeyValuePair<string, object>> data) {
        
        utfWriter.WriteStartObject();
        WriteAwsProperty(utfWriter, values, tags, data);

        WriteLoggingContextData(utfWriter);
        WriteKeyValuePairData(utfWriter, data);
        WriteMetricValues(utfWriter, values);
        WriteKeyValuePairData(utfWriter,  tags);

        utfWriter.WriteEndObject();
        utfWriter.Flush();
    }

    private void WriteLoggingContextData(Utf8JsonWriter utfWriter) {
        
        utfWriter.WritePropertyValue("awsRequestId",lambdaContextAccessor.LambdaContext?.AwsRequestId ?? "");

        if (contextAccessor != null) {
            foreach (var logData in contextAccessor.LogData) {
                if ((logData.Feature & LoggingDataFeature.MetricData) != LoggingDataFeature.MetricData) {
                    continue;
                }
                
                utfWriter.WritePropertyValue(logData.Key, logData.Value);
            }
        }
    }

    private void WriteMetricValues(Utf8JsonWriter utfWriter, List<CloudWatchMetrics.MetricValue> values) {
        foreach (var value in values) {
            utfWriter.WriteNumber(value.Definition.Name, value.Value);
        }
    }

    private void WriteKeyValuePairData(Utf8JsonWriter utf8JsonWriter, ImmutableList<KeyValuePair<string, object>> data) {
        foreach (var valuePair in data) {
            utf8JsonWriter.WritePropertyName(valuePair.Key);

            switch (valuePair.Value) {
                case string stringValue:
                    utf8JsonWriter.WriteStringValue(stringValue);
                    break;
                case double doubleValue:
                    utf8JsonWriter.WriteNumberValue(doubleValue);
                    break;
                case decimal decimalValue:
                    utf8JsonWriter.WriteNumberValue(decimalValue);
                    break;
                case int intValue:
                    utf8JsonWriter.WriteNumberValue(intValue);
                    break;
                case long longValue:
                    utf8JsonWriter.WriteNumberValue(longValue);
                    break;
                case uint uintValue:
                    utf8JsonWriter.WriteNumberValue(uintValue);
                    break;
                case ulong ulongValue:
                    utf8JsonWriter.WriteNumberValue(ulongValue);
                    break;
                case Enum enumValue:
                    utf8JsonWriter.WriteStringValue(
                        Enum.GetName(enumValue.GetType(), enumValue) ?? enumValue.ToString());
                    break;
                case bool boolValue:
                    utf8JsonWriter.WriteBooleanValue(boolValue);
                    break;
                default:
                    utf8JsonWriter.WriteStringValue(valuePair.Value.ToString());
                    break;
            }
        }
    }

    private void WriteAwsProperty(Utf8JsonWriter utfWriter, 
        List<CloudWatchMetrics.MetricValue> values,
        ImmutableList<KeyValuePair<string, object>> tags, 
        ImmutableList<KeyValuePair<string, object>> data) {

        utfWriter.WritePropertyName("_aws");
        utfWriter.WriteStartObject();
        utfWriter.WriteNumber("Timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        
        utfWriter.WritePropertyName("CloudWatchMetrics");
        utfWriter.WriteStartArray();
        utfWriter.WriteStartObject();
        utfWriter.WriteString("Namespace", namespaceProvider.GetNamespace());
        
        WriteDimensions(utfWriter,  tags);
        WriteMetrics(utfWriter, values, tags, data);
        
        utfWriter.WriteEndObject();
        utfWriter.WriteEndArray();
        utfWriter.WriteEndObject();
    }

    private void WriteMetrics(Utf8JsonWriter utf8JsonWriter, List<CloudWatchMetrics.MetricValue> values, ImmutableList<KeyValuePair<string, object>> tags, ImmutableList<KeyValuePair<string, object>> data) {

        utf8JsonWriter.WritePropertyName("Metrics");
        utf8JsonWriter.WriteStartArray();

        foreach (var metricValue in values) {
            utf8JsonWriter.WriteStartObject();
            utf8JsonWriter.WriteString("Name", metricValue.Definition.Name);
            utf8JsonWriter.WriteString("Unit", metricValue.Definition.Units.Name);
            utf8JsonWriter.WriteEndObject();
        }
        
        utf8JsonWriter.WriteEndArray();
    }

    private void WriteDimensions(
        Utf8JsonWriter utf8JsonWriter, 
        ImmutableList<KeyValuePair<string, object>> tags) {
        utf8JsonWriter.WritePropertyName("Dimensions");
        utf8JsonWriter.WriteStartArray();

        foreach (var dimensionSet in dimensionSetProvider.GetDimensionSets(
                     tags.Select(tag => tag.Key).ToList())) {

            if (dimensionSet.Count == 0) {
                continue;
            }
            utf8JsonWriter.WriteStartArray();
            foreach (var dimension in dimensionSet) {
                utf8JsonWriter.WriteStringValue(dimension);
            }
            utf8JsonWriter.WriteEndArray();
        }
        utf8JsonWriter.WriteEndArray();
    }

}