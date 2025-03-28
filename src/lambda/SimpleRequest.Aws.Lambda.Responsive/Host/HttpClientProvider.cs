using Amazon.Lambda.RuntimeSupport;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

public interface ILambdaHttpClientProvider {
    HttpClient GetClient();
}

[SingletonService]
public class LambdaHttpClientProvider(LambdaEnvironment lambdaEnvironment) : ILambdaHttpClientProvider {
    private HttpClient? _client;
    
    public HttpClient GetClient() {
        return _client ??= new HttpClient {
            BaseAddress = //new Uri("http://localhost:9090")
                new Uri("http://" + lambdaEnvironment.RuntimeServerHostAndPort),
            Timeout = TimeSpan.FromDays(1)
        };
    }
}