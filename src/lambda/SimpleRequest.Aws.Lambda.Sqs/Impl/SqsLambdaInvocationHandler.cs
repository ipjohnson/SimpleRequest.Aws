using System.Text;
using System.Text.Json;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.SQSEvents;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Aws.Host.Sqs.Impl;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.QueryParameters;
using SimpleRequest.Runtime.Serializers.Json;

namespace SimpleRequest.Aws.Lambda.Sqs.Impl;

[SingletonService]
public class SqsLambdaInvocationHandler(IServiceProvider serviceProvider,
    IRequestInvocationEngine requestInvocationEngine,
    DataServices requestServices,
    IAwsJsonSerializerOptions awsJsonSerializerOptions,
    LambdaContextAccessor lambdaContextAccessor,
    ILambdaContextToHeaderMapper headerMapper)
    : ILambdaInvocationHandler {
    private readonly MemoryStream _outputStream = new ();
    private readonly MemoryStream _inputStream = new ();

    public async Task<InvocationResponse> Invoke(InvocationRequest invocation) {
        var sqsEvent = 
            await JsonSerializer.DeserializeAsync<SQSEvent>(invocation.InputStream, awsJsonSerializerOptions.Options);

        lambdaContextAccessor.LambdaContext = invocation.LambdaContext;
        var failures = new List<SQSBatchResponse.BatchItemFailure>();
        var headers = headerMapper.GetHeaders(invocation.LambdaContext);
        
        if (sqsEvent != null) {
            foreach (var eventRecord in sqsEvent.Records) {
                if (!await ProcessEventRecord(
                        invocation, 
                        eventRecord,
                        new Dictionary<string,StringValues>(headers))) {
                    failures.Add(
                        new SQSBatchResponse.BatchItemFailure{ItemIdentifier = eventRecord.MessageId});
                }
            }
        }
        
        await JsonSerializer.SerializeAsync(
            _outputStream, new SQSBatchResponse(failures), awsJsonSerializerOptions.Options);

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
            headers,
            new QueryParametersCollection(new Dictionary<string, string>()),
            new RequestCookies());
        
        var responseData = new ResponseData(
            new Dictionary<string, StringValues>(), new ResponseCookies());

        var metrics = serviceProvider.GetRequiredService<IMetricLogger>();
        
        var context = new RequestContext(
            scoped.ServiceProvider,
            requestData,
            responseData,
            metrics,
            requestServices,
            CancellationToken.None,
            scoped.ServiceProvider.GetRequiredService<IRequestLogger>());
        
        context.Items.Set(SqsConstants.ContextItem, eventRecord);
        
        await requestInvocationEngine.Invoke(context);

        await metrics.DisposeAsync();

        return responseData.ExceptionValue == null;
    }
}