using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime.Attributes;

namespace ResponsiveLambdaProject.Handlers;

public class NewLineDelimiterHandler {
    public record Response(string Message);
    
    [Get("/new-line-delimiter")]
    [ContentType("application/x-ndjson")]
    public async IAsyncEnumerable<Response> Handle() {
        for (var i = 0; i < 10; i++) {
            yield return new Response($"Event {i}");
            await Task.Delay(500);
        }
    }
}