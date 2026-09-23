using System;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ControlClientEventHubTests
{
    [Fact]
    public async Task Publish_ClosesAnOverloadedSubscriberSoItMustResynchronise()
    {
        ControlClientEventHub hub = new ControlClientEventHub();
        using ControlClientEventSubscription subscription = hub.Subscribe("S-1-5-21-100", 10);
        ControlClientEvent clientEvent = new ControlClientEvent { EventType = ControlClientEventType.OperationStatusUpdated, PublishedUtc = DateTime.UtcNow };

        for (int index = 0; index < 33; index++)
        {
            hub.Publish("S-1-5-21-100", 11, clientEvent);
        }

        for (int index = 0; index < 32; index++)
        {
            Assert.True(subscription.Reader.TryRead(out _));
        }

        Assert.False(await subscription.Reader.WaitToReadAsync());
    }

    [Fact]
    public void Publish_DeliversToAnotherAuthorisedSessionForTheSameUser()
    {
        ControlClientEventHub hub = new ControlClientEventHub();
        using ControlClientEventSubscription subscription = hub.Subscribe("S-1-5-21-100", 10);
        ControlClientEvent clientEvent = new ControlClientEvent { EventType = ControlClientEventType.OperationDecisionUpdated, PublishedUtc = DateTime.UtcNow };

        hub.Publish("S-1-5-21-100", 11, clientEvent);

        Assert.True(subscription.Reader.TryRead(out ControlClientEvent? delivered));
        Assert.Same(clientEvent, delivered);
    }
}
