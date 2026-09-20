using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DisplayMagician.UserAgent;

/// <summary>Persists the state required to restore a temporary shortcut after an Agent interruption.</summary>
public sealed class ShortcutRecoveryStore
{
    private readonly string _recoveryFilePath;

    public ShortcutRecoveryStore(string userDataPath)
    {
        if (string.IsNullOrWhiteSpace(userDataPath))
        {
            throw new ArgumentException("A user data path is required.", nameof(userDataPath));
        }

        _recoveryFilePath = Path.Combine(Path.GetFullPath(userDataPath), "Settings", "ShortcutRecovery.json");
    }

    public ShortcutRecoveryRecord? GetPending()
    {
        if (!File.Exists(_recoveryFilePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(_recoveryFilePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<ShortcutRecoveryRecord>(json);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            return null;
        }
    }

    public void Save(ShortcutRecoveryRecord recoveryRecord)
    {
        ArgumentNullException.ThrowIfNull(recoveryRecord);
        string json = JsonSerializer.Serialize(recoveryRecord);
        string directoryPath = Path.GetDirectoryName(_recoveryFilePath)!;
        Directory.CreateDirectory(directoryPath);
        string temporaryPath = Path.Combine(directoryPath, $".{Path.GetFileName(_recoveryFilePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (FileStream stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(_recoveryFilePath))
            {
                File.Replace(temporaryPath, _recoveryFilePath, $"{_recoveryFilePath}.bak", ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, _recoveryFilePath);
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

    public void Clear()
    {
        if (File.Exists(_recoveryFilePath))
        {
            File.Delete(_recoveryFilePath);
        }
    }
}

public sealed class ShortcutRecoveryRecord
{
    public Guid OperationId { get; set; }
    public string ShortcutId { get; set; } = string.Empty;
    public string DisplayProfileId { get; set; } = string.Empty;
    public string AudioProfileId { get; set; } = string.Empty;
    public bool RequiresDisplayRestore { get; set; }
    public bool RequiresAudioRestore { get; set; }
    public DateTime CreatedUtc { get; set; }
}