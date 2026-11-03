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
    [InlineData("5")]
    public void LegacyProfileMigratesAdapterAndDisplayTogetherWithoutLosingSettings(string version)
    {
        JObject root = Document(version);
        string original = root.ToString();
        string migrated = AgentConfigMigrationRunner.MigrateDisplayProfilesJson(original);
        JObject result = JObject.Parse(migrated);
        JObject config = (JObject)result["Profiles"]![0]!["IntelDisplayConfig"]!;
        string key = IntelAdapterIdentity.Key(Properties(1));
        Assert.Equal("6", result["ProfileFileVersion"]);
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
    [InlineData("\"7\"")]
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

    [Fact]
    public void MissingLocationRemainsUnknownAcrossRepositoryAndRecoveryRoundTrips()
    {
        JObject root = Document("5");
        JObject profile = (JObject)root["Profiles"]![0]!;
        JObject config = (JObject)profile["IntelDisplayConfig"]!;
        config["IsInUse"] = true;
        JObject defaults = JObject.Parse(DisplayMagician.UserAgent.UserProfileOperationService.SerializeDisplayRecoveryProfile(new ProfileItem()));
        foreach (JProperty property in defaults.Properties())
            if (profile[property.Name] == null) profile[property.Name] = property.Value.DeepClone();
        JObject adapter = (JObject)((JObject)config["PhysicalAdapters"]!).Properties().Single().Value;
        ((JObject)adapter["AdapterProperties"]!).Remove("AdapterBdf");
        string json = AgentConfigMigrationRunner.MigrateDisplayProfilesJson(root.ToString());
        for (int index = 0; index < 3; index++)
        {
            ProfileItem loaded = JsonConvert.DeserializeObject<ProfileItem>(JObject.Parse(json)["Profiles"]![0]!.ToString(),
                DisplayMagician.UserAgent.UserProfileOperationService.RecoveryJsonSettings)!;
            Assert.True(IntelAdapterIdentity.RequiresRecapture(loaded.IntelDisplayConfig));
            string recovery = AgentConfigMigrationRunner.MigrateDisplayRecoveryJson(
                DisplayMagician.UserAgent.UserProfileOperationService.SerializeDisplayRecoveryProfile(loaded));
            ProfileItem restored = JsonConvert.DeserializeObject<ProfileItem>(recovery)!;
            Assert.True(IntelAdapterIdentity.RequiresRecapture(restored.IntelDisplayConfig));
            Assert.DoesNotContain("-PCI_", Assert.Single(restored.IntelDisplayConfig.PhysicalAdapters).Key);
            json = AgentConfigMigrationRunner.MigrateDisplayProfilesJson(new JObject
            {
                ["ProfileFileVersion"] = "6", ["Profiles"] = new JArray(JObject.Parse(recovery))
            }.ToString());
        }
    }

    [Fact]
    public void KnownZeroPciLocationIsNotConfusedWithAnUnknownLocation()
    {
        JObject root = Document("5");
        JObject adapter = (JObject)((JObject)root["Profiles"]![0]!["IntelDisplayConfig"]!["PhysicalAdapters"]!).Properties().Single().Value;
        adapter["AdapterProperties"]!["AdapterBdf"]!["Bus"] = 0;
        JObject migrated = JObject.Parse(AgentConfigMigrationRunner.MigrateDisplayProfilesJson(root.ToString()));
        Assert.Null(migrated["Profiles"]![0]!["IntelDisplayConfig"]!["IdentityRequiresRecapture"]);
        Assert.Contains("-PCI_00_00_00", ((JObject)migrated["Profiles"]![0]!["IntelDisplayConfig"]!["PhysicalAdapters"]!).Properties().Single().Name);
    }

    [Fact]
    public void ConnectivityIdentifiersMigrateWithTheOwningAdapterAndKeepThePort()
    {
        JObject root = Document("5");
        JObject profile = (JObject)root["Profiles"]![0]!;
        JObject config = (JObject)profile["IntelDisplayConfig"]!;
        JObject adapter = (JObject)((JObject)config["PhysicalAdapters"]!).Properties().Single().Value;
        adapter["Name"] = "Intel card";
        const string identifier = "IntelIGCL#Intel card#4660#1#DP#mux#output#converter#EDID#product#7";
        config["DisplayIdentifiers"] = new JArray(identifier);
        profile["ProfileDisplayIdentifiers"] = new JArray(identifier, "WINAPI|unchanged");
        JObject migrated = JObject.Parse(AgentConfigMigrationRunner.MigrateDisplayProfilesJson(root.ToString()));
        string expected = IntelAdapterIdentity.ScopeIdentifier(identifier, IntelAdapterIdentity.Key(Properties(1)));
        Assert.Equal(expected, migrated["Profiles"]![0]!["IntelDisplayConfig"]!["DisplayIdentifiers"]![0]);
        Assert.Equal(expected, migrated["Profiles"]![0]!["ProfileDisplayIdentifiers"]![0]);
        Assert.Equal("WINAPI|unchanged", migrated["Profiles"]![0]!["ProfileDisplayIdentifiers"]![1]);
        Assert.True(IntelAdapterIdentity.TryGetOutput(expected, out string owner, out uint encoder));
        Assert.Equal(IntelAdapterIdentity.Key(Properties(1)), owner);
        Assert.Equal(7u, encoder);
    }

    [Fact]
    public void AmbiguousLegacyConnectivityRequiresRecaptureInsteadOfGuessing()
    {
        JObject root = Document("5");
        JObject config = (JObject)root["Profiles"]![0]!["IntelDisplayConfig"]!;
        JObject adapters = (JObject)config["PhysicalAdapters"]!;
        JObject adapter = (JObject)adapters.Properties().Single().Value;
        adapter["Name"] = "Intel card";
        JObject second = (JObject)adapter.DeepClone();
        second["AdapterProperties"]!["AdapterBdf"]!["Bus"] = 2;
        adapters[IntelAdapterIdentity.Key(Properties(2))] = second;
        config["DisplayIdentifiers"] = new JArray("IntelIGCL#Intel card#4660#1#DP#mux#output#converter#EDID#product#7");
        JObject migrated = JObject.Parse(AgentConfigMigrationRunner.MigrateDisplayProfilesJson(root.ToString()));
        Assert.True((bool)migrated["Profiles"]![0]!["IntelDisplayConfig"]!["IdentityRequiresRecapture"]!);
    }

    [Fact]
    public void ExistingCapturedKeyIsNotReplacedByDefaultedPropertyData()
    {
        JObject root = Document("5");
        JObject config = (JObject)root["Profiles"]![0]!["IntelDisplayConfig"]!;
        JObject adapters = (JObject)config["PhysicalAdapters"]!;
        JObject adapter = (JObject)adapters.Properties().Single().Value;
        adapters.RemoveAll();
        string key = IntelAdapterIdentity.Key(Properties(1));
        adapters[key] = adapter;
        adapter["AdapterProperties"]!["AdapterBdf"]!["Bus"] = 0;
        JObject migrated = JObject.Parse(AgentConfigMigrationRunner.MigrateDisplayProfilesJson(root.ToString()));
        Assert.NotNull(migrated["Profiles"]![0]!["IntelDisplayConfig"]!["PhysicalAdapters"]![key]);
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
