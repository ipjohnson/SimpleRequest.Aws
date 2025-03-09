using System.Collections.Immutable;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Logging;

namespace SimpleRequest.Aws.Lambda.Runtime.Logging;

[SingletonService]
public class CloudWatchMetrics(ICloudWatchMetricSerializer cloudWatchSerializer) : IMetricLogger {
    public record MetricValue(IMetricDefinition Definition, double Value, bool Increment);
    
    private ImmutableList<KeyValuePair<string, object>> _tags = ImmutableList<KeyValuePair<string, object>>.Empty;
    private ImmutableList<KeyValuePair<string, object>> _data = ImmutableList<KeyValuePair<string, object>>.Empty;
    private List<MetricValue> _values = new ();
    private List<MetricValue> _timers = new ();

    public Task Flush() {
        if (_values.Count > 0) {
            return SerializeMetrics();
        }
        
        return Task.CompletedTask;
    }

    private async Task SerializeMetrics() {
        var serializedLines = cloudWatchSerializer.GetSerializedLines(_tags, _data, _values);

        _tags = ImmutableList<KeyValuePair<string, object>>.Empty;
        _data = ImmutableList<KeyValuePair<string, object>>.Empty;
        _timers.Clear();
        _values.Clear();
        
        foreach (var serializedLine in serializedLines) {
            await Console.Out.WriteLineAsync(serializedLine);
        }
    }

    public void StartTimer(IMetricDefinition metricDefinition) {
        _timers.Add(new MetricValue(metricDefinition, MachineTimestamp.Now.GetElapsedMilliseconds(),false));
    }

    public void StopTimer(IMetricDefinition metricDefinition) {
        var now = MachineTimestamp.Now;
        var metricValue = _timers.SingleOrDefault(t => t.Definition.Equals( metricDefinition));

        if (metricValue != null) {
            _timers.Remove(metricValue);
            Record(metricValue.Definition, now.GetElapsedMilliseconds() - metricValue.Value);
        }
    }

    public void Increment(IMetricDefinition metricDefinition, double amount = 1) {
        _values.Add(new(metricDefinition, amount, true));
    }

    public void Record(IMetricDefinition metric, double value) {
        _values.Add(new MetricValue(metric, value, false));
    }

    public void Tag(string tagName, object tagValue) {
        _tags = _tags.Add(KeyValuePair.Create(tagName, tagValue));
    }

    public void ClearTags(Func<KeyValuePair<string, object>, bool>? predicate = null) {
        if (predicate is null) {
            _tags = ImmutableList<KeyValuePair<string, object>>.Empty;
        }
        else {
            _tags = _tags.Where(x => !predicate(x)).ToImmutableList();
        }
    }

    public void Data(string dataName, object dataValue) {
        _data = _data.Add(KeyValuePair.Create(dataName, dataValue));
    }

    public void ClearData(Func<KeyValuePair<string, object>, bool>? predicate = null) {
        if (predicate is null) {
            _data = ImmutableList<KeyValuePair<string, object>>.Empty;
        }
        else {
            _data = _data.Where(x => !predicate(x)).ToImmutableList();
        }
    }

    public IMetricLogger Clone() {
        var cwm = new CloudWatchMetrics(cloudWatchSerializer);
        cwm._data = _data;
        cwm._tags = _tags;
        return cwm;
    }

    public async ValueTask DisposeAsync() {
        await Flush();
    }
}