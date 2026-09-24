using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ProtocolReliabilityTests
{
    [Fact]
    public async Task EnvelopeSerializer_AcceptsMessagesBelowTheSharedFiveMiBLimit()
    {
        ControlEnvelope envelope = new ControlEnvelope
        {
            MessageType = ControlMessageType.ListProfiles,
            Payload = new string('a', ControlProtocol.MaximumMessageLength - 1024)
        };
        await using MemoryStream stream = new MemoryStream();

        await ControlEnvelopeSerializer.WriteAsync(stream, envelope, CancellationToken.None);

        Assert.True(stream.Length <= ControlProtocol.MaximumMessageLength + sizeof(int));
        stream.Position = 0;
        ControlEnvelope? reread = await ControlEnvelopeSerializer.ReadAsync(stream, CancellationToken.None);
        Assert.NotNull(reread);
        Assert.Equal(envelope.RequestId, reread!.RequestId);
    }

    [Fact]
    public async Task EnvelopeSerializer_RejectsMessagesAboveTheSharedFiveMiBLimit()
    {
        ControlEnvelope envelope = new ControlEnvelope
        {
            MessageType = ControlMessageType.ListProfiles,
            Payload = new string('a', ControlProtocol.MaximumMessageLength)
        };
        await using MemoryStream stream = new MemoryStream();

        await Assert.ThrowsAsync<InvalidDataException>(() => ControlEnvelopeSerializer.WriteAsync(stream, envelope, CancellationToken.None));
    }

    [Fact]
    public async Task EnvelopeSerializer_RejectsAnEmptyRequestIdFromAnUntrustedWireMessage()
    {
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(new ControlEnvelope
        {
            MessageType = ControlMessageType.ListProfiles,
            RequestId = Guid.Empty
        });
        await using MemoryStream stream = new MemoryStream();
        await stream.WriteAsync(BitConverter.GetBytes(payload.Length));
        await stream.WriteAsync(payload);
        stream.Position = 0;

        await Assert.ThrowsAsync<InvalidDataException>(() => ControlEnvelopeSerializer.ReadAsync(stream, CancellationToken.None));
    }

    [Fact]
    public async Task EnvelopeSerializer_RejectsTruncatedPayloads()
    {
        await using MemoryStream stream = new MemoryStream();
        await stream.WriteAsync(BitConverter.GetBytes(32));
        await stream.WriteAsync(new byte[8]);
        stream.Position = 0;

        await Assert.ThrowsAsync<EndOfStreamException>(() => ControlEnvelopeSerializer.ReadAsync(stream, CancellationToken.None));
    }

    [Fact]
    public async Task EnvelopeSerializer_RejectsInvalidLengthPrefixes()
    {
        await using MemoryStream stream = new MemoryStream(BitConverter.GetBytes(ControlProtocol.MaximumMessageLength + 1));

        await Assert.ThrowsAsync<InvalidDataException>(() => ControlEnvelopeSerializer.ReadAsync(stream, CancellationToken.None));
    }

    [Fact]
    public async Task EnvelopeSerializer_RoundTripsProtocolAndFuturePairingContracts()
    {
        ControlEnvelope envelope = new ControlEnvelope
        {
            MessageType = ControlMessageType.GetServiceStatus,
            Hello = new ProtocolHello
            {
                ClientKind = ControlClientKind.RemoteApplication,
                ClientId = "com.displaymagician.future-client",
                DeviceId = "device-123",
                DisplayName = "Future client",
                RequiredCapabilities = new[] { "operation-status" }
            },
            Payload = JsonSerializer.Serialize(new DevicePairingRequest { PairingCode = "123456", DeviceId = "device-123", DeviceDisplayName = "Future client", DevicePublicKey = "public-key-placeholder" })
        };
        await using MemoryStream stream = new MemoryStream();

        await ControlEnvelopeSerializer.WriteAsync(stream, envelope, CancellationToken.None);
        stream.Position = 0;
        ControlEnvelope? reread = await ControlEnvelopeSerializer.ReadAsync(stream, CancellationToken.None);
        DevicePairingRequest? pairingRequest = JsonSerializer.Deserialize<DevicePairingRequest>(reread!.Payload);

        Assert.NotNull(reread);
        Assert.Equal(ControlClientKind.RemoteApplication, reread.Hello.ClientKind);
        Assert.Equal("operation-status", Assert.Single(reread.Hello.RequiredCapabilities));
        Assert.NotNull(pairingRequest);
        Assert.Equal("device-123", pairingRequest!.DeviceId);
    }
}
