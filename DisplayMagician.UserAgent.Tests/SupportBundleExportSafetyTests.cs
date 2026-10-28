using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class SupportBundleExportSafetyTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-SupportSafety-{Guid.NewGuid():N}");
    private readonly string _userDataPath;
    private readonly UserSupportBundleGenerator _generator;

    public SupportBundleExportSafetyTests()
    {
        _userDataPath = Path.Combine(_root, "UserData");
        Directory.CreateDirectory(Path.Combine(_userDataPath, "Logs"));
        File.WriteAllText(Path.Combine(_userDataPath, "Logs", "UserAgent.log"), "retained log");
        _generator = new UserSupportBundleGenerator(_userDataPath,
            new AgentRegistration { UserSid = "S-1-5-18", Version = "4-test" }, Path.Combine(_root, "NoLegacyLogs"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedCollection_PreservesDestinationAndRemovesTemporaryArchive(bool hasExistingDestination)
    {
        string destination = Path.Combine(_root, "Support.zip");
        if (hasExistingDestination)
        {
            File.WriteAllText(destination, "previous export");
        }

        Assert.Throws<ArgumentException>(() => _generator.Create(destination, "\0"));

        Assert.Equal(hasExistingDestination, File.Exists(destination));
        if (hasExistingDestination)
        {
            Assert.Equal("previous export", File.ReadAllText(destination));
        }
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
    }

    [Fact]
    public void FailedPublication_PreservesExistingDestinationAndRemovesTemporaryArchive()
    {
        string destination = Path.Combine(_root, "Support.zip");
        File.WriteAllText(destination, "previous export");
        using (FileStream locked = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.ThrowsAny<IOException>(() => _generator.Create(destination, ""));
        }

        Assert.Equal("previous export", File.ReadAllText(destination));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExportInsideSource_ExcludesOldDestinationAndTemporaryArchive(bool hasExistingDestination)
    {
        string destination = Path.Combine(_userDataPath, "Logs", "Support.zip");
        if (hasExistingDestination)
        {
            File.WriteAllText(destination, "previous export");
        }

        UserSupportBundleResult result = _generator.Create(destination, "");

        using ZipArchive archive = ZipFile.OpenRead(destination);
        Assert.NotNull(archive.GetEntry("Logs/UserAgent.log"));
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName.Contains("Support.zip", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Warnings, warning => warning.Contains("Support.zip", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(destination)!, "*.tmp"));
        using StreamReader reader = new StreamReader(archive.GetEntry("support-manifest.json")!.Open());
        using JsonDocument manifest = JsonDocument.Parse(reader.ReadToEnd());
        foreach (JsonElement entry in manifest.RootElement.GetProperty("IncludedEntries").EnumerateArray())
        {
            ZipArchiveEntry actualEntry = archive.GetEntry(entry.GetProperty("Path").GetString()!)!;
            Assert.NotNull(actualEntry);
            Assert.Equal(actualEntry.Length, entry.GetProperty("Length").GetInt64());
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LinkedLogDirectory_DoesNotCollectExternalFilesOrFollowCycles(bool cycle)
    {
        string external = Path.Combine(_root, "Private");
        Directory.CreateDirectory(external);
        File.WriteAllText(Path.Combine(external, "Private.log"), "private contents");
        string link = Path.Combine(_userDataPath, "Logs", "Link");
        CreateJunction(link, cycle ? Path.Combine(_userDataPath, "Logs") : external);
        try
        {
            string destination = Path.Combine(_root, "Support.zip");
            UserSupportBundleResult result = _generator.Create(destination, "");

            using ZipArchive archive = ZipFile.OpenRead(destination);
            Assert.NotNull(archive.GetEntry("Logs/UserAgent.log"));
            Assert.DoesNotContain(archive.Entries, entry => entry.FullName.Contains("Link", StringComparison.Ordinal));
            Assert.Contains(result.Warnings, warning => warning.Contains("filesystem link", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(link);
        }
    }

    [Fact]
    public void UnreadableLogFolder_IsReportedWithoutLosingReadableLogs()
    {
        DirectoryInfo blocked = Directory.CreateDirectory(Path.Combine(_userDataPath, "Logs", "Blocked"));
        File.WriteAllText(Path.Combine(blocked.FullName, "UserAgent-old.log"), "inaccessible log");
        DirectorySecurity restricted = blocked.GetAccessControl();
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        FileSystemAccessRule deny = new FileSystemAccessRule(identity.User!, FileSystemRights.ListDirectory, AccessControlType.Deny);
        restricted.AddAccessRule(deny);
        blocked.SetAccessControl(restricted);
        try
        {
            string destination = Path.Combine(_root, "Support.zip");
            UserSupportBundleResult result = _generator.Create(destination, "");

            using ZipArchive archive = ZipFile.OpenRead(destination);
            Assert.NotNull(archive.GetEntry("Logs/UserAgent.log"));
            Assert.Contains(result.Warnings, warning => warning.Contains("Blocked could not be enumerated", StringComparison.Ordinal));
            using StreamReader reader = new StreamReader(archive.GetEntry("support-manifest.json")!.Open());
            using JsonDocument manifest = JsonDocument.Parse(reader.ReadToEnd());
            Assert.Contains(manifest.RootElement.GetProperty("Warnings").EnumerateArray(),
                warning => warning.GetString()!.Contains("Blocked could not be enumerated", StringComparison.Ordinal));
        }
        finally
        {
            restricted.RemoveAccessRuleSpecific(deny);
            blocked.SetAccessControl(restricted);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LinkedStagingAncestor_DoesNotBypassMachineSourceContainment(bool configuration)
    {
        string external = Path.Combine(_root, "Private");
        string source = Path.Combine(external, "Nested");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "Private.json"), "private contents");
        string backups = Path.Combine(_userDataPath, "Backups");
        Directory.CreateDirectory(backups);
        string link = Path.Combine(backups, "SupportStaging");
        CreateJunction(link, external);
        try
        {
            string destination = Path.Combine(_root, "Support.zip");
            string stagedSource = Path.Combine(link, "Nested");
            UserSupportBundleResult result = _generator.Create(destination,
                configuration ? "" : stagedSource, configuration ? stagedSource : "");

            using ZipArchive archive = ZipFile.OpenRead(destination);
            Assert.DoesNotContain(archive.Entries, entry => entry.FullName.Contains("Private", StringComparison.Ordinal));
            Assert.Contains(result.Warnings, warning => warning.Contains("filesystem link", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(link);
        }
    }

    private static void CreateJunction(string link, string target)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe")
        {
            Arguments = $"/d /c mklink /J \"{link}\" \"{target}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using Process process = Process.Start(startInfo)!;
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        Assert.True(process.WaitForExit(10000), "Junction creation did not finish.");
        Assert.True(process.ExitCode == 0, output + error);
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
    }
}
