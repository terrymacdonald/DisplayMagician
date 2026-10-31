using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class InstallationResetTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ResetTests-" + Guid.NewGuid().ToString("N"));
    private StoragePaths Paths => new(Path.Combine(_root, "Data"));

    [Fact]
    public void FullResetDeletesDataForAllUsersRetainsLogsAndSuppressesLegacyImport()
    {
        Paths.EnsureMachineDirectories();
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        string sid = identity.User!.Value;
        string otherSid = "S-1-5-21-100-200-300-1001";
        string profile = Path.Combine(_root, "WindowsProfile");
        string legacy = Path.Combine(profile, "AppData", "Local", "DisplayMagician");
        foreach (string owner in new[] { sid, otherSid })
        {
            UserStoragePaths user = Paths.GetUserPaths(owner);
            user.EnsureDirectories();
            File.WriteAllText(Path.Combine(user.SettingsPath, "Settings.json"), "corrupt settings");
            File.WriteAllText(Path.Combine(user.ProfilesPath, "DisplayProfiles.json"), "old profiles");
            File.WriteAllText(Path.Combine(user.ShortcutsPath, "Shortcuts.json"), "old shortcuts");
            File.WriteAllText(Path.Combine(user.BackupsPath, "Settings.json.bak"), "old backup");
            File.WriteAllText(Path.Combine(user.LogsPath, "Desktop.log"), "retained user log");
        }
        Directory.CreateDirectory(Path.Combine(legacy, "Logs"));
        File.WriteAllText(Path.Combine(legacy, "Settings.json.old"), "old legacy preferences");
        File.WriteAllText(Path.Combine(legacy, "Logs", "old.log"), "retained legacy log");
        File.WriteAllText(Path.Combine(Paths.MachinePath, "PairedClients.json"), "old credentials");
        File.WriteAllText(Path.Combine(Paths.MachinePath, "GatewayEventJournal.json"), "corrupt history");
        File.WriteAllText(Path.Combine(Paths.MachineLogsPath, "ControlService.log"), "retained machine log");
        File.WriteAllText(Path.Combine(Paths.MachineDiagnosticsPath, "Audit.jsonl"), "retained audit");

        InstallationResetStore.Reset(Paths, new Dictionary<string, string> { [sid] = profile });

        Assert.False(File.Exists(Path.Combine(Paths.MachinePath, "PairedClients.json")));
        Assert.False(File.Exists(Path.Combine(Paths.MachinePath, "GatewayEventJournal.json")));
        Assert.False(File.Exists(Path.Combine(legacy, "Settings.json.old")));
        Assert.Equal("retained legacy log", File.ReadAllText(Path.Combine(legacy, "Logs", "old.log")));
        Assert.Equal("retained machine log", File.ReadAllText(Path.Combine(Paths.MachineLogsPath, "ControlService.log")));
        Assert.Equal("retained audit", File.ReadAllText(Path.Combine(Paths.MachineDiagnosticsPath, "Audit.jsonl")));
        foreach (string owner in new[] { sid, otherSid })
        {
            UserStoragePaths user = Paths.GetUserPaths(owner);
            Assert.Empty(Directory.GetFiles(user.SettingsPath));
            Assert.Empty(Directory.GetFiles(user.ProfilesPath));
            Assert.Empty(Directory.GetFiles(user.ShortcutsPath));
            Assert.Empty(Directory.GetFiles(user.BackupsPath));
            Assert.Equal("retained user log", File.ReadAllText(Path.Combine(user.LogsPath, "Desktop.log")));
            MigrationMarker marker = JsonSerializer.Deserialize<MigrationMarker>(File.ReadAllText(user.MigrationMarkerPath))!;
            Assert.True(marker.WasInstallationReset);
            Assert.Empty(marker.Files);
        }
        ControlServiceSettingsStore defaults = new(Paths);
        Assert.True(defaults.Get().SuppressLegacyUserImport);
        Assert.Equal("Warn", defaults.Get().Logging.SavedLevel);
        Assert.False(defaults.Get().Gateway.AllowPublicNetworks);
        Assert.True(File.Exists(Path.Combine(Paths.MachinePath, "GatewayIdentityReset.json")));
        Assert.Empty(Directory.GetFiles(Paths.RootPath, "*.reset", SearchOption.AllDirectories));
    }

    [Fact]
    public void ResetCannotFollowACacheJunctionIntoUnrelatedFiles()
    {
        Paths.EnsureMachineDirectories();
        string outside = Path.Combine(_root, "Unrelated");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "keep.txt"), "unrelated file");
        string link = Path.Combine(Paths.MachinePath, "Cache");
        using Process command = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{link}\" \"{outside}\"")
        { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true })!;
        command.WaitForExit();
        Assert.Equal(0, command.ExitCode);
        InstallationResetStore.Reset(Paths, new Dictionary<string, string>());
        Assert.Equal("unrelated file", File.ReadAllText(Path.Combine(outside, "keep.txt")));
        Assert.False(Directory.Exists(link));
    }

    [Fact]
    public void ResetPreventsLaterLegacyReimportForANewUser()
    {
        InstallationResetStore.Reset(Paths, new Dictionary<string, string>());
        string legacy = Path.Combine(_root, "LateLegacy");
        Directory.CreateDirectory(legacy);
        File.WriteAllText(Path.Combine(legacy, "Settings.json"), "old configuration");
        UserStoragePaths user = Paths.GetUserPaths("S-1-5-21-100-200-300-1001");
        user.EnsureDirectories();
        UserDataMigrationResult result = new UserDataMigrationRunner(new LegacyFileMigration(), new ControlServiceSettingsStore(Paths)).Migrate(legacy, user);
        Assert.True(result.IsSuccessful);
        Assert.Empty(Directory.GetFiles(user.SettingsPath));
        Assert.True(JsonSerializer.Deserialize<MigrationMarker>(File.ReadAllText(user.MigrationMarkerPath))!.WasInstallationReset);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void ResetRequiresBothAdministratorAndExplicitConfirmation(bool administrator, bool confirmed)
    {
        ControlResponse response = InstallationResetHost.ValidateRequest(Request(confirmed), administrator);
        Assert.False(response.IsSuccessful);
        Assert.Equal(administrator ? ControlErrorCode.InvalidRequest : ControlErrorCode.AdministratorRequired, response.ErrorCode);
    }

    [Fact]
    public void ResetNegotiatesSharedContractsAndRejectsIncompatibleRequests()
    {
        ControlEnvelope request = Request(true);
        ControlResponse response = InstallationResetHost.ValidateRequest(request, true);
        Assert.True(response.IsSuccessful);
        Assert.Equal(InstallationResetProtocol.EndpointKind, response.ProtocolWelcome!.EndpointKind);
        Assert.True(ControlProtocol.IsCompatibleWelcome(request.Hello, response.ProtocolWelcome));
        request.Hello = null!;
        Assert.False(InstallationResetHost.ValidateRequest(request, true).IsSuccessful);
        request = Request(true);
        request.ProtocolVersion++;
        Assert.Equal(ControlErrorCode.UnsupportedProtocolVersion, InstallationResetHost.ValidateRequest(request, true).ErrorCode);
    }

    private static ControlEnvelope Request(bool confirmed)
    {
        ControlEnvelope request = new() { MessageType = ControlMessageType.ResetInstallation, Hello = ControlProtocol.CreateHello(ControlClientKind.DesktopApplication, "test"), Payload = JsonSerializer.Serialize(new InstallationResetRequest { Confirmed = confirmed }) };
        request.Hello.RequiredCapabilities = new[] { ControlCapabilities.ProtocolNegotiation, InstallationResetProtocol.Capability };
        return request;
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
