using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

/// <summary>Agent-owned persistence boundary for shortcut definitions. Runtime execution moves to ShortcutRunner separately.</summary>
public sealed class ShortcutStore
{
    private readonly string _shortcutFilePath;

    public ShortcutStore(string userDataPath)
    {
        _shortcutFilePath = Path.Combine(userDataPath, "Shortcuts", "Shortcuts.json");
    }

    public RepositorySnapshot GetSnapshot()
    {
        byte[] content = File.Exists(_shortcutFilePath) ? File.ReadAllBytes(_shortcutFilePath) : Array.Empty<byte>();
        return new RepositorySnapshot { Repository = RepositoryKind.Shortcuts, Revision = content.Length == 0 ? 0 : BitConverter.ToInt64(SHA256.HashData(content), 0), Json = content.Length == 0 ? string.Empty : Encoding.Unicode.GetString(content).TrimStart('\uFEFF') };
    }

    public RepositoryCommitResult Commit(RepositoryCommitRequest request)
    {
        RepositorySnapshot snapshot = GetSnapshot();
        if (request.ExpectedRevision != snapshot.Revision)
        {
            return new RepositoryCommitResult { WasConflict = true, Snapshot = snapshot };
        }

        using JsonDocument _ = JsonDocument.Parse(request.Json);
        string directory = Path.GetDirectoryName(_shortcutFilePath)!;
        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(directory, $".Shortcuts.{Guid.NewGuid():N}.tmp");
        File.WriteAllText(temporaryPath, request.Json, Encoding.Unicode);
        if (File.Exists(_shortcutFilePath)) File.Replace(temporaryPath, _shortcutFilePath, null); else File.Move(temporaryPath, _shortcutFilePath);
        return new RepositoryCommitResult { Snapshot = GetSnapshot() };
    }
}
