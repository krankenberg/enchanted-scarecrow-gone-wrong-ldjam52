using System;
using System.Collections.Generic;
using Godot;

namespace ldjam52.Game.Events
{
    public class EventBus
    {
        private static EventBus Instance => _instance ??= new EventBus();

        private static EventBus _instance;

        private readonly Dictionary<Type, List<IEventHandler>> _eventHandler;

        private readonly Stack<IEventHandler> _eventHandlersToBeDeleted;

        private bool _duringEventProcessing;

        private EventBus()
        {
            _eventHandler = new Dictionary<Type, List<IEventHandler>>();
            _eventHandlersToBeDeleted = new Stack<IEventHandler>();
        }

        private void InternalEmitEvent(IEvent evt)
        {
            _duringEventProcessing = true;

            var eventType = evt.GetType();
            if (_eventHandler.ContainsKey(eventType))
            {
                var eventHandlers = _eventHandler[eventType];
                for (var i = 0; i < eventHandlers.Count; i++)
                {
                    var eventHandler = eventHandlers[i];
                    eventHandler.ProcessEvent(evt);
                }

                if (eventHandlers.Count == 0)
                {
                    GD.Print($"Event of type '{eventType.Name}' emitted, but no event handlers registered.");
                }
            }
            else
            {
                GD.Print($"Event of type '{eventType.Name}' emitted, but no event handlers registered.");
            }

            while (_eventHandlersToBeDeleted.Count > 0)
            {
                var eventHandler = _eventHandlersToBeDeleted.Pop();
                DeleteEventHandler(eventHandler);
            }

            _duringEventProcessing = false;
        }

        private IEventHandler InternalRegister<T>(Action<T> processEventAction) where T : class, IEvent
        {
            var eventType = typeof(T);
            if (!_eventHandler.ContainsKey(eventType))
            {
                _eventHandler[eventType] = new List<IEventHandler>();
            }

            var eventHandler = new EventHandler<T>(processEventAction);
            _eventHandler[eventType].Add(eventHandler);
            return eventHandler;
        }

        private void InternalUnregister(ref IEventHandler eventHandler)
        {
            if (_duringEventProcessing)
            {
                _eventHandlersToBeDeleted.Push(eventHandler);
            }
            else
            {
                DeleteEventHandler(eventHandler);
            }

            eventHandler = null;
        }

        private void DeleteEventHandler(IEventHandler eventHandler)
        {
            _eventHandler[eventHandler.GetEventType()].Remove(eventHandler);
        }

        public static void EmitEvent(IEvent evt)
        {
            Instance.InternalEmitEvent(evt);
        }

        public static IEventHandler Register<T>(Action<T> eventHandler) where T : class, IEvent
        {
            return Instance.InternalRegister(eventHandler);
        }

        public static void Unregister(ref IEventHandler eventHandler)
        {
            Instance.InternalUnregister(ref eventHandler);
        }
    }
}
