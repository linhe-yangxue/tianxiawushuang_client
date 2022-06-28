using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataTable;
using Logic;

public class PVP_FourVSFourEnterWindow :tWindow 
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_four_vs_four_pk_button", new DefineFactory<Button_four_vs_four_pk_button>());
	}

	public override void Open(object param)
	{
		base.Open (param);
		tEvent evt = Net.StartEvent("CS_Request4v4State");
		evt.set ("JUST_GET_RNAK", true);
		evt.DoEvent();
	}

	public override bool Refresh(object param) 
	{
		tEvent respEvt = param as tEvent;
		int mRank = respEvt.get("MY_RANK");
		SetText ("now_rank_number", mRank.ToString ());
		return true; 
	}
}

public class Button_four_vs_four_pk_button : CEvent
{
	public override bool _DoEvent()
	{
		GlobalModule.ClearAllWindow ();
		DataCenter.OpenWindow("PVP_FOUR_VS_FOUR_READY_WINDOW");
		return true;
	}
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class PVP_FourVSFourReadyWindow : tWindow
{
	public override void Open(object param)
	{
		base.Open(param);
		if(param is Boolean && !(bool)param)
			return;

		DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
		DataCenter.OpenWindow("PVP_FOUR_VS_FOUR_WINDOW");
		DataCenter.OpenWindow("PVP_PLAYER_FOUR_VS_FOUR_RANK_WINDOW", param);
	}
	
	public override void Close ()
	{
		base.Close ();
		DataCenter.CloseWindow("PVP_PLAYER_FOUR_VS_FOUR_RANK_WINDOW");
		DataCenter.CloseWindow("PVP_FOUR_VS_FOUR_WINDOW");
	}
}

//4V4  页面
//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class PVP_FourVSFourWindow : tWindow
{
	enum RankType
	{
		COMPETITOR,
		GLOBAL,
		FRIEND,
	}

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_four_vs_four_battle_button", new DefineFactory<Button_four_vs_four_rank_button>());
		EventCenter.Self.RegisterEvent("Button_four_vs_four_overall_rank_button", new DefineFactory<Button_four_vs_four_rank_button>());
		EventCenter.Self.RegisterEvent("Button_four_vs_four_friend_rank_button", new DefineFactory<Button_four_vs_four_rank_button>());
		EventCenter.Self.RegisterEvent("Button_challenge_other_button", new DefineFactory<Button_challenge_other_button>());
		EventCenter.Self.RegisterEvent("Button_four_vs_four_refresh_button", new DefineFactory<Button_four_vs_four_rank_button>());
	}

	public override void Open(object param)
	{
		base.Open (param);

		SetButtonRankType ();

		if((bool)param)
			RequestRank (RankType.COMPETITOR);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		tEvent request;
	
		switch(keyIndex )
		{
		case "REQUEST_RANK":
			RankType type = (RankType)objVal;
			RequestRank (type);
			break;
		case "REFRESH_RANK":
			ShowRank((tEvent)objVal);
			break;
		}
	}

	private void RequestRank(RankType type)
	{
		string strRequestEventName = "CS_Search4v4Competitor";
		switch(type)
		{
		case RankType.GLOBAL :
			strRequestEventName = "CS_Request4v4GlobalRanks";
			break;
		case RankType.FRIEND:
			strRequestEventName = "CS_Request4v4FriendRanks";
			break;
		}

		tEvent evt = Net.StartEvent(strRequestEventName);
		evt.set ("RANK_TYPE", (int)type);
		evt.DoEvent();
	}

	private void ShowRank(tEvent respEvt)
	{
		RankType type = (RankType)respEvt.getObject ("RANK_TYPE");
		string parentName = GetParentNameByRankType(type);
		GameCommon.ToggleTrue (GetSub (parentName + "_button"));
		SetRankVisible (parentName);

		if(type == RankType.COMPETITOR)
			GameCommon.GetButtonData(GetSub ("four_vs_four_refresh_button")).set ("RANK_TYPE", RankType.COMPETITOR);

		UIGridContainer grid = GetSub (parentName).GetComponentInChildren<UIGridContainer>();

		object obj;
		respEvt.getData("RESULT_TABLE", out obj);
		NiceTable data = obj as NiceTable;

		if(data == null) 
		{
			grid.MaxCount = 0;
			if(type == RankType.COMPETITOR)
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_PVP_YOU_ARE_CHAMPION);
			else
				DataCenter.OpenMessageWindow ("数据暂未处理", true);
			return;
		}

		grid.MaxCount = data.GetRecordCount();
		int i = 0;
		foreach(DataRecord record in data.Records())
		{
			GameObject subCell = grid.controlList[i++];
			subCell.SetActive(true);
			
			int roleIconIndex = record["ICON"];
			string name = record["NAME"];
			int fightNum = record["FIGHT_POWER"];
			int level = record["LEVEL"];
			int rank = record["RANK"];
			int DBID = record["DBID"];
			int vipLevel = record["VIPLEVEL"];
			int iID = record["ID"];

			if(type == RankType.COMPETITOR)
			{
				UISprite icon = GameCommon.FindObject (subCell, "photo_icon").GetComponent<UISprite>();
				GameCommon.SetPalyerIcon (icon, roleIconIndex);
			}
			GameCommon.SetUIText (subCell, "name_label", name);
			GameCommon.SetUIText (subCell, "level_label", level.ToString ());
			GameCommon.SetUIText (subCell, "rank_label", rank.ToString ());
			GameCommon.SetUIText (subCell, "fight_strength_number", fightNum.ToString ());
			
			if(type == RankType.GLOBAL)
			{
				NiceData visitButtonData = GameCommon.GetButtonData (subCell, "visit_button");
				if(visitButtonData != null)
				{
					visitButtonData.set ("FRIEND_ID", DBID);
					visitButtonData.set ("FRIEND_NAME", name);
					visitButtonData.set ("WINDOW_NAME", "PVP_FOUR_VS_FOUR_READY_WINDOW");
				}
				else DEBUG.LogError ("can not find button data>>>>>>visit_button");

				GameCommon.SetUIVisiable (subCell, "visit_button", !(DBID < 0));
			}
			else if(type == RankType.FRIEND)
			{
				//set button data
			}
			else if(type == RankType.COMPETITOR)
			{
				NiceData buttonData = GameCommon.GetButtonData (subCell, "challenge_other_button");
				if(buttonData != null)
				{
					buttonData.set ("PLAYER_NAME", name);
					buttonData.set ("FIGHT_NUM", fightNum);
					buttonData.set ("DBID", DBID);
					buttonData.set ("RANK", rank);
				}
				else DEBUG.LogError ("can not find button data>>>>>>challenge_other_button");
			}
		}
	}

	private void SetButtonRankType()
	{
		GameCommon.GetButtonData(GetSub ("four_vs_four_battle_button")).set ("RANK_TYPE", RankType.COMPETITOR);
		GameCommon.GetButtonData(GetSub ("four_vs_four_overall_rank_button")).set ("RANK_TYPE", RankType.GLOBAL);
		GameCommon.GetButtonData(GetSub ("four_vs_four_friend_rank_button")).set ("RANK_TYPE", RankType.FRIEND);
	}

	private void SetRankVisible(string visibleRankName)
	{
		SetVisible (false, "four_vs_four_battle", "four_vs_four_overall_rank", "four_vs_four_friend_rank");
		SetVisible (visibleRankName, true);
	}

	private string GetParentNameByRankType(RankType type)
	{
		string strName = "four_vs_four_battle";
		switch(type)
		{
		case RankType.GLOBAL:
			strName = "four_vs_four_overall_rank";
			break;
		case RankType.FRIEND:
			strName = "four_vs_four_friend_rank";
			break;
		}

		return strName;
	}

}

public class Button_four_vs_four_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PVP_FOUR_VS_FOUR_WINDOW", "REQUEST_RANK", getObject("RANK_TYPE"));
		return true;
	}
}

public class Button_challenge_other_button : CEvent
{
	public override bool _DoEvent()
	{
		string name = getObject ("PLAYER_NAME").ToString ();
		int fightNum = (int)getObject ("FIGHT_NUM");
		string strParam = fightNum.ToString ().PadLeft (10, '0') + name; 

		DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_TEAM_WINDOW", strParam);
		DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW");
		DataCenter.CloseWindow ("PVP_FOUR_VS_FOUR_WINDOW");
		DataCenter.CloseWindow ("PVP_PLAYER_FOUR_VS_FOUR_RANK_WINDOW");

		int DBID = (int)getObject ("DBID");
		int rank = (int)getObject ("RANK");
		DataCenter.SetData ("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW", "CURRENT_OPPONENT_RANK", rank);
		DataCenter.SetData ("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW", "CURRENT_OPPONENT_DBID", DBID);

		return true;
	}
}

//4V4 个人信息 页面
//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class PVP_PlayerFourVSFourRankWindow : tWindow
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_four_vs_four_rank_award_button", new DefineFactory<Button_four_vs_four_rank_award_button>());
		EventCenter.Self.RegisterEvent("Button_get_award_button", new DefineFactory<Button_get_award_button>());
	}

	public override void Open(object param)
	{
		base.Open(param);
		tEvent evt = Net.StartEvent("CS_Request4v4State");
		evt.DoEvent();
	}

	public override bool Refresh (object param)
	{
		tEvent respEvt = param as tEvent;
		int mRank = respEvt.get("MY_RANK");
//		int mMaxRank = respEvt.get("MY_MAX_RANK");
//		int mCompetCount = respEvt.get("COMPET_COUNT");

		int currentRankNumber = respEvt.get("MY_RANK");
		int highestRankNumber = 1;
		int currentRankAwardNumber = 55;
		int allRankAwardNumber = 8855;
		int pvpTicketNumber = 8;
		
		SetText ("current_rank_number", currentRankNumber.ToString ());
		SetText ("highest_rank_number", highestRankNumber.ToString ());
		SetText ("current_rank_award_number", currentRankAwardNumber.ToString () + "/小时");
		SetText ("all_rank_award_number", allRankAwardNumber.ToString ());
		SetText ("pvp_ticket_number", pvpTicketNumber.ToString ());

		DataCenter.Set ("CUR_FOUR_VS_FOUR_RANK", currentRankNumber);
		return true;
	}

}


public class Button_four_vs_four_rank_award_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("PVP_RANK_AWARDS_WINDOW", 6);
		return true;
	}
}

public class Button_get_award_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_PVP_FOUR_VS_FOUR_GET_RANK_AWARD_SUCCESS);
//		DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_RANK_AWARDS_WINDOW");
		return true;
	}
}

//统一用6V6的排行奖励
/*
//pvp4V4排行奖励窗口
public class PVP_FourVSFourRankAwardsWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_close_pvp_4v4_rank_awards_window_button", new DefineFactory<Button_close_pvp_4v4_rank_awards_window_button>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);
		Refresh(param);
	}
	
	public override bool Refresh (object param)
	{
		int iCounts = 0;
		int iAdd = 7000;
		foreach(KeyValuePair<int, DataRecord> d in DataCenter.mPvpRankAwardsConfig.GetAllRecord ())
		{
			if(d.Value["ARENA_TYPE"] == 6) iCounts++;
		}

		UIGridContainer grid = GetSub ("grid").GetComponent<UIGridContainer>();
		grid.MaxCount = iCounts;
		for(int i = 0; i < iCounts; i++)
		{
			GameObject obj = grid.controlList[i];
			int iIndex = i + iAdd;
			int iMinRank = TableCommon.GetNumberFromRankaward (iIndex, "RANKMIN");
			int iMaxRank = TableCommon.GetNumberFromRankaward (iIndex, "RANKMAX");
			string strRankNum = "";
			if(iMinRank == iMaxRank) strRankNum = iMaxRank.ToString () + "名";
			else strRankNum = iMinRank.ToString () + "~" + iMaxRank.ToString () + "名";
			GameCommon.SetUIText (obj, "rank_number", strRankNum);
			
			int iconCount = 1;
			for(int j = 1; j < 4; j++)
			{
				int iCount = TableCommon.GetNumberFromRankaward (iIndex, "AWARD_COUNT" + j.ToString ());
				if(iCount != 0) iconCount = j;
			}
			
			UIGridContainer gridIcons = GameCommon.FindObject (obj, "grid_icons").GetComponent<UIGridContainer>();
			gridIcons.MaxCount = iconCount;
			for(int n = 0; n < iconCount; n++)
			{
				GameObject iconObj = gridIcons.controlList[n];
				int itemType = TableCommon.GetNumberFromRankaward (iIndex, "AWARD_TYPE" + (n + 1).ToString ());
				int itemID = TableCommon.GetNumberFromRankaward (iIndex, "AWARD_ITEM" + (n + 1).ToString ());
				int itemCount = TableCommon.GetNumberFromRankaward (iIndex, "AWARD_COUNT" + (n + 1).ToString ());
				
				GameCommon.SetUIText (iconObj, "award_number_label", itemCount.ToString ());
				
				GameCommon.SetUIVisiable (iconObj, "level_lable", false);
				GameCommon.SetUIVisiable (iconObj, "star_level_label", false);
				if(itemType == 0)
				{
					GameCommon.SetUIVisiable (iconObj, "level_lable", true);
					GameCommon.SetUIVisiable (iconObj, "star_level_label", true);
					GameCommon.SetPetIcon (iconObj.GetComponent<UISprite>(), itemID);
					GameCommon.SetStarLevelLabel (iconObj, TableCommon.GetNumberFromActiveCongfig (itemID, "STAR_LEVEL"), "star_level_label");
				}
				else 
				{
					GameCommon.SetItemIcon (iconObj.GetComponent<UISprite>(), itemType, itemID);
				}
			}
		}
		return true;
	}
}
class Button_close_pvp_4v4_rank_awards_window_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PVP_FOUR_VS_FOUR_RANK_AWARDS_WINDOW");
		return true;
	}
}
*/

//4V4调宠界面
public class PVP_FourVSFourTeamWindow : PVP_PetTeamWindow
{
	private const string strButtonName = "pvp_pet_check_";
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_adjust_role_button", new DefineFactory<Button_adjust_role_button>());
		EventCenter.Self.RegisterEvent("Button_back_4v4_button", new DefineFactory<Button_back_4v4_button>());
	}

	public override void Open(object param)
	{
		mPetCount = 3;
		base.Open(param);
		SetOpponentInfo(param.ToString ());
		SetRoleInfo();
	}

	public override PetData GetPetData(PetLogicData petLogic, int pos)
	{
		return petLogic.GetPetDataByTeamPos(pos + 1);
	}

    public override void SetPetSkill(PetData petData, GameObject parentObj)
	{
        if (petData == null)
            return;

        int iActiveSkillIndex = petData.GetSkillIndexByIndex(0); //TableCommon.GetNumberFromActiveCongfig(petData.mModelIndex, "PET_SKILL_1");
		if(iActiveSkillIndex == 0)
			return;

		UISprite skillSprite = GameCommon.FindComponent<UISprite>(parentObj, "skill_icon");
		string strSkillIconName = TableCommon.GetStringFromSkillConfig(iActiveSkillIndex, "SKILL_SPRITE_NAME");
		string strSkillIconAtlas = TableCommon.GetStringFromSkillConfig(iActiveSkillIndex, "SKILL_ATLAS_NAME");
		GameCommon.SetIcon(skillSprite, strSkillIconAtlas, strSkillIconName);

        // name
        GameCommon.SetPetSkillName(parentObj, iActiveSkillIndex, "skill_name");

        // level
        GameCommon.SetPetSkillLevel(parentObj, iActiveSkillIndex, "skill_level", petData);

        NiceData buttonData = GameCommon.GetButtonData(parentObj, "Button1");
        buttonData.set("INDEX", iActiveSkillIndex);
        buttonData.set("TABLE_NAME", "SKILL");
	}

	void SetRoleInfo()
	{
		string strAtlasName = TableCommon.GetStringFromActiveCongfig (RoleLogicData.GetMainRole().tid, "HEAD_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromActiveCongfig (RoleLogicData.GetMainRole().tid, "HEAD_SPRITE_NAME");
		GameCommon.SetIcon (GetComponent<UISprite>("role_photo"), strAtlasName, strSpriteName);
	}

	void SetOpponentInfo(string info)
	{
		if(info.Length < 11)
			return ;
		string name = info.Substring (10);
		int fightNum = Convert.ToInt32 (info.Substring (0, 10));
		SetText ("opponent_battle_number", fightNum.ToString ());
		SetText ("opponent_name", name);


	}

}

class Button_adjust_role_button : CEvent
{
	public override bool _DoEvent()
	{
		CS_RequestRoleEquip quest = Net.StartEvent("CS_RequestRoleEquip") as CS_RequestRoleEquip;
		quest.set ("WINDOW_NAME", "ROLE_INFO_WINDOW");

		quest.mAction = () =>
		{
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);

			DataCenter.CloseWindow ("PVP_FOUR_VS_FOUR_TEAM_WINDOW");
			DataCenter.CloseWindow ("PVP_FOUR_VS_FOUR_READY_WINDOW");
			DataCenter.CloseWindow ("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW");
		};

		quest.mBackAction = () =>{
			MainUIScript.Self.GoBack();
			GlobalModule.ClearAllWindow ();
			DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_READY_WINDOW", false);
			DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_TEAM_WINDOW");
			DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW");
			DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
		};
		quest.DoEvent();
		tEvent gemQuest = Net.StartEvent("CS_RequestGem");
		gemQuest.DoEvent();

		return true;
	}
}
class Button_back_4v4_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PVP_FOUR_VS_FOUR_TEAM_WINDOW");
		DataCenter.CloseWindow ("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW");
		DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_WINDOW", false);
		DataCenter.OpenWindow ("PVP_PLAYER_FOUR_VS_FOUR_RANK_WINDOW");		
		return true;
	}
}

//-----------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------
public class PVP_FourVSFourPetBagWindow : PVP_PetBagWindow
{
    protected override PetData GetPVPUsePet(PetLogicData petLogic, int teamPos)
    {
    	return petLogic.GetPetDataByTeamPos(teamPos + 1);
    }
	
	protected override bool IsUsePVPPet(PetLogicData petLogic, PetData petData)
	{
		return petLogic.IsFourVsFourPVPUsePet(petData.itemId);
	}
	
}

//-----------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------
public class PVP_FourVSFourVictoryPredictWindow : tWindow
{
	public PetData[] mPets = new PetData[3];
	public PetData[] yPets = new PetData[3];
    public PetFightDetail[] yPetDetails = new PetFightDetail[3];

    //by chenliang
    //begin

//    public override void OnOpen()
//    {
//        GetMyPets();
//        GetYourPets();
//        Refresh(null);
//    }
//--------------
    public override void OnOpen()
    {
        GlobalModule.DoCoroutine(__OpenAsync());
    }

    /// <summary>
    /// 异步打开窗口
    /// </summary>
    /// <returns></returns>
    private IEnumerator __OpenAsync()
    {
        yield return null;
        GetMyPets();
        yield return null;
        GetYourPets();
        yield return null;
        Refresh(null);
    }

    //end

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
        //by chenliang
        //begin

// 		if(keyIndex == "START_BATTLE")
// 		{
// 			StartFourVsFourBattle();
// 		}
//-----------------
        switch(keyIndex)
        {
            case "INIT_BATTLE": __InitFourVsFourBattle(); break;
            case "START_BATTLE": __StartFourVsFourBattle(); break;
        }

        //end
	}

    //by chenliang
    //begin

//	void StartFourVsFourBattle()
//--------------
    //将战斗开始步骤改为初始化、开始两部分
    private void __InitFourVsFourBattle()

    //end
	{
		int pvpBattleIndex = 20002;

		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mStageTable.GetAllRecord())
		{
            if (v.Value.get("TYPE") == (int)STAGE_TYPE.PVP4)
            {
                pvpBattleIndex = v.Key;
            }
		}

		MainProcess.ClearBattle();
		DataCenter.Set("CURRENT_STAGE", pvpBattleIndex);
		
        //by chenliang
        //begin

//      MainProcess.LoadBattleScene();
// 		PVP4Battle battle = MainProcess.mStage as PVP4Battle;
// 		//battle.mOpponentData = (NiceData)getObject ("OPEN");
// 		for(int i = 0; i < 3; i++)
// 		{
// 			if(yPets[i] != null)
// 			battle.SetOpponentPet (yPets[i], i);
// 		}

        //end
	}
    //by chenliang
    //begin

    private void __StartFourVsFourBattle()
    {
        PVP4Battle battle = MainProcess.mStage as PVP4Battle;
        bool isRobot = ArenaBase.IsRobot(ArenaBase.getChallengeUid());
        battle.SetOpponentCharacterDetail(ArenaBase.challengeTarget.petFDs[0], ArenaBase.challengeTarget.power, isRobot);
        //battle.mOpponentData = (NiceData)getObject ("OPEN");

        for (int i = 0; i < 3; i++)
        {
            if (yPets[i] != null)
            {
                battle.SetOpponentPetAndDetail(i, yPets[i], yPetDetails[i]);
            }
        }
    }

    //end

	public override bool Refresh (object param)
	{
		RoleLogicData logicData = RoleLogicData.Self;
		//NiceData data = (NiceData)getObject ("OPEN");
        SetRoleInfo(GetSub("role_bg_top"), logicData.character.tid, logicData.character.level, logicData.name, (int)GameCommon.GetPower(), RoleLogicData.Self.vipLevel);
		//SetRoleInfo (GetSub ("role_bg_bottom"), (int)data["OBID"], logicData.iconIndex, (int)data["LEVEL"], data["NAME"].ToString ());
        //RoleData opponent = PeakPvpData.challengeTarget.character;
        var characterDetail = ArenaBase.challengeTarget.petFDs[0];
        SetRoleInfo(GetSub("role_bg_bottom"), characterDetail.tid, ArenaBase.challengeTarget.level, ArenaBase.challengeTarget.name, ArenaBase.challengeTarget.power, ArenaBase.getChallengeVipLevel());

		UIGridContainer grid = GetComponent<UIGridContainer>("Grid");
		grid.MaxCount = 3;
		for(int i = 0; i < 3; i++)
		{
			GameObject subcellObj = grid.controlList[i];
			PetData mPet = mPets[i];
			PetData yPet = yPets[i];
			if(mPet == null  && yPet == null)
			{
				subcellObj.SetActive (false);
				continue;
			}
			else
			{
				GameObject mySriteObj = GameCommon.FindObject (subcellObj, "my_srite");
				GameObject youSrite = GameCommon.FindObject (subcellObj, "you_srite");
				GameObject pkSprite = GameCommon.FindObject (subcellObj, "pk_sprite");
                if (mPet != null)
                {
                    mySriteObj.SetActive(true);
                    SetPetInfo(mySriteObj, mPet);
                }
                else
                {
                    mySriteObj.SetActive(false);
                }

                if (yPet != null)
                {
                    youSrite.SetActive(true);
                    SetPetInfo(youSrite, yPet);
                }
                else
                {
                    youSrite.SetActive(false);
                }

                //if(mPet != null && yPet != null)
                //{
                //    ELEMENT_RELATION type = GameCommon.GetElmentRaletion (DataCenter.mActiveConfigTable.GetRecord (mPet.tid), 
                //                                                          DataCenter.mActiveConfigTable.GetRecord (yPet.tid));
                //    string strSpriteName = "ui_dengyu";
                //    switch(type)
                //    {
                //        case ELEMENT_RELATION.ADVANTAGEOUS :
                //            strSpriteName = "ui_dayu";
                //            break;
                //        case ELEMENT_RELATION.INFERIOR :
                //            strSpriteName = "ui_xiaoyu";
                //            break;
                //    }
                //    pkSprite.GetComponent<UISprite>().spriteName = strSpriteName;
                //}
                //else
					pkSprite.SetActive (false);
			}
		}
		return true;
	}

	void GetMyPets()
	{
        for (int i = 0; i < 3; ++i)
        {
            mPets[i] = TeamManager.GetPetDataByTeamPos(i + 1);//PetLogicData.Self.GetFourVsFourPetDataByPos(i + 1);
        }
	}

	void GetYourPets()
	{
        //NiceData niceData = (NiceData)getObject ("OPEN");
        //for(int i = 0; i < 3; i++)
        //{
        //	string fieldName = "PET" + (i + 1).ToString ();
        //	if((int)niceData[fieldName] != 0)
        //	{
        //		DataRecord data = DataCenter.mActiveConfigTable.GetRecord ((int)niceData[fieldName]);
        //		if(data == null)
        //		{
        //			DEBUG.Log ("pet  DataRecord is null>>>>>>" + niceData[fieldName].ToString ());
        //			continue;
        //		}
        //
        //		PetData petData  = new PetData();
        //		petData.tid = (int)niceData[fieldName];
        //		petData.level = 1;
        //		petData.strengthenLevel = 0;
        //		petData.starLevel = TableCommon.GetNumberFromActiveCongfig (petData.tid, "STAR_LEVEL");
        //        petData.teamPos = -1;
        //		yPets[i] = petData;
        //	}
        //}
        PetFightDetail[] details = ArenaBase.challengeTarget.petFDs;
        int len = Mathf.Min(details.Length - 1, 3);

        for (int i = 0; i < len; ++i)
        {
            var detail = details[i + 1];

            if (detail != null)
            {
                var data = new PetData();
                data.tid = detail.tid;
                data.teamPos = -1;
                yPets[i] = data;
                yPetDetails[i] = detail;
            }
        }

        //for (int i = 0; i < 3; ++i)
        //{
            
        //    //by chenliang
        //    //begin

        //    if (allPets.Length <= 0)
        //        break;

        //    //end
        //    PetData pet = allPets.FirstOrDefault<PetData>(p => p.teamPos == i + 1);

        //    if (pet != null)
        //    {
        //        pet.starLevel = TableCommon.GetNumberFromActiveCongfig(pet.tid, "STAR_LEVEL");
        //        pet.teamPos = -1;
        //    }

        //    yPets[i] = pet;
        //}
	}

	void SetRoleInfo(GameObject parentObj, int modelIndex, int playerLevel, string name, int power, int vipLevel)
	{
		GameCommon.SetUIVisiable (parentObj, "sprite_boy", false);
		GameCommon.SetUIVisiable (parentObj, "sprite_girl", false);
        GameCommon.SetUIText(parentObj, "vip_level", vipLevel.ToString());

		GameCommon.SetUIText (parentObj, "name_lable", name);
        GameCommon.SetUIText(parentObj, "role_level_num", playerLevel.ToString());
        GameCommon.SetUIText(parentObj, "fight_strength_number", power.ToString());

		//设置头像
        GameCommon.SetRoleIcon(GameCommon.FindObject(parentObj, "photo").GetComponent<UISprite>(), modelIndex, GameCommon.ROLE_ICON_TYPE.CIRCLE);
	}

	void SetPetInfo(GameObject parentObj, PetData data)
	{
		GameCommon.SetPetIcon(parentObj.GetComponent <UISprite>(), data.tid);
		GameCommon.SetUIText (parentObj, "level_label", "Lv." + data.level.ToString ());
		GameCommon.SetStarLevelLabel (parentObj, data.starLevel,"star_level_label" );

		string str = data.strengthenLevel == 0? "" : "+" + data.strengthenLevel.ToString ();
		GameCommon.SetUIText (parentObj, "streng_then_level_label", str);
	}
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class OpponentPlayerInfoWindow : BattlePlayerInfoWindow
{
	public override void SetRoleData()
	{
		RoleData data = new RoleData();
		data.tid = OpponentCharacter.mInstance.mConfigIndex;
		data.level = OpponentCharacter.mInstance.mLevel;
		mRoleData = data;
	}

	public override PetData GetPetData(int pos)
	{
		return OpponentCharacter.mInstance.GetPetData (pos);
	}
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class PVP_FourVSFourFinishWindow : tWindow
{
	public override void Open(object param)
	{
		base.Open(param);
		tEvent respEvt = param as tEvent;
		if (respEvt != null)
		{
			bool bWin = (bool)respEvt["WIN"];
			SetVisible("win", bWin);
			SetVisible("fail", !bWin);

			int oldRankNum = 0;
			int newRankNum = 0;
			if(bWin)
			{
				oldRankNum = (int)respEvt["OLD_ARENA"];
			 	newRankNum = (int)respEvt["NEW_ARENA"];
				DataCenter.Set ("CUR_FOUR_VS_FOUR_RANK", newRankNum);
			}
			else
			{
				oldRankNum = DataCenter.Get ("CUR_FOUR_VS_FOUR_RANK");
				newRankNum = DataCenter.Get ("CUR_FOUR_VS_FOUR_RANK");
			}
			SetText ("old_rank_number", oldRankNum.ToString ());
			SetText ("new_rank_number", newRankNum.ToString ());

			ResetShowModel(bWin);

			SetText ("vip_level", RoleLogicData.Self.vipLevel.ToString ());
			SetText ("my_name", RoleLogicData.Self.name);

			GameCommon.SetPalyerIcon (GetComponent<UISprite>("my_photo"), RoleLogicData.Self.iconIndex);
		}
	}

	private void ResetShowModel(bool bWin)
	{
		ObjectManager.Self.ClearAll();
		GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
		BaseObject model = GameCommon.ShowCharactorModel(uiPoint, 1.6f);
		
		if (bWin)
			model.PlayAnim("win");
		else
			model.PlayAnim("lose");
	}
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class PVP_RankAwardsWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_close_pvp_rank_awards_window_button", new DefineFactory<Button_close_pvp_rank_awards_window_button>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);
		Refresh(param);
	}
	
	public override bool Refresh (object param)
	{
		int iCounts = 0;
		int iAdd = 1000;
		int type = (int)param;
		
		bool bVisible = type == 6 ? true : false;
		SetVisible ("4v4_award_title", bVisible);
		SetVisible ("award_title", !bVisible);
		
		foreach(KeyValuePair<int, DataRecord> d in DataCenter.mPvpRankAwardsConfig.GetAllRecord ())
		{
			if(d.Value["ARENA_TYPE"] == type && d.Value["INDEX"] >= 1000) iCounts++;
		}
		iAdd += 1000 * type;
		
		//		int counts = DataCenter.mPvpRankAwardsConfig.GetRecordCount () - 1;
		UIGridContainer grid = GetSub ("grid").GetComponent<UIGridContainer>();
		grid.MaxCount = iCounts;
		for(int i = 0; i < iCounts; i++)
		{
			GameObject obj = grid.controlList[i];
			int iIndex = i + iAdd;
			int iMinRank = TableCommon.GetNumberFromRankaward (iIndex, "RANKMIN");
			int iMaxRank = TableCommon.GetNumberFromRankaward (iIndex, "RANKMAX");
			string strRankNum = "";
			if(iMinRank == iMaxRank) strRankNum = iMaxRank.ToString () + "名";
			else strRankNum = iMinRank.ToString () + "~" + iMaxRank.ToString () + "名";
			GameCommon.SetUIText (obj, "rank_number", strRankNum);
			
			int iconCount = 1;
			for(int j = 1; j < 4; j++)
			{
				int iCount = TableCommon.GetNumberFromRankaward (iIndex, "AWARD_COUNT" + j.ToString ());
				if(iCount != 0) iconCount = j;
			}
			
			UIGridContainer gridIcons = GameCommon.FindObject (obj, "grid_icons").GetComponent<UIGridContainer>();
			gridIcons.MaxCount = iconCount;
			for(int n = 0; n < iconCount; n++)
			{
				GameObject iconObj = gridIcons.controlList[n];
				int itemType = TableCommon.GetNumberFromRankaward (iIndex, "AWARD_TYPE" + (n + 1).ToString ());
				int itemID = TableCommon.GetNumberFromRankaward (iIndex, "AWARD_ITEM" + (n + 1).ToString ());
				int itemCount = TableCommon.GetNumberFromRankaward (iIndex, "AWARD_COUNT" + (n + 1).ToString ());
				
				GameCommon.SetUIText (iconObj, "award_number_label", itemCount.ToString ());
				
				GameCommon.SetUIVisiable (iconObj, "level_lable", false);
				GameCommon.SetUIVisiable (iconObj, "star_level_label", false);
				GameCommon.SetUIVisiable (iconObj, "fragment_sprite", false);
				GameCommon.SetStarLevelLabel(iconObj, 0);

				if(itemType == (int)ITEM_TYPE.PET)
				{
					GameCommon.SetUIVisiable (iconObj, "level_lable", true);
//					GameCommon.SetUIVisiable (iconObj, "star_level_label", true);
					GameCommon.SetPetIcon (iconObj.GetComponent<UISprite>(), itemID);
					GameCommon.SetStarLevelLabel (iconObj, TableCommon.GetNumberFromActiveCongfig (itemID, "STAR_LEVEL"), "star_level_label");
				}
				else if(itemType == (int)ITEM_TYPE.PET_FRAGMENT)
				{
					GameCommon.SetUIVisiable (iconObj, "fragment_sprite", true);
					GameCommon.SetUIVisiable (iconObj, "level_lable", true);
//					GameCommon.SetUIVisiable (iconObj, "star_level_label", true);
					int fragmentPetID = TableCommon.GetNumberFromFragment(itemID, "ITEM_ID");
					GameCommon.SetPetIcon (iconObj.GetComponent<UISprite>(), fragmentPetID);
					GameCommon.SetStarLevelLabel (iconObj, TableCommon.GetNumberFromActiveCongfig (fragmentPetID, "STAR_LEVEL"), "star_level_label");
					
					int elementIndex = TableCommon.GetNumberFromActiveCongfig(fragmentPetID, "ELEMENT_INDEX");
					GameCommon.SetElementFragmentIcon (iconObj, "fragment_sprite", elementIndex);
				}
				else 
				{
					GameCommon.SetItemIcon (iconObj.GetComponent<UISprite>(), itemType, itemID);
				}
			}
		}
		return true;
	}
}

class Button_close_pvp_rank_awards_window_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PVP_RANK_AWARDS_WINDOW");
		return true;
	}
}

