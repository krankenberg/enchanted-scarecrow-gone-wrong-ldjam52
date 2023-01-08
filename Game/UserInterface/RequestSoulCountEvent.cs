using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.UserInterface;

public class RequestSoulCountEvent : IEvent
{
    public Action<int> Callback;

    public void Emit()
    {
        EventBus.EmitEvent(this);
    }

    public static void Listen(Action<RequestSoulCountEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
