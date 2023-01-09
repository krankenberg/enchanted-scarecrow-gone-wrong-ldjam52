using System;
using ldjam52.Game.Events;

namespace ldjam52.Game.Farmers;

public class FarmerSpawnedEvent : IEvent
{
    public Farmer Farmer;

    public static void Emit(Farmer farmer)
    {
        var farmerSpawnedEvent = new FarmerSpawnedEvent();
        farmerSpawnedEvent.Farmer = farmer;
        EventBus.EmitEvent(farmerSpawnedEvent);
    }

    public static void Listen(Action<FarmerSpawnedEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
