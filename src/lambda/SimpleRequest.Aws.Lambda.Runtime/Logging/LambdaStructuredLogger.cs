using CompiledTemplateEngine.Runtime.Utilities;
using Microsoft.Extensions.Logging;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers.Json;

namespace SimpleRequest.Aws.Lambda.Runtime.Logging;


public class LambdaStructuredLogger : ILogger {
    private readonly ILambdaContextAccessor _lambdaContextAccessor;
    private readonly StructuredLogLineBuilder _structuredLogLineBuilder;

    public LambdaStructuredLogger(IJsonSerializer jsonSerializer,
        ILambdaContextAccessor lambdaContextAccessor,
        ILoggingContextAccessor? loggingContextAccessor,
        IMemoryStreamPool memoryStreamPool,
        IUtf8WriterPool utf8WriterPool,
        string categoryName) {
        _lambdaContextAccessor = lambdaContextAccessor;
        _structuredLogLineBuilder = 
            new StructuredLogLineBuilder(
                lambdaContextAccessor, loggingContextAccessor, jsonSerializer, memoryStreamPool, utf8WriterPool, categoryName);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) {
        var serializedData = _structuredLogLineBuilder.Build(logLevel, eventId, state, exception, formatter);
        
        _lambdaContextAccessor.LambdaContext?.Logger.LogLine(serializedData);
    }
    
    public bool IsEnabled(LogLevel logLevel) {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
}