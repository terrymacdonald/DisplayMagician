namespace DisplayMagician.Contracts;

public static class ControlServiceRetryPolicy
{
    public static bool ShouldRetryAfterStartingAgent(ControlResponse response)
    {
        return response != null && !response.IsSuccessful && response.ErrorCode == ControlErrorCode.AgentUnavailable;
    }
}
