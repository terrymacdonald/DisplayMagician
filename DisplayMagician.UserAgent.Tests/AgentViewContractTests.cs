using System;
using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AgentViewContractTests
{
    [Fact]
    public void ProfileListResult_RoundTripsCurrentLayoutAndProfileSettings()
    {
        DisplayProfileSettings settings = new DisplayProfileSettings
        {
            ApplyWallpaper = true,
            BackgroundDescription = "Span across displays",
            ForceExplorerRestart = true,
            ApplyProfileCount = 3,
            ApplyProfileDelay = 750
        };
        ProfileListResult profileList = new ProfileListResult
        {
            SavedProfiles =
            [
                new DisplayProfileView
                {
                    Id = "saved-profile",
                    Name = "Desk",
                    ThumbnailPngBase64 = Convert.ToBase64String([137, 80, 78, 71]),
                    ConnectedDisplayCount = 2,
                    PrimaryDisplayWidth = 3840,
                    PrimaryDisplayHeight = 2160,
                    IsSaved = true,
                    IsActive = true,
                    IsValid = true,
                    Settings = settings
                }
            ],
            CurrentLayout = new DisplayProfileView
            {
                Id = "current-layout",
                Name = "Current layout",
                IsSaved = false,
                IsValid = true,
                Settings = settings
            }
        };

        ProfileListResult? roundTripped = JsonSerializer.Deserialize<ProfileListResult>(JsonSerializer.Serialize(profileList));

        Assert.NotNull(roundTripped);
        Assert.Single(roundTripped!.SavedProfiles);
        Assert.Equal("saved-profile", roundTripped.SavedProfiles[0].Id);
        Assert.Equal(2, roundTripped.SavedProfiles[0].ConnectedDisplayCount);
        Assert.Equal(3840, roundTripped.SavedProfiles[0].PrimaryDisplayWidth);
        Assert.True(roundTripped.SavedProfiles[0].Settings.ApplyWallpaper);
        Assert.Equal("Span across displays", roundTripped.SavedProfiles[0].Settings.BackgroundDescription);
        Assert.True(roundTripped.SavedProfiles[0].Settings.ForceExplorerRestart);
        Assert.Equal(3, roundTripped.SavedProfiles[0].Settings.ApplyProfileCount);
        Assert.Equal(750, roundTripped.SavedProfiles[0].Settings.ApplyProfileDelay);
        Assert.NotNull(roundTripped.CurrentLayout);
        Assert.False(roundTripped.CurrentLayout!.IsSaved);
    }

    [Fact]
    public void UpdateDisplayProfileSettingsRequest_RoundTripsAllMutableSettings()
    {
        UpdateDisplayProfileSettingsRequest request = new UpdateDisplayProfileSettingsRequest
        {
            ProfileId = "profile-id",
            Settings = new DisplayProfileSettings
            {
                ApplyWallpaper = true,
                BackgroundDescription = "Solid color",
                ForceExplorerRestart = true,
                ApplyProfileCount = 2,
                ApplyProfileDelay = 500
            }
        };

        UpdateDisplayProfileSettingsRequest? roundTripped = JsonSerializer.Deserialize<UpdateDisplayProfileSettingsRequest>(JsonSerializer.Serialize(request));

        Assert.NotNull(roundTripped);
        Assert.Equal("profile-id", roundTripped!.ProfileId);
        Assert.True(roundTripped.Settings.ApplyWallpaper);
        Assert.Equal("Solid color", roundTripped.Settings.BackgroundDescription);
        Assert.True(roundTripped.Settings.ForceExplorerRestart);
        Assert.Equal(2, roundTripped.Settings.ApplyProfileCount);
        Assert.Equal(500, roundTripped.Settings.ApplyProfileDelay);
    }

    [Fact]
    public void AudioProfileListResult_RoundTripsSavedProfilesAndCurrentLayout()
    {
        AudioProfileListResult profileList = new AudioProfileListResult
        {
            CanAccessAudioSettings = true,
            SavedProfiles =
            [
                new AudioProfileView
                {
                    Id = "saved-audio-profile",
                    Name = "Headphones",
                    IsSaved = true,
                    IsActive = true,
                    SettingsText = "Default device: Headphones",
                    UnavailableDeviceNames = ["Microphone"]
                }
            ],
            CurrentLayout = new AudioProfileView
            {
                Id = "current-audio-layout",
                Name = "Current audio setup",
                IsSaved = false,
                SettingsText = "Default device: Speakers"
            }
        };

        AudioProfileListResult? roundTripped = JsonSerializer.Deserialize<AudioProfileListResult>(JsonSerializer.Serialize(profileList));

        Assert.NotNull(roundTripped);
        Assert.True(roundTripped!.CanAccessAudioSettings);
        AudioProfileView savedProfile = Assert.Single(roundTripped.SavedProfiles);
        Assert.Equal("saved-audio-profile", savedProfile.Id);
        Assert.True(savedProfile.IsSaved);
        Assert.True(savedProfile.IsActive);
        Assert.Equal("Microphone", Assert.Single(savedProfile.UnavailableDeviceNames));
        Assert.NotNull(roundTripped.CurrentLayout);
        Assert.False(roundTripped.CurrentLayout!.IsSaved);
    }

    [Fact]
    public void ShortcutListResult_RoundTripsAgentRenderedIconPayload()
    {
        string iconPngBase64 = Convert.ToBase64String([137, 80, 78, 71, 13, 10, 26, 10]);
        ShortcutListResult shortcutList = new ShortcutListResult
        {
            Shortcuts =
            [
                new ShortcutView
                {
                    Id = "shortcut-id",
                    Name = "Game shortcut",
                    Category = ShortcutCategory.Game,
                    ProfileId = "display-profile",
                    IconPngBase64 = iconPngBase64
                }
            ]
        };

        ShortcutListResult? roundTripped = JsonSerializer.Deserialize<ShortcutListResult>(JsonSerializer.Serialize(shortcutList));

        Assert.NotNull(roundTripped);
        ShortcutView shortcut = Assert.Single(roundTripped!.Shortcuts);
        Assert.Equal("shortcut-id", shortcut.Id);
        Assert.Equal(ShortcutCategory.Game, shortcut.Category);
        Assert.Equal("display-profile", shortcut.ProfileId);
        Assert.Equal(iconPngBase64, shortcut.IconPngBase64);
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, Convert.FromBase64String(shortcut.IconPngBase64!));
    }
}
