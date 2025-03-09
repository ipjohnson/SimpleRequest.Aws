using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleRequest.Aws.Lambda.Runtime.Serializer;
using SimpleRequest.Runtime.ContextAccessor;
using SimpleRequest.Runtime.Diagnostics;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Aws.Lambda.Runtime.Impl;

public abstract class BatchIoFilter<TMessage, TRecord> : IRequestFilter {
    private readonly IContextAccessor? _contextAccessor;
    private readonly IRequestLoggingDataProviderService _requestLoggingDataProviderService;
    private readonly ILoggingContextAccessor? _loggingContextAccessor;
    protected readonly IContentSerializerManager ContentSerializerManager;
    protected readonly IContentSerializer JsonSerializer;
    private static readonly MetricDefinition sqsMessageDuration =
        new ("SqsMessageDuration", MetricUnits.Milliseconds);
    
    protected BatchIoFilter(
        IRequestLoggingDataProviderService requestLoggingDataProviderService,
        IContentSerializerManager contentSerializerManager,
        IAwsJsonSerializerOptions jsonSerializerOptions,
        IContextAccessor? contextAccessor,
        ILoggingContextAccessor? loggingContextAccessor) {
        _contextAccessor = contextAccessor;
        _loggingContextAccessor = loggingContextAccessor;
        _requestLoggingDataProviderService = requestLoggingDataProviderService;
        ContentSerializerManager = contentSerializerManager;
        JsonSerializer = new SystemTextJsonSerializer(jsonSerializerOptions.Options);
    }

    public async Task Invoke(IRequestChain requestChain) {
        var message = await DeserializeRequest(requestChain);

        if (message == null) {
            return;
        }

        var failures = new List<TRecord>();
        
        foreach (var record in GetRecords(requestChain, message)) {
            await using var scope = requestChain.Context.ServiceProvider.CreateAsyncScope();
            var timestamp = MachineTimestamp.Now;
            
            var forkChain = requestChain.Fork(requestChain.Context.Clone(scope.ServiceProvider));
            if (_contextAccessor != null) {
                _contextAccessor.Context = forkChain.Context;
            }
            var context = forkChain.Context;
            
            context.RequestData.Body = null;
            context.ResponseData.Body = null;
            context.InvokeParameters = context.RequestHandlerInfo?.InvokeInfo.CreateParameters(context);

            IDisposable? logScope = null;
            
            try {
                logScope = await ProcessRecord(forkChain, message, record);
            }
            catch (Exception e) {
                context.RequestLogger.Instance.LogError(
                    e, "An error occured while processing record: {message}", e.Message);
                context.ResponseData.ExceptionValue = e;
                context.ResponseData.Status = 500;
            }
            finally {
                if (context.ResponseData.ExceptionValue != null) {
                    failures.Add(record);
                }
                
                context.MetricLogger.Record(sqsMessageDuration, timestamp);
            }

            logScope?.Dispose();
            await context.MetricLogger.DisposeAsync();
        }
        
        var response = GetResponse(failures);

        if (requestChain.Context.ResponseData.Body != null) {
            await JsonSerializer.Serialize(requestChain.Context.ResponseData.Body, response);
        }
    }

    protected abstract object GetResponse(List<TRecord> failures);

    protected abstract Task<IDisposable?> ProcessRecord(IRequestChain forkChain, TMessage message, TRecord record);

    protected abstract IEnumerable<TRecord> GetRecords(IRequestChain requestChain, TMessage message);

    protected async ValueTask<TMessage?> DeserializeRequest(IRequestChain requestChain) {
        if (requestChain.Context.RequestData.Body == null) {
            return default;
        }
        
        return await JsonSerializer.Deserialize<TMessage>(requestChain.Context.RequestData.Body);
    }

    protected IDisposable? SetupLoggingScope(IRequestContext context) {
        var loggingData = _requestLoggingDataProviderService.GetRequestLoggingData(context);

        if (loggingData.Count > 0) {
            foreach (var requestLoggingData in loggingData) {
                if ((requestLoggingData.Feature & LoggingDataFeature.MetricData) ==
                    LoggingDataFeature.MetricData) {
                    context.MetricLogger.Data(requestLoggingData.Key, requestLoggingData.Value);
                }

                if ((requestLoggingData.Feature & LoggingDataFeature.MetricTag) ==
                    LoggingDataFeature.MetricTag) {
                    context.MetricLogger.Tag(requestLoggingData.Key, requestLoggingData.Value);
                }
            }

            if (_loggingContextAccessor == null) {
                return context.RequestLogger.Instance.BeginScope(
                    loggingData.Where(
                        d => (d.Feature & LoggingDataFeature.LogData) == LoggingDataFeature.LogData).Select(d => d.AsKeyValuePair()).ToList());
            }
            
            _loggingContextAccessor.SetList(loggingData);
        }

        return null;
    }
}