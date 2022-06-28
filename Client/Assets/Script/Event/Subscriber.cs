using System;
using System.Collections.Generic;


namespace Utilities.Events
{
    public class Subscription : IDisposable
    {
        public Observer observer { get; private set; }
        public string evt { get; private set; }
        public EventCallBack<object> callback { get; private set; }
        public bool cancled { get; private set; }

        public bool enabled
        {
            get { return callback.enabled; }
            set { callback.enabled = value; }
        }

        public Subscription(Observer observer, string evt, Action<object> action)
            : this(observer, evt, action, 0, EventCallBackMode.Repeat)
        { }

        public Subscription(Observer observer, string evt, Action<object> action, EventCallBackMode mode)
            : this(observer, evt, action, 0, mode)
        { }

        public Subscription(Observer observer, string evt, Action<object> action, int priority)
            : this(observer, evt, action, priority, EventCallBackMode.Repeat)
        { }

        public Subscription(Observer observer, string evt, Action<object> action, int priority, EventCallBackMode mode)
            : this(observer, evt, new EventCallBack<object>(action, priority, mode))
        { }

        public Subscription(Observer observer, string evt, EventCallBack<object> callback)
        {
            this.observer = observer;
            this.evt = evt;
            this.callback = callback;
            this.cancled = false;

            this.observer.Add(this.evt, this.callback);
        }

        public void Cancle()
        {
            if (!cancled)
            {
                observer.Remove(evt, callback);
                cancled = true;
            }
        }

        private bool isDisposed = false;

        public void Dispose()
        {
            if (!isDisposed)
            {
                Cancle();
                isDisposed = true;
                System.GC.SuppressFinalize(this);
            }
        }
    }

    public class Subscriber : IDisposable
    {
        private List<Subscription> _subscriptions = new List<Subscription>();
        private bool _enabled = true;

        public bool enabled
        {
            get 
            { 
                return _enabled; 
            }
            set 
            {
                _enabled = value;

                foreach (var sub in _subscriptions)
                {
                    sub.enabled = _enabled;
                }
            }
        }

        public void Subscribe(Observer observer, string evt, Action<object> action)
        {
            Subscribe(observer, evt, action, 0, EventCallBackMode.Repeat);
        }

        public void Subscribe(Observer observer, string evt, Action<object> action, int priority)
        {
            Subscribe(observer, evt, action, priority, EventCallBackMode.Repeat);
        }

        public void Subscribe(Observer observer, string evt, Action<object> action, EventCallBackMode mode)
        {
            Subscribe(observer, evt, action, 0, mode);
        }

        public void Subscribe(Observer observer, string evt, Action<object> action, int priority, EventCallBackMode mode)
        {
            EventCallBack<object> callback = new EventCallBack<object>(action, priority, mode);
            Subscribe(observer, evt, callback);
        }

        public void Subscribe(Observer observer, string evt, EventCallBack<object> callback)
        {
            Cancle(observer, evt);
            Append(observer, evt, callback);
        }

        public void Append(Observer observer, string evt, Action<object> action)
        {
            Append(observer, evt, action, 0, EventCallBackMode.Repeat);
        }

        public void Append(Observer observer, string evt, Action<object> action, int priority)
        {
            Append(observer, evt, action, priority, EventCallBackMode.Repeat);
        }

        public void Append(Observer observer, string evt, Action<object> action, EventCallBackMode mode)
        {
            Append(observer, evt, action, 0, mode);
        }

        public void Append(Observer observer, string evt, Action<object> action, int priority, EventCallBackMode mode)
        {
            EventCallBack<object> callback = new EventCallBack<object>(action, priority, mode);
            Append(observer, evt, callback);
        }

        public void Append(Observer observer, string evt, EventCallBack<object> callback)
        {
            Subscription s = new Subscription(observer, evt, callback);
            s.enabled = _enabled;
            _subscriptions.Add(s);
        }

        public void Cancle(Observer observer, string evt)
        {
            for (int i = 0; i < _subscriptions.Count; ++i)
            {
                if (_subscriptions[i].observer == observer && _subscriptions[i].evt == evt)
                {
                    _subscriptions[i].Cancle();
                    _subscriptions.RemoveAt(i);
                    --i;
                }
            }
        }

        public void CancleAll()
        {
            foreach (var s in _subscriptions)
            {
                s.Cancle();
            }

            _subscriptions.Clear();
        }

        private bool isDisposed = false;

        public void Dispose()
        {
            if (!isDisposed)
            {
                CancleAll();
                isDisposed = true;
                System.GC.SuppressFinalize(this);
            }
        }
    }
}