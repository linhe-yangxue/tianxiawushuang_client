using UnityEngine;
using WellFired;


[USequencerFriendlyName("Play Animator")]
[USequencerEvent("Extensions/Play Animator")]
public class USPlayAnimator : USEventBase
{
    public string animName = "idle";

    public override void FireEvent()
    {
        ActiveObject obj = USAttachInfo.GetAttachActiveObject(AffectedObject);
        
        if (obj != null)
        {
            obj.PlayAnim(animName);
        }
    }

    public override void ProcessEvent(float runningTime)
    { }
}