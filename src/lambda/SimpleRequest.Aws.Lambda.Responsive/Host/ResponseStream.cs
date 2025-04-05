using System.Buffers;
using System.IO.Pipelines;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

public class ResponseStream(
    IRequestContext requestContext,
    PipeWriter pipeWriter, 
    Action<IRequestContext> beginResponse,
    Action<IRequestContext> writeHeaders) : Stream {
    private int _writtenBytes;
    private bool _beginResponse = false;
    private bool _headersWritten = false;

    public override async ValueTask DisposeAsync() {
        await FlushAsync(CancellationToken.None);

        await pipeWriter.CompleteAsync();
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) {
        Console.WriteLine("Write async");
        
        if (!_headersWritten) {
            _headersWritten = true;
            writeHeaders(requestContext);
        }
        
        if (!_beginResponse) {
            _beginResponse = true;
            beginResponse(requestContext);
        }
        
        await pipeWriter.WriteAsync(buffer, cancellationToken);
        
        await pipeWriter.FlushAsync(cancellationToken);
    }

    public override async Task FlushAsync(CancellationToken cancellationToken) {
        await pipeWriter.FlushAsync(cancellationToken);
        
        if (!_beginResponse) {
            _beginResponse = true;
            Console.WriteLine("Begin response");
            beginResponse(requestContext);
            Console.WriteLine("End response");
        }
    }

    public override void Flush() {
        
    }

    public override int Read(byte[] buffer, int offset, int count) {
        throw new NotSupportedException("Reading from this stream is not supported");
    }

    public override long Seek(long offset, SeekOrigin origin) {
        throw new NotSupportedException("Seeking in this stream is not supported");
    }

    public override void SetLength(long value) {
        throw new NotSupportedException("SetLength is not supported for this stream");
    }

    public override void Write(byte[] buffer, int offset, int count) {
        if (!_headersWritten) {
            _headersWritten = true;
            writeHeaders(requestContext);
        }
        
        if (offset == 0 && count == buffer.Length) {
            _writtenBytes += count;
            pipeWriter.Write(buffer);
        }
        else {
            _writtenBytes += count;
            pipeWriter.Write(buffer.AsMemory(offset, count).Span);
        }
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => _writtenBytes;

    public override long Position {
        get => _writtenBytes;
        set => throw new NotSupportedException("Seeking in this stream is not supported");
    }
}