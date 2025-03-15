using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Sqs;
using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Aws.Lambda.Runtime.Interfaces;
using SimpleRequest.Runtime;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.CwDashboard;

public partial class CloudWatchLambdaAttribute : ILambdaHostAttribute, ISimpleRequestEntryAttribute;

[DependencyModule]
[LambdaHost]
[AwsHostSqs]
[EnhancedLoggingSupport]
public partial class CloudWatchLambda;