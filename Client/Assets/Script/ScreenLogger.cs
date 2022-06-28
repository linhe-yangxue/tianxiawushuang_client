using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ScreenLogger : MonoBehaviour
{
    private class LogMsg
    {
        public string msg = "";
        public LOG_LEVEL level = LOG_LEVEL.GENERAL;

        public LogMsg(string msg, LOG_LEVEL level)
        {
            this.msg = msg;
            this.level = level;
        }
    }

    private class LogQueue : IEnumerable, IEnumerable<LogMsg>
    {
        private LogMsg[] queue;
        private int capacity = 0;
        private int head = 0;
        private int tail = 0;

        public LogQueue(int capacity)
        {
            this.queue = new LogMsg[capacity + 1];
            this.capacity = capacity;
        }

        public void Push(string msg, LOG_LEVEL level)
        {       
            queue[tail] = new LogMsg(msg, level);
            tail = Next(tail);

            if (head == tail)
            {
                head = Next(head);
            }
        }

        public void Clear()
        {
            head = 0;
            tail = 0;
        }

        public bool Empty()
        {
            return head == tail;
        }

        public IEnumerator<LogMsg> GetEnumerator()
        {
            int current = head;

            while (current != tail)
            {
                yield return queue[current];
                current = Next(current);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int Next(int current)
        {
            return current >= capacity ? 0 : current + 1;
        }
    }

    private static ScreenLogger instance = null;
    private static LogQueue queue;
    private static GUIStyle normalStyle;
    private static GUIStyle warnStyle;
    private static GUIStyle errorStyle;

    static ScreenLogger()
    {
        queue = new LogQueue(20);

        normalStyle = new GUIStyle();
        normalStyle.normal.textColor = Color.white;

        warnStyle = new GUIStyle();
        warnStyle.normal.textColor = Color.yellow;

        errorStyle = new GUIStyle();
        errorStyle.normal.textColor = Color.red;
    }

    public static void Log(string msg)
    {
        Log(msg, LOG_LEVEL.GENERAL);
    }

    public static void Log(string msg, LOG_LEVEL level)
    {
        if (instance == null)
        {
            GameObject helper = new GameObject("ScreenLogger");           
            DontDestroyOnLoad(helper);
            instance = helper.AddComponent<ScreenLogger>();
        }

        if (instance != null)
        {
            switch (level)
            {
                case LOG_LEVEL.GENERAL:
                    queue.Push(DateTime.Now.ToString("T") + "> " + msg, level);
                    break;

                case LOG_LEVEL.WARN:
                    queue.Push(DateTime.Now.ToString("T") + "> " + msg, level);
                    break;

                case LOG_LEVEL.ERROR:
                    queue.Push(DateTime.Now.ToString("T") + "> " + msg, level);
                    break;

                case LOG_LEVEL.HIGH:
                    queue.Push(DateTime.Now.ToString("T") + "> " + msg, level);
                    break;
            }       
        }      
    }

    private void OnGUI()
    {
        if (queue.Empty())
            return;

        GUILayout.BeginVertical();

        if (GUILayout.Button("clear", GUILayout.Width(60)))
        {
            queue.Clear();
        }

        foreach (var log in queue)
        {
            switch (log.level)
            {
                case LOG_LEVEL.GENERAL:
                    GUILayout.Label(log.msg, normalStyle);
                    break;

                case LOG_LEVEL.WARN:
                    GUILayout.Label(log.msg, warnStyle);
                    break;

                case LOG_LEVEL.ERROR:
                    GUILayout.Label(log.msg, errorStyle);
                    break;

                case LOG_LEVEL.HIGH:
                    GUILayout.Label(log.msg, errorStyle);
                    break;
            }       
        }

        GUILayout.EndVertical();
    }
}