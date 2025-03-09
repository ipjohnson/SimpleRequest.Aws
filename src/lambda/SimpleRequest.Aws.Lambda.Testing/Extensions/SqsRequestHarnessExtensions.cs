using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Runtime.Serializers;
using SimpleRequest.Testing;

namespace SimpleRequest.Aws.Lambda.Testing.Extensions;

public static class SqsRequestHarnessExtensions {
    public static Task<ResponseModel> SendSqs(this LambdaHarness requestHarness, string handlerName, params object[] args) {
        var messages = new SQSEvent.SQSMessage[args.Length];
        var jsonSerializer = requestHarness.ServiceProvider.GetRequiredService<IJsonSerializer>();

        for (var i = 0; i < args.Length; i++) {
            messages[i] = new SQSEvent.SQSMessage {
                Body = jsonSerializer.Serialize(args[i]),
            };
        }
        
        return SendSqs(requestHarness, handlerName, messages);
    }
    
    public static Task<ResponseModel> SendSqs(this LambdaHarness requestHarness, string handlerName, params SQSEvent.SQSMessage[] messages) {
        return requestHarness.Invoke( new SQSEvent { Records = messages.ToList() }, handlerName);
    }

    public static async Task AssertNoFailures(this ResponseModel response) {
        response.AssertOk();
        var batchResponse = await response.Get<SQSBatchResponse>();

        if (batchResponse.BatchItemFailures.Count > 0) {
            throw new Exception($"Expected no errors, but found {batchResponse.BatchItemFailures.Count}.");
        }
    }
}