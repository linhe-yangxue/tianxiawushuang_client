using UnityEngine;
using System.Collections.Generic;
using WellFired;


[USequencerFriendlyName("Set Active")]
[USequencerEvent("Extensions/Set Active")]
public class USSetActive : USEventBase
{
    public bool active = false;

    private List<bool> history = new List<bool>();

    public override void FireEvent()
    {
        if (history.Count == 0)
        {
            history.Add(AffectedObject.activeSelf);
        }

        history.Add(active);
        AffectedObject.SetActive(active);
    }

    public override void ProcessEvent(float runningTime)
    { }

    public override void UndoEvent()
    {
        history.RemoveAt(history.Count - 1);
        AffectedObject.SetActive(history[history.Count - 1]);
    }

    public override void StopEvent()
    {
        if (history.Count > 0)
        {
            AffectedObject.SetActive(history[0]);
            history.Clear();
        }
    }
}