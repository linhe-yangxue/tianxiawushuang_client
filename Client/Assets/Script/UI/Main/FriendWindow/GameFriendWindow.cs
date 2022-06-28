using UnityEngine;
using System.Collections.Generic;
using Logic;
using System;
using DataTable;
using System.Linq;

public class GameFriendWindow : tWindow
{
	int mMaxFriendNum;
	int mCurrentFriendNum;

	public GameFriendWindow(GameObject objParent)
	{
		mGameObjUI = GameCommon.FindObject (objParent, "game_friend_window");
	}	
	
	public override void Init()
	{
		mMaxFriendNum = (int)DataCenter.mSpiritSendConfig.GetData(1, "FRIEND_NUM");
	}
	
	public override void Open(object param)
	{
		base.Open (param);

		/*
		if(!get ("NOT_GET_DATA_FROM_NET"))
		{
			tEvent evt = Net.StartEvent("CS_RequestFriendList");
			evt.set ("IS_GAME_FRIEND_LIST",  true);
			evt.DoEvent();

			set ("NOT_GET_DATA_FROM_NET", true);
		}
		*/
		FriendNetEvent.RequestFriendList(FriendListSuccess, FriendListFail);
	}

	void FriendListSuccess(string text) {

		GuideManager.Notify(GuideIndex.EnterWorldMap);
		GuideManager.Notify(GuideIndex.EnterWorldMap2);
		GuideManager.Notify(GuideIndex.EnterWorldMap3);

		FriendLogicData logicData = new FriendLogicData();
		logicData = JCode.Decode<FriendLogicData>(text);

		DataCenter.RegisterData("GAME_FRIEND_LIST", logicData);

		FriendNetEvent.RequestGetSpiritSendList(GetSpiritSendListSuccess, GetSpiritSendListFail);

        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FRIEND_CHANGE, false);
	}

	void FriendListFail(string text) {
		DEBUG.Log("FriendErr:" + text);
	}

	void GetSpiritSendListSuccess(string text) {
		SC_RequestGetSpiritSendList ls = JCode.Decode<SC_RequestGetSpiritSendList>(text);
		FriendLogicData friendLogicData = DataCenter.GetData("GAME_FRIEND_LIST") as FriendLogicData;
		if(ls.arr.Length > 0) {
			//begin
			List<string> sendSpriritList = ls.arr.ToList();
			for(int i =0; i < friendLogicData.arr.Length; i++)
			{
				friendLogicData.arr[i].enableSendSpirit = (sendSpriritList.IndexOf(friendLogicData.arr[i].friendId) == -1);
			}
			//end
		}else {
			foreach(FriendData fdd in friendLogicData.arr) {
				fdd.enableSendSpirit = true;
			}
		}
		DataCenter.SetData ("GAME_FRIEND_WINDOW", "REFRESH", true);
	}

	void GetSpiritSendListFail(string text) {
		DEBUG.Log("Request Get Spirit Send List Err:" + text);
	}

	public override bool Refresh(object param)
	{
		base.Refresh (param);
		int iPraiseFriendNum = 0;

        FriendLogicData friendLogicData = DataCenter.GetData("GAME_FRIEND_LIST") as FriendLogicData;
        if (friendLogicData != null)
        {
            FriendData[] friendList = friendLogicData.arr;
            mCurrentFriendNum = friendList.Length;
            DataCenter.Set("CURRENT_FRIEND_NUM", mCurrentFriendNum);
            if (mCurrentFriendNum == 0)
            {
                string str = DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_FRIEND_TIPS, "STRING_CN");
                GameCommon.SetUIText(mGameObjUI, "label_no_friend_tips", str);
                GameCommon.SetUIVisiable(mGameObjUI, "label_no_friend_tips", true);
            }
            else
            {
                GameCommon.SetUIVisiable(mGameObjUI, "label_no_friend_tips", false);
            }
            int maxCount = friendList.Length;
            GameCommon.SetUIText(mGameObjUI, "game_friend_num_label", maxCount.ToString() + " / " + mMaxFriendNum.ToString());
            string maxCountTips = string.Format(TableCommon.getStringFromStringList(STRING_INDEX.FRIEND_MAX_COUNT_TIPS), mMaxFriendNum);
            GameCommon.SetUIText(mGameObjUI, "max_count_tip_label", maxCountTips);
            UIGridContainer grid = GameCommon.FindObject(mGameObjUI, "grid").GetComponent<UIGridContainer>();
            grid.MaxCount = maxCount;
            // sort 
            Array.Sort(friendList, new FriendsSortRule());
			for (int i = 0; i < maxCount; i++) {
				GameObject subcell = grid.controlList [i];
                if (subcell.activeSelf == false)
                {
                    subcell.SetActive(true);
                    grid.Reposition();
                }
				SetButtonData (subcell, "delete_friend_button", friendList [i]);
				SetButtonData (subcell, "praise_friend_button", friendList [i]);
				SetButtonData (subcell, "visit_friend_button", friendList [i]);
				
				FriendData friendInfo = friendList [i];

				long iOffTime = CommonParam.NowServerTime () - friendInfo.lastLoginTime;
				if (iOffTime < 60)
					GameCommon.SetUIText (subcell, "off_time_label", "0分钟前");
				else if (iOffTime < 3600 && iOffTime >= 60)
					GameCommon.SetUIText (subcell, "off_time_label", (iOffTime / 60).ToString () + "分钟前");
				else if (iOffTime < 3600 * 24 && iOffTime >= 3600)
					GameCommon.SetUIText (subcell, "off_time_label", (iOffTime / 3600).ToString () + "小时前");
				else if (iOffTime >= 3600 * 24)
					GameCommon.SetUIText (subcell, "off_time_label", (iOffTime / (3600 * 24)).ToString () + "天前");

				GameCommon.SetUIVisiable (subcell, "praise_friend_button", friendInfo.enableSendSpirit);
				GameCommon.SetUIVisiable (subcell, "praise_friend_cd_button", !friendInfo.enableSendSpirit);
				SetText ("praise_friend_cd_label", "");

				int iPetID = friendInfo.mModel;
				GameCommon.SetStrengthenLevelLabel (subcell, friendInfo.mStrengthenLevel, "strengthen_level_label");
				GameCommon.SetUIText (subcell, "player_name_label", friendInfo.name);
				GameCommon.SetUIText (subcell, "level_label", "Lv " + friendInfo.level);
				GameCommon.SetRoleIcon (GameCommon.FindObject (subcell, "pet_icon").GetComponent<UISprite> (), friendInfo.icon, GameCommon.ROLE_ICON_TYPE.PHOTO);
				//GameCommon.SetPetIconWithElementAndStarAndLevel (subcell, "pet_icon", "element", "star_level_label", "level_label", friendInfo.mLevel, friendInfo.mModel);
			}
		} else {
			GameCommon.SetUIText (mGameObjUI, "game_friend_num_label", "0 / " + mMaxFriendNum.ToString ());
		}	
		SetPraiseAllFriendButton(iPraiseFriendNum);
        GuideManager.Notify(GuideIndex.EnterFriendWindow);
		return true;
	}

	void SetPraiseAllFriendButton(int iPraiseFriendNum)
	{
		UIImageButton button = GameCommon.GetUIButton (mGameObjUI, "praise_all_friend_button");
		if(button != null)
			button.isEnabled = iPraiseFriendNum == 0? false : true;
	}

	public void PraiseFreindCDTimenOver(GameObject obj)
	{
		GameCommon.SetUIVisiable (obj, "praise_friend_button", true);
		GameCommon.SetUIVisiable (obj, "praise_friend_cd_button", false);
	}
	
	
	void SetButtonData(GameObject obj, string strName, FriendData friendData)
	{
		NiceData buttonData = GameCommon.GetButtonData (obj, strName);
		if(buttonData != null)
		{
			buttonData.set ("FRIEND_ID", friendData.friendId);
			buttonData.set ("MAIL_ID", friendData.mailID);
			buttonData.set ("FRIEND_NAME", friendData.name);
			buttonData.set ("WINDOW_NAME", "FRIEND_WINDOW");
            buttonData.set("FRIEND_DATA", friendData);
		}
		else DEBUG.Log ("button data is null >" + strName);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch(keyIndex)
		{
		case "CLEAN_ALL_UI":
			UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
			grid.MaxCount = 0;
			break;
		case"PRAISE_FRIEND_CD":
			DeleteAndPraise (false, true);
			break;
		case "SEND":
			DeleteAndPraise (true, false);
			break;
		case "DELETED_REFRESH":
			DeleteAndPraise (false, false);
            break;
        }
	}
	
	void DeleteAndPraise(bool bIsSend, bool b)
	{
		object obj;
		getData ("CURRENT_FRIEND_DATA", out obj);
		NiceData friendData = obj as NiceData;
		if(friendData != null)
		{
			if(bIsSend)
			{
				string strMyName = RoleLogicData.Self.name;
				string iFriendID = Convert.ToString(friendData.getObject ("FRIEND_ID"));
				bool isDelete = (bool)friendData.getObject ("IS_DELETE");
				if(isDelete)
				{
					//message of delete friend
					FriendNetEvent.RequestDeleteFriend(iFriendID, RequestDeleteFriendSuccess, RequestDeleteFriendFail);
					/*
					tEvent evt = Net.StartEvent("CS_DeleteFriend");
					evt.set("FRIEND_ID", iFriendID);
					evt.DoEvent();
					*/
				}
				else
				{
					//message of send friend point
					tEvent evt = Net.StartEvent("CS_SendSpirit");
					evt.set("FRIEND_ID", iFriendID);
					evt.set("FRIEND_NAME", strMyName);
					evt.DoEvent();
				}
			}
			else
			{
				GameObject parentObj = friendData.getObject ("PARENT") as GameObject;
				if(b)
				{
					GameCommon.SetUIVisiable (parentObj, "praise_friend_button", false);
					GameCommon.SetUIVisiable (parentObj, "praise_friend_cd_button", true);
					SetText ("praise_friend_cd_label", "23:59:59");
					int iPraiseFriendCDTime =(int)DataCenter.mGlobalConfig.GetData("PRAISE_FRIEND_CD_TIME", "VALUE");
					SetCountdown (parentObj, "praise_friend_cd_label", (Int64)(CommonParam.NowServerTime() + iPraiseFriendCDTime), new CallBack(this, "PraiseFreindCDTimenOver", parentObj));
				}
				else
				{
					parentObj.SetActive (false);
					UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
					grid.Reposition ();
					GameCommon.SetUIText (mGameObjUI, "game_friend_num_label", (--mCurrentFriendNum).ToString () + " / " + mMaxFriendNum.ToString ());
                    DataCenter.Set("CURRENT_FRIEND_NUM", mCurrentFriendNum);
				}
			}
		}
		else DEBUG.Log ("friend data  is null");
	}


	void RequestDeleteFriendSuccess(string text) {
		SC_RequestDeleteFriend dFriend = JCode.Decode<SC_RequestDeleteFriend>(text);

		if(dFriend.ret == (int)STRING_INDEX.ERROR_NONE) {
			DataCenter.SetData("GAME_FRIEND_WINDOW", "DELETED_REFRESH", true);
			DEBUG.Log("DeleteFriendSuccess:" + text);
		}
	}

	void RequestDeleteFriendFail(string text) {

		DEBUG.Log("DeleteFriendErr:" + text);
	}
}

public class Button_visit_friend_button : CEvent
{
	public override bool _DoEvent()
	{
		string iFriendID = Convert.ToString(getObject ("FRIEND_ID"));
		string strName = getObject ("FRIEND_NAME").ToString ();
		string strWindowName = getObject ("WINDOW_NAME").ToString ();
		FriendNetEvent.RequestVisitPlayer(iFriendID, RequestVisitPlayerSuccess, ResponeVisitPlayerFail);
		/*
		tEvent evt = Net.StartEvent("CS_VisitFriend");
		evt.set("FRIEND_ID",  iFriendID);
		evt.set ("FRIEND_NAME", strName);
		evt.set ("WINDOW_NAME", strWindowName);
		evt.DoEvent();
		*/
		
		return true;
	}

	void RequestVisitPlayerSuccess(string text) {
		SC_ResponeVisitPlayer vPlayer = JCode.Decode<SC_ResponeVisitPlayer>(text);
		if(vPlayer.ret == (int)STRING_INDEX.ERROR_NONE) {
			string strWindowName = get ("WINDOW_NAME");
			//respEvt.set ("WINDOW_NAME", strWindowName);
			string iFriendID = get ("FRIEND_ID").ToString();
			//respEvt.set ("FRIEND_ID", iFriendID);

			if (strWindowName == "RANK_WINDOW")
				GlobalModule.ClearAllWindow();
			else if(strWindowName == "FRIEND_WINDOW")
				GlobalModule.ClearAllWindow();
            else if(strWindowName == UIWindowString.arena_record_window 
                || strWindowName == UIWindowString.arena_rank_window)
            {
                //by chenliang
                //begin

                //关闭主场景
                GlobalModule.ClearAllWindow();

                //end
                ArenaBase.openFourWindow(false);
                DataCenter.CloseWindow(UIWindowString.arena_record_window);
                DataCenter.CloseWindow(UIWindowString.arena_rank_window);
            }
            else if(strWindowName == UIWindowString.arena_main_window)
            {
                //by chenliang
                //begin

                //关闭主场景
                GlobalModule.ClearAllWindow();

                //end
                ArenaBase.openFourWindow(false);
            }
            //by chenliang
            //begin

            else if(strWindowName == "BOSS_RAID_WINDOW" ||
                    strWindowName == "RAMMBOCK_WINDOW"  ||
                    strWindowName == "PEAK_PVP_WINDOW"  ||
                    strWindowName == "UNION_WINDOW"     ||
                    strWindowName == "RANKLIST_MAINUI_WINDOW")
            {
                tWindow tmpWin = DataCenter.GetData(strWindowName) as tWindow;
                if (tmpWin != null)
                    tmpWin.mGameObjUI.SetActive(false);
                GlobalModule.ClearAllWindow();
                DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "CLEAR_GRID", true);
            }

            //end
			else
				DataCenter.CloseWindow(strWindowName);

			DEBUG.Log("visit player...");
			DataCenter.OpenWindow ("FRIEND_VISIT_WINDOW", strWindowName);
			//DataCenter.OpenWindow ("BACK_GROUP_FRIEND_VISIT_WINDOW");
			//DataCenter.OpenWindow ("INFO_GROUP_WINDOW");

			DataCenter.SetData ("FRIEND_VISIT_WINDOW", "REFRESH", vPlayer);
		}
	}

	void ResponeVisitPlayerFail(string text) {

	}
}

public class Button_praise_friend_button : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = getObject ("BUTTON") as GameObject;
        GameObject parentObj = obj.transform.parent.gameObject;

        NiceData buttonData = obj.GetComponent<UIButtonEvent>().mData;
		if(buttonData != null)
		{
			string friendId = (string)buttonData.get("FRIEND_ID").mObj;
            FriendNetEvent.RequestSendSpirit(friendId, (string text) =>
            {
                SC_RequestSendSpirit Spirit = JCode.Decode<SC_RequestSendSpirit>(text);
                if (Spirit.ret == (int)STRING_INDEX.ERROR_NONE)
                {
                    NiceData nData = GameCommon.GetButtonData(obj);
                    FriendData fData = nData.get("FRIEND_DATA").mObj as FriendData;
                    fData.enableSendSpirit = false;
                    DataCenter.SetData("GAME_FRIEND_WINDOW", "REFRESH", true);
                    DataCenter.OnlyTipsLabelMessage(STRING_INDEX.SUCCESS_SEND_SPIRIT);
                }
            }, RequestSendSpiritFail);
		}
		else DEBUG.Log ("praise friend data  is null");

		return true;
	}

    void RequestSendSpiritFail(string text)
    {
        DataCenter.OpenMessageWindow(text);
	}
}

public class Button_delete_friend_button : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = getObject ("BUTTON") as GameObject;
		GameObject parentObj = obj.transform.parent.parent.gameObject;
		
		NiceData buttonData = obj.GetComponent<UIButtonEvent>().mData;
		if(buttonData != null)
		{
			bool bIsDelete = true;
			set ("IS_DELETE", bIsDelete);
			set ("PARENT", parentObj);
			DataCenter.SetData ("GAME_FRIEND_WINDOW", "CURRENT_FRIEND_DATA", buttonData);
			
			string name = getObject ("FRIEND_NAME").ToString ();
			DataCenter.OpenMessageOkWindow (STRING_INDEX.ERROR_FRIEND_DELETE, name, "GAME_FRIEND_WINDOW");
		}
		else DEBUG.Log ("delete friend data  is null");
		
		return true;
	}
}

public class Button_praise_all_friend_button : CEvent
{
	public override bool _DoEvent()
	{
		tEvent evt = Net.StartEvent("CS_SendSpirit");
		evt.set("FRIEND_NAME", RoleLogicData.Self.name);
		evt.DoEvent();
		return true;
	}
}



