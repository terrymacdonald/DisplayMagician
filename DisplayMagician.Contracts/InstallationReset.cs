namespace DisplayMagician.Contracts;

/// <summary>Local administrator-only maintenance protocol; never offered through the remote Gateway.</summary>
public static class InstallationResetProtocol
{
    public const string CommandLineOption = "--reset-installation";
    public const string MaintenanceCommandLineOption = "--installation-reset-pipe";
    public const string PipePrefix = "DisplayMagician.InstallationReset.";
    public const string Capability = "installation-reset";
    public const string EndpointKind = "ControlServiceReset";
    public const string MaintenanceLockName = "Global\\DisplayMagician.InstallationReset";
}

public sealed class InstallationResetRequest
{
    public bool Confirmed { get; set; }
}
