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
            catch (Exception ex)
            {
                _logger.Error(ex, "SessionLauncherPipeServer/RunAsync: Unable to serve a Session Launcher request.");
            }
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
        return NamedPipeServerStreamAcl.Create(ControlProtocol.SessionLauncherPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        ControlEnvelope? request = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
        UserAgentLaunchResult result;
        if (request == null || request.ProtocolVersion != ControlProtocol.CurrentVersion || request.MessageType != ControlMessageType.LaunchUserAgent || !IsControlService(pipe))
        {
            result = new UserAgentLaunchResult { IsSuccessful = false, Message = "The Session Launcher rejected the caller or request." };
        }
        else
        {
            UserAgentLaunchRequest? launchRequest = JsonSerializer.Deserialize<UserAgentLaunchRequest>(request.Payload);
            result = launchRequest == null ? new UserAgentLaunchResult { IsSuccessful = false, Message = "The User Agent launch request was invalid." } : _processLauncher.Launch(launchRequest);
        }

        Guid requestId = request?.RequestId ?? Guid.NewGuid();
        await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope { MessageType = ControlMessageType.LaunchUserAgent, RequestId = requestId, Payload = JsonSerializer.Serialize(result) }, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsControlService(NamedPipeServerStream pipe)
    {
        string? clientSid = null;
        pipe.RunAsClient(() => { using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query); clientSid = identity.User?.Value; });
        if (!string.Equals(clientSid, new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null).Value, StringComparison.OrdinalIgnoreCase) || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            return false;
        }

        try
        {
            using Process process = Process.GetProcessById(checked((int)processId));
            string expectedPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "DisplayMagician.ControlService.exe"));
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
        if (string.IsNullOrWhiteSpace(request.UserSid) || request.SessionId < 0 || WTSGetActiveConsoleSessionId() != (uint)request.SessionId)
        {
            return new UserAgentLaunchResult { IsSuccessful = false, Message = "The requested User Agent session is not the active physical console session." };
        }

        string executablePath = Path.Combine(AppContext.BaseDirectory, "DisplayMagician.UserAgent.exe");
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
            if (!CreateProcessAsUser(primaryToken, executablePath, $"\"{executablePath}\"", IntPtr.Zero, IntPtr.Zero, false, 0x00000400, environment, Path.GetDirectoryName(executablePath), ref startupInfo, out PROCESS_INFORMATION processInformation))
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
