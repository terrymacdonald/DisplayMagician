using System;
using System.Collections.Generic;
using DisplayMagician.UserAgent;
using DisplayMagician.UserAgent.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class UserProfileRecoverySnapshotTests
{
    [Fact]
    public void DisplayRecoverySnapshotPreservesConfigurationWithoutRenderingOrPersistingIcons()
    {
        ProfileItem original = new ProfileItem
        {
            UUID = Guid.NewGuid().ToString("D"), Name = "Unsaved original layout",
            ProfileDisplayIdentifiers = new List<string> { "test-display" },
            ApplyProfileCount = 2, ApplyProfileDelay = 1, ForceExplorerRestart = true
        };

        string json = UserProfileOperationService.SerializeDisplayRecoveryProfile(original);
        JObject fields = JObject.Parse(json);
        Assert.Null(fields["ProfileBitmap"]);
        Assert.Null(fields["ProfileShortcutBitmap"]);
        Assert.Null(fields["SavedProfileIconCacheFilename"]);
        Assert.NotNull(fields["WindowsDisplayConfig"]);
        Assert.NotNull(fields["NVIDIADisplayConfig"]);
        Assert.NotNull(fields["AMDDisplayConfig"]);
        Assert.NotNull(fields["IntelDisplayConfig"]);
        Assert.NotNull(fields["WallpaperConfiguration"]);

        ProfileItem restored = JsonConvert.DeserializeObject<ProfileItem>(json, UserProfileOperationService.RecoveryJsonSettings)!;
        Assert.Equal(original.UUID, restored.UUID);
        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.ProfileDisplayIdentifiers, restored.ProfileDisplayIdentifiers);
        Assert.Equal(2, restored.ApplyProfileCount);
        Assert.Equal(1, restored.ApplyProfileDelay);
        Assert.True(restored.ForceExplorerRestart);
    }
}
