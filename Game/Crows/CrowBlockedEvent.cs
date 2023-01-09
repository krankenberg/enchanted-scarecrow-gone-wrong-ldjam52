using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Crows;

public class CrowBlockedEvent : IEvent
{
    public static void Emit()
    {
        EventBus.EmitEvent(new CrowBlockedEvent());
    }

    public static void Listen(Action<CrowBlockedEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
