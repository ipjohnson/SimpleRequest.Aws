using Amazon.Lambda.Core;

namespace SimpleRequest.Aws.Lambda.Responsive.Host.Context;

internal class InternalCognitoIdentity : ICognitoIdentity{

    public string IdentityId {
        get;
        set;
    } = "IdentityId";

    public string IdentityPoolId {
        get;
        set;
    } = "IdentityPoolId";
}