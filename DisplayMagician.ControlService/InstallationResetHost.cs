using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Win32;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Administrator-only recovery endpoint, started before normal stores are constructed.</summary>
internal static class InstallationResetHost
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string ControlServiceName = "DisplayMagicianControlService";
    private const string GatewayName = "DisplayMagicianGateway";
    private const string LauncherName = "DisplayMagicianSessionLauncher";

    internal static async Task RunAsync(string pipeName)
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        if (!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)) throw new UnauthorizedAccessException("Reset requires administrator approval.");
        if (!pipeName.StartsWith(InstallationResetProtocol.PipePrefix, StringComparison.Ordinal)
            || !Guid.TryParseExact(pipeName.Substring(InstallationResetProtocol.PipePrefix.Length), "N", out _))
            throw new ArgumentException("Invalid reset endpoint.");
        using Semaphore mutex = new(1, 1, InstallationResetProtocol.MaintenanceLockName);
        bool acquired = mutex.WaitOne(0);
        if (!acquired) throw new IOException("Another installation reset is already running.");
        try
        {
            PipeSecurity security = new();
            security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
            security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
            using NamedPipeServerStream pipe = NamedPipeServerStreamAcl.Create(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security);
            using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(5));
            await pipe.WaitForConnectionAsync(timeout.Token).ConfigureAwait(false);
            bool administrator = false;
            string? localLogonSid = identity.Groups?.OfType<SecurityIdentifier>().FirstOrDefault(group => group.Value.StartsWith("S-1-5-5-", StringComparison.Ordinal))?.Value;
            pipe.RunAsClient(() =>
            {
                using WindowsIdentity caller = WindowsIdentity.GetCurrent();
                // The UI and its maintenance child inherit the same local logon token.
                // A separate/network logon must use neither this endpoint nor administrator group membership alone.
                administrator = new WindowsPrincipal(caller).IsInRole(WindowsBuiltInRole.Administrator)
                    && caller.User == identity.User && localLogonSid != null
                    && caller.Groups?.OfType<SecurityIdentifier>().Any(group => group.Value == localLogonSid) == true;
            });
            if (!GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint clientPid)) throw new IOException("The reset caller could not be identified.");
            ControlEnvelope? request = await ControlEnvelopeSerializer.ReadAsync(pipe, timeout.Token).ConfigureAwait(false);
            if (request == null) return;
            ControlResponse response = ValidateRequest(request, administrator);
            if (response.IsSuccessful)
            {
                using IDisposable scope = SupportLogScope.BeginRequest(request.RequestId);
                try
                {
                    await ResetAsync((int)clientPid, request.RequestId, timeout.Token).ConfigureAwait(false);
                    response.Message = "DisplayMagician was reset for all Windows users. Logs were retained. Close this window and start DisplayMagician again.";
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "InstallationResetHost/RunAsync: Installation reset could not complete. Resolve the reported problem and retry reset.");
                    response.IsSuccessful = false;
                    response.ErrorCode = ControlErrorCode.ExecutionFailed;
                    response.Message = "Reset could not complete: " + ex.Message + " Check the ControlService logs before retrying.";
                }
            }
            await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope { MessageType = request.MessageType, RequestId = request.RequestId, Payload = JsonSerializer.Serialize(response) }, CancellationToken.None).ConfigureAwait(false);
        }
        finally { mutex.Release(); }
    }

    internal static ControlResponse ValidateRequest(ControlEnvelope request, bool administrator)
    {
        if (!administrator) return new ControlResponse { ErrorCode = ControlErrorCode.AdministratorRequired, Message = "Reset requires administrator approval." };
        if (request.ProtocolVersion != ControlProtocol.CurrentVersion)
            return new ControlResponse { ErrorCode = ControlErrorCode.UnsupportedProtocolVersion, Message = "Incompatible reset protocol." };
        if (!ControlProtocol.TryCreateWelcome(request.Hello, InstallationResetProtocol.EndpointKind,
            new[] { ControlCapabilities.ProtocolNegotiation, InstallationResetProtocol.Capability }, out ProtocolWelcome? welcome, out ControlErrorCode error, out string message))
            return new ControlResponse { ErrorCode = error, Message = message };
        try
        {
            if (request.MessageType != ControlMessageType.ResetInstallation || JsonSerializer.Deserialize<InstallationResetRequest>(request.Payload)?.Confirmed != true)
                return new ControlResponse { ErrorCode = ControlErrorCode.InvalidRequest, Message = "A confirmed installation reset is required.", ProtocolWelcome = welcome };
        }
        catch (Exception ex) when (ex is JsonException || ex is ArgumentNullException)
        { return new ControlResponse { ErrorCode = ControlErrorCode.InvalidRequest, Message = "Invalid reset request.", ProtocolWelcome = welcome }; }
        return new ControlResponse { IsSuccessful = true, ProtocolWelcome = welcome };
    }

    private static async Task ResetAsync(int recoveryClientPid, Guid requestId, CancellationToken cancellationToken)
    {
        string installRoot = Directory.GetParent(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))!.FullName;
        Dictionary<string, string> profiles = GetWindowsProfiles();
        using ServiceController service = new(ControlServiceName);
        using ServiceController gateway = new(GatewayName);
        using ServiceController launcher = new(LauncherName);
        bool stopped = false;
        try
        {
            SetServiceState(gateway, false);
            StopDesktopClients(installRoot, recoveryClientPid);
            service.Refresh();
            if (service.Status == ServiceControllerStatus.Running)
            {
                using NamedPipeClientStream control = new(".", ControlProtocol.ClientPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await control.ConnectAsync(5000, cancellationToken).ConfigureAwait(false);
                ControlEnvelope prepare = new() { MessageType = ControlMessageType.PrepareInstallationReset, RequestId = requestId, Hello = ControlProtocol.CreateHello(ControlClientKind.ControlService, "DisplayMagician.Reset") };
                prepare.Hello.RequiredCapabilities = new[] { ControlCapabilities.ProtocolNegotiation, InstallationResetProtocol.Capability };
                await ControlEnvelopeSerializer.WriteAsync(control, prepare, cancellationToken).ConfigureAwait(false);
                ControlEnvelope reply = await ControlEnvelopeSerializer.ReadAsync(control, cancellationToken).ConfigureAwait(false) ?? throw new IOException("ControlService did not confirm reset preparation.");
                ControlResponse result = JsonSerializer.Deserialize<ControlResponse>(reply.Payload) ?? throw new IOException("Invalid reset preparation response.");
                if (reply.RequestId != prepare.RequestId || reply.MessageType != prepare.MessageType || !ControlProtocol.IsCompatibleWelcome(prepare.Hello, result.ProtocolWelcome))
                    throw new IOException("ControlService did not negotiate reset preparation.");
                if (!result.IsSuccessful) throw new IOException(result.Message);
            }
            // No writer may survive deletion. Offline recovery never force-kills a UserAgent that might still restore displays.
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            while (HasUserAgents() && DateTime.UtcNow < deadline) await Task.Delay(200, cancellationToken).ConfigureAwait(false);
            if (HasUserAgents()) throw new IOException("A UserAgent is still running. Finish shortcut/display recovery and close it before resetting.");
            SetServiceState(service, false);
            SetServiceState(launcher, false);
            stopped = true;
            StopDesktopClients(installRoot, recoveryClientPid);
            if (HasUserAgents()) throw new IOException("A UserAgent started during reset preparation. No data was deleted; retry reset.");
            Logger.Warn("InstallationResetHost/ResetAsync: Administrator confirmed full installation reset; deleting configuration/data for all users and retaining logs.");
            InstallationResetStore.Reset(new StoragePaths(), profiles);
            SetServiceState(service, true);
            SetServiceState(gateway, true);
            Logger.Info("InstallationResetHost/ResetAsync: Installation reset completed; ControlService and Gateway restarted.");
        }
        finally
        {
            // A failed reset must not quietly leave the application services disabled.
            try { if (stopped) SetServiceState(service, true); SetServiceState(gateway, true); }
            catch (Exception ex) { Logger.Error(ex, "InstallationResetHost/ResetAsync: Could not restart application services after reset; start them from Windows Services."); }
        }
    }

    private static void SetServiceState(ServiceController service, bool running)
    {
        service.Refresh();
        if (service.Status == ServiceControllerStatus.StopPending)
        {
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
            service.Refresh();
        }
        else if (service.Status == ServiceControllerStatus.StartPending)
        {
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            service.Refresh();
        }
        ServiceControllerStatus desired = running ? ServiceControllerStatus.Running : ServiceControllerStatus.Stopped;
        if (service.Status == desired) return;
        if (running && service.Status == ServiceControllerStatus.Stopped) service.Start();
        else if (!running && service.Status != ServiceControllerStatus.StopPending) service.Stop();
        service.WaitForStatus(desired, TimeSpan.FromSeconds(30));
    }

    private static void StopDesktopClients(string root, int recoveryClientPid)
    {
        foreach (string name in new[] { "DisplayMagician", "DisplayMagicianConsole" })
            foreach (Process process in Process.GetProcessesByName(name))
                using (process)
                {
                    if (process.Id == recoveryClientPid) continue;
                    if (!string.Equals(process.MainModule?.FileName, Path.Combine(root, name + ".exe"), StringComparison.OrdinalIgnoreCase))
                        throw new IOException("Another DisplayMagician installation is running. Close it before resetting shared application data.");
                    process.Kill(); // These clients own no hardware recovery; do not run their stale-settings shutdown saves.
                    if (!process.WaitForExit(10000)) throw new IOException("A DisplayMagician desktop client did not stop.");
                }
    }

    private static bool HasUserAgents()
    {
        foreach (Process process in Process.GetProcessesByName("DisplayMagician.UserAgent"))
            using (process)
                if (!process.HasExited) return true;
        return false;
    }

    private static Dictionary<string, string> GetWindowsProfiles()
    {
        Dictionary<string, string> profiles = new(StringComparer.OrdinalIgnoreCase);
        using RegistryKey list = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList") ?? throw new IOException("Windows profiles could not be enumerated.");
        foreach (string sid in list.GetSubKeyNames())
        {
            if (!sid.StartsWith("S-1-5-21-", StringComparison.Ordinal) && !sid.StartsWith("S-1-12-1-", StringComparison.Ordinal)) continue;
            if (sid.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) continue;
            using RegistryKey? key = list.OpenSubKey(sid);
            if (key?.GetValue("ProfileImagePath") is string path) profiles[sid] = Environment.ExpandEnvironmentVariables(path);
        }
        return profiles;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(Microsoft.Win32.SafeHandles.SafePipeHandle pipe, out uint processId);
}
