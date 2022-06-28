using UnityEngine;
using System.Collections;
using WellFired;


[USequencerFriendlyName("Play Effect")]
[USequencerEvent("Extensions/Play Effect")]
public class USPlayEffect : USEventBase
{
    public int index = 0;
    public GameObject target;
    public float flySpeed = 5f;
    public float damageRange = 2f;

    public override void FireEvent()
    {
        ActiveObject obj = USAttachInfo.GetAttachActiveObject(AffectedObject);
        ActiveObject targetObj = USAttachInfo.GetAttachActiveObject(target);
        BaseEffect effect = obj.PlayEffect(index, targetObj);
        USEffect useff = effect.mGraphObject.AddComponent<USEffect>();
        useff.effect = effect as Effect;
        useff.range = damageRange;
        useff.speed = flySpeed;
    }

    public override void ProcessEvent(float runningTime)
    { }
}