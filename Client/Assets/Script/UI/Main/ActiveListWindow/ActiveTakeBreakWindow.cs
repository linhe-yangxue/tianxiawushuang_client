using UnityEngine;
using System.Collections;
using DataTable;
using System;
using Logic;

public class ActiveTakeBreakWindow : tWindow 
{
	bool mbTime1;
	bool mbTime2;
	int mCurConfigIndex;

	public override void Init ()
	{
		EventCenter.Register ("Button_take_break_get_award_button", new DefineFactory<Button_take_break_get_award_button>());
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
		mbTime1 = evt.get("TIME_1");
		mbTime2 = evt.get("TIME_2");
		RefreshTakeBreak();
		return true;
	}

	public override void onChange (string keyIndex, object objVal)
	{
		switch(keyIndex)
		{
		case "SNED_MESSAGER":
			SendMessage(objVal.ToString ());
			break;
		case "SEND_MESSAGE_RESULT":
			SendMessageResult(objVal);
			break;
		}
		base.onChange (keyIndex, objVal);
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
	
	void SendMessageResult(object obj)
	{
		tEvent evt = obj as tEvent;
		bool bTime1 = evt.get ("TIME_1");
		bool bTime2 = evt.get ("TIME_2");

		if(bTime1)
		{
			mbTime1 = bTime1;
			SetButtonGrey(GetSub ("take_break_get_award_button"), true);
			
			int iType = DataCenter.mActiveTakeBreakConfig.GetRecord (1001)["TYPE"];
			int iNum = DataCenter.mActiveTakeBreakConfig.GetRecord (1001)["NUMBER"];
			GameCommon.RoleChangeNumericalAboutRole (iType, iNum);
		}
		else if(bTime2)
		{
			mbTime2 = bTime2;
			SetButtonGrey(GetSub ("take_break_get_award_button"), true);
			
			int iType = DataCenter.mActiveTakeBreakConfig.GetRecord (1002)["TYPE"];
			int iNum = DataCenter.mActiveTakeBreakConfig.GetRecord (1002)["NUMBER"];
			GameCommon.RoleChangeNumericalAboutRole (iType, iNum);
		}
	}

	void RefreshTakeBreak()
	{
		GameObject takeBreakGetAwardButton = GetSub ("take_break_get_award_button");
		UISprite icon = GetSub ("take_break_award_icon").GetComponent<UISprite>();
		bool bIsGetAward = false;
		
		DateTime nowTime = GameCommon.NowDateTime ();
		int hour = nowTime.Hour;
		int breakPoint = DataCenter.mActiveTakeBreakConfig.GetRecord (1001)["END_TIME"];
		foreach(var r in DataCenter.mActiveTakeBreakConfig.GetAllRecord ())
		{
			DataRecord record = r.Value;
			int startTime = record["START_TIME"];
			int endTime = record["END_TIME"];
			string strDescribe = record["DESCRIPTION"];
			int iType = record["TYPE"];
			int iItemID = record["GOODS"];
			int iItemNum = record["NUMBER"];
			
			if(startTime < breakPoint) 
			{
				GameCommon.GetButtonData (takeBreakGetAwardButton).set ("WHICH_COUNT", "TIME_1");
				bIsGetAward = mbTime1;
			}
			else 
			{
				GameCommon.GetButtonData (takeBreakGetAwardButton).set ("WHICH_COUNT", "TIME_2");
				bIsGetAward = mbTime2;
			}
			
			if(hour >= startTime && hour < endTime  && !bIsGetAward)
				SetButtonGrey(takeBreakGetAwardButton, false);
			else
				SetButtonGrey(takeBreakGetAwardButton, true);
			
			if((hour < breakPoint && startTime < breakPoint) || (hour > breakPoint && startTime >= breakPoint))
			{
				SetText ("take_break_describe_label", strDescribe);
				GameCommon.SetItemIcon (icon, iType, iItemID);
				SetText ("take_break_award_num", "X" + iItemNum.ToString ());
			}
		}
	}

	void SetButtonGrey(GameObject obj , bool bIsGrey)
	{
		UIImageButton button = obj.GetComponent<UIImageButton>();
		if(button != null)
			button.isEnabled = !bIsGrey;
	}
}



class Button_take_break_get_award_button : CEvent
{
	public override bool _DoEvent ()
	{
		string strWhichCount = getObject ("WHICH_COUNT").ToString ();
		DataCenter.SetData ("ACTIVE_TAKE_BREAK_WINDOW", "SNED_MESSAGER", strWhichCount);
		return true;
	}
}

