using System;
using Utilities.Tasks;


public enum NotifyType
{
    Announcement,
    Achievement
}


public class WindowNotification : ATask
{
    public readonly string mWindow;
    public tNiceData mInfo;

    public WindowNotification()
        : base()
    { }

    public WindowNotification(int priority, string windowName)
        : base(priority)
    {
        this.mWindow = windowName;
    }

    public override void Execute()
    {
        DataCenter.OpenWindow(mWindow, mInfo);
    }

    public override void Terminate()
    {
        DataCenter.CloseWindow(mWindow);
    }

    public void Finish()
    {
        base.Finish();
    }
}


public class Notification
{
    // 显示在顶部的通知，包括任务完成通知和广播，在同一队列中维护
    // 任务完成提示比广播消息优先级高
    // 好友消息的广播，比全体玩家消息的广播优先级高
    private static TaskQueue<WindowNotification> topQueue = new TaskQueue<WindowNotification>();

    public static void Notify(NotifyType type, tNiceData info)
    {
        WindowNotification notification = null;
        int priority = GetPriority(type, info);

        switch (type)
        {
            case NotifyType.Achievement:
                notification = new WindowNotification(priority, "TASK_ACHIEVE_WINDOW") { mInfo = info };
                break;

            case NotifyType.Announcement:
                notification = new WindowNotification(priority, "ANNOUNCEMENT_WINDOW") { mInfo = info };
                break;
        }

        if (notification != null)
            topQueue.Accept(notification);
    }

    public static void Next()
    {
        if (topQueue.current != null)
            topQueue.current.Finish();
    }

    public static void Clear()
    {
        topQueue.Clear();
    }

    public static void Stop()
    {
        topQueue.Stop();
    }

    public static void Start()
    {
        topQueue.Start();
    }

    public static void Remove(Predicate<WindowNotification> filter)
    {
        topQueue.Remove(filter);
    }

    public static void Refuse(Predicate<WindowNotification> filter)
    {
        topQueue.Refuse(filter);
    }

    public static void RemoveAll()
    {
        Remove(x => true);
    }

    public static void RefuseAll()
    {
        Refuse(x => true);
    }

    public static void RefuseNone()
    {
        Refuse(null);
    }

    private static int GetPriority(NotifyType type, tNiceData info)
    {
        switch (type)
        {
            case NotifyType.Achievement:    
                return 2000;

            case NotifyType.Announcement:
                {
                    int announceType = info.get("TYPE");

                    if (announceType == 5)
                        return 1100;
                    else
                        return 1000;
                }
        }

        return 0;
    }
}