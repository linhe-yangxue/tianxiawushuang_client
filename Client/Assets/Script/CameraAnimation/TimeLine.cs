using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;


[Serializable]
public struct TimeStamp
{
    public int id;
    public float time;

    public TimeStamp(int id, float time)
    {
        this.id = id;
        this.time = time;
    }
}


public class TimeLine : IEnumerable, IEnumerable<TimeStamp>
{
    private List<TimeStamp> timeStampList = new List<TimeStamp>();

    public float totalTime
    {
        get { return timeStampList.Count > 0 ? timeStampList[timeStampList.Count - 1].time : 0f; }
    }

    public TimeLine()
    { }

    public TimeLine(IEnumerable<TimeStamp> timeStamps)
    {
        if (timeStamps != null)
        {
            foreach (var timeStamp in timeStamps)
            {
                this[timeStamp.id] = timeStamp.time;
            }
        }
    }

    public float this[int id]
    {
        get
        {
            int index = timeStampList.FindIndex(x => x.id == id);
            return index >= 0 ? timeStampList[index].time : -1f;
        }
        set
        {
            int index = timeStampList.FindIndex(x => x.id == id);

            if (value < 0f)
            {
                if (index >= 0)
                {
                    timeStampList.RemoveAt(index);
                }
            }
            else
            {
                if (index >= 0)
                {
                    TimeStamp stamp = timeStampList[index];

                    if (value < stamp.time)
                    {
                        if (index > 0 && value < timeStampList[index - 1].time)
                        {
                            stamp.time = value;
                            timeStampList.RemoveAt(index);

                            for (int i = index - 2; i >= 0; --i)
                            {
                                if (timeStampList[i].time <= value)
                                {
                                    timeStampList.Insert(i + 1, stamp);
                                    return;
                                }
                            }

                            timeStampList.Insert(0, stamp);
                        }
                        else
                        {
                            stamp.time = value;
                            timeStampList[index] = stamp;
                        }
                    }
                    else if (value > stamp.time)
                    {
                        if (index < timeStampList.Count - 1 && value > timeStampList[index + 1].time)
                        {
                            stamp.time = value;
                            timeStampList.RemoveAt(index);

                            for (int i = index + 2; i < timeStampList.Count; ++i)
                            {
                                if (timeStampList[i].time >= value)
                                {
                                    timeStampList.Insert(i, stamp);
                                    return;
                                }
                            }

                            timeStampList.Add(stamp);
                        }
                        else
                        {
                            stamp.time = value;
                            timeStampList[index] = stamp;
                        }
                    }
                }
                else
                {
                    TimeStamp stamp = new TimeStamp(id, value);

                    for (int i = timeStampList.Count - 1; i >= 0; --i)
                    {
                        if (value >= timeStampList[i].time)
                        {
                            timeStampList.Insert(i + 1, stamp);
                            return;
                        }
                    }

                    timeStampList.Insert(0, stamp);
                }
            }
        }
    }

    public IEnumerator<TimeStamp> GetEnumerator()
    {
        foreach (var stamp in timeStampList)
        {
            yield return stamp;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public void Clear()
    {
        timeStampList.Clear();
    }
}


public class TimeLineRoutine : Routine
{
    public TimeLine timeLine { get; private set; }

    public event Action<TimeStamp> onReach;

    public float elapsed
    {
        get
        {
            if (Routine.IsActive(this))
            {
                return Time.time - startTime;
            }
            else if (status == RoutineStatus.Done)
            {
                return timeLine.totalTime;
            }
            else if (status == RoutineStatus.Broken)
            {
                return breakTime - startTime;
            }
            else
            {
                return 0f;
            }
        }
    }

    private float startTime = 0f;
    private float breakTime = 0f;

    public TimeLineRoutine(TimeLine timeLine)
    {
        this.timeLine = timeLine;
        Bind(DoTimeLine());
    }

    private IEnumerator DoTimeLine()
    {
        startTime = Time.time;

        using (IEnumerator<TimeStamp> iter = timeLine.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                TimeStamp stamp = iter.Current;
                yield return new WaitUntil(() => Time.time - startTime >= stamp.time);

                if (onReach != null)
                {
                    onReach(stamp);
                }
            }
        }
    }

    protected override void OnBreak()
    {
        breakTime = Time.time;
    }
}