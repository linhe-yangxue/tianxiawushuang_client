using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;
using Utilities;

enum ACTIVE_TYPE
{
	MIN = 0,
	ACTIVE_PVE_DOUBLE,
	ACTIVE_RECHARGE_AWARD,
	ACTIVE_CONSUME_AWARD,
	ACTIVE_LIMIT_BUY_PET,
	ACTIVE_LIMIT_BUY_GIFT_BAG,
	ACTIVE_LOGIN_GIVE_GIFT_BAG,
	ACTIVE_TASK,
	ACTIVE_RANK,
	ACTIVE_TAKE_BREAK,
	ACTIVE_UPGRADE_LEVEL,

	MAX,
}

public class ActiveListWindowUI : MonoBehaviour
{
	ActiveListWindow mActiveListWindow;
	void Start () 
	{
		mActiveListWindow = new ActiveListWindow(gameObject) { mWinName = "ACTIVE_LIST_WINDOW" };
		DataCenter.Self.registerData ("ACTIVE_LIST_WINDOW", mActiveListWindow);

		DataCenter.OpenWindow ("BACK_ACTIVE_LIST_WINDOW");
		DataCenter.OpenWindow("INFO_GROUP_WINDOW");

		DataCenter.OpenWindow ("ACTIVE_LIST_WINDOW", true);
	}

	public void FixedUpdate()
	{
		mActiveListWindow.FixedUpdate ();
	}

	void OnDestroy()
	{
		DataCenter.CloseWindow ("BACK_ACTIVE_LIST_WINDOW");
		DataCenter.Remove("ACTIVE_LIST_WINDOW");
		
		GameObject obj = GameObject.Find ("create_scene");
		if(obj != null)
			GameObject.Destroy (obj);
	}
}


public class ActiveListWindow : tWindow
{
	ACTIVE_TYPE mCurrentType = ACTIVE_TYPE.ACTIVE_PVE_DOUBLE;

	UIGridContainer mGrid;
	UIScrollView mView;
	UIPanel mPanel;
	float mViewX;

	public ActiveListWindow(GameObject obj)
	{
		mGameObjUI = obj;
		mGameObjUI.transform.name = "active_list_window";
	}

	public override void Init ()
	{
		EventCenter.Register ("Button_active_list_window_info_back", new DefineFactory<Button_active_list_window_info_back>());
		EventCenter.Register ("Button_active_window_button", new DefineFactory<Button_active_window_button>());
		EventCenter.Register ("Button_lucky_guy_recharge_button", new DefineFactory<Button_vip_recharge_button>());
		EventCenter.Register ("Button_active_shop_button", new DefineFactory<Button_ShopBtn>());
	}

	public override void OnOpen ()
	{
		DataCenter.OpenWindow ("BACK_ACTIVE_LIST_WINDOW");

		 mGrid = GetComponent<UIGridContainer>("grid");
		 mView = GetComponent<UIScrollView>("ScrollView");
		 mPanel = GetComponent<UIPanel>("ScrollView");

		Refresh (null);
	}

	public override bool Refresh (object param)
	{
		RefreshTab ();
		ShowWindow(GetFirstActiveIndex ());
		return true;
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case"CHANGE_TAB_POS":
			ChangeTabPos ((int)objVal);
			break;
		case "SHOW_WINDOW":
			ShowWindow((int)objVal);
			break;
		case"UI_HIDDEN":
			UiHidden((bool)objVal);
			break;
		}
	}

	public void UiHidden(bool isHidden)
	{
		SetVisible (isHidden, "button_scrollview_context", "active_shop_button", "center_background");
//		SetVisible (!isHidden, "black_background");
		
		if(isHidden)
		{
			DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
			DataCenter.OpenWindow ("BACK_ACTIVE_LIST_WINDOW");
		}
		else 
		{
			DataCenter.CloseWindow ("INFO_GROUP_WINDOW");
			DataCenter.CloseWindow ("BACK_ACTIVE_LIST_WINDOW");
		}
	}

	void RefreshTab()
	{
		SetVisible ("active_list_button_back_btn", false);
		SetVisible ("active_list_button_forward_btn", true);

//		UIGridContainer grid = GetComponent<UIGridContainer>("grid");
		mGrid.MaxCount  = GetActiveCount ();
		int index = 0;
		foreach(var v in DataCenter.mOperateEventConfig.Records ())
		{
			if(v["INDEX"] == 0 || !activeIsValid (v["INDEX"])) continue;

			GameObject subCell = mGrid.controlList[index];
			subCell.transform.name = "active_window_button";

			if(index == 0) GameCommon.ToggleTrue (subCell);

			string title = v["TITLE"];
			int configIndex = v["INDEX"];

			foreach (UILabel l in subCell.GetComponentsInChildren<UILabel>())
			{
				if(l != null) l.text = title;
			}
			GameCommon.GetButtonData (subCell).set ("POS", index);
			GameCommon.GetButtonData (subCell).set ("INDEX", configIndex);

			index ++;
		}

		UIScrollView view = GetComponent<UIScrollView>("ScrollView");
		view.ResetPosition ();
	}

	public void FixedUpdate()
	{
		if(mView.transform.position.x != mViewX)
		{
			mViewX = mView.transform.position.x;
			SetVisible ("active_list_button_back_btn", !mPanel.IsVisible (mGrid.controlList[0].transform.position));
			SetVisible ("active_list_button_forward_btn", !mPanel.IsVisible (mGrid.controlList[mGrid.MaxCount - 1].transform.position));
		}
	}

	void ChangeTabPos(int pos)
	{
//		UIGridContainer grid = GetComponent<UIGridContainer>("grid");
//		UIScrollView view = GetComponent<UIScrollView>("ScrollView");
//		UIPanel panel = GetComponent<UIPanel>("ScrollView");

		if(pos < 0 || pos > mGrid.MaxCount - 1) return;

		if(pos == 0 || pos == mGrid.MaxCount - 1) 
		{
			mView.SetDragAmount (pos/(mGrid.MaxCount - 1), 0, false);
			return;
		}

		bool bBack = mPanel.IsVisible (mGrid.controlList[pos - 1].transform.position);        // panel.ConstrainTargetToBounds (backObj.transform, false);
		bool bBefore = mPanel.IsVisible (mGrid.controlList[pos + 1].transform.position);     // panel.ConstrainTargetToBounds (beforeObj.transform, false);

		if(!bBack) 
		{
			mView.SetDragAmount ((float)(pos - 1)/(float)mGrid.MaxCount, 0, false);
		}
		else if(!bBefore)
		{
			mView.SetDragAmount ((float)(pos + 1)/(float)mGrid.MaxCount, 0, false);
		}

	}

	void ShowWindow(int configIndex)
	{
		ACTIVE_TYPE type = (ACTIVE_TYPE)((int)DataCenter.mOperateEventConfig.GetData (configIndex, "EVENT_TYPE"));

		DataCenter.CloseWindow (mCurrentType.ToString () + "_WINDOW");
		DataCenter.OpenWindow (type.ToString () + "_WINDOW", configIndex);
		mCurrentType = type;
	}

	int GetActiveCount()
	{
		int count = 0;
		foreach(var v in DataCenter.mOperateEventConfig.Records ())
		{
			if(v["INDEX"] != 0  && activeIsValid (v["INDEX"])) count ++;
		}

		return count;
	}

	int GetFirstActiveIndex()
	{
		foreach(var v in DataCenter.mOperateEventConfig.Records ())
		{
			int index = v["INDEX"];
			if(index != 0 && activeIsValid (index)) return index;
		}

		return 0;
	}

	bool activeIsValid(int configIndex)
	{
		DataRecord record = DataCenter.mOperateEventConfig.GetRecord (configIndex);
		string strStartTime = record["START_TIME"];
		string strEndTime = record["AWARD_TIME"];
		string[] strStartTimes = strStartTime.Split ('_');
		string[] strEndTimes = strEndTime.Split ('_');
		DateTime startData = new DateTime(Convert.ToInt32 (strStartTimes[0]), Convert.ToInt32 (strStartTimes[1]), Convert.ToInt32 (strStartTimes[2]), 0, 0, 0);
		DateTime endData = new DateTime(Convert.ToInt32 (strEndTimes[0]), Convert.ToInt32 (strEndTimes[1]), Convert.ToInt32 (strEndTimes[2]), 23, 59, 59);

		Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
		Int64 endSeconds = GameCommon.DateTime2TotalSeconds(endData);
		Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ());

		if(nowSeconds < startSeconds || nowSeconds > endSeconds) return false;

		return true;
	}

}

public class ActiveTotalWindow : tWindow
{
	public int mConfigIndex = 0;
	public string mDesLabelName = "";
	public string mCountdownLabelName = "";

	public override void Open (object param)
	{
		base.Open (param);
		mConfigIndex = (int)param;
	}

	public override void OnOpen ()
	{
		RefreshFixationComponetns();
	}

	public void RefreshFixationComponetns()
	{
		DataRecord record = DataCenter.mOperateEventConfig.GetRecord (mConfigIndex);
		
		string strDes = record["DESCRIBE"];
		SetText (mDesLabelName, strDes);

		if(GameCommon.FindComponent<UILabel>(GetSub (mCountdownLabelName), "active_label") != null)
		{
			GameCommon.SetUIVisiable (GetSub (mCountdownLabelName), "active_label", !IsAwardTime ());
			GameCommon.SetUIVisiable (GetSub (mCountdownLabelName), "award_label", IsAwardTime ());
		}
		
		CountdownUI countdownUI = GetComponent<CountdownUI>(mCountdownLabelName);
		if(countdownUI != null) MonoBehaviour.Destroy (countdownUI);

		string endTimeField = IsAwardTime () ? "AWARD_TIME" : "END_TIME";
		SetCountdownTime (mCountdownLabelName, GetEndSeconds (endTimeField), new CallBack(this, "ActiveIsOver", endTimeField));
	}

	public bool IsAwardTime()
	{
		Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ());

		return (nowSeconds > GetEndSeconds ("END_TIME") && nowSeconds < GetEndSeconds ("AWARD_TIME"));
	}

	public Int64 GetEndSeconds(string endTimeField)
	{
		DataRecord record = DataCenter.mOperateEventConfig.GetRecord (mConfigIndex);
		string strEndTime = record[endTimeField];
		string[] strEndTimes = strEndTime.Split ('_');
		DateTime endData = new DateTime(Convert.ToInt32 (strEndTimes[0]), Convert.ToInt32 (strEndTimes[1]), Convert.ToInt32 (strEndTimes[2]), 23, 59, 59);
		Int64 endSeconds = GameCommon.DateTime2TotalSeconds(endData);
		
		return endSeconds;
	}

	public void ActiveIsOver(object param)
	{
		if(GetEndSeconds ("AWARD_TIME") != GetEndSeconds ("END_TIME") && param.ToString() == "END_TIME")
		{
			RefreshFixationComponetns();
			CanGetAwardThenRefresh();
			return ;
		}

		DataCenter.OpenWindow ("ACTIVE_LIST_WINDOW");
	}

	public virtual void CanGetAwardThenRefresh()
	{

	}

	public void SetButtonGrey(GameObject obj , bool bIsGrey)
	{
		UIImageButton button = obj.GetComponent<UIImageButton>();
		if(button != null)
			button.isEnabled = !bIsGrey;
		
		obj.GetComponent<UIButtonEvent>().enabled = !bIsGrey;
	}

}


//-------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------


class Button_active_list_window_info_back : CEvent
{
	public override bool _DoEvent ()
	{
		MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}
}

class Button_active_window_button : CEvent
{
	public override bool _DoEvent ()
	{
		int pos = (int)getObject ("POS");
		DataCenter.SetData ("ACTIVE_LIST_WINDOW", "CHANGE_TAB_POS", pos);

		int configIndex = (int)getObject ("INDEX");
		DataCenter.SetData ("ACTIVE_LIST_WINDOW", "SHOW_WINDOW", configIndex);
		return true;
	}
}


public class Button_active_shop_button : CEvent
{
	public override bool _DoEvent()
	{
		if(GameCommon.bIsWindowOpen (ACTIVE_TYPE.ACTIVE_LIMIT_BUY_PET.ToString () + "_WINDOW"))
		{
			DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LIMIT_BUY_PET.ToString () + "_WINDOW", "DELET", true);
			GameObject obj = getObject ("BUTTON") as GameObject;
			obj.StartCoroutine (OpenShopWindow ());
		}
		else 
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);

		return true;
	}

	IEnumerator OpenShopWindow()
	{
		yield return new WaitForEndOfFrame();
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
	}
}

//---------------------------------------------------------------------------------
//---------------------------------------------------------------------------------
public class CS_UpdateHDData : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		switch ((STRING_INDEX)result)
		{
		case STRING_INDEX.ERROR_NONE:
			respEvt.set ("TIME_1", (bool)get ("TIME_1"));
			respEvt.set ("TIME_2", (bool)get ("TIME_2"));
			respEvt.set ("CASH_EXTRA", (bool)get ("CASH_EXTRA"));
			respEvt.set ("HD_ID", (int)get ("HD_ID"));

			int configIndex = (int)get ("CONFIG_INDEX");
			ACTIVE_TYPE type = (ACTIVE_TYPE)((int)DataCenter.mOperateEventConfig.GetData (configIndex, "EVENT_TYPE"));
			DataCenter.SetData (type.ToString () + "_WINDOW", "SEND_MESSAGE_RESULT", respEvt);
			break;
		}
	}
}

public class CS_RequestHDData : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int configIndex = (int)get ("CONFIG_INDEX");
		ACTIVE_TYPE type = (ACTIVE_TYPE)((int)DataCenter.mOperateEventConfig.GetData (configIndex, "EVENT_TYPE"));
		DataCenter.SetData (type.ToString () + "_WINDOW", "REFRESH", respEvt);
	}
}



