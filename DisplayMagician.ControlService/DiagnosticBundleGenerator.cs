using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

/// <summary>Creates a bounded support bundle from machine-owned diagnostics without user profile data.</summary>
public sealed class DiagnosticBundleGenerator
{
    private const long MaximumLogBytesPerFile = 5 * 1024 * 1024;
    private readonly StoragePaths _storagePaths;
    private readonly ControlStateCoordinator _stateCoordinator;

    public DiagnosticBundleGenerator(StoragePaths storagePaths, ControlStateCoordinator stateCoordinator)
    {
        _storagePaths = storagePaths ?? throw new ArgumentNullException(nameof(storagePaths));
        _stateCoordinator = stateCoordinator ?? throw new ArgumentNullException(nameof(stateCoordinator));
    }

    public string Create()
    {
        _storagePaths.EnsureMachineDirectories();
        string bundlePath = Path.Combine(_storagePaths.MachineDiagnosticsPath, $"DisplayMagicianDiagnostics-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        using FileStream output = new FileStream(bundlePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using ZipArchive archive = new ZipArchive(output, ZipArchiveMode.Create);
        AddJsonEntry(archive, "service-status.json", _stateCoordinator.GetStatus(DateTime.UtcNow));
        AddExistingFile(archive, Path.Combine(_storagePaths.MachinePath, "DisplayControlLease.json"), "DisplayControlLease.json", null);
        AddExistingFile(archive, Path.Combine(_storagePaths.MachinePath, "OperationStatuses.json"), "OperationStatuses.json", null);
        AddExistingFile(archive, Path.Combine(_storagePaths.MachinePath, "ScheduleState.json"), "ScheduleState.json", null);
        AddExistingFile(archive, Path.Combine(_storagePaths.MachineDiagnosticsPath, "Audit.jsonl"), "Audit.jsonl", null);
        AddExistingFile(archive, Path.Combine(_storagePaths.MachineDiagnosticsPath, "RecoveryAdministration.json"), "RecoveryAdministration.json", null);
        if (Directory.Exists(_storagePaths.MachineLogsPath))
        {
            foreach (string logPath in Directory.GetFiles(_storagePaths.MachineLogsPath, "ControlService-*.log", SearchOption.TopDirectoryOnly))
            {
                AddExistingFile(archive, logPath, Path.Combine("Logs", Path.GetFileName(logPath)), MaximumLogBytesPerFile);
            }
        }

        return bundlePath;
    }

    private static void AddJsonEntry(ZipArchive archive, string entryName, object value)
    {
        ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using StreamWriter writer = new StreamWriter(entry.Open());
        writer.Write(JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void AddExistingFile(ZipArchive archive, string sourcePath, string entryName, long? maximumBytes)
    {
        if (!File.Exists(sourcePath))
        {
            return;
        }

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
    }
}