using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable ;
using System.Collections.Generic;

enum SIGN_BUTTON_TYPE
{
	NONE,
	DAILY,
	LUXURY
}

public class ActivitySignWindow : tWindow 
{
	private SIGN_BUTTON_TYPE mCurrSignBtnType = SIGN_BUTTON_TYPE.NONE;
	int iIndexNum = 0;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_icon_tips_btn", new DefineFactory<Button_icon_tips_btn>());
		EventCenter.Self.RegisterEvent ("Button_title_group(Clone)_0", new DefineFactory<Button_daily_sign>());
		EventCenter.Self.RegisterEvent ("Button_title_group(Clone)_1", new DefineFactory<Button_luxury_sign>());
		EventCenter.Self.RegisterEvent ("Button_luxury_recharge_button", new DefineFactory<Button_luxury_recharge_button>());
		EventCenter.Self.RegisterEvent ("Button_luxury_receive_button", new DefineFactory<Button_luxury_receive_button>());

		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mDailySignEvent.GetAllRecord ())
		{
			if(v.Key != null)
			{
				iIndexNum ++;
			}
		}
	}

	public override void Open(object param)
	{
		base.Open (param);
		GameCommon.ToggleTrue(GameCommon.FindObject (mGameObjUI, "title_group(Clone)_0"));
		GameCommon.FindObject (mGameObjUI, "daily_grid").SetActive (true);
		GameCommon.FindObject (mGameObjUI, "luxury_info").SetActive (false);
		mCurrSignBtnType = SIGN_BUTTON_TYPE.DAILY;

//		Refresh (param);
	}

    private void RefreshNewMark() 
    {
        UIGridContainer _titleGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI,"title_grid");
        if(_titleGrid == null)
            return;
        if(_titleGrid.MaxCount < 2)
            return;
        //每日签到页签
        GameObject _daySignBtnObj = _titleGrid.controlList[0];
        GameCommon.SetNewMarkVisible(_daySignBtnObj, SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_DAILY_SIGN));
        //豪华签到页签
        GameObject _luxurySignBtnObj = _titleGrid.controlList[1];
        GameCommon.SetNewMarkVisible(_luxurySignBtnObj,SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_LUXURY_SIGN));
        //刷新活动标签页
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_SIGN);
    }
	public override bool Refresh(object param)
	{
		//每日签到去掉后有报错信息
		UpdateDailySignUI(ActivitySignLogicData.Self.mCurIndex);
        RefreshNewMark();
		return base.Refresh (param);
	}
	public override void OnClose ()
	{
		mCurrSignBtnType = SIGN_BUTTON_TYPE.NONE;

		base.OnClose ();
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		
		switch(keyIndex)
		{
		case "DAILY_SIGN_BUTTON":
			if(mCurrSignBtnType != SIGN_BUTTON_TYPE.DAILY)
			{
				mCurrSignBtnType = SIGN_BUTTON_TYPE.DAILY;
				GameCommon.ToggleTrue(GameCommon.FindObject (mGameObjUI, "title_group(Clone)_0"));
				GameCommon.FindObject (mGameObjUI, "daily_grid").SetActive (true);
				GameCommon.FindObject (mGameObjUI, "luxury_info").SetActive (false);
				UpdateDailySignUI(ActivitySignLogicData.Self.mCurIndex);
			}
			break;
		case "LUXURY_SIGN_BUTTON":
			if(mCurrSignBtnType != SIGN_BUTTON_TYPE.LUXURY)
			{
				mCurrSignBtnType = SIGN_BUTTON_TYPE.LUXURY;
				GameCommon.ToggleTrue(GameCommon.FindObject (mGameObjUI, "title_group(Clone)_1"));
				GameCommon.FindObject (mGameObjUI, "daily_grid").SetActive (false);
				GameCommon.FindObject (mGameObjUI, "luxury_info").SetActive (true);
				UpdateLuxurySignUI();
			}
			break;
		case "SEND_MESSAGE":
			SendDailySign((int)objVal);
			break;
		case "SIGN_SUCCESS":
			SuccessGetRewards((string)objVal);
			break;
		case "CAN_GET":
			//			ItemDataBase items = (ItemDataBase)objVal;
			CanGetRewards((string)objVal);
			break;
		case "LUXURY_SUCCESS":
//			ItemDataBase items = (ItemDataBase)objVal;
			GetRewards((string)objVal);
			break;
            //case "RESET_DAILY_NEWMARK":
            //break;
		}
	}
	void CanGetRewards(string text)
	{
		SC_LuxurySignQuery luxuryItem=JCode.Decode<SC_LuxurySignQuery>(text);
		if (luxuryItem.isRecharge == 1 && luxuryItem .isSign == 0) {
			GameCommon.FindObject (mGameObjUI, "luxury_recharge_button").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "luxury_receive_button").SetActive (true);
			GameCommon.FindObject (mGameObjUI, "luxury_have_receive_button").SetActive (false);
		} else if (luxuryItem.isRecharge == 1 && luxuryItem .isSign == 1) {
			GameCommon.FindObject (mGameObjUI, "luxury_recharge_button").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "luxury_receive_button").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "luxury_have_receive_button").SetActive (true);
		} else {
			GameCommon.FindObject (mGameObjUI, "luxury_recharge_button").SetActive (true);
			GameCommon.FindObject (mGameObjUI, "luxury_receive_button").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "luxury_have_receive_button").SetActive (false);
		}
	}
	void GetRewards(string text)
	{
		SC_LuxurySign luxuryItem = JCode.Decode <SC_LuxurySign> (text);
		if (null == luxuryItem)
			return;
		if (luxuryItem.isSignSuccess) {
			GameCommon.FindObject (mGameObjUI, "luxury_recharge_button").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "luxury_receive_button").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "luxury_have_receive_button").SetActive (true);
			PackageManager.UpdateItem(luxuryItem.item);
            //added by xuke 刷新红点状态
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_LUXURY_SIGN, false);
            RefreshNewMark();
            //end
		}
	}

	void SendDailySign(int dayIndex)
	{
		if( dayIndex > ActivitySignLogicData.Self.mCurIndex )
		{
			return;
		}else if(dayIndex == ActivitySignLogicData.Self.mCurIndex)
		{
			NetManager.RequestDailySignReward();
		}
	}

	public void SuccessGetRewards(string text)
	{
		SC_DailySign dailySignItem = JCode.Decode<SC_DailySign>(text);
		if( null == dailySignItem )
			return ;
		
		if(dailySignItem.isSignSuccess)
		{
			UIGridContainer mGrid = GameCommon.FindObject (mGameObjUI, "daily_grid").GetComponent<UIGridContainer>();
			mGrid.MaxCount = iIndexNum;

			GameObject obj = mGrid.controlList[ActivitySignLogicData.Self.mCurIndex - 1].transform.Find ("item_get_icon/item_get_effect").gameObject;
			GameObject itemGetIconObj = mGrid.controlList[ActivitySignLogicData.Self.mCurIndex - 1].transform.Find ("item_get_icon").gameObject;
			obj.SetActive (true);
			itemGetIconObj.SetActive (true);

			TweenScale twS = TweenScale.Begin (itemGetIconObj, 0.01f, new Vector3 (1.4f, 1.4f, 1.4f));
			TweenScale twScaleLater = itemGetIconObj.gameObject.GetComponent<TweenScale>();
			twScaleLater.from = new Vector3 (1.4f, 1.4f, 1.4f);
			twScaleLater.to = new Vector3 (1.0f, 1.0f, 1.0f);
			twScaleLater.duration = 0.3f;
			twScaleLater.delay = 0.1f;
			
			mGrid.controlList[ActivitySignLogicData.Self.mCurIndex - 1].transform.Find ("today_chack").gameObject.SetActive (false);
			GlobalModule.DoLater (() => 
            {
                if (obj != null)
                    obj.SetActive(false);
            }, 1.8f);

			ActivitySignLogicData.Self.mHasSigned = true;
			GlobalModule.DoLater (() => UpdateDailySignUI(ActivitySignLogicData.Self.mCurIndex), 1.0f);

			List<ItemDataBase> itemDataList = PackageManager.UpdateItem (dailySignItem.item);
//			GlobalModule.DoLater(() => DataCenter.OpenWindow ("GET_REWARDS_WINDOW", new ItemDataProvider(itemDataList)), 0.5f);
			GlobalModule.DoLater(() => DataCenter.OpenWindow ("AWARDS_TIPS_WINDOW", itemDataList), 0.5f);
            //added by xuke 刷新红点
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_DAILY_SIGN, false);
            RefreshNewMark();
            //end
		}
	}

	public void UpdateDailySignUI(int curDay)
	{
		UIGridContainer mGrid = GameCommon.FindObject (mGameObjUI, "daily_grid").GetComponent<UIGridContainer>();
		mGrid.MaxCount = iIndexNum;
		int iIndex = 1001;

		for(int i = 0; i < mGrid.MaxCount; i++)
		{
			GameObject obj = mGrid.controlList[i];

			int iItemID = TableCommon.GetNumberFromDailySign (iIndex + i, "ITEM_ID");
			int iItemNum = TableCommon.GetNumberFromDailySign (iIndex + i, "COUNT");
			int iVipLevel = TableCommon.GetNumberFromDailySign (iIndex + i, "VIP_LEVEL");
			GameCommon.SetUIText (obj, "title_num", (i + 1).ToString ());
			GameCommon.SetUIText (obj, "item_num", "x" + iItemNum.ToString ());

			UISprite _uisprite = obj.transform.Find ("icon_tips_btn/item_icon").gameObject.GetComponent <UISprite>();
			obj.transform.Find ("item_get_icon/item_get_effect").gameObject.SetActive (false);

			GameCommon.SetOnlyItemIcon(obj, "item_icon", iItemID);
			if(i + 1 > curDay)
			{
				AddButtonAction (GameCommon.FindObject (obj, "icon_tips_btn"), () => GameCommon.SetItemDetailsWindow (iItemID));
			}

			if(iVipLevel == 0)
			{
				GameCommon.FindObject (obj, "vip_num").SetActive (false);
			}else 
			{
				GameCommon.SetUIText (obj, "vip_num", "V" + iVipLevel.ToString () + "双倍");
			}

            obj.transform.Find ("item_get_icon").gameObject.SetActive (i + 1 < curDay || (i + 1 == curDay && ActivitySignLogicData.Self.mHasSigned));
//			obj.transform.Find ("item_get_icon/item_get_effect").gameObject.SetActive (i + 1 < curDay || (i + 1 == curDay && !ActivitySignLogicData.Self.mHasSigned));
			obj.transform.Find ("today_chack").gameObject.SetActive (i + 1 == curDay && !ActivitySignLogicData.Self.mHasSigned);
			GameCommon.GetButtonData(GameCommon.FindObject(obj, "icon_tips_btn")).set("DAY_INDEX", i + 1);
		}
	}
	public void UpdateLuxurySignUI()
	{
		//GameCommon.FindObject (mGameObjUI, "luxury_recharge_button").SetActive (true);
		//GameCommon.FindObject (mGameObjUI, "luxury_receive_button").SetActive (false);
		//GameCommon.FindObject (mGameObjUI, "luxury_have_receive_button").SetActive (false);
		//int iGroupId = TableCommon.GetNumberFromCharacterLevelExp(RoleLogicData.Self.character.level, "SIGN_GROUP_ID");
		//List<ItemDataBase> items = GameCommon.GetItemGroup(iGroupId,false);
		string tm = DataCenter.mCharacterLevelExpTable.GetData (RoleLogicData.Self.character.level, "SIGN_GROUP_ID");
		List<ItemDataBase> items = GameCommon.ParseItemList (tm);

		UIGridContainer mGrid = GameCommon.FindObject (mGameObjUI, "luxury_grid").GetComponent<UIGridContainer>();
		//mGrid.MaxCount = items.Count;
		mGrid.MaxCount = items.Count;
		if (mGrid.MaxCount == 2) {
			mGrid.transform.localPosition = new Vector3 (27,80,0);
		}
 		for(int i = 0; i < mGrid.MaxCount; i++)
		{
			GameObject obj = mGrid.controlList[i];
			int iItemNum = items[i].itemNum;
			int iItemID = items[i].tid;
			GameCommon.SetOnlyItemIcon(obj, "item_icon", iItemID);
			GameCommon.SetOnlyItemCount(obj, "item_num", iItemNum);
			AddButtonAction (GameCommon.FindObject (obj, "icon_tips_btn"), () => GameCommon.SetItemDetailsWindow (iItemID));
		}
		GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "luxury_receive_button")).set("SET_ITEM_DATA", items);
	}
	
}
public class Button_icon_tips_btn : CEvent
{
	public override bool _DoEvent()
	{
		int dayIndex = get ("DAY_INDEX");
		DataCenter.SetData ("ACTIVITY_SIGN_WINDOW", "SEND_MESSAGE", dayIndex);
		return true;
	}
}
public class Button_daily_sign : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_SIGN_WINDOW", "DAILY_SIGN_BUTTON", true);
		return true;
	}
}
public class Button_luxury_sign : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_SIGN_WINDOW", "LUXURY_SIGN_BUTTON", true);
		NetManager.RequestLuxuryQueryd();
		return true;
	}
}

public class Button_luxury_recharge_button : CEvent
{
	public override bool _DoEvent()
	{
        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, () =>
        {
            DataCenter.OpenWindow("ACTIVITY_WINDOW");
            DataCenter.SetData("ACTIVITY_SIGN_WINDOW", "LUXURY_SIGN_BUTTON", true);
            NetManager.RequestLuxuryQueryd();
        }, CommonParam.rechageDepth);

		return true;
	}
}

public class Button_luxury_receive_button : CEvent
{
	public override bool _DoEvent()
	{
//		ItemDataBase itemDate = (ItemDataBase)getObject ("SET_ITEM_DATA");
		NetManager.RequestLuxuryReward();
		//DataCenter.SetData("ACTIVITY_SIGN_WINDOW", "LUXURY_SUCCESS", true);
		return true;
	}
}
