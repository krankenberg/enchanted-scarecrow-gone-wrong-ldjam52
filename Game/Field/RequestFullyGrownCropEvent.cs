using System;
using ldjam52.Game.Events;
using ldjam52.Game.Field.Crops;

namespace ldjam52.Game.Field;

public class RequestFullyGrownCropEvent : IEvent
{
    public bool Consumed;
    public Action<Crop> Callback;

    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<RequestFullyGrownCropEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
