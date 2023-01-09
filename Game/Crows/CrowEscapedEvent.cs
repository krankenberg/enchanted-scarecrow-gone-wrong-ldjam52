using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Crows;

public class CrowEscapedEvent : IEvent
{
    public static void Emit()
    {
        EventBus.EmitEvent(new CrowEscapedEvent());
    }

    public static void Listen(Action<CrowEscapedEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
