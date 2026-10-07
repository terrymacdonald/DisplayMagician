using DisplayMagician.UserAgent.Runtime;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class DisplayMagicianSerializationBinderTests
{
    [Fact]
    public void TryMigrateLegacyRuntimeTypeNames_RewritesOnlyLegacyTypeMetadata()
    {
        string legacyJson = "{\"Description\":\"DisplayMagicianShared.ScreenPosition\",\"$type\":\"DisplayMagicianShared.ScreenPosition, DisplayMagicianShared\",\"Name\":\"Primary\"}";

        bool wasMigrated = DisplayMagicianSerializationBinder.TryMigrateLegacyRuntimeTypeNames(legacyJson, out string migratedJson);

        Assert.True(wasMigrated);
        Assert.Contains("\"Description\": \"DisplayMagicianShared.ScreenPosition\"", migratedJson);
        Assert.Contains("\"$type\": \"DisplayMagician.UserAgent.Runtime.ScreenPosition, DisplayMagician.UserAgent\"", migratedJson);
        Assert.Equal(typeof(ScreenPosition), DisplayMagicianSerializationBinder.Instance.BindToType("DisplayMagician.UserAgent", "DisplayMagician.UserAgent.Runtime.ScreenPosition"));
    }

    [Fact]
    public void TryMigrateLegacyRuntimeTypeNames_LeavesCurrentTypeMetadataUnchanged()
    {
        string currentJson = "{\"$type\":\"DisplayMagician.UserAgent.Runtime.ScreenPosition, DisplayMagician.UserAgent\"}";

        bool wasMigrated = DisplayMagicianSerializationBinder.TryMigrateLegacyRuntimeTypeNames(currentJson, out string migratedJson);

        Assert.False(wasMigrated);
        Assert.Equal(currentJson, migratedJson);
    }
}