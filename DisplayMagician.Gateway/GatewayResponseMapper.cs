using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Http;

namespace DisplayMagician.Gateway;

/// <summary>Maps shared Control Service failures to HTTP status and Problem Details.</summary>
public static class GatewayResponseMapper
{
    public static IResult Map(HttpContext context, ControlResponse response)
    {
        if (response.IsSuccessful) return Results.Json(response);

        (int status, string code, string title) = response.ErrorCode switch
        {
            ControlErrorCode.Unauthorized or ControlErrorCode.AdministratorRequired => (StatusCodes.Status403Forbidden, "capability-denied", "Capability denied"),
            ControlErrorCode.ProfileNotFound or ControlErrorCode.AudioProfileNotFound or ControlErrorCode.ShortcutNotFound or ControlErrorCode.OperationNotFound => (StatusCodes.Status404NotFound, "resource-not-found", "Resource not found"),
            ControlErrorCode.DisplayControlBusy => (StatusCodes.Status409Conflict, "operation-busy", "Operation busy"),
            ControlErrorCode.DecisionUnavailable => (StatusCodes.Status409Conflict, "decision-already-resolved", "Decision unavailable"),
            ControlErrorCode.AgentUnavailable or ControlErrorCode.AgentNotConnected or ControlErrorCode.AgentNotHealthy => (StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Target unavailable"),
            ControlErrorCode.InvalidRequest or ControlErrorCode.ValidationFailed => (StatusCodes.Status400BadRequest, "validation-failed", "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "execution-failed", "Execution failed")
        };
        return GatewayProblemDetails.CreateResult(context, status, code, title, "The requested operation could not be completed.");
    }
}
