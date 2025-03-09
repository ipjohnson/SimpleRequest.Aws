using System.Text;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.SQSEvents;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Aws.Lambda.Sqs.Impl;

[SingletonService]
public class SqsLambdaInvocationHandler(IServiceProvider serviceProvider,
    IRequestInvocationEngine requestInvocationEngine,
    RequestServices requestServices,
    IJsonSerializer jsonSerializer,
    LambdaContextAccessor lambdaContextAccessor,
    ILambdaContextToHeaderMapper headerMapper,
    SqsMessageContext sqsMessageContext)
    : ILambdaInvocationHandler {
    private readonly MemoryStream _outputStream = new ();
    private readonly MemoryStream _inputStream = new ();

    public async Task<InvocationResponse> Invoke(InvocationRequest invocation) {
        var sqsEvent = await jsonSerializer.Deserialize<SQSEvent>(invocation.InputStream);

        lambdaContextAccessor.LambdaContext = invocation.LambdaContext;
        var failures = new List<SQSBatchResponse.BatchItemFailure>();
        var headers = headerMapper.GetHeaders(invocation.LambdaContext);
        
        if (sqsEvent != null) {
            foreach (var eventRecord in sqsEvent.Records) {
                sqsMessageContext.Message = eventRecord;
                if (!await ProcessEventRecord(
                        invocation, 
                        eventRecord,
                        new Dictionary<string,StringValues>(headers))) {
                    failures.Add(
                        new SQSBatchResponse.BatchItemFailure{ItemIdentifier = eventRecord.MessageId});
                }
            }
        }
        
        await jsonSerializer.Serialize(_outputStream, new SQSBatchResponse(failures));

        _outputStream.Position = 0;
        
        return new InvocationResponse(_outputStream, false);
    }

    private async Task<bool> ProcessEventRecord(
        InvocationRequest invocation, 
        SQSEvent.SQSMessage eventRecord, Dictionary<string, StringValues> headers) {
        await using var scoped = serviceProvider.CreateAsyncScope();
    
        _inputStream.Position = 0;
        await _inputStream.WriteAsync(Encoding.UTF8.GetBytes(eventRecord.Body));
        _inputStream.Position = 0;

        if (!invocation.LambdaContext.ClientContext.Custom.TryGetValue("Content-Type", out var contentTypeString)) {
            contentTypeString = "application/json";
        }
        
        var requestData = new RequestData(
            invocation.LambdaContext.FunctionName, 
            "POST",
            _inputStream, 
            contentTypeString, 
            new PathTokenCollection(), 
            headers);
        
        var responseData = new ResponseData(
            new Dictionary<string, StringValues>());

        var metrics = serviceProvider.GetRequiredService<IMetricLogger>();
        
        var context = new RequestContext(
            scoped.ServiceProvider,
            requestData,
            responseData,
            metrics,
            requestServices,
            CancellationToken.None,
            scoped.ServiceProvider.GetRequiredService<IRequestLogger>());
        
        await requestInvocationEngine.Invoke(context);

        await metrics.DisposeAsync();

        return responseData.ExceptionValue == null;
    }
}