using System;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class PipeSecurityTests
{
    [Theory]
    [InlineData(typeof(ControlClientPipeServer))]
    [InlineData(typeof(NamedPipeControlServer))]
    public void CreatePipe_GrantsAuthenticatedUsersAndLocalServiceWithoutGrantingWorld(Type serverType)
    {
        MethodInfo createPipe = serverType.GetMethod("CreatePipe", BindingFlags.NonPublic | BindingFlags.Static)!;

        using NamedPipeServerStream pipe = (NamedPipeServerStream)createPipe.Invoke(null, null)!;
        AuthorizationRuleCollection rules = pipe.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
        SecurityIdentifier authenticatedUsers = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
        SecurityIdentifier localService = new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null);
        SecurityIdentifier world = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        Assert.Contains(rules.OfType<PipeAccessRule>(), rule =>
            rule.AccessControlType == AccessControlType.Allow &&
            rule.IdentityReference == authenticatedUsers &&
            (rule.PipeAccessRights & PipeAccessRights.ReadWrite) == PipeAccessRights.ReadWrite);
        Assert.Contains(rules.OfType<PipeAccessRule>(), rule =>
            rule.AccessControlType == AccessControlType.Allow &&
            rule.IdentityReference == localService &&
            (rule.PipeAccessRights & PipeAccessRights.FullControl) == PipeAccessRights.FullControl);
        Assert.DoesNotContain(rules.OfType<PipeAccessRule>(), rule =>
            rule.AccessControlType == AccessControlType.Allow && rule.IdentityReference == world);
    }
}