using System.Runtime.Versioning;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime.Attributes;

namespace CloudWatchDashboardProject.Handlers;

public class IndexHandler {
    [Post("/")]
    [Template("Index")]
    public object Handle() {
        return new { };
    }
}