using System;
using ldjam52.Game.Events;
using ldjam52.Game.Field.Crops;

namespace ldjam52.Game.Field;

public class CropLandedEvent : IEvent
{
    public Crop Crop;

    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<CropLandedEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
