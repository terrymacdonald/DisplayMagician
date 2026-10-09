using System;
using DisplayMagician.Gateway;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayTrafficLimiterTests
{
    [Fact]
    public void PerDeviceMutationLimitResetsAfterOneMinute()
    {
        GatewayTrafficLimiter limiter = new GatewayTrafficLimiter();
        DateTime now = DateTime.UtcNow;
        for (int index = 0; index < 10; index++) Assert.True(limiter.TryConsumeDeviceRequest("phone", true, now));
        Assert.False(limiter.TryConsumeDeviceRequest("phone", true, now));
        Assert.True(limiter.TryConsumeDeviceRequest("another-phone", true, now));
        Assert.True(limiter.TryConsumeDeviceRequest("phone", true, now.AddMinutes(1)));
    }

    [Fact]
    public void FailedAuthenticationLimitBlocksSourceTemporarily()
    {
        GatewayTrafficLimiter limiter = new GatewayTrafficLimiter();
        DateTime now = DateTime.UtcNow;
        for (int index = 0; index < 20; index++) limiter.RecordFailedAuthentication("192.0.2.1", now);
        Assert.True(limiter.IsSourceBlocked("192.0.2.1", now));
        Assert.False(limiter.IsSourceBlocked("192.0.2.2", now));
        Assert.False(limiter.IsSourceBlocked("192.0.2.1", now.AddMinutes(1)));
    }
}
