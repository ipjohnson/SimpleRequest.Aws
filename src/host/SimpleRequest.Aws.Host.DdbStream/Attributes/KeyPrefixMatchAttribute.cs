using Amazon.Lambda.DynamoDBEvents;
using SimpleRequest.Aws.Host.DdbStream.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Host.DdbStream.Attributes;

public class KeyPrefixMatchAttribute(string key, params string[] prefixes) : Attribute, IExtendedRouteMatch {
    public bool IsMatch(IRequestContext context) {

        if (context.Items.Get(DdbConstants.DdbRecordKey) is 
            DynamoDBEvent.DynamodbStreamRecord keys) {

            if (keys.Dynamodb.Keys.TryGetValue(key, out var value)) {
                foreach (var prefix in prefixes) {
                    if (value.S.StartsWith(prefix)) {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
}