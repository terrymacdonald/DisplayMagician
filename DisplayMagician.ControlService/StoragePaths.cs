using System;
using System.IO;
using System.Security.Principal;

namespace DisplayMagician.ControlService;

public sealed class StoragePaths
{
    public StoragePaths(string? rootPath = null)
    {
        RootPath = rootPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician");
        MachinePath = Path.Combine(RootPath, "Machine");
        UsersPath = Path.Combine(RootPath, "Users");
    }

    public string RootPath { get; }

    public string MachinePath { get; }

    public string UsersPath { get; }

    public string MachineLogsPath => Path.Combine(MachinePath, "Logs");

    public string MachineBackupsPath => Path.Combine(MachinePath, "Backups");

    public void EnsureMachineDirectories()
    {
        Directory.CreateDirectory(MachinePath);
        Directory.CreateDirectory(MachineLogsPath);
        Directory.CreateDirectory(MachineBackupsPath);
        Directory.CreateDirectory(UsersPath);
    }

    public UserStoragePaths GetUserPaths(string userSid)
    {
        if (string.IsNullOrWhiteSpace(userSid))
        {
            throw new ArgumentException("A Windows SID is required.", nameof(userSid));
        }

        SecurityIdentifier sid = new SecurityIdentifier(userSid);
        string userPath = Path.Combine(UsersPath, sid.Value);
        return new UserStoragePaths(sid.Value, userPath);
    }
}

public sealed class UserStoragePaths
{
    public UserStoragePaths(string userSid, string rootPath)
    {
        UserSid = userSid;
        RootPath = rootPath;
    }

    public string UserSid { get; }

    public string RootPath { get; }

    public string ProfilesPath => Path.Combine(RootPath, "Profiles");

    public string AudioProfilesPath => Path.Combine(RootPath, "AudioProfiles");

    public string ShortcutsPath => Path.Combine(RootPath, "Shortcuts");

    public string SettingsPath => Path.Combine(RootPath, "Settings");

    public string MessagesPath => Path.Combine(RootPath, "Messages");

    public string BackupsPath => Path.Combine(RootPath, "Backups");

    public string LogsPath => Path.Combine(RootPath, "Logs");

    public string MigrationMarkerPath => Path.Combine(RootPath, "Migration.json");

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(ProfilesPath);
        Directory.CreateDirectory(AudioProfilesPath);
        Directory.CreateDirectory(ShortcutsPath);
        Directory.CreateDirectory(SettingsPath);
        Directory.CreateDirectory(MessagesPath);
        Directory.CreateDirectory(BackupsPath);
        Directory.CreateDirectory(LogsPath);
    }
}
