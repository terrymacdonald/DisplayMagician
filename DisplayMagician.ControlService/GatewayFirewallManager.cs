using System;
using System.Diagnostics;
using System.IO;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Reconciles the installer-owned Gateway firewall rules with administrator-approved settings.</summary>
public sealed class GatewayFirewallManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string[] RuleNames =
    {
        "DisplayMagician Gateway (Private)",
        "DisplayMagician Gateway (Domain)",
        "DisplayMagician Gateway (Public)"
    };
    private static readonly string[] RuleProfiles = { "private", "domain", "public" };
    private readonly object _syncRoot = new object();
    private readonly GatewaySettingsStore _settingsStore;

    public GatewayFirewallManager(GatewaySettingsStore settingsStore)
    {
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
    }

    public void Reconcile()
    {
        lock (_syncRoot)
        {
            ApplyRules(_settingsStore.Get());
        }
    }

    public GatewaySettings UpdateAndApply(GatewaySettings settings)
    {
        lock (_syncRoot)
        {
            GatewaySettings previous = _settingsStore.Get();
            GatewaySettings updated = _settingsStore.Update(settings);
            try
            {
                ApplyRules(updated);
                return updated;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GatewayFirewallManager/UpdateAndApply: Could not update the Gateway firewall rules; restoring the previous configuration.");
                try
                {
                    _settingsStore.Update(previous);
                    ApplyRules(previous);
                }
                catch (Exception restoreError)
                {
                    Logger.Error(restoreError, "GatewayFirewallManager/UpdateAndApply: Could not fully restore the previous Gateway settings and firewall rules.");
                }

                throw;
            }
        }
    }

    private static void ApplyRules(GatewaySettings settings)
    {
        if (settings.LanPort is < 1 or > 65535)
        {
            throw new ArgumentException("The Gateway port is invalid.", nameof(settings));
        }

        for (int index = 0; index < RuleNames.Length; index++)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "netsh.exe"))
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add("advfirewall");
            startInfo.ArgumentList.Add("firewall");
            startInfo.ArgumentList.Add("set");
            startInfo.ArgumentList.Add("rule");
            startInfo.ArgumentList.Add($"name={RuleNames[index]}");
            startInfo.ArgumentList.Add("new");
            startInfo.ArgumentList.Add($"profile={RuleProfiles[index]}");
            startInfo.ArgumentList.Add("dir=in");
            startInfo.ArgumentList.Add("protocol=tcp");
            startInfo.ArgumentList.Add("action=allow");
            startInfo.ArgumentList.Add("remoteip=any");
            startInfo.ArgumentList.Add($"localport={settings.LanPort}");
            startInfo.ArgumentList.Add(index == 2 && !settings.AllowPublicNetworks ? "enable=no" : "enable=yes");

            using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Windows could not start the firewall configuration command.");
            if (!process.WaitForExit(3000))
            {
                process.Kill(entireProcessTree: true);
                throw new TimeoutException($"Updating the {RuleNames[index]} firewall rule timed out.");
            }

            string result = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Updating the {RuleNames[index]} firewall rule failed: {result.Trim()}");
            }
        }
    }
}
