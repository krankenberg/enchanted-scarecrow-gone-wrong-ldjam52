using System;
using ldjam52.Game.Events;
using ldjam52.Game.Field.Crops;

namespace ldjam52.Game.Tutorial;

public class SpawnTutorialCropEvent : IEvent
{
    public Action<Crop> Callback;

    public static void Emit(Action<Crop> callback)
    {
        var spawnTutorialCrop = new SpawnTutorialCropEvent();
        spawnTutorialCrop.Callback = callback;
        EventBus.EmitEvent(spawnTutorialCrop);
    }

    public static void Listen(Action<SpawnTutorialCropEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
