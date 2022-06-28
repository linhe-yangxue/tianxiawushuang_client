using UnityEngine;
using System.Collections;

public class ActiveBirthSelModelForUI : ActiveBirthForUI {

	public override void SetCollider(BaseObject obj)
	{
//		if(obj != null && mbIsCanOperate)
//		{
//			CapsuleCollider coll = obj.mMainObject.GetComponent<CapsuleCollider>();
//			if(coll == null)
//			{
//				coll = obj.mMainObject.AddComponent<CapsuleCollider>();
//				coll.center = new Vector3(0, 1, 0);
//				coll.height = 2;
//				coll.isTrigger = true;
//			}
//			UISelModelMastButton mastBtn = obj.mMainObject.GetComponent<UISelModelMastButton>();
//			if(mastBtn == null)
//			{
//				mastBtn = obj.mMainObject.AddComponent<UISelModelMastButton>();
//                mastBtn.mObject = obj;
//
//			}
//		}
	}

	public override void SetCollider(GameObject gameobj, BaseObject obj)
	{
//		if(gameobj != null && obj != null && mbIsCanOperate)
//		{
//			UISelModelMastButton mastBtn = gameobj.GetComponent<UISelModelMastButton>();
//			if(mastBtn == null)
//			{
//				mastBtn = gameobj.AddComponent<UISelModelMastButton>();
//			}
//			mastBtn.mObject = obj;
//		}
	}
}