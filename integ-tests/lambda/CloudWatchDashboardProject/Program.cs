using CloudWatchDashboardProject;
using SimpleRequest.Aws.Lambda.Runtime;

await Test.Main();

LambdaHost.Run<Application>();