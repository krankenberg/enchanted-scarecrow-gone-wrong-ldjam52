using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Crows;

public class CrowsOnFieldEvent : IEvent
{
    public bool CrowsAvailable;

    public static void Emit(bool crowsAvailable)
    {
        var crowsOnFieldEvent = new CrowsOnFieldEvent();
        crowsOnFieldEvent.CrowsAvailable = crowsAvailable;
        EventBus.EmitEvent(crowsOnFieldEvent);
    }

    public static void Listen(Action<CrowsOnFieldEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
