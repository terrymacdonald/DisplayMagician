using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using DisplayMagician.Contracts;
using Microsoft.Win32;

namespace DisplayMagician.UserAgent;

/// <summary>Exports a comprehensive, redacted support ZIP from the current user's authoritative Agent storage.</summary>
public sealed class UserSupportBundleGenerator
{
    private readonly string _userDataPath;
    private readonly AgentRegistration _registration;

    public UserSupportBundleGenerator(string userDataPath, AgentRegistration registration)
    {
        _userDataPath = string.IsNullOrWhiteSpace(userDataPath) ? throw new ArgumentException("A user data path is required.", nameof(userDataPath)) : userDataPath;
        _registration = registration ?? throw new ArgumentNullException(nameof(registration));
    }

    public UserSupportBundleResult Create(string destinationPath, string machineLogsStagingPath, string machineConfigurationStagingPath = "", string[]? machineCollectionWarnings = null)
    {
        if (string.IsNullOrWhiteSpace(destinationPath) || !Path.IsPathFullyQualified(destinationPath) || !string.Equals(Path.GetExtension(destinationPath), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The support ZIP destination must be an absolute .zip path.", nameof(destinationPath));
        }

        string fullDestinationPath = Path.GetFullPath(destinationPath);
        string? destinationDirectory = Path.GetDirectoryName(fullDestinationPath);
        if (string.IsNullOrWhiteSpace(destinationDirectory) || !Directory.Exists(destinationDirectory))
        {
            throw new DirectoryNotFoundException("The selected support ZIP folder does not exist.");
        }

        List<string> warnings = new List<string>();
        if (machineCollectionWarnings != null)
        {
            foreach (string warning in machineCollectionWarnings)
            {
                if (!string.IsNullOrWhiteSpace(warning))
                {
                    warnings.Add(warning);
                }
            }
        }

        List<SupportBundleEntry> includedEntries = new List<SupportBundleEntry>();
        using FileStream output = new FileStream(fullDestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using ZipArchive archive = new ZipArchive(output, ZipArchiveMode.Create);
        AddDirectory(archive, "Configuration/Profiles", Path.Combine(_userDataPath, "Profiles"), null, warnings, includedEntries);
        AddDirectory(archive, "Configuration/AudioProfiles", Path.Combine(_userDataPath, "AudioProfiles"), null, warnings, includedEntries);
        AddDirectory(archive, "Configuration/Shortcuts", Path.Combine(_userDataPath, "Shortcuts"), null, warnings, includedEntries);
        AddDirectory(archive, "Configuration/Settings", Path.Combine(_userDataPath, "Settings"), null, warnings, includedEntries);
        AddDirectory(archive, "Logs", Path.Combine(_userDataPath, "Logs"), null, warnings, includedEntries);
        string legacyLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician", "Logs");
        if (Directory.Exists(legacyLogPath))
        {
            AddDirectory(archive, "Logs/Legacy", legacyLogPath, null, warnings, includedEntries);
        }

        AddMachineLogs(archive, machineLogsStagingPath, warnings, includedEntries);
        AddMachineConfiguration(archive, machineConfigurationStagingPath, warnings, includedEntries);
        AddDirectory(archive, "Configuration/LegacyFiles", Path.Combine(_userDataPath, "LegacyFiles"), null, warnings, includedEntries);
        AddFile(archive, Path.Combine(_userDataPath, "Migration.json"), "Configuration/Migration.json", null, warnings, includedEntries);
        AddManifest(archive, warnings, includedEntries);

        return new UserSupportBundleResult { DestinationPath = fullDestinationPath, Warnings = warnings.ToArray() };
    }

    private void AddMachineLogs(ZipArchive archive, string machineLogsStagingPath, List<string> warnings, List<SupportBundleEntry> includedEntries)
    {
        string stagingRoot = Path.GetFullPath(Path.Combine(_userDataPath, "Backups", "SupportStaging")) + Path.DirectorySeparatorChar;
        string fullMachineLogsPath = string.IsNullOrWhiteSpace(machineLogsStagingPath) ? string.Empty : Path.GetFullPath(machineLogsStagingPath);
        if (!fullMachineLogsPath.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("Control Service logs were not available.");
            return;
        }

        AddDirectory(archive, "MachineLogs", fullMachineLogsPath, null, warnings, includedEntries);
    }

    private void AddMachineConfiguration(ZipArchive archive, string machineConfigurationStagingPath, List<string> warnings, List<SupportBundleEntry> includedEntries)
    {
        string stagingRoot = Path.GetFullPath(Path.Combine(_userDataPath, "Backups", "SupportStaging")) + Path.DirectorySeparatorChar;
        string fullMachineConfigurationPath = string.IsNullOrWhiteSpace(machineConfigurationStagingPath) ? string.Empty : Path.GetFullPath(machineConfigurationStagingPath);
        if (!fullMachineConfigurationPath.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("Control Service configuration was not available.");
            return;
        }

        AddDirectory(archive, "Configuration/Machine", fullMachineConfigurationPath, null, warnings, includedEntries);
    }

    private void AddManifest(ZipArchive archive, List<string> warnings, List<SupportBundleEntry> includedEntries)
    {
        ZipArchiveEntry entry = archive.CreateEntry("support-manifest.json", CompressionLevel.Optimal);
        using StreamWriter writer = new StreamWriter(entry.Open());
        writer.Write(JsonSerializer.Serialize(new
        {
            SchemaVersion = 1,
            LogContractVersion = 1,
            ConfigurationSchemaVersion = 1,
            CreatedUtc = DateTime.UtcNow,
            AgentVersion = _registration.Version,
            ComponentVersions = GetComponentVersions(),
            UserSid = _registration.UserSid,
            IncludedEntries = includedEntries,
            Warnings = warnings
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private Dictionary<string, string> GetComponentVersions()
    {
        Dictionary<string, string> componentVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["UserAgent"] = _registration.Version
        };

        try
        {
            using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(@"Software\DisplayMagician", false);
            string? installDirectory = registryKey?.GetValue("InstalledDir") as string;
            string? installerVersion = registryKey?.GetValue("InstalledVersion") as string;
            if (!string.IsNullOrWhiteSpace(installerVersion))
            {
                componentVersions["Installer"] = installerVersion;
            }

            if (string.IsNullOrWhiteSpace(installDirectory) || !Directory.Exists(installDirectory))
            {
                return componentVersions;
            }

            AddComponentVersion(componentVersions, "DesktopApp", Path.Combine(installDirectory, "DisplayMagician.exe"));
            AddComponentVersion(componentVersions, "DesktopConsole", Path.Combine(installDirectory, "DisplayMagicianConsole.exe"));
            AddComponentVersion(componentVersions, "ControlService", Path.Combine(installDirectory, "ControlService", "DisplayMagician.ControlService.exe"));
            AddComponentVersion(componentVersions, "SessionLauncher", Path.Combine(installDirectory, "SessionLauncher", "DisplayMagician.SessionLauncher.exe"));
            AddComponentVersion(componentVersions, "UserAgent", Path.Combine(installDirectory, "UserAgent", "DisplayMagician.UserAgent.exe"));
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException || ex is System.ComponentModel.Win32Exception || ex is ArgumentException || ex is NotSupportedException)
        {
            // The support bundle remains useful when installation metadata is unavailable.
        }

        return componentVersions;
    }

    private static void AddComponentVersion(Dictionary<string, string> componentVersions, string componentName, string executablePath)
    {
        if (!File.Exists(executablePath))
        {
            return;
        }

        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(executablePath);
        string? version = string.IsNullOrWhiteSpace(versionInfo.ProductVersion) ? versionInfo.FileVersion : versionInfo.ProductVersion;
        if (!string.IsNullOrWhiteSpace(version))
        {
            componentVersions[componentName] = version;
        }
    }

    private static void AddDirectory(ZipArchive archive, string entryRoot, string directoryPath, long? maximumBytesPerFile, List<string> warnings, List<SupportBundleEntry> includedEntries)
    {
        if (!Directory.Exists(directoryPath))
        {
            warnings.Add($"{entryRoot} was not available.");
            return;
        }

        foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(directoryPath, filePath);
            AddFile(archive, filePath, Path.Combine(entryRoot, relativePath), maximumBytesPerFile, warnings, includedEntries);
        }
    }

    private static void AddFile(ZipArchive archive, string sourcePath, string entryName, long? maximumBytes, List<string> warnings, List<SupportBundleEntry> includedEntries)
    {
        if (!File.Exists(sourcePath))
        {
            warnings.Add($"{entryName} was not available.");
            return;
        }

        try
        {
            using FileStream input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using Stream output = entry.Open();
            long remainingBytes = maximumBytes ?? input.Length;
            byte[] buffer = new byte[81920];
            while (remainingBytes > 0)
            {
                int read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, remainingBytes));
                if (read == 0)
                {
                    break;
                }

                output.Write(buffer, 0, read);
                remainingBytes -= read;
            }

            if (maximumBytes.HasValue && input.Length > maximumBytes.Value)
            {
                warnings.Add($"{entryName} was limited to {maximumBytes.Value} bytes.");
            }

            includedEntries.Add(new SupportBundleEntry { Path = entryName.Replace('\\', '/'), Length = input.Length });
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            warnings.Add($"{entryName} could not be added: {ex.Message}");
        }
    }

    private sealed class SupportBundleEntry
    {
        public string Path { get; set; } = string.Empty;

        public long Length { get; set; }
    }
}
