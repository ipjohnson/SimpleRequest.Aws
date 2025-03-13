using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Web;
using SimpleRequest.Runtime;
using SimpleRequest.Web.Runtime;

namespace WebLambdaProject;

[DependencyModule]
[SimpleRequestWeb.Attribute]
[WebLambda.Attribute]
[EnhancedLoggingSupport.Attribute]
public partial class Application;