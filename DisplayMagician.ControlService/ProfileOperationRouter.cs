using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

public sealed class ProfileOperationRouter
{
    private readonly ControlStateCoordinator _coordinator;
    private readonly AgentCommandClient _agentCommandClient;

    public ProfileOperationRouter(ControlStateCoordinator coordinator, AgentCommandClient agentCommandClient)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _agentCommandClient = agentCommandClient ?? throw new ArgumentNullException(nameof(agentCommandClient));
    }

    public Task<ControlResponse> ListProfilesAsync(string userSid, int sessionId, CancellationToken cancellationToken)
    {
        return SendToAgentAsync(userSid, sessionId, new ControlEnvelope { MessageType = ControlMessageType.ListProfiles }, cancellationToken);
    }

    public async Task<ControlResponse> ApplyProfileAsync(string userSid, int sessionId, string profileId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A display profile ID is required." };
        }

        LeaseDecision leaseDecision = _coordinator.TryAcquireDisplayControl(userSid, sessionId, ConsoleSessionLocator.GetActiveConsoleSessionId(), DateTime.UtcNow);
        if (!leaseDecision.IsGranted)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = leaseDecision.ErrorCode, Message = leaseDecision.Message, LeaseDecision = leaseDecision };
        }

        return await SendToAgentAsync(userSid, sessionId, new ControlEnvelope
        {
            MessageType = ControlMessageType.ApplyProfile,
            Payload = JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = profileId })
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ControlResponse> SendToAgentAsync(string userSid, int sessionId, ControlEnvelope command, CancellationToken cancellationToken)
    {
        AgentRegistration? agent = _coordinator.GetAgentRegistration(userSid, sessionId);
        if (agent == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent is not connected for this session." };
        }

        try
        {
            return await _agentCommandClient.SendAsync(agent, command, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.IO.IOException || ex is TimeoutException)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent command endpoint is unavailable." };
        }
    }
}
