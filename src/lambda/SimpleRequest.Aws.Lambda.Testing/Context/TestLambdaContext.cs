using Amazon.Lambda.Core;

namespace SimpleRequest.Aws.Lambda.Testing.Context;

public class TestLambdaContext(string functionName, string? awsRequestId = null) : ILambdaContext {

    public string AwsRequestId {
        get;
    } = awsRequestId ?? Guid.NewGuid().ToString();

    public IClientContext ClientContext {
        get;
        set;
    } = new TestClientContext();

    public string FunctionName => functionName;

    public string FunctionVersion {
        get;
        set;
    } = "FunctionVersion";

    public ICognitoIdentity Identity {
        get;
        set;
    } = new TestCognitoIdentity();

    public string InvokedFunctionArn {
        get;
        set;
    } = "InvokedFunctionArn";

    public ILambdaLogger Logger {
        get;
        set;
    } = new TestLambdaLogger();

    public string LogGroupName {
        get;
        set;
    } = "LogGroup";

    public string LogStreamName {
        get;
        set;
    } = "LogStreamName";

    public int MemoryLimitInMB {
        get;
        set;
    } = 100;

    public TimeSpan RemainingTime {
        get;
        set;
    } = TimeSpan.FromMilliseconds(1000);
}