using System;
using ldjam52.Game.Crows;
using ldjam52.Game.Events;

namespace ldjam52.Game.Tutorial;

public class SpawnTutorialCrowEvent : IEvent
{
    public Action<Crow> Callback;

    public static void Emit(Action<Crow> callback)
    {
        var spawnTutorialCrow = new SpawnTutorialCrowEvent();
        spawnTutorialCrow.Callback = callback;
        EventBus.EmitEvent(spawnTutorialCrow);
    }

    public static void Listen(Action<SpawnTutorialCrowEvent> eventHandler)
    {
        EventBus.Register(eventHandler);
    }
}
