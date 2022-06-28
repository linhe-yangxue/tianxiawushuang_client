using UnityEngine;
using System.Collections;
using WellFired;


[USequencerFriendlyName("Create Role")]
[USequencerEvent("Extensions/Create Role")]
public class USCreateRole : USEventBase
{
    public int activeIndex = 0;

    public override void FireEvent()
    {
        if (activeIndex > 0 && AffectedObject != null)
        {
            ActiveObject obj = ObjectManager.Self.CreateObject(activeIndex) as ActiveObject;
            obj.mMainObject.transform.parent = AffectedObject.transform;
            obj.mMainObject.transform.localPosition = Vector3.zero;
            obj.mMainObject.transform.localRotation = Quaternion.identity;
            obj.SetVisible(true);            
            obj.mWarnEffect.SetVisible(false);

            NavMeshAgent agent = obj.mMainObject.GetComponent<NavMeshAgent>();

            if(agent != null)
                Destroy(agent);

            USAttachInfo.Attach(AffectedObject, obj);
        }
    }

    public override void ProcessEvent(float runningTime)
    { }

    public override void UndoEvent()
    {
        ActiveObject obj = USAttachInfo.GetAttachActiveObject(AffectedObject);

        if (obj != null)
        {
            obj.Destroy();
            obj.OnDestroy();
            USAttachInfo.Attach(AffectedObject, null);
        }
    }

    public override void StopEvent()
    {
        UndoEvent();
    }
}