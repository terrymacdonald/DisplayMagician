using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DisplayMagician.Contracts;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.UserAgent.Runtime;
using AgentConfigMigrationRunner = DisplayMagician.UserAgent.Runtime.ConfigMigrationRunner;
using DisplayMagician.UserAgent.Runtime.Intel;
using IGCLWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class IntelIdentityMigrationTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-IntelIdentity", Guid.NewGuid().ToString("N"));

    [Fact]
    public void IdenticalCardsAndTheirPortsHaveDistinctStableKeys()
    {
        DeviceAdapterPropertiesDto first = Properties(1);
        DeviceAdapterPropertiesDto second = Properties(2);
        Assert.Equal(IntelAdapterIdentity.LegacyKey(first), IntelAdapterIdentity.LegacyKey(second));
        Assert.NotEqual(IntelAdapterIdentity.Key(first), IntelAdapterIdentity.Key(second));
        Assert.NotEqual(IntelAdapterIdentity.DisplayKey(first, 1), IntelAdapterIdentity.DisplayKey(second, 1));
        first.DriverVersion++;
        first.Name = "Changed driver name";
        Assert.Equal(IntelAdapterIdentity.Key(Properties(1)), IntelAdapterIdentity.Key(first));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    [InlineData("4")]
    public void LegacyProfileMigratesAdapterAndDisplayTogetherWithoutLosingSettings(string version)
    {
        JObject root = Document(version);
        string original = root.ToString();
        string migrated = AgentConfigMigrationRunner.MigrateDisplayProfilesJson(original);
        JObject result = JObject.Parse(migrated);
        JObject config = (JObject)result["Profiles"]![0]!["IntelDisplayConfig"]!;
        string key = IntelAdapterIdentity.Key(Properties(1));
        Assert.Equal("5", result["ProfileFileVersion"]);
        Assert.True((bool)config["PhysicalAdapters"]![key]!["IsCombinedDisplay"]!);
        Assert.Equal(key, config["PhysicalAdapters"]![key]!["AdapterID"]);
        Assert.Equal("keep adapter", config["PhysicalAdapters"]![key]!["FutureSetting"]);
        Assert.Equal("keep display", config["Displays"]![key + "-PORT_7"]!["FutureSetting"]);
        Assert.Equal(key, config["Displays"]![key + "-PORT_7"]!["DeviceID"]);
        Assert.Equal("keep root", result["FutureSetting"]);
        Assert.Equal(migrated, AgentConfigMigrationRunner.MigrateDisplayProfilesJson(migrated));
    }

    [Fact]
    public void MigrationBacksUpOriginalAndDoesNotRewriteOnSecondRead()
    {
        Directory.CreateDirectory(_root);
        string path = Path.Combine(_root, "DisplayProfiles.json");
        File.WriteAllText(path, Document("4").ToString(), Encoding.Unicode);
        byte[] before = File.ReadAllBytes(path);
        AgentConfigMigrationRunner.MigrateDisplayProfilesFile(path);
        string backup = Assert.Single(Directory.GetFiles(_root, "*.identity-migration.*.bak"));
        Assert.Equal(before, File.ReadAllBytes(backup));
        byte[] migrated = File.ReadAllBytes(path);
        AgentConfigMigrationRunner.MigrateDisplayProfilesFile(path);
        Assert.Equal(migrated, File.ReadAllBytes(path));
        Assert.Equal(backup, Assert.Single(Directory.GetFiles(_root, "*.identity-migration.*.bak")));
    }

    [Theory]
    [InlineData("\"6\"")]
    [InlineData("\"0\"")]
    [InlineData("\"unknown\"")]
    [InlineData("5")]
    [InlineData("true")]
    [InlineData("{}")]
    public void UnsupportedVersionsPreserveOriginalAndExistingBackup(string version)
    {
        Directory.CreateDirectory(_root);
        string path = Path.Combine(_root, "DisplayProfiles.json");
        string json = "{\"ProfileFileVersion\":" + version + ",\"Profiles\":[]}";
        File.WriteAllText(path, json, Encoding.Unicode);
        File.WriteAllText(path + ".bak", "previous backup");
        byte[] before = File.ReadAllBytes(path);
        Assert.Throws<JsonSerializationException>(() => AgentConfigMigrationRunner.MigrateDisplayProfilesFile(path));
        Assert.Equal(before, File.ReadAllBytes(path));
        Assert.Equal("previous backup", File.ReadAllText(path + ".bak"));
        Assert.Equal(2, Directory.GetFiles(_root).Length);
        Assert.Throws<System.Text.Json.JsonException>(() => RepositoryDocumentValidation.Validate(json, RepositoryKind.DisplayProfiles));
    }

    [Fact]
    public void MissingLocationIsPreservedRatherThanInventedAsZero()
    {
        JObject root = Document("4");
        JObject adapters = (JObject)root["Profiles"]![0]!["IntelDisplayConfig"]!["PhysicalAdapters"]!;
        JObject adapter = (JObject)adapters.Properties().Single().Value;
        ((JObject)adapter["AdapterProperties"]!).Remove("AdapterBdf");
        JObject result = JObject.Parse(AgentConfigMigrationRunner.MigrateDisplayProfilesJson(root.ToString()));
        Assert.NotNull(result["Profiles"]![0]!["IntelDisplayConfig"]!["PhysicalAdapters"]![IntelAdapterIdentity.LegacyKey(Properties(1))]);
    }

    [Fact]
    public void CollidingMigrationDoesNotOverwriteEitherSavedEntry()
    {
        JObject root = Document("4");
        JObject adapters = (JObject)root["Profiles"]![0]!["IntelDisplayConfig"]!["PhysicalAdapters"]!;
        adapters[IntelAdapterIdentity.Key(Properties(1))] = adapters.Properties().Single().Value.DeepClone();
        Assert.Throws<JsonSerializationException>(() => AgentConfigMigrationRunner.MigrateDisplayProfilesJson(root.ToString()));
    }

    [Fact]
    public void RecoverySnapshotUsesSameIdentityMigration()
    {
        string profile = Document("4")["Profiles"]![0]!.ToString();
        JObject result = JObject.Parse(AgentConfigMigrationRunner.MigrateDisplayRecoveryJson(profile));
        Assert.NotNull(result["IntelDisplayConfig"]!["PhysicalAdapters"]![IntelAdapterIdentity.Key(Properties(1))]);
    }

    [Fact]
    public void LegacyIdentityRequiresRecaptureBeforeAnyNativeWrites()
    {
        INTEL_DISPLAY_CONFIG config = new() { IsInUse = true };
        config.PhysicalAdapters.Add(IntelAdapterIdentity.LegacyKey(Properties(1)), new INTEL_ADAPTER());
        ProfileItem profile = new() { IntelDisplayConfig = config };
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(profile.SetActiveWithDiagnostics(diagnostics));
        DisplayConfigurationIssue issue = Assert.Single(diagnostics.Result.Issues);
        Assert.Equal("AdapterIdentity", issue.Stage);
        Assert.Contains("Recapture", issue.Message);
        Assert.Equal(DisplayConfigurationSeverity.Fatal, issue.Severity);
    }

    [Fact]
    public void MigratedIdentityIsEligibleWithoutConsultingHardware()
    {
        INTEL_DISPLAY_CONFIG config = new() { IsInUse = true };
        config.PhysicalAdapters.Add(IntelAdapterIdentity.Key(Properties(1)), new INTEL_ADAPTER());
        Assert.True(IntelAdapterIdentity.ValidateForApply(config, new DisplayConfigurationDiagnostics()));
    }

    private static DeviceAdapterPropertiesDto Properties(byte bus) => new()
    {
        PciVendorId = 0x8086, PciDeviceId = 0x1234, PciSubsysVendorId = 0x8086, PciSubsysId = 1,
        AdapterBdf = new AdapterBdfDto { Bus = bus, Device = 0, Function = 0 }
    };

    private static JObject Document(string version)
    {
        DeviceAdapterPropertiesDto properties = Properties(1);
        string old = IntelAdapterIdentity.LegacyKey(properties);
        return new JObject
        {
            ["ProfileFileVersion"] = version, ["FutureSetting"] = "keep root",
            ["Profiles"] = new JArray(new JObject
            {
                ["UUID"] = Guid.NewGuid().ToString("D"), ["IntelDisplayConfig"] = new JObject
                {
                    ["CombinedDisplayIsInUse"] = true,
                    ["PhysicalAdapters"] = new JObject { [old] = new JObject
                    {
                        ["AdapterID"] = old, ["AdapterProperties"] = JObject.FromObject(properties),
                        ["IsCombinedDisplay"] = true, ["FutureSetting"] = "keep adapter"
                    } },
                    ["Displays"] = new JObject { ["VEN_8086&DEV_1234&REV_00-PORT_7"] = new JObject
                    {
                        ["DeviceID"] = old, ["DisplayDeviceID"] = "VEN_8086&DEV_1234&REV_00-PORT_7", ["FutureSetting"] = "keep display"
                    } }
                }
            })
        };
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
