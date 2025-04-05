using System.Reflection;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Aws.Lambda.Testing.Context;
using SimpleRequest.Runtime.Compression;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Pools;
using SimpleRequest.Runtime.Serializers;
using SimpleRequest.Runtime.Serializers.Json;
using SimpleRequest.Testing;

namespace SimpleRequest.Aws.Lambda.Testing;

public class LambdaHarness(
    IStreamCompressionService streamCompressionService,
    IAwsJsonSerializerOptions options,
    IServiceProvider serviceProvider,
    ILambdaInvocationHandler handler,
    IMemoryStreamPool memoryStreamPool,
    IContentSerializerManager contentSerializer) {
    private static readonly ConstructorInfo? _constructorInfo = 
        typeof(InvocationRequest).GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance, Array.Empty<Type>());
    private static readonly PropertyInfo? _context = typeof(InvocationRequest).GetProperty("LambdaContext");
    private static readonly PropertyInfo? _inputStream = typeof(InvocationRequest).GetProperty("InputStream");
    private readonly SystemTextJsonSerializer _serializer = new (options.Options);
    
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
            streamCompressionService,
            serviceProvider,
            new ResponseData (new Dictionary<string, StringValues>(), new ResponseCookies()) 
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
        if (payload is string stringPayload) {
            await memoryStream.Item.WriteAsync(
                Encoding.UTF8.GetBytes(stringPayload));
        }
        else if (payload is byte[] bytePayload){
            await memoryStream.Item.WriteAsync(bytePayload);
        }
        else if (payload is Stream streamPayload){
            await streamPayload.CopyToAsync(memoryStream.Item);
        }
        else {
            await _serializer.SerializeAsync(memoryStream.Item, payload);
        }
        
        memoryStream.Item.Position = 0;
    }
}