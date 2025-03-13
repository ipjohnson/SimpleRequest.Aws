using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Host.Sqs;
using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Aws.Lambda.Runtime.Interfaces;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.Sqs;

public partial class SqsLambdaAttribute : ILambdaHostAttribute, ISimpleRequestEntryAttribute;

[DependencyModule]
[LambdaHost]
[AwsHostSqs]
public partial class SqsLambda;