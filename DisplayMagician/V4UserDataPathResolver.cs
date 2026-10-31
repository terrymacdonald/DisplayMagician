using System;
using System.IO;
using System.Security.Principal;
using System.Text.Json;

namespace DisplayMagician;

public static class V4UserDataPathResolver
{
    public static bool WasInstallationReset { get; private set; }
    public static bool TryGetMigratedUserDataPath(out string userDataPath)
    {
        userDataPath = string.Empty;
        WasInstallationReset = false;

        try
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            string userSid = identity.User?.Value;
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
            WasInstallationReset = marker.RootElement.TryGetProperty("WasInstallationReset", out JsonElement reset) && reset.ValueKind == JsonValueKind.True;
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is PlatformNotSupportedException)
        {
            return false;
        }
    }
}
