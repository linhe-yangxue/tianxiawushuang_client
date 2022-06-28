using UnityEngine;
using System.Collections.Generic;
using Logic;
using System;
using DataTable;

public class AddFriendWindow : tWindow
{
	int mMaxFriendNum = 50;
	public AddFriendWindow(GameObject objParent)
	{
		mGameObjUI = GameCommon.FindObject (objParent, "add_friend_window");
	}
	
	public override void Init ()
	{
		mMaxFriendNum = (int)DataCenter.mSpiritSendConfig.GetData(1, "FRIEND_NUM");
		
		DataCenter.RegisterData ("FIND_FRIEND_LIST_INFO_WINDOW", new FindFriendListInfoWindow(mGameObjUI));
		DataCenter.CloseWindow ("FIND_FRIEND_LIST_INFO_WINDOW");
	}
	
	public override void Open(object param)
	{
		base.Open (param);
		/*
		tEvent evt = Net.StartEvent("CS_RequestInviteList");
		evt.DoEvent();
		*/
		FriendNetEvent.RequestFriendRequestList(RequestFriendInviteSuccess, RequestFriendInviteFail);
		DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "CLEAN_FIND_FRIEND_INPUT", true);
		DataCenter.OpenWindow ("FIND_FRIEND_LIST_INFO_WINDOW", "");
	}

	void RequestFriendInviteSuccess(string text) {
		DEBUG.Log("InviteFriend:" + text);
		FriendLogicData logicData = JCode.Decode<FriendLogicData>(text);
		DataCenter.Remove ("REQUEST_INVITE_FRIEND_LIST");
        DataCenter.RegisterData("REQUEST_INVITE_FRIEND_LIST", logicData);
		DEBUG.Log("InviteFriend:" + text + logicData.arr.Length);
//		DataCenter.SetData ("ADD_FRIEND_WINDOW", "CLEAN_ALL_UI", true);
		DataCenter.SetData ("ADD_FRIEND_WINDOW", "UPDATE_INVITE_FRIEND_NUM", true);
		DataCenter.SetData ("ADD_FRIEND_WINDOW", "REFRESH", true);

        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FRIEND_REQUEST, logicData.arr.Length > 0);
	}

	void RequestFriendInviteFail(string text) {
		/*
		DataCenter.SetData ("ADD_FRIEND_WINDOW", "CLEAN_ALL_UI", true);
		DataCenter.SetData ("ADD_FRIEND_WINDOW", "UPDATE_INVITE_FRIEND_NUM", true);
		DataCenter.SetData ("ADD_FRIEND_WINDOW", "REFRESH", true);
		*/
		DEBUG.Log("InviteFriendErr:" + text);
	}

	public override bool Refresh(object param)
	{
		base.Refresh (param);
		FriendLogicData friendLogicData =  DataCenter.GetData("REQUEST_INVITE_FRIEND_LIST") as FriendLogicData;
		if(friendLogicData != null)
		{
			FriendData[] friendList = friendLogicData.arr;
			string num = friendList.Length.ToString();
			GameCommon.SetUIText (mGameObjUI, "request_friend_num_info_label", num + " / " + mMaxFriendNum.ToString ());
			
			int iMaxCount = Convert.ToInt32 (num);
			UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "request_friend_scrollview_context_grid").GetComponent<UIGridContainer>();
			grid.MaxCount = iMaxCount;
			for(int i = 0; i < iMaxCount; i++)
			{
				GameObject subcell = grid.controlList[i];
                if (subcell.activeSelf == false)
                {
                    subcell.SetActive(true);
                    grid.Reposition();
                }
				SetButtonData (subcell, "agreed_button", friendList[i]);
				SetButtonData (subcell, "refused_button", friendList[i]);
				
				FriendData friendInfo = friendList[i];
                long iOffTime = CommonParam.NowServerTime() - friendInfo.lastLoginTime;//friendInfo.mLoginTime;
                if (iOffTime < 60) GameCommon.SetUIText(subcell, "off_time_label", TableCommon.getStringFromStringList(STRING_INDEX.ARENA_TIMES_TIPS_ONE));
                else if (iOffTime < 3600 && iOffTime >= 60) GameCommon.SetUIText(subcell, "off_time_label", (iOffTime / 60).ToString() + TableCommon.getStringFromStringList(STRING_INDEX.ARENA_TIMES_TIPS_TWO));
                else if (iOffTime < 3600 * 24 && iOffTime >= 3600) GameCommon.SetUIText(subcell, "off_time_label", (iOffTime / 3600).ToString() + TableCommon.getStringFromStringList(STRING_INDEX.ARENA_TIMES_TIPS_THREE));
                else if (iOffTime >= 3600 * 24) GameCommon.SetUIText(subcell, "off_time_label", (iOffTime / (3600 * 24)).ToString() + TableCommon.getStringFromStringList(STRING_INDEX.ARENA_TIMES_TIPS_FOUR));
				
				GameCommon.SetStrengthenLevelLabel (subcell, friendInfo.mStrengthenLevel, "strengthen_level_label");
				GameCommon.SetUIText (subcell, "player_name_label", friendInfo.name);
				GameCommon.SetRoleIcon (GameCommon.FindObject (subcell, "pet_icon").GetComponent<UISprite>(),friendInfo.icon ,GameCommon.ROLE_ICON_TYPE.PHOTO);
				GameCommon.SetUIText (subcell, "level_label", "Lv " + friendInfo.level);
//				GameCommon.SetPetIconWithElementAndStarAndLevel (subcell, "pet_icon", "element", "star_level_label", "level_label", friendInfo.mLevel, friendInfo.mModel);
			}
           
		}
		else GameCommon.SetUIText (mGameObjUI, "request_friend_num_info_label", "0 / " + mMaxFriendNum.ToString ());
		
		DataCenter.SetData ("FRIEND_WINDOW", "NEW_REQUEST_FRIEND_ICON", true);
        GuideManager.Notify(GuideIndex.EnterAddFriendPage);
		return true;
	}
	
	void SetButtonData(GameObject obj , string strName , FriendData friendData)
	{
		NiceData buttonData = GameCommon.GetButtonData (obj, strName);
		if(buttonData != null)
		{
			buttonData.set ("FRIEND_ID", friendData.friendId);
			buttonData.set ("MAIL_ID", friendData.mailID);
			buttonData.set ("FRIEND_NAME", friendData.name);
		}
		else DEBUG.Log ("button data is null >" + strName);
	}
	
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch(keyIndex)
		{
		case  "CLEAN_ALL_UI":
			UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "request_friend_scrollview_context_grid").GetComponent<UIGridContainer>();
			grid.MaxCount = 0;
			break;
		case  "UPDATE_INVITE_FRIEND_NUM":
			RoleLogicData roleLogicData = RoleLogicData.Self;
			FriendLogicData friendLogicData =  DataCenter.GetData("REQUEST_INVITE_FRIEND_LIST") as FriendLogicData;
			if(friendLogicData != null){ 
				roleLogicData.mInviteNum = friendLogicData.arr.Length;
				if(roleLogicData.mInviteNum ==0){
					GameCommon.SetUIText (mGameObjUI ,"label_add_friend_tips",DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_ADD_FRIEND_TIPS,"STRING_CN"));
					GameCommon.SetUIVisiable(mGameObjUI ,"label_add_friend_tips",true);
				}
				else {
					GameCommon.SetUIVisiable(mGameObjUI ,"label_add_friend_tips",false);
				}
			}
			else roleLogicData.mInviteNum = 0;
			break;
		case "SUCCEED":
			AgreeAndRefuseFriend(false, true);
			break;
		case "SEND":
			AgreeAndRefuseFriend (true, false);
			break;
		case "REFUSE":
			AgreeAndRefuseFriend(false, false);
			break;
		}
	}
	
	void AgreeAndRefuseFriend(bool isSend, bool b)
	{
		object obj;
		getData ("CURRENT_FRIEND_DATA", out obj);
		NiceData friendData = obj as NiceData;
		if(friendData != null)
		{	
			if(isSend)
			{
				string strFriendID = Convert.ToString(friendData.getObject ("FRIEND_ID"));
				int iMailID = Convert.ToInt32 (friendData.getObject ("MAIL_ID"));
				
                //by chenliang
                //begin

// 				tEvent evt = Net.StartEvent("CS_RefuseFriend");
// 				evt.set("FRIEND_ID", iFriendID);
// 				evt.set("MAIL_ID", iMailID);
// 				evt.DoEvent();
//------------------------
                CS_RefuseFriend.Send(strFriendID);

                //end
			}
			else
			{
				GameObject refuseParentObj = friendData.getObject ("PARENT") as GameObject;
				refuseParentObj.SetActive (false);

				UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "request_friend_scrollview_context_grid").GetComponent<UIGridContainer>();
				grid.Reposition ();
				if(b)
				{
                    if (DataCenter.Get("IS_FRIEND_OTHER_FULL"))
                    {
                        DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FRIEND_OTHER_FULL);
                        DataCenter.Set("IS_FRIEND_OTHER_FULL", false);
                    }
                    else
                    {
                        string name = friendData.getObject("FRIEND_NAME").ToString();

                        //GuideManager.Notify(GuideIndex.AddFriendSucceed);
                        DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_FRIEND_SUCCESS, name);
                    }
				}

				RoleLogicData roleLogicData = RoleLogicData.Self;
				roleLogicData.mInviteNum --;
                if (roleLogicData.mInviteNum == 0)
                {
                    SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FRIEND_REQUEST, false);
                }
				GameCommon.SetUIText (mGameObjUI, "request_friend_num_info_label", roleLogicData.mInviteNum.ToString () + " / " + mMaxFriendNum.ToString ());
				DataCenter.SetData ("FRIEND_WINDOW", "NEW_REQUEST_FRIEND_ICON", true);
			}
		}
		else DEBUG.Log ("friend data  is null");
	}
	
	public override  void onRemove()
	{
		base.onRemove ();
		DataCenter.Remove ("FIND_FRIEND_LIST_INFO_WINDOW");
	}
}


public class Button_find_friend_confirm_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "FIND_FRIEND", true);
		return true;
	}
}

public class Button_agreed_button : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = getObject ("BUTTON") as GameObject;
		GameObject parentObj = obj.transform.parent.parent.gameObject;
		
		NiceData buttonData = obj.GetComponent<UIButtonEvent>().mData;
		if(buttonData != null)
		{
			set ("PARENT", parentObj);
			DataCenter.SetData ("ADD_FRIEND_WINDOW", "CURRENT_FRIEND_DATA", buttonData);

			string iFriendID = Convert.ToString(getObject ("FRIEND_ID"));
			int iMailID = Convert.ToInt32 (getObject ("MAIL_ID"));
            int mMaxFriendNum = (int)DataCenter.mSpiritSendConfig.GetData(1, "FRIEND_NUM");
            int currentFriendNum = DataCenter.Get("CURRENT_FRIEND_NUM");

            if (currentFriendNum >= mMaxFriendNum)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FRIEND_FULL);
            }
            else
            {
                FriendNetEvent.RequestAcceptFriend(iFriendID, RequestAddFriendSuccess, RequestAddFriendFail);
            }
			/*
			tEvent evt = Net.StartEvent("CS_AcceptFriend");
			evt.set("FRIEND_ID",  iFriendID );
			evt.set("MAIL_ID",  iMailID );
			evt.DoEvent();
			*/
		}
		else DEBUG.Log ("agreed button data  is null");

		DataCenter.SetData ("GAME_FRIEND_WINDOW", "NOT_GET_DATA_FROM_NET", false);

		return true;
	}

	void RequestAddFriendSuccess(string text) {
        SC_RequestAcceptFriend agreeF = JCode.Decode<SC_RequestAcceptFriend>(text);
		if(agreeF.ret == (int)STRING_INDEX.ERROR_NONE) {
            if (agreeF.friendFull == 1)
            {
                DataCenter.Set("IS_FRIEND_OTHER_FULL", true);
            }
			DataCenter.SetData ("ADD_FRIEND_WINDOW", "SUCCEED", true);
		}

		DEBUG.Log("AddFriendSuccess:" + text);
	}

	void RequestAddFriendFail(string text) {
		DEBUG.Log("AddFriendFail:" + text);
	}
}

public class Button_refused_button : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = getObject ("BUTTON") as GameObject;
		GameObject parentObj = obj.transform.parent.parent.gameObject;
		
		NiceData buttonData = obj.GetComponent<UIButtonEvent>().mData;
		if(buttonData != null)
		{
			set ("PARENT", parentObj);
			DataCenter.SetData ("ADD_FRIEND_WINDOW", "CURRENT_FRIEND_DATA", buttonData);
			
			string strName = getObject ("FRIEND_NAME").ToString ();
			DataCenter.OpenMessageOkWindow (STRING_INDEX.ERROR_FRIEND_REFUSED, strName, "ADD_FRIEND_WINDOW");
		}
		else DEBUG.Log ("refused button data  is null");
		
		return true;
	}
}

public class Button_request_friend_button : CEvent
{
	string iFriendID;

	public override bool _DoEvent()
	{
		GameObject obj = getObject ("BUTTON") as GameObject;
		GameObject parentObj = obj.transform.parent.gameObject;
		
		NiceData buttonData = obj.GetComponent<UIButtonEvent>().mData;

		if(buttonData != null)
		{
			set ("PARENT", parentObj);
			DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "CURRENT_FRIEND_DATA", buttonData);
			
			iFriendID = Convert.ToString(getObject ("FRIEND_ID"));
            int mMaxFriendNum = (int)DataCenter.mSpiritSendConfig.GetData(1, "FRIEND_NUM");
            int currentFriendNum = DataCenter.Get("CURRENT_FRIEND_NUM");
           
            if (currentFriendNum >= mMaxFriendNum)
			{
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FRIEND_FALL_RECOMMEND_FAIL);
			}else if(iFriendID ==Convert.ToString(CommonParam.mUId))
			{
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FRIEND_RECOMMEND_SELF, true);
			}
			else
			{
				FriendNetEvent.RequestAddFriendRequest(iFriendID, AddFriendSuccess, AddFriendFail);
			}

			/*
			tEvent evt = Net.StartEvent ("CS_AddFriend");
			evt.set ("FRIEND_ID", iFriendID);
			evt.set ("WINDOW_NAME", "FRIEND_WINDOW");
			evt.DoEvent ();
			*/
		}
		else DEBUG.Log ("request friend data  is null");
		
		return true;
	}

	void AddFriendSuccess(string text) {
		SC_SendFriendRequest result = JCode.Decode<SC_SendFriendRequest>(text);
		// 对方的好友申请列表已满
		if (result.friendReqFull == 1) 
		{
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_FRIEND_REQUEST_FULL);
			return;
		}else if(result.friendsAlready == 1)
		{
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_FRIEND_IS_FRIEND, true);
			return;
		}else if(result.ret == (int)STRING_INDEX.ERROR_NONE) {
			DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "ERROR_NONE", true);
		    FriendLogicData fld = FindFriendListInfoWindow.logicData;
			List<FriendData> lfds = new List<FriendData>();
			foreach(FriendData fd in fld.arr) {
				if(fd.friendId != iFriendID.ToString()) {
					lfds.Add(fd);
				}
			}

			fld.arr = lfds.ToArray();
			FindFriendListInfoWindow.logicData = fld;
			DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "REFRESH", true);
		}

	}

	void AddFriendFail(string text) {
		DEBUG.Log("AddFriendErr:"+ text);

		DEBUG.Log("AddFriendErr:"+ text);
		int val;
		int.TryParse(text, out val);
		if(val == (int)FRIEND_ERROR_CODE.FRIEND_REQUEST_ALREADY_EXISTS) {
			DEBUG.Log("AddFriendErr:"+ "好友已经申请过了！");
		}

	}
}


