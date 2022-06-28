using UnityEngine;
using System.Collections;
using WellFired;


[USequencerFriendlyName("Do Skill")]
[USequencerEvent("Extensions/Do Skill")]
public class USDoSkill : USEventBase
{
    public int skillIndex = 0;
    public GameObject target;

    public override void FireEvent()
    {
        ActiveObject obj = USAttachInfo.GetAttachActiveObject(AffectedObject);
        ActiveObject targetObj = USAttachInfo.GetAttachActiveObject(target);

        if (obj != null)
        {
            obj.DoSkill(skillIndex, targetObj, null);
        }
    }

    public override void ProcessEvent(float runningTime)
    { }
}