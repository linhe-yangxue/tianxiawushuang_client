using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;


public class CGTimeLine : CGComponent
{
    private static Dictionary<int, CGTimeLine> cgTimeLines = new Dictionary<int, CGTimeLine>();

    public static CGTimeLine GetByGroup(int group)
    {
        CGTimeLine line;

        if (cgTimeLines.TryGetValue(group, out line) && line != null)
        {
            return line;
        }

        return null;
    }

    [Serializable]
    public class CGEventObject
    {
        public GameObject gameObject = null;
        public float time = 0f;
    }

    public int group = 0;
    public float duration = 1000f;
    public List<CGEventObject> cgEventObjects = new List<CGEventObject>();

    private TimeLine timeLine = new TimeLine();
    private TimeLineRoutine timeLineRoutine;

    private void Reset()
    {
        CGTimeLine[] allLine = FindObjectsOfType<CGTimeLine>();
        int maxGroup = 0;

        foreach (var p in allLine)
        {
            if (p != this && p.group > maxGroup)
            {
                maxGroup = p.group;
            }
        }

        this.group = maxGroup + 1;
    }

    protected override void OnInit()
    {
        if (cgTimeLines.ContainsKey(group))
        {
            cgTimeLines[group] = this;
        }
        else
        {
            cgTimeLines.Add(group, this);
        }

        target = gameObject;
        BuildTimeLine();

#if UNITY_EDITOR
        style.fontSize = 30;
        style.normal.textColor = Color.white;
#endif
    }

    private void OnDestroy()
    {
        if (cgTimeLines.ContainsKey(group) && cgTimeLines[group] == this)
        {
            cgTimeLines.Remove(group);
        }
    }

    protected override IEnumerator DoPlay()
    {
        timeLineRoutine = new TimeLineRoutine(timeLine);
        timeLineRoutine.onReach += OnEvent;
        return timeLineRoutine;
    }

    protected override IEnumerator DoPlayDemonstrate()
    {
        timeLineRoutine = new TimeLineRoutine(timeLine);
        timeLineRoutine.onReach += OnDemonEvent;
        return timeLineRoutine;
    }

#if UNITY_EDITOR
    private GUIStyle style = new GUIStyle();

    private void OnGUI()
    {
        if (demonstrate && Application.isPlaying && timeLineRoutine != null)
        {
            float elapsed = Mathf.Min(timeLineRoutine.elapsed, duration);
            string text = elapsed.ToString("#0.00");
            GUI.Label(new Rect(0, 0, 100, 100), text, style);
        }
    }
#endif

    // 只能于编辑模式下调用
    public void Refresh()
    {
        List<CGEventObject> temp = new List<CGEventObject>();

        foreach (var cgObj in cgEventObjects)
        {
            AddEventObject(temp, cgObj);
        }

        cgEventObjects = temp;
    }

    // 只能于编辑模式下调用
    public void DeleteAtIndex(int index)
    {
        if (cgEventObjects == null || index >= cgEventObjects.Count)
        {
            return;
        }

        cgEventObjects.RemoveAt(index);
    }

    // 只能于编辑模式下调用
    public void InsertAtIndex(int index)
    {
        if (cgEventObjects == null || index >= cgEventObjects.Count)
        {
            return;
        }

        var newCGEventObject = new CGEventObject();
        newCGEventObject.gameObject = null;
        newCGEventObject.time = cgEventObjects[index].time;
        cgEventObjects.Insert(index, newCGEventObject);
    }

    // 只能于编辑模式下调用
    public void Add()
    {
        if (cgEventObjects == null)
        {
            return;
        }

        var newCGEventObject = new CGEventObject();
        newCGEventObject.gameObject = null;
        newCGEventObject.time = cgEventObjects.Count > 0 ? cgEventObjects[cgEventObjects.Count - 1].time : 0f;
        cgEventObjects.Add(newCGEventObject);
    }

    private void BuildTimeLine()
    {
        for (int i = 0; i < cgEventObjects.Count; ++i)
        {
            if (cgEventObjects[i].time <= duration)
            {
                timeLine[i] = cgEventObjects[i].time;
            }
        }

        timeLine[-1] = duration + 0.01f;
    }

    private void AddEventObject(List<CGEventObject> list, CGEventObject cgObj)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            if (cgObj.time < list[i].time)
            {
                list.Insert(i, cgObj);
                return;
            }
        }

        list.Add(cgObj);
    }

    private void OnEvent(TimeStamp stamp)
    {
#if UNITY_EDITOR
        if (demonstrate && stamp.id == -1)
        {
            CGTools.ClearCG();
        }
#endif

        if (stamp.id >= 0 && stamp.id < cgEventObjects.Count)
        {
            GameObject obj = cgEventObjects[stamp.id].gameObject;

            var cgComp = obj.GetComponent<CGComponent>();

            if (cgComp != null)
            {
                CGTools.TagAsCGRoutine(cgComp.Play());
            }

            var cgPlay = obj.GetComponent<CGPlayAnimEffect>();

            if (cgPlay != null)
            {
                cgPlay.Play(null);
            }
        }
    }

    private void OnDemonEvent(TimeStamp stamp)
    {
#if UNITY_EDITOR
        if (demonstrate && stamp.id == -1)
        {
            CGTools.ClearCG();
        }
#endif

        if (stamp.id >= 0 && stamp.id < cgEventObjects.Count)
        {
            GameObject obj = cgEventObjects[stamp.id].gameObject;

            var cgComp = obj.GetComponent<CGComponent>();

            if (cgComp != null)
            {
                CGTools.TagAsCGRoutine(cgComp.PlayDemonstrate());
            }

            var cgPlay = obj.GetComponent<CGPlayAnimEffect>();

            if (cgPlay != null)
            {
                cgPlay.Play(null);
            }
        }
    }
}