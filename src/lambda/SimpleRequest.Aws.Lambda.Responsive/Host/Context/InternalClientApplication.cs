using Amazon.Lambda.Core;

namespace SimpleRequest.Aws.Lambda.Responsive.Host.Context;

internal class TestClientApplication : IClientApplication{

    public string AppPackageName {
        get;
        set;
    } = "TestApp";

    public string AppTitle {
        get;
        set;
    } = "TestAppTitle";

    public string AppVersionCode {
        get;
        set;
    } = "TestAppVersionCode";

    public string AppVersionName {
        get;
        set;
    } = "TestAppVersionName";

    public string InstallationId {
        get;
        set;
    } = "TestAppInstallationId";
}