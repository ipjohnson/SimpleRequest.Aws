using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Pools;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

public interface ILambdaInvokeEngine {
    Task InvokeAsync();
}

[SingletonService]
public class LambdaInvokeEngine: ILambdaInvokeEngine {
    private readonly IStringBuilderPool _stringBuilderPool;
    private readonly ILambdaServerProxy _lambdaServerProxy;
    private readonly IRequestInvocationEngine _invocationEngine;

    public LambdaInvokeEngine(
        IStringBuilderPool stringBuilderPool,
        ILambdaServerProxy lambdaServerProxy,
        IRequestInvocationEngine invocationEngine) {
        _stringBuilderPool = stringBuilderPool;
        _lambdaServerProxy = lambdaServerProxy;
        _invocationEngine = invocationEngine;

        _outputPipe = new Pipe();
    }

    private Pipe _outputPipe;
    private bool _continue = true;
    private Task? _responseTask;
    
    public async Task InvokeAsync() {
        while (_continue) {
            var tuple = await _lambdaServerProxy.GetNextInvokeContext();

            if (tuple == null) {
                _continue = false;
                continue;
            }
            
            var context = tuple.Value.Item1;
            
            var responseStream = 
                new ResponseStream(
                    tuple.Value.Item1,
                    _outputPipe.Writer,
                    BeginResponse, 
                    WriteHeaders);

            context.ResponseData.Body = responseStream;

            await _invocationEngine.Invoke(context);
            Console.WriteLine("Invoke finished");
            
            await responseStream.FlushAsync();

            Console.WriteLine("Starting complete async");
            await _outputPipe.Writer.CompleteAsync();
            Console.WriteLine("Complete async finished");
            
            if (_responseTask != null) {
                await _responseTask;
                _responseTask = null;
            }
            
            await _outputPipe.Reader.CompleteAsync();
            
            await responseStream.DisposeAsync();
            
            _outputPipe.Reset();
            
            tuple.Value.Item2.Dispose();
        }
    }

    private void WriteHeaders(IRequestContext context) {
        using var poolItem = _stringBuilderPool.Get();
        var cookies = new List<string>();

        foreach (var cookie in (context.ResponseData.Cookies as ResponseCookies)?.GetCookies() ?? []) {
            cookie.WriteTo(poolItem.Item);
            cookies.Add(poolItem.Item.ToString());
            poolItem.Item.Clear();
        }
        
        var responseHeader = new ResponseHeader(
            context.ResponseData.Status ?? 200,
            context.ResponseData.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString()),
            cookies.Select(cookie => cookie));

        if (context.ResponseData.ContentType != null) {
            responseHeader.Headers["Content-Type"] = context.ResponseData.ContentType;
        }
        
        var outputStream = _outputPipe.Writer.AsStream(true);
        
        JsonSerializer.Serialize(outputStream, responseHeader);
        outputStream.Write("\0\0\0\0\0\0\0\0"u8);
    }

    private void BeginResponse(IRequestContext context) {
        Console.WriteLine("Begin response");
        _responseTask = Task.Run(
            () => _lambdaServerProxy.SendResponse(context, _outputPipe.Reader));
        Console.WriteLine("Response task started");
    }
}

public class ResponseHeader {
    public ResponseHeader(int statusCode, 
        IDictionary<string, string> headers, 
        IEnumerable<string> cookies) {
        StatusCode = statusCode;
        Headers = headers;
        Cookies = cookies;
    }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get;  }
    
    [JsonPropertyName("headers")]
    public IDictionary<string, string> Headers { get; }
    
    [JsonPropertyName("cookies")]
    public IEnumerable<string> Cookies { get; }
}