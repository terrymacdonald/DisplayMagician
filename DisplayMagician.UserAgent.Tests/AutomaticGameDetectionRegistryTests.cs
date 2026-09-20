using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AutomaticGameDetectionRegistryTests
{
    [Fact]
    public void RegisterAutomaticDetection_RejectsTwoShortcutsForTheSameGameMonitorTarget()
    {
        AutomaticGameDetectionRegistry registry = new AutomaticGameDetectionRegistry();

        AutomaticGameDetectionRegistrationResult firstResult = registry.RegisterAutomaticDetection(CreateAutomaticallyDetectedGameShortcut("shortcut-one", "570"));
        AutomaticGameDetectionRegistrationResult secondResult = registry.RegisterAutomaticDetection(CreateAutomaticallyDetectedGameShortcut("shortcut-two", "570"));

        Assert.Equal(AutomaticGameDetectionRegistrationResult.Registered, firstResult);
        Assert.Equal(AutomaticGameDetectionRegistrationResult.ConflictingGameMonitorTarget, secondResult);
        Assert.True(registry.IsAutomaticDetectionRegistered("shortcut-one"));
        Assert.False(registry.IsAutomaticDetectionRegistered("shortcut-two"));
    }

    [Fact]
    public void SuspendAutomaticDetectionForManualRun_RestoresDetectionAfterTheManualRun()
    {
        AutomaticGameDetectionRegistry registry = new AutomaticGameDetectionRegistry();
        ShortcutDefinition shortcut = CreateAutomaticallyDetectedGameShortcut("shortcut-one", "570");
        registry.RegisterAutomaticDetection(shortcut);

        bool wasSuspended = registry.SuspendAutomaticDetectionForManualRun(shortcut.Id);

        Assert.True(wasSuspended);
        Assert.False(registry.IsAutomaticDetectionRegistered(shortcut.Id));
        Assert.Equal(AutomaticGameDetectionRegistrationResult.Registered, registry.RestoreAutomaticDetectionAfterManualRun(shortcut.Id));
        Assert.True(registry.IsAutomaticDetectionRegistered(shortcut.Id));
    }

    [Fact]
    public void RegisterAutomaticDetection_RejectsShortcutsThatAreNotConfiguredForAutomaticDetection()
    {
        AutomaticGameDetectionRegistry registry = new AutomaticGameDetectionRegistry();
        ShortcutDefinition shortcut = CreateGameShortcut("shortcut-one", "570", GameLaunchMode.StartGame);

        Assert.Equal(AutomaticGameDetectionRegistrationResult.NotEligible, registry.RegisterAutomaticDetection(shortcut));
    }

    [Fact]
    public void ReplaceAutomaticDetections_RemovesRegistrationsNoLongerPresentInTheShortcutStore()
    {
        AutomaticGameDetectionRegistry registry = new AutomaticGameDetectionRegistry();
        registry.RegisterAutomaticDetection(CreateAutomaticallyDetectedGameShortcut("removed-shortcut", "570"));

        registry.ReplaceAutomaticDetections(new[] { CreateAutomaticallyDetectedGameShortcut("current-shortcut", "730") });

        Assert.False(registry.IsAutomaticDetectionRegistered("removed-shortcut"));
        Assert.True(registry.IsAutomaticDetectionRegistered("current-shortcut"));
    }

    [Fact]
    public void TryValidateAutomaticDetections_RejectsConflictingGameMonitorTargets()
    {
        bool isValid = AutomaticGameDetectionRegistry.TryValidateAutomaticDetections(
            new[]
            {
                CreateAutomaticallyDetectedGameShortcut("shortcut-one", "570"),
                CreateAutomaticallyDetectedGameShortcut("shortcut-two", "570")
            },
            out string validationError);

        Assert.False(isValid);
        Assert.Contains("shortcut-one", validationError);
    }

    private static ShortcutDefinition CreateAutomaticallyDetectedGameShortcut(string id, string gameAppId)
    {
        return CreateGameShortcut(id, gameAppId, GameLaunchMode.DetectGameRunning);
    }

    private static ShortcutDefinition CreateGameShortcut(string id, string gameAppId, GameLaunchMode gameLaunchMode)
    {
        return new ShortcutDefinition
        {
            Id = id,
            Category = ShortcutDefinitionCategory.Game,
            GameLibrary = 1,
            GameAppId = gameAppId,
            GameLaunchMode = gameLaunchMode
        };
    }
}
