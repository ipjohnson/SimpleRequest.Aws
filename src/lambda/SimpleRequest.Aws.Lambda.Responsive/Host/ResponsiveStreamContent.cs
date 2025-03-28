using System.Net;
using SimpleRequest.Runtime.Diagnostics;

namespace SimpleRequest.Aws.Lambda.Responsive.Host;

public class ResponsiveStreamContent : StreamContent {
    private readonly Stream _stream;
    private readonly int _writeDelay = 100;

    public ResponsiveStreamContent(Stream content) : base(content) {
        _stream = content;
    }

    public ResponsiveStreamContent(Stream content, int writeDelay) : base(content) {
        _stream = content;
        _writeDelay = writeDelay;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) {
        var buffer = new byte[1024];
        var continueReading = true;
        long totalCount = 0;
        
        MachineTimestamp? timestamp = null;

        while (continueReading && !cancellationToken.IsCancellationRequested) {
            var read =
                await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            totalCount += read;
            
            if (read > 0) {
                await stream.WriteAsync(buffer, 0, read, cancellationToken);

                if (timestamp == null || 
                    timestamp.Value.GetElapsedMilliseconds() > _writeDelay) {
                    await stream.FlushAsync(cancellationToken);
                    timestamp = MachineTimestamp.Now;
                }
            }
            else {
                continueReading = false;
                Console.WriteLine($"Total count: {totalCount}");
            }
        }
    }
}