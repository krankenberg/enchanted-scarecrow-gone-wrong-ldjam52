using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Tutorial;

public class BarrierTutorialDoneEvent : IEvent
{
    public static void Emit()
    {
        EventBus.EmitEvent(new BarrierTutorialDoneEvent());
    }

    public static void Listen(Action<BarrierTutorialDoneEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
