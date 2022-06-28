using UnityEngine;
using System.Collections;
using Logic;
using System;

public class ActiveLuckyGuyWindow : tWindow
{
	int mLuckyIndex;
	int mLuckyCount;
	int mCurConfigIndex;

	public override void Init ()
	{
		EventCenter.Register ("Button_lucky_guy_star_button", new DefineFactory<Button_lucky_guy_star_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		mCurConfigIndex = (int)param;
		
		tEvent evt = Net.StartEvent("CS_RequestHDData");
		evt.set ("CONFIG_INDEX", mCurConfigIndex);
		evt.DoEvent();
	}

	public override bool Refresh (object param)
	{
		tEvent evt = param as tEvent;
		mLuckyIndex = evt.get ("CASH_EXTRA_ID");
		mLuckyCount = evt.get ("CASH_EXTRA_COUNT");

		RefreshLuckyGuy();
		return true;
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case "SNED_MESSAGER":
			SendMessage(objVal.ToString ());
			break;
		case "SEND_MESSAGE_RESULT":
			SendMessageResult(objVal);
			break;
		}
	}

	void SendMessage(string strWhichMessage)
	{
		tEvent evt = Net.StartEvent("CS_UpdateHDData");
		evt.set ("TIME_1", false);
		evt.set ("TIME_2", false);
		evt.set ("CASH_EXTRA", false);
		evt.set ("HD_ID", 0);
		evt.set (strWhichMessage, true);
		evt.set("CONFIG_INDEX", mCurConfigIndex);
		evt.DoEvent();
	}
	
	void RefreshLuckyGuy()
	{
		float luckyGuyMultiple = 1.0f;
		if(mLuckyIndex != 0) 
		{
			luckyGuyMultiple = DataCenter.mActiveLuckyGuyConfig.GetRecord (mLuckyIndex)["COEFFICIENT"];
			RoleLogicData.Self.mLuckyGuyMultipleIndex = mLuckyIndex;
		}

		if(2 - mLuckyCount <= 0)
		{
			mLuckyCount = 2;
			SetButtonGrey (GetSub ("lucky_guy_star_button"), true);
		}
		else 
			SetButtonGrey (GetSub ("lucky_guy_star_button"), false);
		
		SetText ("lucky_guy_multiple_label", luckyGuyMultiple.ToString ());
		SetText ("lucky_guy_num", (2 - mLuckyCount).ToString () + "/2");
		
		string strDes = DataCenter.mActiveLuckyGuyConfig.GetRecord (1001)["DESC"];
		strDes = strDes.Replace ("\\n", "\n");
		SetText ("lucky_guy_describe_label", strDes);
		
		float fMultipleAngle = GetLuckyGuyMultipleAngle (luckyGuyMultiple);
		GetSub ("pointer").transform.localEulerAngles =  new Vector3(0, 0, -fMultipleAngle);
	}

	void RotatePointer(Vector3 targetVector)
	{
		SetButtonGrey (GetSub ("lucky_guy_star_button"), true);
		GameObject pointerObj = GetSub ("pointer");
		
		RotateGameObject evt = EventCenter.Start ("RotateGameObject") as RotateGameObject;
		evt.mRotateObj = pointerObj;
		evt.mStartVector = new Vector3(0, 0, pointerObj.transform.localEulerAngles.z);
		evt.mfInitSpeedRate = 0.01f;
		evt.mfAddSpeedRate = 0.002f;
		evt.mfReduceSpeedRate = evt.mfAddSpeedRate;
		evt.mEneVector = targetVector;
		evt.mCallBack = () =>RefreshLuckyGuy ();
		evt.StartUpdate ();
	}

	void SendMessageResult(object obj)
	{
		tEvent evt = obj as tEvent;
		bool bCashExtra = evt.get ("CASH_EXTRA");
		
		if(bCashExtra)
		{
			mLuckyIndex = evt.get ("CASH_EXTRA_ID");
			mLuckyCount ++;
			
			float luckyGuyMultiple = 1.0f;
			if(mLuckyIndex != 0) 
			{
				luckyGuyMultiple = DataCenter.mActiveLuckyGuyConfig.GetRecord (mLuckyIndex)["COEFFICIENT"];
				RoleLogicData.Self.mLuckyGuyMultipleIndex  = mLuckyIndex;
			}
			
			
			float fMultipleAngle = GetLuckyGuyMultipleAngle (luckyGuyMultiple);
			RotatePointer (new Vector3(0, 0, fMultipleAngle));
		}
	}

	float GetLuckyGuyMultipleAngle(float luckyGuyMultiple)
	{
		return (luckyGuyMultiple - 1) * 10 * 30;
	}

	void SetButtonGrey(GameObject obj , bool bIsGrey)
	{
		UIImageButton button = obj.GetComponent<UIImageButton>();
		if(button != null)
			button.isEnabled = !bIsGrey;
	}
}


class Button_lucky_guy_star_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("ACTIVE_LUCKY_GUY_WINDOW", "SNED_MESSAGER", "CASH_EXTRA");
		return true;
	}
}

