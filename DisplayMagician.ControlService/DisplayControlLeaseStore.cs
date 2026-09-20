using System;
using System.IO;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

/// <summary>Durably stores the machine-wide display-control lease, including unresolved recovery state.</summary>
public sealed class DisplayControlLeaseStore
{
    private readonly string _leasePath;

    public DisplayControlLeaseStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _leasePath = Path.Combine(storagePaths.MachinePath, "DisplayControlLease.json");
    }

    public DisplayControlLease? Load()
    {
        if (!File.Exists(_leasePath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DisplayControlLease>(File.ReadAllText(_leasePath, Encoding.UTF8));
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            return null;
        }
    }

    public void Save(DisplayControlLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        Write(JsonSerializer.Serialize(lease));
    }

    public void Clear()
    {
        if (File.Exists(_leasePath))
        {
            File.Delete(_leasePath);
        }
    }

    private void Write(string json)
    {
        string directoryPath = Path.GetDirectoryName(_leasePath)!;
        Directory.CreateDirectory(directoryPath);
        string temporaryPath = Path.Combine(directoryPath, $".{Path.GetFileName(_leasePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (FileStream stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(_leasePath))
            {
                File.Replace(temporaryPath, _leasePath, $"{_leasePath}.bak", ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, _leasePath);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}