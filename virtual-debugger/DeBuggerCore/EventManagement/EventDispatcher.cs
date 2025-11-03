using System;
using System.Collections.Generic;

namespace DeBuggerCore.EventManagement
{
    public class EventDispatcher : Singleton<EventDispatcher>
    {
        private Dictionary<int, HandlerCollection> _handlerDic = new Dictionary<int, HandlerCollection>();

        public void AddEvent(int id, EventHandler handler)
        {
            if (handler == null) return;
            if (!_handlerDic.ContainsKey(id))
            {
                _handlerDic.Add(id, new HandlerCollection());
            }
            _handlerDic[id].AddEvent(handler);
        }

        public void RemoveEvent(int id, EventHandler handler)
        {
            if (handler == null) return;
            if (!_handlerDic.ContainsKey(id)) return;
            _handlerDic[id].RemoveEvent(handler);
            if (_handlerDic[id].Count <= 0)
            {
                _handlerDic.Remove(id);
            }
        }

        public void Fire(object sender, DebuggerEventArgs args)
        {
            if (!_handlerDic.ContainsKey(args.GetId())) return;
            _handlerDic[args.GetId()].Fire(sender, args);
        }

        public void Dispose()
        {
            foreach (var item in _handlerDic)
            {
                item.Value.Dispose();
            }
            _handlerDic.Clear();
            _handlerDic = null;
        }
    }
}
