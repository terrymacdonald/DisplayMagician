using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using DisplayMagician.Contracts;

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

    public UserSupportBundleResult Create(string destinationPath, string machineLogsStagingPath)
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
        using FileStream output = new FileStream(fullDestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using ZipArchive archive = new ZipArchive(output, ZipArchiveMode.Create);
        AddDirectory(archive, "Profiles", Path.Combine(_userDataPath, "Profiles"), null, warnings);
        AddDirectory(archive, "AudioProfiles", Path.Combine(_userDataPath, "AudioProfiles"), null, warnings);
        AddDirectory(archive, "Shortcuts", Path.Combine(_userDataPath, "Shortcuts"), null, warnings);
        AddDirectory(archive, "Settings", Path.Combine(_userDataPath, "Settings"), null, warnings);
        AddDirectory(archive, "Logs", Path.Combine(_userDataPath, "Logs"), null, warnings);
        AddMachineLogs(archive, machineLogsStagingPath, warnings);
        AddDirectory(archive, "LegacyFiles", Path.Combine(_userDataPath, "LegacyFiles"), null, warnings);
        AddFile(archive, Path.Combine(_userDataPath, "Migration.json"), "Migration.json", null, warnings);
        AddManifest(archive, warnings);

        return new UserSupportBundleResult { DestinationPath = fullDestinationPath, Warnings = warnings.ToArray() };
    }

    private void AddMachineLogs(ZipArchive archive, string machineLogsStagingPath, List<string> warnings)
    {
        string stagingRoot = Path.GetFullPath(Path.Combine(_userDataPath, "Backups", "SupportStaging")) + Path.DirectorySeparatorChar;
        string fullMachineLogsPath = string.IsNullOrWhiteSpace(machineLogsStagingPath) ? string.Empty : Path.GetFullPath(machineLogsStagingPath);
        if (!fullMachineLogsPath.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("Control Service logs were not available.");
            return;
        }

        AddDirectory(archive, "MachineLogs", fullMachineLogsPath, null, warnings);
    }

    private void AddManifest(ZipArchive archive, List<string> warnings)
    {
        ZipArchiveEntry entry = archive.CreateEntry("support-manifest.json", CompressionLevel.Optimal);
        using StreamWriter writer = new StreamWriter(entry.Open());
        writer.Write(JsonSerializer.Serialize(new
        {
            SchemaVersion = 1,
            CreatedUtc = DateTime.UtcNow,
            AgentVersion = _registration.Version,
            UserSid = _registration.UserSid,
            Warnings = warnings
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void AddDirectory(ZipArchive archive, string entryRoot, string directoryPath, long? maximumBytesPerFile, List<string> warnings)
    {
        if (!Directory.Exists(directoryPath))
        {
            warnings.Add($"{entryRoot} was not available.");
            return;
        }

        foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(directoryPath, filePath);
            AddFile(archive, filePath, Path.Combine(entryRoot, relativePath), maximumBytesPerFile, warnings);
        }
    }

    private static void AddFile(ZipArchive archive, string sourcePath, string entryName, long? maximumBytes, List<string> warnings)
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
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            warnings.Add($"{entryName} could not be added: {ex.Message}");
        }
    }
}
