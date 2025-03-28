using System.Text.Json;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Pools;

namespace SimpleRequest.Aws.Lambda.Runtime.Impl;

public interface IUtf8WriterPool :
    IItemPool<Utf8JsonWriter>, IDisposable;

[SingletonService(As = typeof(IUtf8WriterPool))]
public class Utf8WriterPool() : ItemPool<Utf8JsonWriter>(Factory, CleanUpAction, DisposeAction), IUtf8WriterPool {

    private static void DisposeAction(Utf8JsonWriter obj) {
        obj.Dispose();
    }

    private static void CleanUpAction(Utf8JsonWriter obj) {
        obj.Reset(Stream.Null);
    }

    private static Utf8JsonWriter Factory() {
        return new Utf8JsonWriter(Stream.Null);
    }
}