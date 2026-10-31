using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

internal static class MachineSupportBundleStager
{
    internal static void Stage(StoragePaths paths, string stagingRoot, List<string> warnings)
    {
        SupportBundleFileAccess.EnsureUnlinkedPath(stagingRoot);
        string logsPath = Path.Combine(stagingRoot, "MachineLogs");
        string configurationPath = Path.Combine(stagingRoot, "Configuration", "Machine");
        Directory.CreateDirectory(logsPath);
        Directory.CreateDirectory(configurationPath);
        SupportBundleFileAccess.EnsureUnlinkedPath(logsPath);
        SupportBundleFileAccess.EnsureUnlinkedPath(configurationPath);

        Stack<string> pending = new Stack<string>();
        pending.Push(paths.MachineLogsPath);
        while (pending.Count > 0)
        {
            string directoryPath = pending.Pop();
            string[] children;
            try
            {
                SupportBundleFileAccess.EnsureUnlinkedPath(directoryPath);
                children = Directory.GetFileSystemEntries(directoryPath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                warnings.Add($"Machine logs folder {Path.GetRelativePath(paths.MachineLogsPath, directoryPath)} could not be collected: {ex.Message}");
                continue;
            }

            foreach (string sourcePath in children)
            {
                try
                {
                    FileAttributes attributes = File.GetAttributes(sourcePath);
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        warnings.Add($"Machine log {Path.GetRelativePath(paths.MachineLogsPath, sourcePath)} was skipped because it is a filesystem link.");
                        continue;
                    }
                    if ((attributes & FileAttributes.Directory) != 0)
                    {
                        pending.Push(sourcePath);
                        continue;
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    warnings.Add($"Machine log {Path.GetFileName(sourcePath)} could not be inspected: {ex.Message}");
                    continue;
                }

                CopyFile(sourcePath, Path.Combine(logsPath, Path.GetRelativePath(paths.MachineLogsPath, sourcePath)), warnings);
            }
        }

        CopyFile(Path.Combine(paths.MachineDiagnosticsPath, "Audit.jsonl"), Path.Combine(logsPath, "Diagnostics", "Audit.jsonl"), warnings);
        foreach (string sourcePath in new[]
        {
            Path.Combine(paths.MachinePath, "DisplayControlLease.json"),
            Path.Combine(paths.MachinePath, "OperationDecisions.json"),
            Path.Combine(paths.MachinePath, "OperationStatuses.json"),
            Path.Combine(paths.MachinePath, "ScheduleState.json"),
            Path.Combine(paths.MachinePath, "GatewaySettings.json"),
            Path.Combine(paths.MachineDiagnosticsPath, "RecoveryAdministration.json"),
            Path.Combine(paths.MachineDiagnosticsPath, "TemporaryDiagnosticLogLevel.json"),
            Path.Combine(paths.MachineDiagnosticsPath, "LoggingSettings.json")
        })
        {
            CopyFile(sourcePath, Path.Combine(configurationPath, Path.GetFileName(sourcePath)), warnings);
            // Backups are optional; unlike a requested primary snapshot, absence is expected.
            string backupPath = sourcePath + ".bak";
            CopyFile(backupPath, Path.Combine(configurationPath, Path.GetFileName(backupPath)), warnings, optional: true);
        }

        string pairedClientsPath = Path.Combine(paths.MachinePath, "PairedClients.json");
        string? redactedJson = null;
        try
        {
            SupportBundleFileAccess.EnsureUnlinkedPath(pairedClientsPath);
            PairedClient[] clients = JsonSerializer.Deserialize<PairedClient[]>(File.ReadAllText(pairedClientsPath))
                ?? throw new JsonException("The paired-device snapshot was null.");
            if (clients.Any(client => client == null)) throw new JsonException("The paired-device snapshot contained a null record.");
            redactedJson = PairedClientSupportSnapshot.Serialize(clients);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            warnings.Add($"PairedClients.json could not be collected as a redacted snapshot: {ex.Message}");
        }
        if (redactedJson != null)
        {
            string destination = Path.Combine(configurationPath, "PairedClients.redacted.json");
            SupportBundleFileAccess.EnsureUnlinkedPath(destination);
            using FileStream output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            using StreamWriter writer = new StreamWriter(output);
            writer.Write(redactedJson);
            warnings.Add("PairedClients.json was included as a redacted snapshot; credential hashes and key material were excluded.");
        }
    }

    internal static void CopyFile(string sourcePath, string destinationPath, List<string> warnings, bool optional = false)
    {
        // Destination failures are fatal: partial collection must not conceal unsafe staging.
        SupportBundleFileAccess.EnsureUnlinkedPath(destinationPath);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        SupportBundleFileAccess.EnsureUnlinkedPath(destinationPath);
        FileStream source;
        try
        {
            SupportBundleFileAccess.EnsureUnlinkedPath(sourcePath);
            source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        }
        catch (FileNotFoundException) when (optional)
        {
            // A rolling backup need not exist until the first replacement.
            return;
        }
        catch (DirectoryNotFoundException) when (optional)
        {
            // Some optional snapshot directories have not been created yet.
            return;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            warnings.Add($"{Path.GetFileName(sourcePath)} could not be collected: {ex.Message}");
            return;
        }

        using (source)
        using (FileStream destination = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            // Bound collection to the opening length so an actively growing log cannot run forever.
            long remaining = source.Length;
            byte[] buffer = new byte[81920];
            while (remaining > 0)
            {
                int read;
                try { read = source.Read(buffer, 0, (int)Math.Min(remaining, buffer.Length)); }
                catch (IOException ex)
                {
                    warnings.Add($"{Path.GetFileName(sourcePath)} was only partially collected: {ex.Message}");
                    return;
                }
                if (read == 0)
                {
                    warnings.Add($"{Path.GetFileName(sourcePath)} ended before all expected bytes could be collected.");
                    return;
                }
                destination.Write(buffer, 0, read);
                remaining -= read;
            }
        }
    }
}
