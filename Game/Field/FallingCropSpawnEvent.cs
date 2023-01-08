using System;
using Godot;
using ldjam52.Game.Events;

namespace ldjam52.Game.Field;

public class FallingCropSpawnEvent : IEvent
{
    public Vector2 Position;

    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<FallingCropSpawnEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
