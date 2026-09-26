using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

public interface ISessionLauncherClient
{
    Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, string? diagnosticLogLevel, CancellationToken cancellationToken);
    Task<UserAgentLaunchResult> StopUserAgentAsync(string userSid, int sessionId, int processId, Guid requestId, CancellationToken cancellationToken);
}

public sealed class SessionLauncherClient : ISessionLauncherClient
{
    private const string SessionLauncherServiceName = "DisplayMagicianSessionLauncher";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public async Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, string? diagnosticLogLevel, CancellationToken cancellationToken)
    {
        await EnsureSessionLauncherRunningAsync(cancellationToken).ConfigureAwait(false);
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.SessionLauncherPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        ControlEnvelope request = new ControlEnvelope
        {
            MessageType = ControlMessageType.LaunchUserAgent,
            RequestId = requestId,
            Payload = JsonSerializer.Serialize(new UserAgentLaunchRequest { UserSid = userSid, SessionId = sessionId, OperationId = operationId, DiagnosticLogLevel = diagnosticLogLevel })
        };
        ControlEnvelope response = await SendAndReceiveAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        if (response.MessageType != request.MessageType)
        {
            throw new InvalidDataException("The Session Launcher returned an invalid response.");
        }

        return JsonSerializer.Deserialize<UserAgentLaunchResult>(response.Payload)
            ?? throw new InvalidDataException("The Session Launcher returned an unreadable response.");
    }

    private static async Task EnsureSessionLauncherRunningAsync(CancellationToken cancellationToken)
    {
        try
        {
            using ServiceController service = new ServiceController(SessionLauncherServiceName);
            service.Refresh();
            if (service.Status == ServiceControllerStatus.Running)
            {
                return;
            }

            if (service.Status == ServiceControllerStatus.Stopped)
            {
                Logger.Info("SessionLauncherClient/EnsureSessionLauncherRunningAsync: Starting the demand-start Session Launcher service.");
                service.Start();
            }

            await Task.Run(() => service.WaitForStatus(ServiceControllerStatus.Running, ControlProtocol.ConnectionTimeout), cancellationToken).ConfigureAwait(false);
            Logger.Info("SessionLauncherClient/EnsureSessionLauncherRunningAsync: Session Launcher service is running.");
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is System.ServiceProcess.TimeoutException)
        {
            Logger.Error(ex, "SessionLauncherClient/EnsureSessionLauncherRunningAsync: Could not start the Session Launcher service.");
            throw new SessionLauncherUnavailableException("The DisplayMagician Session Launcher service could not be started. Please check that the service is installed and can run.", ex);
        }
    }

    public async Task<UserAgentLaunchResult> StopUserAgentAsync(string userSid, int sessionId, int processId, Guid requestId, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.SessionLauncherPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        ControlEnvelope request = new ControlEnvelope { MessageType = ControlMessageType.StopUserAgent, RequestId = requestId, Payload = JsonSerializer.Serialize(new UserAgentStopRequest { UserSid = userSid, SessionId = sessionId, ProcessId = processId }) };
        ControlEnvelope response = await SendAndReceiveAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        if (response.MessageType != request.MessageType)
        {
            throw new InvalidDataException("The Session Launcher returned an invalid response.");
        }

        return JsonSerializer.Deserialize<UserAgentLaunchResult>(response.Payload)
            ?? throw new InvalidDataException("The Session Launcher returned an unreadable response.");
    }

    private static async Task<ControlEnvelope> SendAndReceiveAsync(NamedPipeClientStream pipe, ControlEnvelope request, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(ControlProtocol.ResponseTimeout);
        try
        {
            await ControlEnvelopeSerializer.WriteAsync(pipe, request, timeoutSource.Token).ConfigureAwait(false);
            ControlEnvelope? response = await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutSource.Token).ConfigureAwait(false);
            if (response == null || response.RequestId != request.RequestId)
            {
                throw new InvalidDataException("The Session Launcher returned an invalid response.");
            }

            return response;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new System.TimeoutException("The Session Launcher did not respond within the permitted time.");
        }
    }
}

public sealed class SessionLauncherUnavailableException : InvalidOperationException
{
    public SessionLauncherUnavailableException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
