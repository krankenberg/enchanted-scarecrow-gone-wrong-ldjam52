using System;
using Godot;
using ldjam52.Game.Events;

namespace ldjam52.Game.Field;

public class RequestCropEvent : IEvent
{
    public bool Consumed;
    public Action<Vector2> Callback;

    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<RequestCropEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
    
}
