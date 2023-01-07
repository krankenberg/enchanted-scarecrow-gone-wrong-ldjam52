using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.UserInterface;

public class GameOverEvent : IEvent
{
    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<GameOverEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
