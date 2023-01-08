using System;
using Godot;
using ldjam52.Game.Events;

namespace ldjam52.Game.Scarecrow.Spells;

public class SpawnLivingCropEvent : IEvent
{
    public Vector2 Position;
    
    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<SpawnLivingCropEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
