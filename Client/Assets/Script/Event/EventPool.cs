using System;
using System.Collections.Generic;


namespace Utilities.Events
{
    public class EventPool<TKey, TArg>
    {
        private Dictionary<TKey, EventListener<TArg>> _listeners = new Dictionary<TKey, EventListener<TArg>>();

        public IEnumerable<TKey> keys
        {
            get
            {
                foreach (var pair in _listeners)
                {
                    yield return pair.Key;
                }
            }
        }

        public IEnumerable<EventListener<TArg>> listeners
        {
            get
            {
                foreach (var pair in _listeners)
                {
                    yield return pair.Value;
                }
            }
        }

        public void Add(TKey key, EventCallBack<TArg> callback)
        {
            EventListener<TArg> listener = null;

            if (!_listeners.TryGetValue(key, out listener))
            {
                listener = new EventListener<TArg>();
                _listeners.Add(key, listener);
            }

            listener.Add(callback);
        }

        public void Add(TKey key, Action<TArg> action)
        {
            if (action != null)
            {
                EventCallBack<TArg> callback = new EventCallBack<TArg>(action, 0, EventCallBackMode.OneShot);
                Add(key, callback);
            }
        }

        public void Add(TKey key, Action action)
        {
            if (action != null)
            {
                EventCallBack<TArg> callback = new EventCallBack<TArg>(x => action(), 0, EventCallBackMode.OneShot);
                Add(key, callback);
            }
        }

        public void Remove(TKey key, EventCallBack<TArg> callback)
        {
            EventListener<TArg> listener = null;

            if (_listeners.TryGetValue(key, out listener))
            {
                listener.Remove(callback);
            }
        }

        public void Clear(TKey key)
        {
            _listeners.Remove(key);
        }

        public void ClearAll()
        {
            _listeners.Clear();
        }

        public void Notify(TKey key, TArg arg)
        {
            EventListener<TArg> listener = null;

            if (_listeners.TryGetValue(key, out listener))
            {
                listener.Notify(arg);
            }
        }

        public void Notify(TKey key)
        {
            Notify(key, default(TArg));
        }
    }


    public class Observer : EventPool<string, object> 
    { }
}