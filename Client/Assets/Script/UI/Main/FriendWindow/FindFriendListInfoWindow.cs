using UnityEngine;
using System.Collections.Generic;
using Logic;
using System;
using DataTable;

public class FindFriendListInfoWindow : tWindow
{
	public static FriendLogicData logicData;
	public FriendData[] sFriendData=null;
	public FindFriendListInfoWindow(GameObject objParent)
	{
		mGameObjUI = GameCommon.FindObject (objParent, "find_friend_list_info_window");
	}

	public override void Open(object param)
	{
		base.Open (param);

		string val = param.ToString();
		if(!string.IsNullOrEmpty(val))
			FriendNetEvent.RequestSearchFriend(val, RequestSearchFriendSuccess, RequestSearchFriendFail);
		else
		// 获得好友推荐列表
		FriendNetEvent.RequestRecommendFriendList(RecommendFriendSuccess, RecommendFriendFail);
	}
	


		/*
		tEvent evt = Net.StartEvent("CS_SearchFriend");
		evt.set("NAME", param.ToString ());
		evt.DoEvent();
		*/

	void RecommendFriendSuccess(string text) {
		
		logicData = new FriendLogicData ();
		logicData = JCode.Decode<FriendLogicData>(text);
		DataCenter.Remove ("FRIEND_SEARCH_DATA");

//		DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "CLEAN_ALL_UI", true);
		DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "REFRESH", true);
		 
	}

	void RecommendFriendFail(string text) {
		DEBUG.Log("Request Recommend Friend Err:" + text);
	}

	void RequestSearchFriendSuccess(string text) {
		GetFriendData mfrinedData = JCode.Decode<GetFriendData>(text);
		DataCenter.Remove ("FRIEND_SEARCH_DATA");
		if (mfrinedData.arr.Length ==0) {
				DataCenter.ErrorTipsLabelMessage (STRING_INDEX.ERROR_FRIEND_SEARCH_NULL);
			}
		sFriendData =new FriendData[mfrinedData.arr.Length];
		for (int i=0; i<mfrinedData.arr.Length; i++) {

			DataCenter.RegisterData ("FRIEND_SEARCH_DATA", mfrinedData.arr[i]);
			sFriendData[i]= mfrinedData.arr[i];
//		DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "CLEAN_ALL_UI", true);
		}
		DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "REFRESH", true);
		DEBUG.Log("SearchFriend:" + text);
	}

	void RequestSearchFriendFail(string text) {
		DataCenter.ErrorTipsLabelMessage (STRING_INDEX.ERROR_FRIEND_SEARCH_NULL);
		DEBUG.Log("SearchFriendErr:" + text);
	}

	public override bool Refresh(object param)
	{ 
		base.Refresh (param);
		FriendData[] friendDataList = null;
		FriendData mFriendLogicData =  DataCenter.GetData("FRIEND_SEARCH_DATA") as FriendData;
		if(logicData != null && mFriendLogicData == null) {
			friendDataList = logicData.arr;
		} else if(mFriendLogicData != null) {
			//friendDataList = new FriendData[]{ mFriendLogicData };
			friendDataList =sFriendData ;
		}
		UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "recommend_friend_scrollview_context_grid").GetComponent<UIGridContainer>();
		if (friendDataList != null) {
			grid.MaxCount = friendDataList.Length;
			for (int i = 0; i < friendDataList.Length; i++) {
				GameObject subcell = grid.controlList [i];
			
				NiceData buttonData = GameCommon.GetButtonData (subcell, "request_friend_button");
				if (buttonData != null) {
					buttonData.set ("FRIEND_ID", friendDataList [i].friendId);
					buttonData.set ("MAIL_ID", friendDataList [i].mailID);
					buttonData.set ("FRIEND_NAME", friendDataList [i].name);
				} else
					DEBUG.Log ("button data is null > request_friend_button");
			
				FriendData friendInfo = friendDataList [i];
				GameCommon.SetStrengthenLevelLabel (subcell, friendInfo.mStrengthenLevel, "strengthen_level_label");
				GameCommon.SetUIText (subcell, "player_name_label", friendInfo.name);
				GameCommon.SetUIText (subcell, "level_label", "Lv " + friendInfo.level);
				GameCommon.SetRoleIcon (GameCommon.FindObject (subcell, "pet_icon").GetComponent<UISprite> (), friendInfo.icon, GameCommon.ROLE_ICON_TYPE.PHOTO);

				long iOffTime = CommonParam.NowServerTime () - friendInfo.lastLoginTime;//friendInfo.mLoginTime;
				if (iOffTime < 60)
					GameCommon.SetUIText (subcell, "off_time_label", "0分钟前");
				else if (iOffTime < 3600 && iOffTime >= 60)
					GameCommon.SetUIText (subcell, "off_time_label", (iOffTime / 60).ToString () + "分钟前");
				else if (iOffTime < 3600 * 24 && iOffTime >= 3600)
					GameCommon.SetUIText (subcell, "off_time_label", (iOffTime / 3600).ToString () + "小时前");
				else if (iOffTime >= 3600 * 24)
					GameCommon.SetUIText (subcell, "off_time_label", (iOffTime / (3600 * 24)).ToString () + "天前");
				//GameCommon.SetPetIconWithElementAndStarAndLevel (subcell, "pet_icon", "element", "star_level_label", "level_label", friendInfo.level, friendInfo.mModel);
			}
		}
		return true; 
	}
	
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "recommend_friend_scrollview_context_grid").GetComponent<UIGridContainer>();
		UIInput findFriendInput = GameCommon.FindObject (mGameObjUI.transform.parent.gameObject, "find_friend_input").transform.GetComponent<UIInput>();
		switch(keyIndex)
		{
		case "FIND_FRIEND":
			if(findFriendInput.value != "") Open (findFriendInput.value);
			else DataCenter.ErrorTipsLabelMessage (STRING_INDEX.ERROR_FRIEND_NEED_INPUT_NAME);
			break;
		case "CLEAN_ALL_UI":
			grid.MaxCount = 0;
			break;
		case "CLEAN_FIND_FRIEND_INPUT":
			findFriendInput.value = "";
			break;
		case "ERROR_NONE" :
			/*
			object obj;
			getData ("CURRENT_FRIEND_DATA", out obj);
			NiceData friendData = obj as NiceData;
			if(friendData != null) 
			{
				GameObject parentObj = friendData.getObject ("PARENT") as GameObject;
				parentObj.SetActive (false);

				grid.repositionNow = true;
				if(grid.MaxCount <= 1) 
				{
					tEvent evt = Net.StartEvent("CS_SearchFriend");
					evt.set("NAME", "");
					evt.DoEvent();
				}
			}
			else 
				DEBUG.Log ("friend data  is null");
			*/

			DataCenter.OnlyTipsLabelMessage (STRING_INDEX.ERROR_FRIEND_RECOMMEND_SUCCESS);
			break;
		}
	}
}
