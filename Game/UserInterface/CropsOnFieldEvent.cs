using System;
using ldjam52.Game.Events;
using ldjam52.Game.Field;

namespace ldjam52.Game.UserInterface;

public class CropsOnFieldEvent : IEvent
{
    public bool CropsAvailable;
    
    public static void Emit(bool cropsAvailable)
    {
        var cropsOnFieldEvent = new CropsOnFieldEvent();
        cropsOnFieldEvent.CropsAvailable = cropsAvailable;
        EventBus.EmitEvent(cropsOnFieldEvent);
    }

    public static void Listen(Action<CropsOnFieldEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
