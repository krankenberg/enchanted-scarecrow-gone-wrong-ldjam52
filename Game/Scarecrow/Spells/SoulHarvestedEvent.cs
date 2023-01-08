using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Scarecrow.Spells;

public class SoulHarvestedEvent : IEvent
{
    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<SoulHarvestedEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
