using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Cutscene;

public class CutsceneEndedEvent : IEvent
{
    public static void Emit()
    {
        EventBus.EmitEvent(new CutsceneEndedEvent());
    }

    public static void Listen(Action<CutsceneEndedEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
