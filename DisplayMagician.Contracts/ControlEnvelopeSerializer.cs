using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DisplayMagician.Contracts;

public static class ControlEnvelopeSerializer
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task WriteAsync(Stream stream, ControlEnvelope envelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(envelope);

        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(envelope, _jsonSerializerOptions);
        if (payload.Length > ControlProtocol.MaximumMessageLength)
        {
            throw new InvalidDataException("The control message exceeds the maximum permitted size.");
        }

        byte[] length = BitConverter.GetBytes(payload.Length);
        await stream.WriteAsync(length, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<ControlEnvelope?> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] length = new byte[sizeof(int)];
        if (!await ReadExactlyAsync(stream, length, allowEndOfStream: true, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        int payloadLength = BitConverter.ToInt32(length, 0);
        if (payloadLength <= 0 || payloadLength > ControlProtocol.MaximumMessageLength)
        {
            throw new InvalidDataException("The control message has an invalid length.");
        }

        byte[] payload = new byte[payloadLength];
        await ReadExactlyAsync(stream, payload, allowEndOfStream: false, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<ControlEnvelope>(payload, _jsonSerializerOptions)
            ?? throw new InvalidDataException("The control message could not be deserialized.");
    }

    private static async Task<bool> ReadExactlyAsync(Stream stream, byte[] buffer, bool allowEndOfStream, CancellationToken cancellationToken)
    {
        int bytesRead = 0;
        while (bytesRead < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(bytesRead, buffer.Length - bytesRead), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                if (allowEndOfStream && bytesRead == 0)
                {
                    return false;
                }

                throw new EndOfStreamException("The control message ended before it was complete.");
            }

            bytesRead += read;
        }

        return true;
    }
}
