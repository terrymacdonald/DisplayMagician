using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class SavedResourceChangeDetectorTests
{
    [Fact]
    public void DetectsUpsertsAndDeletesByStableId()
    {
        const string before = "{\"LastUpdated\":\"old\",\"Profiles\":[{\"UUID\":\"unchanged\",\"Name\":\"Same\"},{\"UUID\":\"renamed\",\"Name\":\"Old\"},{\"UUID\":\"deleted\",\"Name\":\"Gone\"}]}";
        const string after = "{\"LastUpdated\":\"new\",\"Profiles\":[{\"UUID\":\"unchanged\",\"Name\":\"Same\"},{\"UUID\":\"renamed\",\"Name\":\"New\"},{\"UUID\":\"added\",\"Name\":\"Added\"}]}";

        GatewaySavedResourceChange[] changes = SavedResourceChangeDetector.Detect(RepositoryKind.DisplayProfiles, before, after);

        Assert.Equal(3, changes.Length);
        Assert.Contains(changes, change => change.Id == "renamed" && change.Action == "upsert" && change.Href == "/v1/display-profiles/renamed");
        Assert.Contains(changes, change => change.Id == "added" && change.Action == "upsert" && change.Href == "/v1/display-profiles/added");
        Assert.Contains(changes, change => change.Id == "deleted" && change.Action == "deleted" && change.Href == null);
        Assert.DoesNotContain(changes, change => change.Id == "unchanged");
    }

    [Theory]
    [InlineData(RepositoryKind.AudioProfiles, "AudioProfiles", "audio-profile", "/v1/audio-profiles/a%20b")]
    [InlineData(RepositoryKind.Shortcuts, "Shortcuts", "shortcut", "/v1/shortcuts/a%20b")]
    public void MapsOtherCollectionsToTheirResourceLinks(RepositoryKind kind, string property, string type, string href)
    {
        GatewaySavedResourceChange change = Assert.Single(SavedResourceChangeDetector.Detect(kind, string.Empty, $"{{\"{property}\":[{{\"UUID\":\"a b\"}}]}}"));

        Assert.Equal(type, change.ResourceType);
        Assert.Equal(href, change.Href);
    }

    [Fact]
    public void RejectsMissingOrDuplicateIdsBeforeAnEdit()
    {
        Assert.Throws<JsonException>(() => SavedResourceChangeDetector.Validate(RepositoryKind.Shortcuts, "{\"Shortcuts\":[{}]}"));
        Assert.Throws<JsonException>(() => SavedResourceChangeDetector.Validate(RepositoryKind.Shortcuts, "{\"Shortcuts\":[{\"UUID\":\"x\"},{\"UUID\":\"X\"}]}"));
    }
}
