using System.Collections.Concurrent;
using CompiledTemplateEngine.Runtime.Utilities;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Logging;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Aws.Lambda.Runtime.Logging;

[SingletonService]
public class LambdaLoggerProvider(
    IJsonSerializer serializer,
    ILambdaContextAccessor lambdaContextAccessor,
    IMemoryStreamPool memoryStreamPool,
    IUtf8WriterPool utf8WriterPool,
    ILoggingContextAccessor? loggingContextAccessor = null)
    : ILoggerProvider {

    private readonly ConcurrentDictionary<string, LambdaStructuredLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public void Dispose() { }

    public ILogger CreateLogger(string categoryName) {
        return _loggers.GetOrAdd(categoryName,
            categoryString => new LambdaStructuredLogger(
                serializer,
                lambdaContextAccessor,
                loggingContextAccessor,
                memoryStreamPool,
                utf8WriterPool,
                categoryString));
    }
}