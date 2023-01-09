using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Field.Crops;

public class LivingCropsOnFieldEvent : IEvent
{
    public bool LivingCropsAvailable;

    public static void Emit(bool livingCropsAvailable)
    {
        var livingCropsOnFieldEvent = new LivingCropsOnFieldEvent();
        livingCropsOnFieldEvent.LivingCropsAvailable = livingCropsAvailable;
        EventBus.EmitEvent(livingCropsOnFieldEvent);
    }

    public static void Listen(Action<LivingCropsOnFieldEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
