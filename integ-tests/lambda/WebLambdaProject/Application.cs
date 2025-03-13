using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Web;
using SimpleRequest.Runtime;
using SimpleRequest.Web.Runtime;

namespace WebLambdaProject;

[DependencyModule]
[WebLambda]
[EnhancedLoggingSupport]
public partial class Application;