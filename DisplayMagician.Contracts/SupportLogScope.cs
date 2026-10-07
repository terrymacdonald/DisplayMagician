using System;
using NLog;

namespace DisplayMagician.Contracts;

/// <summary>Provides the shared asynchronous request and operation correlation scopes for component logs.</summary>
public static class SupportLogScope
{
    public static IDisposable BeginRequest(Guid requestId)
    {
        return ScopeContext.PushProperty("RequestId", requestId == Guid.Empty ? "-" : requestId);
    }

    public static IDisposable BeginOperation(Guid operationId)
    {
        return ScopeContext.PushProperty("OperationId", operationId == Guid.Empty ? "-" : operationId);
    }
}
