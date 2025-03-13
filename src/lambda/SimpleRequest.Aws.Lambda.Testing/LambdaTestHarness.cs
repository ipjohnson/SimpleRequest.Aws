using System.Reflection;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using CompiledTemplateEngine.Runtime.Utilities;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Aws.Lambda.Testing.Context;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Serializers;
using SimpleRequest.Runtime.Serializers.Json;
using SimpleRequest.Testing;

namespace SimpleRequest.Aws.Lambda.Testing;

public class LambdaHarness(
    IAwsJsonSerializerOptions options,
    IServiceProvider serviceProvider,
    ILambdaInvocationHandler handler,
    IMemoryStreamPool memoryStreamPool,
    IContentSerializerManager contentSerializer) {
    private static ConstructorInfo? _constructorInfo = 
        typeof(InvocationRequest).GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance, Array.Empty<Type>());
    private static PropertyInfo? _context = typeof(InvocationRequest).GetProperty("LambdaContext");
    private static PropertyInfo? _inputStream = typeof(InvocationRequest).GetProperty("InputStream");
    private SystemTextJsonSerializer _serializer = new (options.Options);
    
    public IServiceProvider ServiceProvider => serviceProvider;

    public  Task<ResponseModel> Invoke(object payload, string functionName) {
        return Invoke(payload, new TestLambdaContext(functionName));
    }
    
    public async Task<ResponseModel> Invoke(object payload, ILambdaContext context) {
        using var memoryStream = memoryStreamPool.Get();
        await Serialize(payload, memoryStream);
        var request = GetRequest(memoryStream, context);
        
        var response = await handler.Invoke(request);

        return new ResponseModel(
            new ResponseData (new Dictionary<string, StringValues>()) 
                { Body = response.OutputStream },
            contentSerializer
        );
    }

    private InvocationRequest GetRequest(IPoolItemReservation<MemoryStream> memoryStream, ILambdaContext context) {
        if (_constructorInfo == null) {
            throw new InvalidOperationException("No InvocationRequest constructor available.");
        }

        if (_inputStream == null) {
            throw new InvalidOperationException("No input stream available.");
        }

        if (_context == null) {
            throw new InvalidOperationException("No context available.");
        }
        
        var instance = _constructorInfo.Invoke(Array.Empty<object>());
        
        _inputStream.SetValue(instance, memoryStream.Item);
        _context.SetValue(instance, context);
        
        return (InvocationRequest)instance;
    }

    private async Task Serialize(object payload, IPoolItemReservation<MemoryStream> memoryStream) {
        await _serializer.Serialize(memoryStream.Item, payload);
        memoryStream.Item.Position = 0;
    }
}