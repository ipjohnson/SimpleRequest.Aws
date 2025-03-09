using Microsoft.Extensions.Logging;
using SimpleRequest.Functions.Runtime.Attributes;
using SqsLambdaProject.Model;
using SqsLambdaProject.Services;

namespace SqsLambdaProject.Handler;


public class SqsHandler(ISharedInvokeParameters sharedInvokeParameters, ILogger<SqsHandler> logger) {
    
    [Function(Name = "sqs-handler")]
    public async Task Handler(SimpleRecordInput input) {
        sharedInvokeParameters.Add(input);
        logger.LogInformation("Received simple record " + input.Id);
    }
}