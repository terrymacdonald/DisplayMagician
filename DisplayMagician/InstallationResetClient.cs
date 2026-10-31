using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;

namespace DisplayMagician;

internal static class InstallationResetClient
{
    internal static async Task<ControlResponse> ResetAsync()
    {
        string executable = Path.Combine(Application.StartupPath, "ControlService", "DisplayMagician.ControlService.exe");
        if (!File.Exists(executable)) throw new IOException("The installed ControlService recovery executable was not found. Reset is available in an installed DisplayMagician deployment.");
        string pipeName = InstallationResetProtocol.PipePrefix + Guid.NewGuid().ToString("N");
        using Process process = Process.Start(new ProcessStartInfo(executable, InstallationResetProtocol.MaintenanceCommandLineOption + " " + pipeName)
        { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden }) ?? throw new IOException("The reset recovery host could not start.");
        using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(30000, timeout.Token).ConfigureAwait(false);
        ControlEnvelope request = new ControlEnvelope
        {
            MessageType = ControlMessageType.ResetInstallation,
            Hello = ControlProtocol.CreateHello(ControlClientKind.DesktopApplication, "DisplayMagician.Reset"),
            Payload = JsonSerializer.Serialize(new InstallationResetRequest { Confirmed = true })
        };
        request.Hello.RequiredCapabilities = new[] { ControlCapabilities.ProtocolNegotiation, InstallationResetProtocol.Capability };
        await ControlEnvelopeSerializer.WriteAsync(pipe, request, timeout.Token).ConfigureAwait(false);
        ControlEnvelope response = await ControlEnvelopeSerializer.ReadAsync(pipe, timeout.Token).ConfigureAwait(false) ?? throw new IOException("Reset ended without confirmation. Check the ControlService logs before retrying.");
        if (response.RequestId != request.RequestId || response.MessageType != request.MessageType || response.ProtocolVersion != request.ProtocolVersion)
            throw new IOException("The reset response did not match the request.");
        ControlResponse result = JsonSerializer.Deserialize<ControlResponse>(response.Payload) ?? throw new IOException("Invalid reset response.");
        if (result.IsSuccessful && (!ControlProtocol.IsCompatibleWelcome(request.Hello, result.ProtocolWelcome)
            || result.ProtocolWelcome.EndpointKind != InstallationResetProtocol.EndpointKind))
            throw new IOException("The recovery host did not negotiate a compatible reset protocol.");
        return result;
    }
}
