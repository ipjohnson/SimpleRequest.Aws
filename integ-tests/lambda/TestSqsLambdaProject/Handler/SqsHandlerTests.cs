using DependencyModules.xUnit.Attributes;
using SimpleRequest.Aws.Lambda.Testing;
using SimpleRequest.Aws.Lambda.Testing.Extensions;
using SqsLambdaProject.Model;
using SqsLambdaProject.Services;
using Xunit;

namespace TestSqsLambdaProject.Handler;

public class SqsHandlerTests {
    [ModuleTest]
    public async Task SuccessTest(
        LambdaHarness requestHandler, 
        ISharedInvokeParameters sharedInvokeParameters) {
        
        var record = new SimpleRecordInput("hello"," world");
        
        var response = await requestHandler.SendSqs("sqs-handler", record);

        await response.AssertNoFailures();
        
        Assert.Single(sharedInvokeParameters.ParameterSets);
        var invokeArray = sharedInvokeParameters.ParameterSets.Single();

        Assert.Single(invokeArray);
        var invokeRecord = invokeArray.Single();
        
        Assert.IsType<SimpleRecordInput>(invokeRecord);
        Assert.Equal(record, invokeRecord);
    }
    
}