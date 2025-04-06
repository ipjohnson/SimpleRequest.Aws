using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Runtime.Models;
using SimpleRequest.Web.Runtime.Attributes;

namespace ResponsiveLambdaProject.Handlers;

public class ServerSideEventsHandler {
    public record Response(string Message);
    
    [Get("/server-side-events")]
    [ContentType("text/event-stream")]
    public async IAsyncEnumerable<Response> Handle() {
        for (var i = 0; i < 10; i++) {
            yield return new Response($"Event {i}");
            await Task.Delay(500);
        }
    }
    
    [Get("/server-side-events-model")]
    [ContentType("text/event-stream")]
    public async IAsyncEnumerable<ServerSideEventModel> HandleModel() {
        for (var i = 0; i < 10; i++) {
            yield return new ServerSideEventModel(new Response($"Event {i}"), "EventNameTest", "Id", 1);
            await Task.Delay(500);
        }
    }
}