using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Scarecrow.Spells;

public class UseSoulsEvent : IEvent
{
    public int Amount;
    public Action<bool> Callback;
    
    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<UseSoulsEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
