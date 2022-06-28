using UnityEngine;
using Utilities;


public class USAttachInfo : MonoBehaviour
{
    public ActiveObject activeObject;

    public static USAttachInfo Attach(GameObject owner, ActiveObject obj)
    {
        USAttachInfo info = owner.GetOrAddComponent<USAttachInfo>();
        info.activeObject = obj;
        return info;
    }

    public static ActiveObject GetAttachActiveObject(GameObject owner)
    {
        if (owner == null)
            return null;

        USAttachInfo info = owner.GetComponent<USAttachInfo>();

        if (info != null)
            return info.activeObject;

        return null;
    }
}