using System.Text.Json;

namespace SimpleRequest.Aws.Host.Runtime.Serializer;

public static class Utf8JsonWriterExtensions {
    public static void WriteKeyValuePair(this Utf8JsonWriter writer, KeyValuePair<string, object> kvp) {
        WritePropertyValue(writer, kvp.Key, kvp.Value);
    }

    public static void WritePropertyValue(this Utf8JsonWriter utf8JsonWriter, string key, object value) {
        utf8JsonWriter.WritePropertyName(key);

        switch (value) {
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
                utf8JsonWriter.WriteStringValue(value?.ToString() ?? string.Empty);
                break;
        }
    }
}