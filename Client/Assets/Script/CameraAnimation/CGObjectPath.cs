using UnityEngine;
using System.Collections;
using Utilities.Routines;


public class CGObjectPath : ObjectPath
{
    public int objectIndex = 0;
    public bool createNew = false;
    public BaseObject owner { get; private set; }

    protected override void OnPlay()
    {
        if (objectIndex > 0)
        {
            if (createNew)
            {
                owner = CGTools.CreateBaseObject(objectIndex);
            }
            else
            {
                var objMap = ObjectManager.Self.mObjectMap;

                for (int i = objMap.Length - 1; i >= 0; --i)
                {
                    if (objMap[i] != null && objMap[i].mConfigIndex == objectIndex)
                    {
                        owner = objMap[i];
                        break;
                    }
                }
            }
        }

        if (owner != null && owner.mMainObject != null)
        {
            target = owner.mMainObject;
            NavMeshAgent agent = target.GetComponent<NavMeshAgent>();

            if (agent != null)
                agent.enabled = false;

            CGTools.TagAsPathMoved(owner);
        }
    }

    protected override void OnPlayDemonstrate()
    {
        if (objectIndex > 0)
        {
            owner = CGTools.CreateBaseObject(objectIndex);

            if (owner != null && owner.mMainObject != null)
            {
                target = owner.mMainObject;
                NavMeshAgent agent = target.GetComponent<NavMeshAgent>();

                if (agent != null)
                    agent.enabled = false;

                CGTools.TagAsPathMoved(owner);
            }
        }
    }

    protected override void OnCreateNewCornerObject(GameObject cornerObject)
    {
        cornerObject.AddComponent<CGPlayAnimEffect>();
    }

    protected override void OnReachCorner(GameObject cornerObject)
    {
        var cgPlay = cornerObject.GetComponent<CGPlayAnimEffect>();

        if (cgPlay != null && owner != null)
        {
            cgPlay.Play(owner);
        }
    }
}