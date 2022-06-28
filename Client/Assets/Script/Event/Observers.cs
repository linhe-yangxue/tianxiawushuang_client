using System;
using System.Collections.Generic;
using Utilities.Events;


public class SingletonObserver<T> where T : Observer, new()
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }

            return instance;
        }
    }
}

public sealed class OnWindowOpenObserver : Observer
{
    public static OnWindowOpenObserver Instance { get { return SingletonObserver<OnWindowOpenObserver>.Instance; } }
}


public sealed class OnWindowRefreshObserver : Observer
{
    public static OnWindowRefreshObserver Instance { get { return SingletonObserver<OnWindowRefreshObserver>.Instance; } }
}


public sealed class OnWindowCloseObserver : Observer
{
    public static OnWindowCloseObserver Instance { get { return SingletonObserver<OnWindowCloseObserver>.Instance; } }
}


public sealed class OnButtonEventObserver : Observer
{
    public static OnButtonEventObserver Instance { get { return SingletonObserver<OnButtonEventObserver>.Instance; } }
}

//public sealed class OnBattleCustomerActionObserver : Observer
//{
//    public static OnBattleCustomerActionObserver Instance { get { return SingletonObserver<OnBattleCustomerActionObserver>.Instance; } }
//}