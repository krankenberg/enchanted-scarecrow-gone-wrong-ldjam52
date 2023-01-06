using System;

namespace ldjam52.Game.Events
{
    public interface IEventHandler
    {
        public void ProcessEvent(IEvent evt);

        public Type GetEventType();
    }
}
