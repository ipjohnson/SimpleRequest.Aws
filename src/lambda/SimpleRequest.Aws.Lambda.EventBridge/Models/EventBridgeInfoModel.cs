namespace SimpleRequest.Aws.Lambda.EventBridge.Models;

public record EventBridgeInfoModel(
    string Version,
    string Id,
    string DetailType,
    string Source, 
    string Account, 
    string Time,
    string Region, 
    string Resources);