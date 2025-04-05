using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeFuncPack;

internal static class StreamUtils
{
    private const int SegmentSize = 4096;

    internal static async ValueTask<ReadOnlyMemory<byte>> ReadStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        var resultBuffer = new byte[SegmentSize];

        int resultLength = default;

        do
        {
            var bytesRead = await stream.ReadAsync(resultBuffer.AsMemory(resultLength, SegmentSize), cancellationToken);

            if (bytesRead is not > 0)
            {
                break;
            }

            resultLength += bytesRead;

            if (resultBuffer.Length < resultLength + SegmentSize)
            {
                Array.Resize(ref resultBuffer, resultBuffer.Length * 2);
            }
        }
        while (true);

        return resultLength == default
            ? ReadOnlyMemory<byte>.Empty
            : new(resultBuffer, 0, resultLength);
    }
}
