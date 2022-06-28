using UnityEngine;
using System.Collections;
using WellFired;


[USequencerFriendlyName("Finish")]
[USequencerEvent("Extensions/Finish")]
public class USFinish : USEventBase
{
    public override void FireEvent()
    {
        Sequence.Stop();
        CutsceneManager.StartBattle();
    }

    public override void ProcessEvent(float runningTime)
    { }
}