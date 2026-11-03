using System;
using System.Collections.Generic;
using System.Linq;

namespace DisplayMagician.UserAgent.Runtime;

/// <summary>One completed capture for client views, including connectivity advisories.</summary>
internal sealed class DisplayDetectionSnapshot
{
    internal ProfileItem? Profile { get; }
    internal IReadOnlyList<string> ConnectedIdentifiers { get; }
    internal IReadOnlyList<string> IntelConnectedIdentifiers { get; }

    internal DisplayDetectionSnapshot(ProfileItem? profile, IEnumerable<string> identifiers)
    {
        Profile = profile;
        string[] copy = identifiers.ToArray();
        ConnectedIdentifiers = Array.AsReadOnly(copy);
        IntelConnectedIdentifiers = Array.AsReadOnly(copy.Where(id => id.StartsWith("IntelIGCL#", StringComparison.OrdinalIgnoreCase)).ToArray());
    }
}
