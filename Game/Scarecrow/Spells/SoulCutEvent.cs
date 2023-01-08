using System;
using Godot;
using ldjam52.Game.Events;

namespace ldjam52.Game.Scarecrow.Spells;

public class SoulCutEvent : IEvent
{
    public Vector2 Start;
    public Vector2 End;

    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static IEventHandler Listen(Action<SoulCutEvent> eventHandler)
    {
        return EventBus.Register(eventHandler);
    }
}
