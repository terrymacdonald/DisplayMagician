using System;

namespace DisplayMagician.Processes;

/// <summary>A process identity valid only for the lifetime of the current Windows session.</summary>
public readonly record struct ProcessIdentity(int ProcessId, DateTime StartTimeUtc);
