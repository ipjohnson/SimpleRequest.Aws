using SimpleRequest.Models.Attributes;
using SimpleRequest.Runtime.Attributes;

namespace CloudWatchDashboardProject.Handlers;

public class IndexHandler {
    [Post("/")]
    [Template("Index")]
    public object Handle() {
        return new { };
    }
}