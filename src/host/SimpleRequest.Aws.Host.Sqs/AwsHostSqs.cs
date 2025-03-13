using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Runtime;

namespace SimpleRequest.Aws.Host.Sqs;

[DependencyModule]
[AwsHostRuntime.Attribute]
public partial class AwsHostSqs;