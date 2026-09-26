using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32.SafeHandles;
using NLog;

namespace DisplayMagician.SessionLauncher;

public sealed class SessionLauncherWorker : BackgroundService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly SessionLauncherPipeServer _pipeServer;

    public SessionLauncherWorker(SessionLauncherPipeServer pipeServer)
    {
        _pipeServer = pipeServer ?? throw new ArgumentNullException(nameof(pipeServer));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Info("SessionLauncherWorker/ExecuteAsync: Session Launcher pipe listener is starting.");
        await _pipeServer.RunAsync(stoppingToken).ConfigureAwait(false);
        _logger.Info("SessionLauncherWorker/ExecuteAsync: Session Launcher pipe listener has stopped.");
    }
}

public sealed class SessionLauncherPipeServer
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
    private readonly InteractiveUserProcessLauncher _processLauncher;

    public SessionLauncherPipeServer(InteractiveUserProcessLauncher processLauncher)
    {
        _processLauncher = processLauncher ?? throw new ArgumentNullException(nameof(processLauncher));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using NamedPipeServerStream pipe = CreatePipe();
                await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                await HandleClientAsync(pipe, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (EndOfStreamException ex)
            {
                _logger.Debug(ex, "SessionLauncherPipeServer/RunAsync: The Control Service closed the Session Launcher pipe before completing its request.");
            }
            catch (IOException ex)
            {
                _logger.Debug(ex, "SessionLauncherPipeServer/RunAsync: The Session Launcher pipe was disconnected during a Control Service request.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Warn(ex, "SessionLauncherPipeServer/RunAsync: Rejected an unauthorised Session Launcher pipe caller.");
            }
            catch (TimeoutException ex)
            {
                _logger.Warn(ex, "SessionLauncherPipeServer/RunAsync: The Control Service did not send a complete Session Launcher request before the timeout.");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is JsonException)
            {
                _logger.Warn(ex, "SessionLauncherPipeServer/RunAsync: The Control Service sent an invalid Session Launcher request.");
            }
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
        return NamedPipeServerStreamAcl.Create(ControlProtocol.SessionLauncherPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        ControlEnvelope? request = await ReadEnvelopeWithTimeoutAsync(pipe, cancellationToken).ConfigureAwait(false);
        UserAgentLaunchResult result;
        if (request == null || request.ProtocolVersion != ControlProtocol.CurrentVersion || request.MessageType is not (ControlMessageType.LaunchUserAgent or ControlMessageType.StopUserAgent) || !IsControlService(pipe))
        {
            result = new UserAgentLaunchResult { IsSuccessful = false, Message = "The Session Launcher rejected the caller or request." };
        }
        else
        {
            using IDisposable requestScope = SupportLogScope.BeginRequest(request.RequestId);
            if (request.MessageType == ControlMessageType.LaunchUserAgent)
            {
                UserAgentLaunchRequest? launchRequest = JsonSerializer.Deserialize<UserAgentLaunchRequest>(request.Payload);
                using IDisposable? operationScope = launchRequest?.OperationId is Guid operationId && operationId != Guid.Empty ? SupportLogScope.BeginOperation(operationId) : null;
                result = launchRequest == null ? new UserAgentLaunchResult { IsSuccessful = false, Message = "The User Agent launch request was invalid." } : _processLauncher.Launch(launchRequest);
            }
            else
            {
                UserAgentStopRequest? stopRequest = JsonSerializer.Deserialize<UserAgentStopRequest>(request.Payload);
                result = stopRequest == null ? new UserAgentLaunchResult { IsSuccessful = false, Message = "The User Agent stop request was invalid." } : _processLauncher.Stop(stopRequest);
            }
        }

        Guid requestId = request?.RequestId ?? Guid.NewGuid();
        await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope { MessageType = request?.MessageType ?? ControlMessageType.Unknown, RequestId = requestId, Payload = JsonSerializer.Serialize(result) }, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<ControlEnvelope?> ReadEnvelopeWithTimeoutAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(RequestTimeout);
        try
        {
            return await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The Control Service did not send a complete Session Launcher request in time.", ex);
        }
    }

    private static bool IsControlService(NamedPipeServerStream pipe)
    {
        string? clientSid = null;
        pipe.RunAsClient(() => { using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query); clientSid = identity.User?.Value; });
        if (!string.Equals(clientSid, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null).Value, StringComparison.OrdinalIgnoreCase) || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            return false;
        }

        try
        {
            using Process process = Process.GetProcessById(checked((int)processId));
            string expectedPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "ControlService", "DisplayMagician.ControlService.exe"));
            return string.Equals(process.MainModule?.FileName, expectedPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is UnauthorizedAccessException)
        {
            _logger.Warn(ex, "SessionLauncherPipeServer/IsControlService: Could not verify the Control Service executable for pipe process {0}.", processId);
            return false;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint clientProcessId);
}

public sealed class InteractiveUserProcessLauncher
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public UserAgentLaunchResult Launch(UserAgentLaunchRequest request)
    {
        Program.ApplyDiagnosticLogLevel(request.DiagnosticLogLevel);
        if (string.IsNullOrWhiteSpace(request.UserSid) || request.SessionId < 0 || WTSGetActiveConsoleSessionId() != (uint)request.SessionId)
        {
            return new UserAgentLaunchResult { IsSuccessful = false, Message = "The requested User Agent session is not the active physical console session." };
        }

        string executablePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "UserAgent", "DisplayMagician.UserAgent.exe"));
        if (!File.Exists(executablePath))
        {
            _logger.Error("InteractiveUserProcessLauncher/Launch: The fixed User Agent executable was not found at {0}.", executablePath);
            return new UserAgentLaunchResult { IsSuccessful = false, Message = "The installed User Agent executable is unavailable." };
        }

        IntPtr userToken = IntPtr.Zero;
        IntPtr primaryToken = IntPtr.Zero;
        IntPtr environment = IntPtr.Zero;
        try
        {
            if (!WTSQueryUserToken((uint)request.SessionId, out userToken) || !DuplicateTokenEx(userToken, 0x02000000, IntPtr.Zero, 2, 1, out primaryToken))
            {
                return new UserAgentLaunchResult { IsSuccessful = false, Message = "Windows could not obtain an interactive token for the requested session." };
            }

            using WindowsIdentity identity = new WindowsIdentity(primaryToken);
            if (!string.Equals(identity.User?.Value, request.UserSid, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warn("InteractiveUserProcessLauncher/Launch: Rejected SID {0} for session {1} because it did not match the interactive token.", request.UserSid, request.SessionId);
                return new UserAgentLaunchResult { IsSuccessful = false, Message = "The requested user does not own the active console session." };
            }

            CreateEnvironmentBlock(out environment, primaryToken, false);
            STARTUPINFO startupInfo = new STARTUPINFO { cb = Marshal.SizeOf<STARTUPINFO>(), lpDesktop = "winsta0\\default" };
            string diagnosticArgument = string.Equals(request.DiagnosticLogLevel, "Trace", StringComparison.OrdinalIgnoreCase) || string.Equals(request.DiagnosticLogLevel, "Debug", StringComparison.OrdinalIgnoreCase)
                ? $" --diagnostic-log-level {request.DiagnosticLogLevel}" : string.Empty;
            if (!CreateProcessAsUser(primaryToken, executablePath, $"\"{executablePath}\"{diagnosticArgument}", IntPtr.Zero, IntPtr.Zero, false, 0x00000400, environment, Path.GetDirectoryName(executablePath), ref startupInfo, out PROCESS_INFORMATION processInformation))
            {
                return new UserAgentLaunchResult { IsSuccessful = false, Message = "Windows could not start the User Agent in the active console session." };
            }

            CloseHandle(processInformation.hThread);
            CloseHandle(processInformation.hProcess);
            _logger.Info("InteractiveUserProcessLauncher/Launch: Started User Agent for SID {0}, session {1}.", request.UserSid, request.SessionId);
            return new UserAgentLaunchResult { IsSuccessful = true, Message = "The User Agent launch was requested." };
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.ComponentModel.Win32Exception || ex is InvalidOperationException)
        {
            _logger.Error(ex, "InteractiveUserProcessLauncher/Launch: Could not start User Agent for SID {0}, session {1}.", request.UserSid, request.SessionId);
            return new UserAgentLaunchResult { IsSuccessful = false, Message = "The User Agent could not be started in the active console session." };
        }
        finally
        {
            if (environment != IntPtr.Zero) DestroyEnvironmentBlock(environment);
            if (primaryToken != IntPtr.Zero) CloseHandle(primaryToken);
            if (userToken != IntPtr.Zero) CloseHandle(userToken);
        }
    }

    public UserAgentLaunchResult Stop(UserAgentStopRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserSid) || request.SessionId < 0 || request.ProcessId <= 0 || WTSGetActiveConsoleSessionId() != (uint)request.SessionId)
        {
            return new UserAgentLaunchResult { IsSuccessful = false, Message = "The requested User Agent process is not in the active physical-console session." };
        }

        try
        {
            using Process process = Process.GetProcessById(request.ProcessId);
            string executablePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "UserAgent", "DisplayMagician.UserAgent.exe"));
            if (process.SessionId != request.SessionId || !string.Equals(process.MainModule?.FileName, executablePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warn("InteractiveUserProcessLauncher/Stop: Rejected process {0} because it was not the verified User Agent for session {1}.", request.ProcessId, request.SessionId);
                return new UserAgentLaunchResult { IsSuccessful = false, Message = "The requested process is not the verified User Agent for the active session." };
            }

            process.Kill(true);
            if (!process.WaitForExit(10000))
            {
                return new UserAgentLaunchResult { IsSuccessful = false, Message = "The verified User Agent did not stop in time." };
            }

            _logger.Info("InteractiveUserProcessLauncher/Stop: Stopped User Agent process {0} for SID {1}, session {2}.", request.ProcessId, request.UserSid, request.SessionId);
            return new UserAgentLaunchResult { IsSuccessful = true, Message = "The User Agent was stopped." };
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
        {
            _logger.Error(ex, "InteractiveUserProcessLauncher/Stop: Could not stop verified User Agent process {0}.", request.ProcessId);
            return new UserAgentLaunchResult { IsSuccessful = false, Message = "Windows could not stop the User Agent process." };
        }
    }

    [DllImport("kernel32.dll")] private static extern uint WTSGetActiveConsoleSessionId();
    [DllImport("wtsapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool WTSQueryUserToken(uint sessionId, out IntPtr token);
    [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DuplicateTokenEx(IntPtr existingToken, uint access, IntPtr attributes, int impersonationLevel, int tokenType, out IntPtr newToken);
    [DllImport("userenv.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CreateEnvironmentBlock(out IntPtr environment, IntPtr token, bool inherit);
    [DllImport("userenv.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyEnvironmentBlock(IntPtr environment);
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CreateProcessAsUser(IntPtr token, string applicationName, string commandLine, IntPtr processAttributes, IntPtr threadAttributes, bool inheritHandles, uint creationFlags, IntPtr environment, string? currentDirectory, ref STARTUPINFO startupInfo, out PROCESS_INFORMATION processInformation);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CloseHandle(IntPtr handle);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct STARTUPINFO { public int cb; public string? lpReserved; public string? lpDesktop; public string? lpTitle; public int dwX; public int dwY; public int dwXSize; public int dwYSize; public int dwXCountChars; public int dwYCountChars; public int dwFillAttribute; public int dwFlags; public short wShowWindow; public short cbReserved2; public IntPtr lpReserved2; public IntPtr hStdInput; public IntPtr hStdOutput; public IntPtr hStdError; }
    [StructLayout(LayoutKind.Sequential)] private struct PROCESS_INFORMATION { public IntPtr hProcess; public IntPtr hThread; public int dwProcessId; public int dwThreadId; }
}
