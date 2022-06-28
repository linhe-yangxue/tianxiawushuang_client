using System;
using System.Collections.Generic;
using Utilities.Events;


public class ObserverCenter
{
    public static Observer globalObserver { get; private set; }

    static ObserverCenter()
    {
        globalObserver = new Observer();
    }

    public static void Add(string key, EventCallBack<object> callback)
    {
        globalObserver.Add(key, callback);
    }

    public static void Add(string key, Action<object> action)
    {
        globalObserver.Add(key, action);
    }

    public static void Add(string key, Action action)
    {
        globalObserver.Add(key, action);
    }

    public static void Remove(string key, EventCallBack<object> callback)
    {
        globalObserver.Remove(key, callback);
    }

    public static void Clear(string key)
    {
        globalObserver.Clear(key);
    }

    public static void ClearAll()
    {
        globalObserver.ClearAll();
    }

    public static void Notify(string key, object arg)
    {
        globalObserver.Notify(key, arg);
    }

    public static void Notify(string key)
    {
        globalObserver.Notify(key);
    }
}