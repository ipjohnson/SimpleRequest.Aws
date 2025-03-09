using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Logging;
using SqsLambdaProject.Model;

namespace SqsLambdaProject.Logging;

[SingletonService]
public class IdLoggingDataProvider : IRequestLoggingDataProvider {

    public IReadOnlyList<RequestLoggingData> GetRequestLoggingData(IRequestContext context) {
        var requestLoggingData = new List<RequestLoggingData>();

        if (context.InvokeParameters != null) {
            for (var i = 0; i < context.InvokeParameters.ParameterCount; i++) {
                var parameter = context.InvokeParameters.Get<IIdAware>(i);

                if (parameter != null) {
                    requestLoggingData.Add(new RequestLoggingData("id", parameter.Id));
                }
            }
        }
        
        return requestLoggingData;
    }
}