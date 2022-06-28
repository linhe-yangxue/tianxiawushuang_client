using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System;
using DataTable;

public class FriendVisitWindow : tWindow
{
	const int mPlayerMaxStarLevel = 3;
	public override void Init() 
	{
		EventCenter.Self.RegisterEvent("Button_friend_visit_back", new DefineFactory<Button_friend_visit_back>());
		EventCenter.Self.RegisterEvent("Button_click_praise_button", new DefineFactory<Button_click_praise_button>());
	}
	public override void Open (object param)
	{
		base.Open (param);

		DataCenter.OpenWindow ("BACK_GROUP_FRIEND_VISIT_WINDOW");
		DataCenter.OpenWindow ("INFO_GROUP_WINDOW");

		SetButtonData(null, "friend_visit_back", param.ToString());

        //by chenliang
        //begin

// 		GameObject obj = GameObject.Find("Mainmenu_bg").transform.Find("arena_4v4/zjm_tianmozhu01/ec_ui_tianmo").gameObject;
//         GameObject putongObj = GameObject.Find("Mainmenu_bg").transform.Find("arena_4v4/zjm_tianmozhu01/ec_mainputong").gameObject;
//------------------------
        //prefab中arena_4v4/zjm_tianmozhu01已修改为arena_4v4/zjm_tianmozhu
        GameObject obj = GameObject.Find("Mainmenu_bg").transform.Find("arena_4v4/zjm_tianmozhu/ec_ui_tianmo").gameObject;
        GameObject putongObj = GameObject.Find("Mainmenu_bg").transform.Find("arena_4v4/zjm_tianmozhu/ec_mainputong").gameObject;

        //end
        if (obj != null && putongObj != null)
		{
			obj.SetActive(false);
			putongObj.SetActive(true);
		}
		mGameObjUI.SetActive(false);
       
	}

	void SetButtonData(GameObject obj , string strName , string windowName)
	{
		NiceData buttonData = GameCommon.GetButtonData (obj, strName);
		if(buttonData != null)
		{
			buttonData.set ("WINDOW_NAME", windowName);
		}
		else DEBUG.Log ("button data is null >" + strName);
	}

	public override void Close ()
	{
		base.Close ();

		DataCenter.CloseWindow ("BACK_GROUP_FRIEND_VISIT_WINDOW");
		DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "CLEAR_GRID", true);
		DataCenter.CloseWindow("ROLE_SEL_BOTTOM_GROUP");
	}
	
	public override bool Refresh(object param)
	{
		//GameCommon.RestoreCharacterConfigRecord ();
		DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "INIT_FRIEND_ROLE_SELECT", param);
		//GameCommon.ChangeCharacterConfigRecord ();
        
		return true;

		tEvent visitEvent = (tEvent)param;
		string strWindowName = visitEvent.get ("WINDOW_NAME");
		string strFriendID = visitEvent.get ("FRIEND_ID").ToString();
		NiceData friendVisitBackButtonData = GameCommon.GetButtonData ((DataCenter.GetData("BACK_GROUP_FRIEND_VISIT_WINDOW") as tWindow).mGameObjUI, "friend_visit_back");
		friendVisitBackButtonData.set ("WINDOW_NAME", strWindowName);
		NiceData clickPraiseButtonData = GameCommon.GetButtonData (mGameObjUI, "click_praise_button");
		clickPraiseButtonData.set ("FRIEND_ID", strFriendID);

		int iModelIndex;
		int iLevel;
		InitFriendLabelInfo(visitEvent, out iModelIndex, out iLevel);
		return true;


		InitModel (0, iModelIndex, iLevel);

		object obj;
		visitEvent.getData ("PET_DATA", out obj);
		NiceTable petTable = obj as NiceTable;
        
		int i = 1;
		foreach(KeyValuePair<int, DataRecord> r in petTable.GetAllRecord ())
		{
			DataRecord re = r.Value;
			int iPetModelIndex = re.getData ("PET_ID");
			int iPetLevel = re.getData ("PET_LEVEL");
			if(i > 3) return false;

			InitModel (i, iPetModelIndex, iPetLevel);
			i++;
		}
		for(int j=i; j<4; j++)
		{
			GameObject surplusObj = GameCommon.FindObject (mGameObjUI, "info0" + j.ToString ());
			surplusObj.SetActive (false);
		}
		return true;
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

		switch(keyIndex)
		{
		case"ADD_ZAN_NUM":
			string strZanNum = GameCommon.FindObject (mGameObjUI, "praise_num").GetComponent<UILabel>().text;
			int iZanNum = Convert.ToInt32 (strZanNum);
			iZanNum++;
			GameCommon.SetUIText (mGameObjUI, "praise_num", iZanNum.ToString ());
			break;
		}
	}

	void InitFriendLabelInfo (tEvent visitEvent, out int iModelIndex, out int iLevel)
	{
		object friendObj;
		visitEvent.getData ("ROLE_DATA", out friendObj);
		NiceData friendData = friendObj as NiceData;
		iModelIndex = friendData.get ("CHAR_MODEL");
		iLevel = friendData.get ("CHAR_LEVEL");
		SetText ("victory_num", friendData.get ("PVP_WIN_COUNT").ToString ());

		GameCommon.SetUIText (mGameObjUI , "player_name", friendData.get ("NAME"));
		GameCommon.SetUIText (mGameObjUI , "add_visited_num", friendData.get ("VISIT_COUNT"));
		GameCommon.SetUIText (mGameObjUI , "today_visited_num", friendData.get ("DAILY_VISIT_COUNT"));
		GameCommon.SetUIText (mGameObjUI , "vip_level", friendData.get ("VIP_LEVEL"));

		string strZanCount = friendData.get ("ZAN_DATA");
		if( strZanCount == "") strZanCount = "0";
		GameCommon.SetUIText (mGameObjUI , "praise_num", strZanCount);

		int iRoleIconIndex = friendData.get ("ROLE_ICON_INDEX");
		if(iRoleIconIndex < 1000) iRoleIconIndex = 1000;
		string spriteName = TableCommon.GetStringFromVipList (iRoleIconIndex, "VIPICON");
		if(iRoleIconIndex == 1000)
		{
			string [] names = spriteName.Split ('\\');
			if(iModelIndex < 191004)
				spriteName = names[0];
			else 
				spriteName = names[1];
		}
		SetUISprite ("friend_icon", TableCommon.GetStringFromVipList (iRoleIconIndex, "VIPALATS"), spriteName);

		InitFreindPVPInfo(visitEvent, "against_ranking_num", "RANKING");
		InitFreindPVPInfo(visitEvent, "against_score_num", "SCORE");
//		InitFreindPVPInfo(visitEvent, "victory_num", "RANKING_MAX"); 
		InitFreindPVPInfo(visitEvent, "winning_streak_num", "MAX_CONTINUOUS_WIN");
	}

	void InitFreindPVPInfo(tEvent visitEvent, string strLabelName, string strFromNet)
	{
		object obj;
		visitEvent.getData (strFromNet, out obj);
		string strText = obj.ToString();
		SetText (strLabelName, strText);
	}

	public void InitModel(int i, int iModelIndex, int iLevel)
	{
		float fScaleRatio = 1.0f;
		string info = "info";
		GameObject infoObj;
		if(i != 0) 
		{
			info += ("0" + i.ToString ());
			infoObj = GameCommon.FindObject (mGameObjUI, info);
		}
		else
		{
			infoObj = GameCommon.FindObject (mGameObjUI, info);
			int n = iModelIndex%1000;
			int t = UnityEngine.Mathf.CeilToInt ((float)n / mPlayerMaxStarLevel);
			string str = (n - mPlayerMaxStarLevel * (t - 1)).ToString ();
			GameCommon.SetIcon (infoObj, "NameTitle", "ui_chenghao_" + str, "CardAtlas" );
		}

		ActiveBirthForUI activeBirthForUI = GameCommon.FindObject (infoObj, "UIPoint").GetComponent<ActiveBirthForUI>();
		activeBirthForUI.mBirthConfigIndex = iModelIndex;
		if (activeBirthForUI.mIndex == 0) fScaleRatio = 1.5f;
		else if (activeBirthForUI.mIndex == 1) fScaleRatio = 1.0f;
		else if (activeBirthForUI.mIndex == 2) fScaleRatio = 1.2f;
		else if (activeBirthForUI.mIndex == 3) fScaleRatio = 1.0f;
		activeBirthForUI.Init(true, fScaleRatio);

		if(iLevel != 0)
		{
			UIGridContainer grid = infoObj.transform.Find("stars_grid").GetComponent<UIGridContainer>();

			string strName = TableCommon.GetStringFromActiveCongfig(iModelIndex, "NAME");
			GameCommon.SetUIText (infoObj, "name_label", strName);
			GameCommon.SetUIText (infoObj, "level_label", iLevel.ToString ());
			
			int iStarLevel = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "STAR_LEVEL");
			grid.MaxCount = iStarLevel;
			
			float fDX = 10.0f;
			grid.transform.localPosition = new Vector3(-14 + fDX*(6 - iStarLevel), grid.transform.localPosition.y, grid.transform.localPosition.z);
		}
	}
}


public class Button_friend_visit_back : CEvent
{
	public override bool _DoEvent()
	{
		string strWindowName = getObject ("WINDOW_NAME").ToString ();

		DataCenter.CloseWindow ("FRIEND_VISIT_WINDOW");

		if(strWindowName == "PVP_VISIT_FRIEND_BACK") 
		{
			bool b = DataCenter.GetData ("PVP_RANK_WINDOW").get ("IS_PVP_FRIEND_RANK");
			DataCenter.OpenWindow (strWindowName, b);
		}
		else if(strWindowName == "PVP_ATTR_READY_WINDOW")
		{
			bool b = DataCenter.GetData ("PVP_ATTR_READY_RANK_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString ()).get ("IS_PVP_FRIEND_RANK");
			DataCenter.OpenWindow (strWindowName, b);
		}
        else if (strWindowName == "RANK_WINDOW")
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RankWindow);
        }
		else if(strWindowName == "FRIEND_WINDOW")
		{
			DataCenter.OpenWindow ("MAIN_CENTER_GROUP");
			DataCenter.OpenWindow (strWindowName);
		}
		else if(strWindowName == "PVP_FOUR_VS_FOUR_READY_WINDOW")
		{
			DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_READY_WINDOW", false);
			DataCenter.OpenWindow ("PVP_PLAYER_FOUR_VS_FOUR_RANK_WINDOW");
			DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_WINDOW", false);
		}
        //by chenliang
        //begin


        else if (strWindowName == "BOSS_RAID_WINDOW" ||
                 strWindowName == "RAMMBOCK_WINDOW" ||
                 strWindowName == "PEAK_PVP_WINDOW" ||
                 strWindowName == "UNION_WINDOW" ||
                 strWindowName == "RANKLIST_MAINUI_WINDOW")
        {
            tWindow tmpWin = DataCenter.GetData(strWindowName) as tWindow;
            if (tmpWin != null)
                tmpWin.mGameObjUI.SetActive(true);
            if (strWindowName == "PEAK_PVP_WINDOW")
            {
                MainUIScript.Self.OpenMainUI();
                DataCenter.OpenWindow("PEAK_WINDOW_BACK");
            }
            else if (strWindowName == "BOSS_RAID_WINDOW")
            {
                DataCenter.OpenWindow("BOSS_BACK_WINDOW");
                //MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
//                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
				int _preWinType = DataCenter.GetData("INFO_GROUP_WINDOW","PRE_WIN");

				//added by xuke begin
				if(_preWinType == 1)
				{
					MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
					DataCenter.OpenWindow("TRIAL_WINDOW");
					DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
				}
				else
				{
					//DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
					MainUIScript.Self.OpenMainUI();
					DataCenter.OpenWindow("SCROLL_WORLD_MAP_WINDOW");
					//MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
				}
					
				//end
            }
            else
                MainUIScript.Self.GoBack();

			//added by xuke begin

			//end
        }
        else if(strWindowName == UIWindowString.arena_rank_window)
        {
            ArenaBase.openFourWindow(true);
            ArenaNetManager.RequestArenaRankList();
        }
        else if(strWindowName == UIWindowString.arena_record_window)
        {
            ArenaBase.openFourWindow(true);
            DataCenter.OpenWindow(UIWindowString.arena_record_window);
        }
        else if(strWindowName == UIWindowString.arena_main_window)
        {
            ArenaBase.openFourWindow(true);
        }

        //end
		else
			DataCenter.OpenWindow (strWindowName);

		return true;
	}
}

public class Button_click_praise_button : CEvent
{
	public override bool _DoEvent()
	{
		string strFriendID = Convert.ToString(getObject ("FRIEND_ID"));

		tEvent evt = Net.StartEvent("CS_ZanResult");
		evt.set ("FRIEND_ID", strFriendID);
		evt.DoEvent();

		return true;
	}
}

