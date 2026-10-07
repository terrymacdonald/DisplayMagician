using System;
using System.IO;
using System.Security.Principal;
using System.Text.Json;

namespace DisplayMagician.UserAgent.Runtime;

public static class V4UserDataPathResolver
{
    public static bool TryGetMigratedUserDataPath(out string userDataPath)
    {
        userDataPath = string.Empty;

        try
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            string? userSid = identity.User?.Value;
            if (string.IsNullOrWhiteSpace(userSid))
            {
                return false;
            }

            string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Users", userSid);
            string markerPath = Path.Combine(rootPath, "Migration.json");
            if (!File.Exists(markerPath))
            {
                return false;
            }

            using JsonDocument marker = JsonDocument.Parse(File.ReadAllText(markerPath));
            if (!marker.RootElement.TryGetProperty("SchemaVersion", out JsonElement schemaVersion) || schemaVersion.GetInt32() != 1)
            {
                return false;
            }

            userDataPath = rootPath;
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is PlatformNotSupportedException)
        {
            return false;
        }
    }
}
