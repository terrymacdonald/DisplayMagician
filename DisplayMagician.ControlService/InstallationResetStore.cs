using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

/// <summary>Deletes installation data only after the maintenance host has stopped every writer.</summary>
internal static class InstallationResetStore
{
    internal static void Reset(StoragePaths paths, IReadOnlyDictionary<string, string> windowsProfiles)
    {
        string root = Path.GetFullPath(paths.RootPath).TrimEnd(Path.DirectorySeparatorChar);
        SupportBundleFileAccess.EnsureUnlinkedPath(root);
        HashSet<string> users = new(windowsProfiles.Keys, StringComparer.OrdinalIgnoreCase);
        if (Directory.Exists(paths.UsersPath))
        {
            SupportBundleFileAccess.EnsureUnlinkedPath(paths.UsersPath);
            foreach (string directory in Directory.GetDirectories(paths.UsersPath))
            {
                string sid = Path.GetFileName(directory);
                try { _ = paths.GetUserPaths(sid); users.Add(sid); }
                catch (ArgumentException) { } // Non-user folders are still removed below, without creating a user marker.
            }
        }
        ClearDirectory(root, root);
        foreach (string profile in windowsProfiles.Values.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string profileRoot = Path.GetFullPath(profile);
            foreach (string relative in new[] { Path.Combine("AppData", "Local", "DisplayMagician"), Path.Combine("AppData", "Roaming", "DisplayMagician") })
            {
                string legacyRoot = Path.GetFullPath(Path.Combine(profileRoot, relative));
                if (!legacyRoot.StartsWith(profileRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("A legacy reset path escaped its Windows profile.");
                SupportBundleFileAccess.EnsureUnlinkedPath(legacyRoot);
                ClearDirectory(legacyRoot, legacyRoot);
            }
        }
        paths.EnsureMachineDirectories();
        AtomicFileStore.WriteAllText(Path.Combine(paths.MachinePath, "ControlServiceSettings.json"),
            JsonSerializer.Serialize(new ControlServiceSettingsDocument { SuppressLegacyUserImport = true }));
        // Gateway deletes its own CurrentUser CNG keys/certificate under its LocalService identity.
        AtomicFileStore.WriteAllText(Path.Combine(paths.MachinePath, "GatewayIdentityReset.json"), "{\"InstallationReset\":true}");
        foreach (string sid in users)
        {
            UserStoragePaths user = paths.ProvisionUserStorage(sid);
            string legacy = windowsProfiles.TryGetValue(sid, out string? profile)
                ? Path.Combine(profile, "AppData", "Local", "DisplayMagician") : Path.Combine(user.RootPath, "LegacyImportDisabled");
            AtomicFileStore.WriteAllText(user.MigrationMarkerPath, JsonSerializer.Serialize(new MigrationMarker
            { SchemaVersion = 1, MigratedUtc = DateTime.UtcNow, LegacyAppDataPath = legacy, WasInstallationReset = true }));
        }
    }

    private static void ClearDirectory(string root, string directory)
    {
        string full = Path.GetFullPath(directory);
        if (!string.Equals(full, root, StringComparison.OrdinalIgnoreCase) && !full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new IOException("A reset path escaped its data root.");
        if (!Directory.Exists(full)) return;
        SupportBundleFileAccess.EnsureUnlinkedPath(full);
        foreach (string entry in Directory.GetFileSystemEntries(full))
        {
            FileAttributes attributes = File.GetAttributes(entry);
            bool isDirectory = (attributes & FileAttributes.Directory) != 0;
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                // Remove the link itself, never its target (including redirected cache/log folders).
                if (isDirectory) Directory.Delete(entry); else File.Delete(entry);
                continue;
            }
            string name = Path.GetFileName(entry);
            if (isDirectory)
            {
                if (name.Equals("Logs", StringComparison.OrdinalIgnoreCase) || name.Equals("MachineLogs", StringComparison.OrdinalIgnoreCase)) continue;
                ClearDirectory(root, entry);
                if (!Directory.EnumerateFileSystemEntries(entry).Any()) Directory.Delete(entry);
            }
            else if (!name.EndsWith(".log", StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".log.gz", StringComparison.OrdinalIgnoreCase)
                && !name.Contains(".log.", StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase))
            {
                if ((attributes & FileAttributes.ReadOnly) != 0) File.SetAttributes(entry, attributes & ~FileAttributes.ReadOnly);
                File.Delete(entry);
            }
        }
    }
}
