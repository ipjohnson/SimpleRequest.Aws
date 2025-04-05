using System.IO.Pipelines;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

public interface IHeaderWriter {
    void WriteHeader(IRequestContext context, PipeWriter writer);
}

public class HeaderWriter : IHeaderWriter {

    public void WriteHeader(IRequestContext context, PipeWriter writer) {
        
    }
}