using Amazon.Lambda.Core;

namespace SimpleRequest.Aws.Lambda.Testing.Context;

public class TestCognitoIdentity : ICognitoIdentity{

    public string IdentityId {
        get;
        set;
    } = "IdentityId";

    public string IdentityPoolId {
        get;
        set;
    } = "IdentityPoolId";
}