using System;
using ldjam52.Game.Events;
using ldjam52.Game.Field.Crops;

namespace ldjam52.Game.Field;

public class CropDropEvent : IEvent
{
    public Crop Crop;

    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<CropDropEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
