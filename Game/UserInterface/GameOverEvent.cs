using System;
using ldjam52.Game.Events;
using ldjam52.Game.Field;

namespace ldjam52.Game.UserInterface;

public class GameOverEvent : IEvent
{
    public static void Emit()
    {
        EventBus.EmitEvent(new GameOverEvent());
    }

    public static void Listen(Action<GameOverEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
