using System;
using ldjam52.Game.Events;
using ldjam52.Game.Field.Crops;

namespace ldjam52.Game.Field;

public class RequestCropEvent : IEvent
{
    public bool Consumed;
    public Action<Crop> Callback;
    public Action NoCropAvailableCallback;

    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<RequestCropEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
    
}
