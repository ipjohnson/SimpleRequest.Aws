using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Aws.Lambda.Runtime.Impl;
using SimpleRequest.Runtime.Cookies;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Logging;
using SimpleRequest.Runtime.QueryParameters;

namespace SimpleRequest.Aws.Lambda.DdbStream.Impl;

public interface IDynamoDbRequestContextMapper {
    ValueTask<IRequestContext> MapDdbRecordToContext(
        InvocationRequest invocationRequest,
        IServiceProvider serviceProvider, 
        DynamoDBEvent.DynamodbStreamRecord record);
}

[SingletonService]
public class DynamoDbRequestContextMapper(ILambdaContextToHeaderMapper headerMapper) : IDynamoDbRequestContextMapper {

    public ValueTask<IRequestContext> MapDdbRecordToContext(
        InvocationRequest invocationRequest,
        IServiceProvider serviceProvider,
        DynamoDBEvent.DynamodbStreamRecord record) {
        var requestData = MapRequestData(invocationRequest, serviceProvider, record);
        var responseData = MapResponseData(serviceProvider, record);

        return new ValueTask<IRequestContext>(
            new RequestContext(serviceProvider,
                requestData,
                responseData,
                serviceProvider.GetRequiredService<IMetricLogger>(),
                serviceProvider.GetRequiredService<DataServices>(),
                CancellationToken.None, 
                serviceProvider.GetRequiredService<IRequestLogger>()
                )
        );
    }

    private IResponseData MapResponseData(IServiceProvider serviceProvider, DynamoDBEvent.DynamodbStreamRecord record) {
        return new ResponseData(new Dictionary<string, StringValues>(), new ResponseCookies());
    }

    private IRequestData MapRequestData(InvocationRequest invocationRequest, IServiceProvider serviceProvider, DynamoDBEvent.DynamodbStreamRecord record) {
        return new RequestData(
            invocationRequest.LambdaContext.FunctionName,
            "POST",
            null,
            "application/json",
            new EmptyPathTokenCollection(),
            headerMapper.GetHeaders(invocationRequest.LambdaContext),
            new QueryParametersCollection(new Dictionary<string, string>()),
            new RequestCookies()
        );
    }
}