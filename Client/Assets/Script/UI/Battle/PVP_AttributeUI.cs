using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;

//-------------------------------------------------------------------------
public class PVP_AttributeEnterWindow : tWindow
{
	static public int mCurrentAttributeIndex = 0;
	int mRequestNum = 0;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_attribute_enter_ready_button", new DefineFactory<Button_attribute_enter_ready_button>());
    }

	public override void OnOpen ()
	{
		tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
		evt.set ("IS_ATTRIBUTE_WAR", true);
		evt.set ("IS_ATTRIBUTE_ENTER", true);
		evt.DoEvent();
	}

	public override bool Refresh (object param)
	{
		tEvent respEvt = param as tEvent;
		mCurrentAttributeIndex = respEvt.get ("PVP_ELEMENT");
		string strAttributeBackgroundName = DataCenter.mPvpAttributeConfig.GetRecord (mCurrentAttributeIndex)["ATTRIBUTE_BACKGROUND_NAME"];
		Texture attributeBackground = GameCommon.LoadTexture(strAttributeBackgroundName);
		if(attributeBackground != null)
		{
			GetComponent<UITexture>("attribute_background").mainTexture = attributeBackground;
			GetComponent<UITexture>("attribute_background").MakePixelPerfect ();
		}
		else
			DEBUG.LogError ("can't find  texture >>" + strAttributeBackgroundName);

		string strAttributeName = DataCenter.mPvpAttributeConfig.GetRecord (mCurrentAttributeIndex)["ATTRIBUTE_NAME"];
		GameCommon.SetUITextRecursively (mGameObjUI, "attribute_name_background", strAttributeName);

		if (respEvt != null)
		{
			SetText ("now_rank_number", (string)respEvt["RANKING"]);
			
			if(GetSub ("star_time_number") != null && GetSub ("star_time_number").GetComponent<CountdownUI>() == null)
				SetCountdownTime("star_time_number", (Int64)respEvt["PVP_RANK_RESET_TIME"], new CallBack(this, "RefreshData", ++mRequestNum));
		}

//		GameCommon.GetButtonData (GetSub( "attribute_enter_ready_button")).set ("ATTRIBUTE_INDEX", mCurrentAttributeIndex);
//		GameCommon.GetButtonData (GetSub( "attribute_enter_ready_button")).set ("ATTRIBUTE_PVP_DATA", param);

		return true;
	}

	public void RefreshData(object obj)
	{ 
		if(Convert.ToInt32 (obj) == 1)
			OnOpen ();
		else 
			EventCenter.WaitAction (this, "RefreshDataAgain", obj, 5.0f);

		if(GetSub ("star_time_number") != null && GetSub ("star_time_number").GetComponent<CountdownUI>() != null)
			MonoBehaviour.Destroy (GetSub ("star_time_number").GetComponent<CountdownUI>());
	}

	public void RefreshDataAgain(object obj)
	{
		if((DataCenter.GetData ("PVP_ATTR_ENTER_WINDOW") as tWindow).IsOpen ())
			OnOpen();
	}
}

class Button_attribute_enter_ready_button : CEvent
{
    public override bool _DoEvent()
    {
//		string strAttributeIndex = getObject ("ATTRIBUTE_INDEX").ToString ();
        GlobalModule.ClearAllWindow();
		DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
		DataCenter.OpenWindow("PVP_ATTR_READY_WINDOW", false);
        return true;
    }
}
//-------------------------------------------------------------------------
public class PVP_AttributeReadyWindow : tWindow
{
    public override void Open(object param)
    {
        base.Open(param);
		DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
		DataCenter.OpenWindow("PVP_ATTR_READY_RANK_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), param);
        DataCenter.OpenWindow("PVP_ATTR_READY_PLAYER_RANK");
		DataCenter.SetData ("PVP_ATTR_READY_PLAYER_RANK", "SHOW_PET_BUTTON", true);
    }

	public override void Close ()
	{
		base.Close ();
		DataCenter.CloseWindow("PVP_ATTR_READY_RANK_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString ());
		DataCenter.CloseWindow("PVP_ATTR_READY_PLAYER_RANK");
	}
}

public class PVP_AttibuteRankWindow : PVP_RankWindow
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_overall_rank_button", new DefineFactory<Button_overall_rank_button>());
		EventCenter.Self.RegisterEvent("Button_friend_rank_button", new DefineFactory<Button_friend_rank_button>());
	}

	public override void OnOpen ()
	{
		if(mbIsFriendRank)
		{
			tEvent evt = Net.StartEvent("CS_RequestFriendRank");
			evt.set ("IS_ATTRIBUTE_WAR", true);
			evt.DoEvent();
		}
		else
		{
			tEvent evt = Net.StartEvent("CS_RequestPVPRank");
			evt.set ("IS_ATTRIBUTE_WAR", true);
			evt.DoEvent();
		}
	}

	public override void RefreshData(object obj)
	{
		if(GetSub ("pvp_rank_reset_time") != null && GetSub ("pvp_rank_reset_time").GetComponent<CountdownUI>() != null)
			MonoBehaviour.Destroy (GetSub ("pvp_rank_reset_time").GetComponent<CountdownUI>());

		if(Convert.ToInt32 (obj) == 1)
			DataCenter.OpenWindow ("PVP_ATTR_READY_WINDOW", false);
		else
			EventCenter.WaitAction (this, "RefreshDataAgain", obj, 5.0f);
	}

	public override void RefreshDataAgain(object obj)
	{
		if((DataCenter.GetData ("PVP_ATTR_READY_RANK_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString ()) as tWindow).IsOpen ())
			DataCenter.OpenWindow ("PVP_ATTR_READY_WINDOW", false);
	}

	public override void SetVisitButtonData(GameObject obj, string playerID, string name)
	{
		NiceData visitButtonData = GameCommon.GetButtonData (obj, "visit_button");
		visitButtonData.set ("FRIEND_ID", playerID);
		visitButtonData.set ("FRIEND_NAME", name);
		visitButtonData.set ("WINDOW_NAME", "PVP_ATTR_READY_WINDOW");
	}

}

public class Button_overall_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		if((DataCenter.GetData ("PVP_RANK_WINDOW") as tWindow).IsOpen ())
			DataCenter.OpenWindow ("PVP_RANK_WINDOW", false);
		else
			DataCenter.OpenWindow("PVP_ATTR_READY_RANK_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), false);
		return true;
	}
}

public class Button_friend_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		if((DataCenter.GetData ("PVP_RANK_WINDOW") as tWindow).IsOpen ())
			DataCenter.OpenWindow ("PVP_RANK_WINDOW", true);
		else
			DataCenter.OpenWindow("PVP_ATTR_READY_RANK_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), true);
		return true;
	}
}

public class PVP_AttibuteReadyPlayerRankWindow : PVP_PlayerRankWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_prepare_battle_button", new DefineFactory<Button_prepare_battle_button>());
		EventCenter.Self.RegisterEvent("Button_attribute_battle_teame_button", new DefineFactory<Button_prepare_battle_button>());
		EventCenter.Self.RegisterEvent("Button_attribute_rank_award_button", new DefineFactory<Button_attribute_rank_award_button>());

		EventCenter.Self.RegisterEvent("Button_start_attribute_battle_button", new DefineFactory<Button_start_attribute_battle_button>());
		EventCenter.Self.RegisterEvent("Button_attribute_rank_list_button", new DefineFactory<Button_attribute_rank_list_button>());
		Net.Register("CS_RequestStartPVPAttibuteBattle", new DefineFactory<CS_RequestStartPVPAttibuteBattle>());
    }

    public override void OnOpen()
    {
		tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
		evt.set ("IS_ATTRIBUTE_WAR", true);
		evt.set ("IS_ATTRIBUTE_ENTER", false);
		evt.DoEvent();
    }
	
	public override void SetButtonVisible(bool b)
	{
		SetVisible("start_attribute_battle_button", !b);
		SetVisible("prepare_battle_button", b);
		SetVisible("attribute_battle_teame_button", b);
		SetVisible("attribute_rank_list_button", !b);
	}
}

public class Button_prepare_battle_button : CEvent
{
    public override bool _DoEvent()
    {
//        GlobalModule.ClearAllWindow();
//		DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
		DataCenter.CloseWindow ("PVP_ATTR_READY_WINDOW");
        DataCenter.OpenWindow("PVP_ATTR_START_WINDOW");
        return true;
    }
}

public class Button_attribute_rank_award_button :CEvent
{
	public override bool _DoEvent()
	{
		bool isAttributeRank = (getObject ("BUTTON") as GameObject).transform.name == "attribute_rank_award_button" ? true : false;
		int type = isAttributeRank ? (int)PVP_AttributeEnterWindow.mCurrentAttributeIndex : 5;
		DataCenter.OpenWindow ("PVP_RANK_AWARDS_WINDOW", type);
		return true;
	}
}
//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class Button_pvp_attr_pet_check_box : CEvent
{
    public override bool _DoEvent()
    {
        int nPos = get("POS_INDEX");
		DataCenter.SetData("PVP_ATTR_READY_PET_TEAM_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), "SELECT_POS", nPos);
        return true;
    }
}
//-------------------------------------------------------------------------
public class CS_ChangeAttributePVPTeam : BaseNetEvent
{
    public override void _OnResp(tEvent respEvent)
    {
        respEvent.Dump();
        if (respEvent.get("RESULT"))
        {
            NiceData teamPosData = respEvent.getObject("TEAM_DATA") as NiceData;
            if (teamPosData != null)
            {
                //PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
                PetLogicData.mAttributePVPTeam = teamPosData;
				DataCenter.SetData("PVP_ATTR_READY_PET_TEAM_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), "UPDATE_TEAM", true);
				DataCenter.SetData("PVP_ATTR_READY_PET_BAG", "TEAM_ADJUST_LOAD", true);
            }
        }
    }
}

public class CS_RequestPVPAttributeTeam : BaseNetEvent
{
    public override void _OnResp(tEvent respEvent)
    {
        NiceData teamPosData = respEvent.getObject("TEAM_DATA") as NiceData;
        if (teamPosData != null)
        {
            //PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
            PetLogicData.mAttributePVPTeam = teamPosData;
			DataCenter.SetData("PVP_ATTR_READY_PET_TEAM_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), "UPDATE_TEAM", true);
        }
    }
}

public class PVP_AttributePetTeamWindow : tWindow
{
    public PetData mSelectPet;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_attribute_select_pet_button", new DefineFactory<Button_attribute_select_pet_button>());

        EventCenter.Self.RegisterEvent("Button_attribute_finish_select_pet_button", new DefineFactory<Button_attribute_finish_select_pet_button>());
        EventCenter.Self.RegisterEvent("Button_pvp_attr_pet_check_box", new DefineFactory<Button_pvp_attr_pet_check_box>());

        Net.Register("CS_ChangeAttributePVPTeam", new DefineFactory<CS_ChangeAttributePVPTeam>());
        Net.Register("CS_RequestPVPAttributeTeam", new DefineFactory<CS_RequestPVPAttributeTeam>());
    }

    public override void OnOpen()
    {
        ShowSelectPetButton(true);
		ShowSelectPos(false);
        tEvent evt = Net.StartEvent("CS_RequestPVPAttributeTeam");
        evt.DoEvent();
    }

    public void ShowSelectPetButton(bool bShow)
    {
        SetVisible("attribute_select_pet_button", bShow);
        SetVisible("attribute_finish_select_pet_button", !bShow);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "SHOW_SELECT_PET_BUTTON")
        {
            ShowSelectPetButton((bool)objVal);
            return;
        }
        else if (keyIndex == "SELECT_POS")
        {
            int nPos = (int)objVal;
            if (mSelectPet != null)
            {
                PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
                PetData pet = petLogic.GetAttributePVPPet(nPos);
                tEvent evt = Net.StartEvent("CS_ChangeAttributePVPTeam");
                evt.set("EQUIP", true);
                evt.set("PET_ID", mSelectPet.itemId);
                evt.set("TEAM_POS", nPos);

                evt.DoEvent();
            }
            return;
        }
		else if (keyIndex == "UPDATE_TEAM")
        {
            RefreshTeamContext();
            ShowSelectPos(false);
            return;
        }

        base.onChange(keyIndex, objVal);
    }

    public void OnSelectPet(PetData petData)
    {
        mSelectPet = petData;

        PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;

        if (petLogic.InAttributePVPTeam(petData))
        {
            tEvent evt = Net.StartEvent("CS_ChangeAttributePVPTeam");
            evt.set("EQUIP", false);
            evt.set("PET_ID", petData.itemId);
            evt.DoEvent();
            ShowSelectPos(false);
        }
        else
            ShowSelectPos(true);            
    }

    public void ShowSelectPos(bool bShow)
    {
        for (int i = 0; i < 6; ++i)
        {
            ShowButtonFlag(i, bShow);
        }
    }

    void ShowButtonPetInfo(int pos, bool bVisiable)
    {
        string key = "pvp_pet_check_" + pos.ToString();
        GameObject buttonObj = GameCommon.FindObject(mGameObjUI, key);
        GameCommon.SetUIVisiable(buttonObj, "pvp_pet_check_info", bVisiable);
        GameCommon.SetUIVisiable(buttonObj, "back_ground", !bVisiable);
    }

    GameObject GetPVPPetPosObject(int pos)
    {
        string key = "pvp_pet_check_" + pos.ToString();
        return GameCommon.FindObject(mGameObjUI, key);
    }

    void ShowButtonFlag(int pos, bool bVisiable)
    {
		string key = "pvp_pet_check_" + pos.ToString();
        GameObject buttonObj = GameCommon.FindObject(mGameObjUI, key);
        GameCommon.SetUIVisiable(buttonObj, "check_mark_1", bVisiable);
    }


    void RefreshTeamContext()
    {
        PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
        int nAttackScore = 0;
        for (int i = 0; i < 6; i++)
        {
            GameObject obj = GetPVPPetPosObject(i);
            PetData data = petLogic.GetAttributePVPPet(i);
            if (data != null)
            {
                ShowButtonPetInfo(i, true);
				PVP_PetTeamWindow.InitPetData(obj, data);
                GameCommon.SetUIVisiable(obj, "back_ground", false);

				nAttackScore += PVP_PetTeamWindow.TotalAttackScore(data);
            }
            else
            {
                ShowButtonPetInfo(i, false);
            }

			NiceData butData = GameCommon.GetButtonData(obj, "pvp_attr_pet_check_box");
            if (butData != null)
                butData.set("POS_INDEX", i);
			else
				DEBUG.LogError("No exist button  >pvp_attr_pet_check_box at "+obj.name);
        }
        SetText("battle_number", nAttackScore.ToString());
    }
}
public class Button_attribute_select_pet_button : CEvent
{
    public override bool _DoEvent()
    {
		DataCenter.CloseWindow("PVP_ATTR_START_PLAYER_RANK");
		DataCenter.OpenWindow("PVP_ATTR_READY_PET_BAG");
		DataCenter.SetData("PVP_ATTR_READY_PET_TEAM_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), "SHOW_SELECT_PET_BUTTON", false);
        return true;
    }
}

public class Button_attribute_finish_select_pet_button :CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("PVP_ATTR_READY_PET_BAG");
        DataCenter.OpenWindow("PVP_ATTR_START_PLAYER_RANK");
		DataCenter.SetData ("PVP_ATTR_START_PLAYER_RANK", "SHOW_PET_BUTTON", false);
		DataCenter.SetData("PVP_ATTR_READY_PET_TEAM_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), "SHOW_SELECT_PET_BUTTON", true);

		return true;
    }
}

public class PVP_AttributePetBag : PVP_PetBagWindow
{
    protected override PetData GetPVPUsePet(PetLogicData petLogic, int teamPos)
    {
        return petLogic.GetAttributePVPPet(teamPos);
    }

    protected override bool IsUsePVPPet(PetLogicData petLogic, PetData petData)
    {
        return petLogic.InAttributePVPTeam(petData);
    }
}
//-------------------------------------------------------------------------
//-------------------------------------------------------------------------

public class PVP_AttributeStartWindow : tWindow
{
    public override void Open(object param)
    {
        base.Open(param);

		DataCenter.OpenWindow("PVP_ATTR_READY_PET_TEAM_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString ());
        DataCenter.OpenWindow("PVP_ATTR_START_PLAYER_RANK");
		DataCenter.SetData ("PVP_ATTR_START_PLAYER_RANK", "SHOW_PET_BUTTON", false);
    }

	public override void Close ()
	{
		base.Close ();

		DataCenter.CloseWindow("PVP_ATTR_READY_PET_TEAM_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString ());
		DataCenter.CloseWindow("PVP_ATTR_START_PLAYER_RANK");
	}
}

//public class PVP_AttributeStartPlayerRankWindow : tWindow
//{
//    public override void Init()
//    {
//      EventCenter.Self.RegisterEvent("Button_start_attribute_battle_button", new DefineFactory<Button_start_attribute_battle_button>());
//		EventCenter.Self.RegisterEvent("Button_attribute_rank_list_button", new DefineFactory<Button_attribute_rank_list_button>());
//      Net.Register("CS_RequestStartPVPAttibuteBattle", new DefineFactory<CS_RequestStartPVPAttibuteBattle>());
//    }
//
//    public override void OnOpen()
//    {
//      SetVisible("start_attribute_battle_button", true);
//      SetVisible("prepare_battle_button", false);
//		SetVisible("attribute_battle_teame_button", false);
//		SetVisible("attribute_rank_list_button", true);
//    }
//}

public class Button_attribute_rank_list_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PVP_ATTR_START_WINDOW");
		DataCenter.OpenWindow ("PVP_ATTR_READY_WINDOW", false);
		return true;
	}
}

public class Button_start_attribute_battle_button : CEvent
{
    public override bool _DoEvent()
    {
        tEvent evt = Net.StartEvent("CS_RequestStartPVPAttibuteBattle");
        evt.DoEvent();
        return true;
    }
}

public class CS_RequestStartPVPAttibuteBattle : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        respEvt.Dump();

        NiceTable usePetData = respEvt["USE_PET"].mObj as NiceTable;
        if (usePetData != null)
        {
            GlobalModule.ClearAllWindow();

            tEvent evt = EventCenter.Start("TM_WaitShowVictoryWindow");
            if (evt != null)
            {
                evt.set("SEARCH_RESULT", respEvt);
                evt.DoEvent();
            }
            else
                EventCenter.Log(LOG_LEVEL.ERROR, "No register TM_WaitShowVictoryWindow");
        }
        else
        {
			eRequestPVPResult result = (eRequestPVPResult)(int)respEvt["RESULT"];
            if (result == eRequestPVPResult.PVP_UsePetEmpty)
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_EQUIP_PETS_FIRST);
            else if (result == eRequestPVPResult.PVP_TicketEmpty)
                DataCenter.OpenWindow("PVP_BUY_TICKET");
			else if (result == eRequestPVPResult.PVP_NotOpen)
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_NOT_OPEN);
            else
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_SEARCH_AGAIN);
        }
    }
}

