using System;

namespace ldjam52.Game.Events
{
    public class EventHandler<T> : IEventHandler where T : class, IEvent
    {
        private readonly Action<T> _action;
        
        public EventHandler(Action<T> action)
        {
            _action = action;
        }

        public void ProcessEvent(IEvent evt)
        {
            _action.Invoke(evt as T);
        }

        public Type GetEventType()
        {
            return typeof(T);
        }
    }
}
