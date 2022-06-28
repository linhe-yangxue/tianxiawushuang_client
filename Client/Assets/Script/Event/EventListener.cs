using System;
using System.Collections;
using System.Collections.Generic;


namespace Utilities.Events
{
    public enum EventCallBackMode
    {
        Repeat,
        OneShot
    }


    public class EventCallBack<T>
    {
        private int mPriority = 0;

        public Action<T> action { get; set; }
        public EventCallBackMode mode { get; set; }
        public bool enabled { get; set; }

        public EventCallBack()
            : this(null)
        { }

        public EventCallBack(Action<T> action)
            : this(action, 0)
        { }

        public EventCallBack(Action<T> action, int priority)
            : this(action, priority, EventCallBackMode.Repeat)
        { }

        public EventCallBack(Action<T> action, int priority, EventCallBackMode mode)
        {
            this.action = action;
            this.mPriority = priority;
            this.mode = mode;
            this.enabled = true;
        }

        public int priority
        {
            get
            {
                return mPriority;
            }
        }

        public void Invoke(T arg)
        {
            if (action != null && enabled)
            {
                action.Invoke(arg);
            }
        }
    }


    public class EventListener<T>
    {
        private List<EventCallBack<T>> mCallbacks = new List<EventCallBack<T>>();
        private bool hasBreak = false;

        public void Add(EventCallBack<T> callback)
        {
            if (!mCallbacks.Contains(callback))
            {
                Insert(callback);
            }
        }

        public void Add(Action<T> action)
        {
            Add(action, 0);
        }

        public void Add(Action<T> action, int priority)
        {
            EventCallBack<T> callback = new EventCallBack<T>(action, priority);
            callback.mode = EventCallBackMode.OneShot;
            Add(callback);
        }

        public void Notify(T arg)
        {
            EventCallBack<T>[] copy = mCallbacks.ToArray();
            hasBreak = false;

            foreach (var callback in copy)
            {
                callback.Invoke(arg);

                if (callback.mode == EventCallBackMode.OneShot)
                {
                    Remove(callback);
                }

                if (hasBreak)
                {
                    break;
                }
            }
        }

        public void Break()
        {
            hasBreak = true;
        }

        public void Remove(EventCallBack<T> callback)
        {
            mCallbacks.Remove(callback);
        }

        public void RemoveAll(Predicate<EventCallBack<T>> match)
        {
            mCallbacks.RemoveAll(match);
        }

        public void Clear()
        {
            mCallbacks.Clear();
        }

        private void Insert(EventCallBack<T> callback)
        {
            for (int i = mCallbacks.Count - 1; i >= 0; --i)
            {
                if (callback.priority <= mCallbacks[i].priority)
                {
                    mCallbacks.Insert(i + 1, callback);
                    return;
                }
            }

            mCallbacks.Insert(0, callback);
        }
    }
}