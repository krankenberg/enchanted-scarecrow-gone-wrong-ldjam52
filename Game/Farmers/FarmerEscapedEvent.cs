using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Farmers;

public class FarmerEscapedEvent : IEvent
{
    public static void Emit()
    {
        EventBus.EmitEvent(new FarmerEscapedEvent());
    }

    public static void Listen(Action<FarmerEscapedEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
