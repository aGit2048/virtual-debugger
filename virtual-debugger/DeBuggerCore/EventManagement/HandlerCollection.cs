using System;
using System.Collections.Generic;

namespace DeBuggerCore.EventManagement
{
    public class HandlerCollection
    {
        public int Count => _eventHandlerList.Count;

        private List<EventHandler> _eventHandlerList;

        public HandlerCollection()
        {
            _eventHandlerList = new List<EventHandler>();
        }

        public void AddEvent(EventHandler callback)
        {
            if (!_eventHandlerList.Contains(callback))
            {
                _eventHandlerList.Add(callback);
            }
        }

        public void RemoveEvent(EventHandler callback)
        {
            if (_eventHandlerList.Contains(callback))
            {
                _eventHandlerList.Remove(callback);
            }
        }

        public void Fire(object sender, DebuggerEventArgs args)
        {
            foreach (var handler in _eventHandlerList)
            {
                handler?.Invoke(sender, args);
            }
        }

        public void Dispose()
        {
            _eventHandlerList.Clear();
        }
    }
}
