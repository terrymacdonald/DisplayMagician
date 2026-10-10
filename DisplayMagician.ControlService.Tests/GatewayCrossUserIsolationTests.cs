using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayCrossUserIsolationTests
{
    [Fact]
    public void PairedCredentialCanReadOnlyItsOwnersStatusAndDecisions()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            DateTime now = DateTime.UtcNow;
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository clients = new PairedClientRepository(paths);
            string firstCredential = new string('A', 43);
            string secondCredential = new string('B', 43);
            clients.Upsert(new PairedClient { DeviceId = "first-phone", OwnerUserSid = "S-1-5-21-100", PreferredSessionId = 10,
                CredentialHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(firstCredential))), GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead, RemoteClientCapabilities.ProfilesRead, RemoteClientCapabilities.DecisionsRead, RemoteClientCapabilities.DevicesRevoke } });
            clients.Upsert(new PairedClient { DeviceId = "second-phone", OwnerUserSid = "S-1-5-21-200", PreferredSessionId = 20,
                CredentialHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secondCredential))), GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead } });
            GatewayRequestAuthenticator authenticator = new GatewayRequestAuthenticator(clients);
            ControlStateCoordinator coordinator = new ControlStateCoordinator();
            coordinator.RegisterAgent(new AgentRegistration { UserSid = "S-1-5-21-100", SessionId = 10, ProcessId = 1000, IsReady = true }, now);
            coordinator.RegisterAgent(new AgentRegistration { UserSid = "S-1-5-21-200", SessionId = 20, ProcessId = 2000, IsReady = true }, now);
            OperationStatusStore statuses = new OperationStatusStore(paths);
            OperationDecisionStore decisions = new OperationDecisionStore(paths);
            Guid firstOperation = Guid.NewGuid();
            Guid secondOperation = Guid.NewGuid();
            statuses.Publish("S-1-5-21-100", 10, new OperationStatusUpdate { OperationId = firstOperation, Message = "First user's status" }, now);
            statuses.Publish("S-1-5-21-200", 20, new OperationStatusUpdate { OperationId = secondOperation, Message = "Second user's status" }, now);
            decisions.Create("S-1-5-21-100", 10, firstOperation, "First decision", "First", new[] { OperationDecisionChoice.Continue }, OperationDecisionChoice.Continue, now.AddMinutes(10), now);
            decisions.Create("S-1-5-21-200", 20, secondOperation, "Second decision", "Second", new[] { OperationDecisionChoice.Continue }, OperationDecisionChoice.Continue, now.AddMinutes(10), now);
            UnusedAgentCommandClient commandClient = new UnusedAgentCommandClient();
            GatewayPairingPipeServer server = new GatewayPairingPipeServer(new DevicePairingCoordinator(paths, clients), new GatewayIdentityRegistry(), authenticator,
                new GatewayHttpIdempotencyStore(paths), statuses, decisions, new ProfileOperationRouter(coordinator, commandClient), coordinator);
            GatewayAuthenticationResult firstAuthentication = authenticator.Authenticate(new GatewayAuthenticationRequest { BearerCredential = firstCredential }, now);
            GatewayAuthenticationResult secondAuthentication = authenticator.Authenticate(new GatewayAuthenticationRequest { BearerCredential = secondCredential }, now);

            ControlResponse first = ReadStatus(server, firstAuthentication, null);
            Assert.True(first.IsSuccessful);
            Assert.Equal(firstOperation, Assert.Single(first.RemoteUserStatus!.Operations).OperationId);
            Assert.Equal(firstOperation, Assert.Single(first.RemoteUserStatus.PendingDecisions).OperationId);

            ControlResponse crossUser = ReadStatus(server, firstAuthentication, 20);
            Assert.False(crossUser.IsSuccessful);
            Assert.Equal(ControlErrorCode.AgentUnavailable, crossUser.ErrorCode);
            Assert.Null(crossUser.RemoteUserStatus);

            MethodInfo listMethod = typeof(GatewayPairingPipeServer).GetMethod("ListRemote", BindingFlags.Instance | BindingFlags.NonPublic)!;
            ControlEnvelope listRequest = new ControlEnvelope { Payload = JsonSerializer.Serialize(new GatewayRemoteCommand { Authentication = firstAuthentication, TargetSessionId = 20 }) };
            ControlResponse crossUserList = (ControlResponse)listMethod.Invoke(server, new object[] { listRequest, RemoteClientCapabilities.ProfilesRead, ControlMessageType.ListProfiles })!;
            Assert.False(crossUserList.IsSuccessful);
            Assert.Equal(ControlErrorCode.AgentUnavailable, crossUserList.ErrorCode);
            Assert.False(commandClient.WasCalled);

            ControlResponse second = ReadStatus(server, secondAuthentication, null);
            Assert.True(second.IsSuccessful);
            Assert.Equal(secondOperation, Assert.Single(second.RemoteUserStatus!.Operations).OperationId);
            Assert.Equal(secondOperation, Assert.Single(second.RemoteUserStatus.PendingDecisions).OperationId);

            ControlResponse ownOperation = ReadResource(server, "GetRemoteOperation", firstAuthentication, firstOperation.ToString("D"));
            ControlResponse otherOperation = ReadResource(server, "GetRemoteOperation", firstAuthentication, secondOperation.ToString("D"));
            Assert.Equal(firstOperation, ownOperation.OperationStatus?.OperationId);
            Assert.Equal(ControlErrorCode.OperationNotFound, otherOperation.ErrorCode);

            ControlResponse ownDecisions = ReadResource(server, "ListRemoteDecisions", firstAuthentication, firstOperation.ToString("D"));
            ControlResponse otherDecisions = ReadResource(server, "ListRemoteDecisions", firstAuthentication, secondOperation.ToString("D"));
            Assert.Single(ownDecisions.OperationDecisions);
            Assert.Equal(ControlErrorCode.OperationNotFound, otherDecisions.ErrorCode);

            ControlResponse currentDevice = ReadResource(server, "GetRemoteCurrentDevice", firstAuthentication, string.Empty);
            Assert.Equal("first-phone", Assert.Single(currentDevice.PairedClients).DeviceId);

            ControlResponse crossUserRevocation = ReadResource(server, "RevokeRemoteDevice", firstAuthentication, "second-phone");
            Assert.Equal(ControlErrorCode.ResourceNotFound, crossUserRevocation.ErrorCode);
            Assert.NotNull(clients.FindActiveByCredential(secondCredential));

            clients.Revoke("S-1-5-21-100", "first-phone", now.AddMinutes(1));
            ControlResponse revokedCurrentDevice = ReadResource(server, "GetRemoteCurrentDevice", firstAuthentication, string.Empty);
            Assert.Equal(ControlErrorCode.PairingRequired, revokedCurrentDevice.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static ControlResponse ReadStatus(GatewayPairingPipeServer server, GatewayAuthenticationResult authentication, int? targetSessionId)
    {
        MethodInfo method = typeof(GatewayPairingPipeServer).GetMethod("GetRemoteUserStatus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        ControlEnvelope request = new ControlEnvelope { Payload = JsonSerializer.Serialize(new GatewayRemoteStatusRequest { Authentication = authentication, TargetSessionId = targetSessionId }) };
        return (ControlResponse)method.Invoke(server, new object[] { request })!;
    }

    private static ControlResponse ReadResource(GatewayPairingPipeServer server, string methodName, GatewayAuthenticationResult authentication, string payload)
    {
        MethodInfo method = typeof(GatewayPairingPipeServer).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        ControlEnvelope request = new ControlEnvelope { Payload = JsonSerializer.Serialize(new GatewayRemoteCommand { Authentication = authentication, Payload = payload }) };
        return (ControlResponse)method.Invoke(server, new object[] { request })!;
    }

    private sealed class UnusedAgentCommandClient : IAgentCommandClient
    {
        public bool WasCalled { get; private set; }

        public Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            throw new InvalidOperationException("A cross-user request must not send an Agent command.");
        }
    }
}
