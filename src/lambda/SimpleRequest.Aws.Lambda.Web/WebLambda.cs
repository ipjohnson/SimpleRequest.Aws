using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Runtime.Attributes;
using SimpleRequest.Web.Runtime;

namespace SimpleRequest.Aws.Lambda.Web;

[DependencyModule]
[SimpleRequestWeb]
[LambdaHost]
public partial class WebLambda {
    public partial class Attribute : ISimpleRequestEntryAttribute;
}