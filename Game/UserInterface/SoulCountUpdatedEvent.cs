using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.UserInterface;

public class SoulCountUpdatedEvent : IEvent
{
    public int SoulCount;
    
    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static IEventHandler Listen(Action<SoulCountUpdatedEvent> eventHandler)
    {
        return EventBus.Register(eventHandler);
    }
}
