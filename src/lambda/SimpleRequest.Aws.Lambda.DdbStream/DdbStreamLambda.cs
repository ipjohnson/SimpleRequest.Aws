using DependencyModules.Runtime.Attributes;
using SimpleRequest.Aws.Lambda.Runtime;
using SimpleRequest.Aws.Lambda.Runtime.Interfaces;
using SimpleRequest.Runtime.Attributes;

namespace SimpleRequest.Aws.Lambda.DdbStream;

public partial class DdbStreamLambdaAttribute : ILambdaHostAttribute, ISimpleRequestEntryAttribute;

[DependencyModule]
[LambdaHost]
public partial class DdbStreamLambda;