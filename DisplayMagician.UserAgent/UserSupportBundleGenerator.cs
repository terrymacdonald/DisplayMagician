using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;
using Microsoft.Win32;

namespace DisplayMagician.UserAgent;

/// <summary>Exports a comprehensive, redacted support ZIP from the current user's authoritative Agent storage.</summary>
public sealed class UserSupportBundleGenerator
{
    private readonly string _userDataPath;
    private readonly AgentRegistration _registration;
    private readonly string _legacyLogPath;

    public UserSupportBundleGenerator(string userDataPath, AgentRegistration registration, string? legacyLogPath = null)
    {
        _userDataPath = string.IsNullOrWhiteSpace(userDataPath) ? throw new ArgumentException("A user data path is required.", nameof(userDataPath)) : userDataPath;
        _registration = registration ?? throw new ArgumentNullException(nameof(registration));
        _legacyLogPath = string.IsNullOrWhiteSpace(legacyLogPath) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician", "Logs") : legacyLogPath;
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

        string temporaryPath = Path.Combine(destinationDirectory, $".{Path.GetFileName(fullDestinationPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            CreateArchive(temporaryPath, fullDestinationPath, machineLogsStagingPath, machineConfigurationStagingPath, warnings);
            // Publish only after the archive's central directory has been written and flushed.
            if (File.Exists(fullDestinationPath))
            {
                File.Replace(temporaryPath, fullDestinationPath, null, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, fullDestinationPath);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return new UserSupportBundleResult { DestinationPath = fullDestinationPath, Warnings = warnings.ToArray() };
    }

    private void CreateArchive(string temporaryPath, string destinationPath, string machineLogsStagingPath, string machineConfigurationStagingPath, List<string> warnings)
    {
        HashSet<string> excludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { temporaryPath, destinationPath };
        List<SupportBundleEntry> includedEntries = new List<SupportBundleEntry>();
        using FileStream output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using (ZipArchive archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddDirectory(archive, "Configuration/Profiles", Path.Combine(_userDataPath, "Profiles"), null, warnings, includedEntries, excludedPaths);
            AddDirectory(archive, "Configuration/AudioProfiles", Path.Combine(_userDataPath, "AudioProfiles"), null, warnings, includedEntries, excludedPaths);
            AddDirectory(archive, "Configuration/Shortcuts", Path.Combine(_userDataPath, "Shortcuts"), null, warnings, includedEntries, excludedPaths);
            AddDirectory(archive, "Configuration/Settings", Path.Combine(_userDataPath, "Settings"), null, warnings, includedEntries, excludedPaths);
            AddDirectory(archive, "Configuration/Backups", Path.Combine(_userDataPath, "Backups"), null, warnings, includedEntries, excludedPaths, IsConfigurationBackup);
            AddDirectory(archive, "Logs", Path.Combine(_userDataPath, "Logs"), null, warnings, includedEntries, excludedPaths);
            if (Directory.Exists(_legacyLogPath))
            {
                AddDirectory(archive, "Logs/Legacy", _legacyLogPath, null, warnings, includedEntries, excludedPaths, IsRetainedLog);
            }
            AddDirectory(archive, "Logs/Backups", Path.Combine(_userDataPath, "Backups"), null, warnings, includedEntries, excludedPaths, IsRetainedLog);

            AddMachineLogs(archive, machineLogsStagingPath, warnings, includedEntries, excludedPaths);
            AddMachineConfiguration(archive, machineConfigurationStagingPath, warnings, includedEntries, excludedPaths);
            AddDirectory(archive, "Configuration/LegacyFiles", Path.Combine(_userDataPath, "LegacyFiles"), null, warnings, includedEntries, excludedPaths, IsKnownLegacyConfiguration);
            AddFile(archive, Path.Combine(_userDataPath, "Migration.json"), "Configuration/Migration.json", null, warnings, includedEntries);
            AddManifest(archive, warnings, includedEntries);
        }
        output.Flush(flushToDisk: true);
    }

    private void AddMachineLogs(ZipArchive archive, string machineLogsStagingPath, List<string> warnings, List<SupportBundleEntry> includedEntries, ISet<string> excludedPaths)
    {
        string stagingRoot = Path.GetFullPath(Path.Combine(_userDataPath, "Backups", "SupportStaging")) + Path.DirectorySeparatorChar;
        string fullMachineLogsPath = string.IsNullOrWhiteSpace(machineLogsStagingPath) ? string.Empty : Path.GetFullPath(machineLogsStagingPath);
        if (!fullMachineLogsPath.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("Control Service logs were not available.");
            return;
        }

        AddDirectory(archive, "MachineLogs", fullMachineLogsPath, null, warnings, includedEntries, excludedPaths);
    }

    private void AddMachineConfiguration(ZipArchive archive, string machineConfigurationStagingPath, List<string> warnings, List<SupportBundleEntry> includedEntries, ISet<string> excludedPaths)
    {
        string stagingRoot = Path.GetFullPath(Path.Combine(_userDataPath, "Backups", "SupportStaging")) + Path.DirectorySeparatorChar;
        string fullMachineConfigurationPath = string.IsNullOrWhiteSpace(machineConfigurationStagingPath) ? string.Empty : Path.GetFullPath(machineConfigurationStagingPath);
        if (!fullMachineConfigurationPath.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("Control Service configuration was not available.");
            return;
        }

        AddDirectory(archive, "Configuration/Machine", fullMachineConfigurationPath, null, warnings, includedEntries, excludedPaths);
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

    private static void AddDirectory(ZipArchive archive, string entryRoot, string directoryPath, long? maximumBytesPerFile, List<string> warnings, List<SupportBundleEntry> includedEntries, ISet<string> excludedPaths, Func<string, bool>? includeFile = null)
    {
        if (!Directory.Exists(directoryPath))
        {
            warnings.Add($"{entryRoot} was not available.");
            return;
        }

        try
        {
            if (UsesFilesystemLink(directoryPath))
            {
                warnings.Add($"{entryRoot} was skipped because its source uses a filesystem link.");
                return;
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            warnings.Add($"{entryRoot} could not be inspected: {ex.Message}");
            return;
        }

        Stack<string> pending = new Stack<string>();
        pending.Push(directoryPath);
        while (pending.Count > 0)
        {
            string currentDirectory = pending.Pop();
            string[] children;
            try
            {
                if (UsesFilesystemLink(currentDirectory))
                {
                    warnings.Add($"{entryRoot}/{Path.GetRelativePath(directoryPath, currentDirectory)} was skipped because its source uses a filesystem link.");
                    continue;
                }
                children = Directory.GetFileSystemEntries(currentDirectory);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                warnings.Add($"{entryRoot}/{Path.GetRelativePath(directoryPath, currentDirectory)} could not be enumerated: {ex.Message}");
                continue;
            }

            foreach (string sourcePath in children)
            {
                string relativePath = Path.GetRelativePath(directoryPath, sourcePath);
                if (excludedPaths.Contains(Path.GetFullPath(sourcePath))
                    || string.Equals(relativePath, "SupportStaging", StringComparison.OrdinalIgnoreCase)
                    || relativePath.StartsWith($"SupportStaging{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                FileAttributes attributes;
                try
                {
                    attributes = File.GetAttributes(sourcePath);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    warnings.Add($"{entryRoot}/{relativePath} could not be inspected: {ex.Message}");
                    continue;
                }

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    warnings.Add($"{entryRoot}/{relativePath} was skipped because it is a filesystem link.");
                }
                else if ((attributes & FileAttributes.Directory) != 0)
                {
                    pending.Push(sourcePath);
                }
                else if (includeFile == null || includeFile(sourcePath))
                {
                    AddFile(archive, sourcePath, Path.Combine(entryRoot, relativePath), maximumBytesPerFile, warnings, includedEntries);
                }
            }
        }
    }

    // Lexical containment alone does not protect a source beneath a junction or symbolic link.
    private static bool UsesFilesystemLink(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            return true;
        }

        for (DirectoryInfo? ancestor = Directory.GetParent(path); ancestor != null; ancestor = ancestor.Parent)
        {
            if ((ancestor.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsConfigurationBackup(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        if (!fileName.Contains(".pre-v4.bak", StringComparison.OrdinalIgnoreCase)
            && !fileName.Contains(".replace.bak", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return new[] { "DisplayProfiles.json.", "AudioProfiles.json.", "Shortcuts.json.", "Settings.json.", "Donation.json." }
            .Any(prefix => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRetainedLog(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        return fileName.EndsWith(".log", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".log.old", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".log.bak", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsKnownLegacyConfiguration(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        return new[] { "DisplayProfiles.json", "AudioProfiles.json", "Shortcuts.json", "Settings.json", "Donation.json" }
            .Any(name => string.Equals(fileName, name, StringComparison.OrdinalIgnoreCase)
                || string.Equals(fileName, $"{name}.old", StringComparison.OrdinalIgnoreCase));
    }

    private static void AddFile(ZipArchive archive, string sourcePath, string entryName, long? maximumBytes, List<string> warnings, List<SupportBundleEntry> includedEntries)
    {
        if (!File.Exists(sourcePath))
        {
            warnings.Add($"{entryName} was not available.");
            return;
        }

        FileStream input;
        try
        {
            if (UsesFilesystemLink(sourcePath))
            {
                warnings.Add($"{entryName} was skipped because it is a filesystem link.");
                return;
            }

            input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            warnings.Add($"{entryName} could not be added: {ex.Message}");
            return;
        }

        using (input)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName.Replace('\\', '/'), CompressionLevel.Optimal);
            using Stream output = entry.Open();
            long remainingBytes = maximumBytes ?? input.Length;
            long copiedBytes = 0;
            byte[] buffer = new byte[81920];
            while (remainingBytes > 0)
            {
                int read;
                try
                {
                    read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, remainingBytes));
                }
                catch (IOException ex)
                {
                    warnings.Add($"{entryName} could only be partially collected: {ex.Message}");
                    break;
                }
                if (read == 0)
                {
                    break;
                }

                output.Write(buffer, 0, read);
                remainingBytes -= read;
                copiedBytes += read;
            }

            if (maximumBytes.HasValue && input.Length > maximumBytes.Value)
            {
                warnings.Add($"{entryName} was limited to {maximumBytes.Value} bytes.");
            }

            SupportBundleEntry description = DescribeEntry(entryName.Replace('\\', '/'), copiedBytes);
            if (remainingBytes > 0)
            {
                description.CollectionResult = "Partial";
                warnings.Add($"{entryName} ended before all expected bytes could be collected.");
            }
            includedEntries.Add(description);
        }
    }

    private static SupportBundleEntry DescribeEntry(string entryName, long length)
    {
        SupportBundleEntry entry = new SupportBundleEntry
        {
            Path = entryName,
            Length = length,
            CollectionResult = "Included"
        };

        if (entryName.StartsWith("Logs/Legacy/", StringComparison.OrdinalIgnoreCase)
            || entryName.StartsWith("Logs/Backups/", StringComparison.OrdinalIgnoreCase))
        {
            entry.Category = "LegacyLog";
            entry.Component = GetLogComponent(Path.GetFileName(entryName));
            entry.Format = "legacy-text";
            return entry;
        }

        if (entryName.StartsWith("Logs/", StringComparison.OrdinalIgnoreCase))
        {
            entry.Category = "Log";
            entry.Format = "logfmt-v1";
            entry.Component = GetLogComponent(Path.GetFileName(entryName));
            return entry;
        }

        if (entryName.StartsWith("MachineLogs/", StringComparison.OrdinalIgnoreCase))
        {
            entry.Category = "Log";
            string fileName = Path.GetFileName(entryName);
            entry.Component = fileName.StartsWith("Installer-", StringComparison.OrdinalIgnoreCase) ? "Installer" : GetLogComponent(fileName) ?? "ControlService";
            entry.Format = string.Equals(entry.Component, "Installer", StringComparison.Ordinal) ? "msi-verbose"
                : fileName.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase) ? "jsonl" : "logfmt-v1";
            return entry;
        }

        entry.Category = entryName.StartsWith("Configuration/Backups/", StringComparison.OrdinalIgnoreCase) ? "ConfigurationBackup" : "Configuration";
        entry.Component = entryName.StartsWith("Configuration/Machine/", StringComparison.OrdinalIgnoreCase) ? "ControlService" : "UserAgent";
        entry.Format = entryName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            || entryName.EndsWith(".json.bak", StringComparison.OrdinalIgnoreCase)
            || entryName.EndsWith(".json.old", StringComparison.OrdinalIgnoreCase)
            || entry.Category == "ConfigurationBackup" ? "json" : "file";
        return entry;
    }

    private static string? GetLogComponent(string fileName)
    {
        if (fileName.StartsWith("DesktopConsole", StringComparison.OrdinalIgnoreCase))
        {
            return "DesktopConsole";
        }

        if (fileName.StartsWith("UserAgent", StringComparison.OrdinalIgnoreCase))
        {
            return "UserAgent";
        }

        if (fileName.StartsWith("ControlService", StringComparison.OrdinalIgnoreCase))
        {
            return "ControlService";
        }

        if (fileName.StartsWith("SessionLauncher", StringComparison.OrdinalIgnoreCase))
        {
            return "SessionLauncher";
        }

        if (fileName.StartsWith("DisplayMagician", StringComparison.OrdinalIgnoreCase))
        {
            return "DesktopApp";
        }

        return null;
    }

    private sealed class SupportBundleEntry
    {
        public string Path { get; set; } = string.Empty;

        public long Length { get; set; }

        public string Category { get; set; } = string.Empty;

        public string? Component { get; set; }

        public string Format { get; set; } = string.Empty;

        public string CollectionResult { get; set; } = string.Empty;
    }
}
