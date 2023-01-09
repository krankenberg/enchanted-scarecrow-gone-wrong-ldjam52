using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Field.Crops;

public class CropEscapedEvent : IEvent
{
    public static void Emit()
    {
        EventBus.EmitEvent(new CropEscapedEvent());
    }

    public static void Listen(Action<CropEscapedEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
