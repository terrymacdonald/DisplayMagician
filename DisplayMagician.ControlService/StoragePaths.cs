using System;
using System.IO;
using System.Security.AccessControl;
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

    /// <summary>
    /// Creates a per-user data root that the interactive user can modify without granting
    /// access to any other user's DisplayMagician data. This must run from the elevated
    /// installer or the Control Service before the WinForms application adopts the v4 path.
    /// </summary>
    public UserStoragePaths ProvisionUserStorage(string userSid)
    {
        UserStoragePaths userPaths = GetUserPaths(userSid);
        userPaths.EnsureDirectories();

        SecurityIdentifier userIdentity = new SecurityIdentifier(userPaths.UserSid);
        SecurityIdentifier administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        SecurityIdentifier localSystem = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        foreach (string path in userPaths.GetAllPaths())
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            DirectorySecurity security = directory.GetAccessControl();
            InheritanceFlags inheritance = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            security.SetAccessRule(new FileSystemAccessRule(userIdentity, FileSystemRights.Modify, inheritance, PropagationFlags.None, AccessControlType.Allow));
            security.SetAccessRule(new FileSystemAccessRule(administrators, FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow));
            security.SetAccessRule(new FileSystemAccessRule(localSystem, FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow));
            directory.SetAccessControl(security);
        }

        return userPaths;
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

    public string IconsPath => Path.Combine(RootPath, "Icons");

    public string WallpaperPath => Path.Combine(RootPath, "Wallpaper");

    public string BackupsPath => Path.Combine(RootPath, "Backups");

    public string LogsPath => Path.Combine(RootPath, "Logs");

    public string MigrationMarkerPath => Path.Combine(RootPath, "Migration.json");

    public string LegacyFilesPath => Path.Combine(RootPath, "LegacyFiles");

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(ProfilesPath);
        Directory.CreateDirectory(AudioProfilesPath);
        Directory.CreateDirectory(ShortcutsPath);
        Directory.CreateDirectory(SettingsPath);
        Directory.CreateDirectory(MessagesPath);
        Directory.CreateDirectory(IconsPath);
        Directory.CreateDirectory(WallpaperPath);
        Directory.CreateDirectory(BackupsPath);
        Directory.CreateDirectory(LogsPath);
        Directory.CreateDirectory(LegacyFilesPath);
    }

    internal string[] GetAllPaths()
    {
        return new[]
        {
            RootPath,
            ProfilesPath,
            AudioProfilesPath,
            ShortcutsPath,
            SettingsPath,
            MessagesPath,
            IconsPath,
            WallpaperPath,
            BackupsPath,
            LogsPath,
            LegacyFilesPath
        };
    }
}
