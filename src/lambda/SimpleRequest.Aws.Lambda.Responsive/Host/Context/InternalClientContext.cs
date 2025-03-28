using Amazon.Lambda.Core;

namespace SimpleRequest.Aws.Lambda.Responsive.Host.Context;

internal class InternalClientContext : IClientContext {

    public IDictionary<string, string> Environment {
        get;
        set;
    } = new Dictionary<string, string>();

    public IClientApplication Client {
        get;
        set;
    } = new TestClientApplication();

    public IDictionary<string, string> Custom {
        get;
        set;
    } = new Dictionary<string, string>();
}