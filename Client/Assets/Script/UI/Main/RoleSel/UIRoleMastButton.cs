//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UIRoleMastButton : MonoBehaviour
{

    public BaseObject mObject;

    void OnDrag(Vector2 delta)
    {
		if(mObject != null)
			mObject.mMainObject.transform.Rotate(new Vector3(0, -delta.x, 0));





//		if(mObject != null)
//		{
//			Transform tran = mObject.mMainObject.transform;
//			if(tran != null)
//			{
//				tran.Find ("_role_").Rotate(new Vector3(0, delta.x, 0));
//			}
//		}
    }

	
    void OnClick()
    {
        ActiveObject obj = mObject as ActiveObject;

		if(obj != null)
		{        
            AnimatorStateInfo stateInfo = obj.mAnim.GetCurrentStateInfo();

            if (obj.mAnim.mNowAnimState != "cute" && !stateInfo.IsName("cute"))
            {
                Logic.tEvent idleEvent = Logic.EventCenter.Self.StartEvent("RoleSelUI_PlayIdleEvent");
                mObject.PlayMotion("cute", idleEvent);
            }
		}
    }
}
