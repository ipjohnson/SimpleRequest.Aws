using System.Text.Json;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleRequest.Aws.Host.DdbStream.Impl;
using SimpleRequest.Aws.Host.Runtime.Serializer;
using SimpleRequest.Aws.Lambda.Runtime.Context;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Lambda.DdbStream.Impl;

[SingletonService]
public class DdbLambdaInvocationHandler(
    IAwsJsonSerializerOptions options,
    IRequestInvocationEngine requestInvocationEngine,
    LambdaContextAccessor lambdaContextAccessor,
    IServiceProvider serviceProvider,
    IDynamoDbRequestContextMapper mapper,
    ILogger<DdbLambdaInvocationHandler> handler) : ILambdaInvocationHandler {
    private readonly MemoryStream _outputStream = new ();
    
    public async Task<InvocationResponse> Invoke(InvocationRequest invocation) {
        lambdaContextAccessor.LambdaContext = invocation.LambdaContext;
        
        var ddbEvent =
            await JsonSerializer.DeserializeAsync<DynamoDBEvent>(invocation.InputStream, options.Options);

        var response = new StreamsEventResponse {
            BatchItemFailures = 
                new List<StreamsEventResponse.BatchItemFailure>(),
        };
        
        handler.LogInformation("Processing event: " + ddbEvent + " count " + ddbEvent?.Records.Count);
        
        if (ddbEvent != null) {
            foreach (var eventRecord in ddbEvent.Records) {
                using var scope = serviceProvider.CreateScope();
                
                if (!await ProcessEventRecord(scope.ServiceProvider, invocation, ddbEvent, eventRecord)) {
                    response.BatchItemFailures.Add(new StreamsEventResponse.BatchItemFailure {
                        ItemIdentifier = eventRecord.EventID
                    });
                }
            }
        }
        
        return new InvocationResponse(_outputStream, false);
    }

    private async Task<bool> ProcessEventRecord(
        IServiceProvider provider,
        InvocationRequest invocation,
        DynamoDBEvent ddbEvent, 
        DynamoDBEvent.DynamodbStreamRecord eventRecord) {
        var result = true;

        try {
            var context = await mapper.MapDdbRecordToContext(invocation, provider, eventRecord);
            
            context.Items.Set(DdbConstants.DdbRecordKey, eventRecord);
            
            await requestInvocationEngine.Invoke(context);
        }
        catch (Exception e) {
            handler.LogError(e, "An error occured while processing event: " + e.Message);
            result = false;
        }
        
        return result;
    }
}