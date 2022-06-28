using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using DataTable;

//-------------------------------------------------------------------------
enum eRequestPVPResult
{
    PVP_Succeed,
    PVP_TicketEmpty,
    PVP_SearchFail,
    PVP_UsePetEmpty,
	PVP_NotOpen,
}
//-------------------------------------------------------------------------
public class PVP_SixEnterWindow : tWindow
{
	int mRequestNum = 0;
	public override void OnOpen ()
	{
		tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
		evt.set ("PVP_SIX_ENTER", true);
		evt.DoEvent();
	}

	public override bool Refresh (object param)
	{
		tEvent respEvt = param as tEvent;
		if (respEvt != null)
		{
			SetText ("now_rank_number", (string)respEvt["RANKING"]);

			if(GetSub ("star_time_number") != null && GetSub ("star_time_number").GetComponent<CountdownUI>() == null)
				SetCountdownTime("star_time_number", (Int64)respEvt["PVP_RANK_RESET_TIME"], new CallBack(this, "RefreshData", ++mRequestNum));

			int iPvpAttribute = respEvt.get ("PVP_ELEMENT");
			DataCenter.SetData ("PVP_SELECT_ENTER", "ATTRIBUTE_PVP_OPEN_OR_CLOSE", iPvpAttribute);
		}
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
		if((DataCenter.GetData ("PVP_SIX_ENTER_WINDOW") as tWindow).IsOpen ())
			OnOpen ();
	}
}


public class PVP_SelectEnterWindow : tWindow
{
	public override void Init()
	{
        EventCenter.Self.RegisterEvent("Button_speel_pk_button", new DefineFactory<Button_speel_pk_button>());
        EventCenter.Self.RegisterEvent("Button_attribute_select_enter_button", new DefineFactory<Button_attribute_select_enter_button>());
        EventCenter.Self.RegisterEvent("Button_pet_battle_button", new DefineFactory<Button_pet_battle_button>());
		EventCenter.Self.RegisterEvent("Button_endless_battle_button", new DefineFactory<Button_endless_battle_button>());
		EventCenter.Self.RegisterEvent("Button_top_battle_button", new DefineFactory<Button_top_battle_button>());
        
	}

    public override void Open(object param)
    {
        base.Open(param);

		if(param == "FOUR_VS_FOUR")
		{
			GameCommon.ToggleTrue (GetSub ("top_battle_button"));
			DataCenter.OpenWindow("PVP_FOUR_VS_FOUR_ENTER");
		}
		else
		{
			GameCommon.ToggleTrue (GetSub ("pet_battle_button"));
	        DataCenter.OpenWindow("PVP_SIX_ENTER_WINDOW");
		}

		DataCenter.OpenWindow ("BACK_GROUP_PVP_WINDOW");
		DataCenter.SetData ("BACK_GROUP_PVP_WINDOW", "BACK_WINDOW", true);

		InitButtonIsUnLockOrNot();
    }

	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ToggleCloseButCanClick (GetSub ("pet_battle_button"), UNLOCK_FUNCTION_TYPE.PVP_PET, "checkmark");
		GameCommon.ToggleCloseButCanClick (GetSub ("attribute_select_enter_button"), UNLOCK_FUNCTION_TYPE.PVP_PET, "checkmark");
		GameCommon.ToggleCloseButCanClick (GetSub ("endless_battle_button"), UNLOCK_FUNCTION_TYPE.ENDLESS_PVP_ROLE, "checkmark");
		GameCommon.ToggleCloseButCanClick (GetSub ("top_battle_button"), UNLOCK_FUNCTION_TYPE.PEAK_FIGHT, "checkmark");
	}

	public override void Close ()
	{
		DataCenter.CloseWindow ("BACK_GROUP_PVP_WINDOW");
		base.Close ();
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "ATTRIBUTE_PVP_OPEN_OR_CLOSE")
		{
			GameObject attributeSelectEnterButton = GetSub ("attribute_select_enter_button");
			int iPvpAttribute = Convert.ToInt32 (objVal);
			GameCommon.GetButtonData (attributeSelectEnterButton).set ("PVP_ELEMENT", iPvpAttribute);
		}
	}

	public static void CloseAllWindow()
	{
		DataCenter.CloseWindow("PVP_ATTR_ENTER_WINDOW");
		DataCenter.CloseWindow("PVP_SIX_ENTER_WINDOW");
		DataCenter.CloseWindow("PVP_ENDLESS_BATTLE_ENTER_WINDOW");
		DataCenter.CloseWindow ("PVP_FOUR_VS_FOUR_ENTER");
	}

}

class Button_pet_battle_button : CEvent
{
    public override bool _DoEvent()
    {
		UNLOCK_FUNCTION_TYPE  type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");
		if(GameCommon.ButtonEnbleShowInfo(type))
			return true;

        //GlobalModule.ClearAllWindow();
		if((DataCenter.GetData ("PVP_SIX_ENTER_WINDOW") as tWindow).IsOpen ())
			return true;

		PVP_SelectEnterWindow.CloseAllWindow ();
        DataCenter.OpenWindow("PVP_SIX_ENTER_WINDOW");

        return true;
    }
}

class Button_attribute_select_enter_button : CEvent
{
    public override bool _DoEvent()
	{
		UNLOCK_FUNCTION_TYPE  type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");
		if(GameCommon.ButtonEnbleShowInfo(type))
			return true;

		if((DataCenter.GetData ("PVP_ATTR_ENTER_WINDOW") as tWindow).IsOpen ())
			return true;

		if(Convert.ToInt32(getObject ("PVP_ELEMENT")) != -1)
		{
			PVP_SelectEnterWindow.CloseAllWindow ();
	        DataCenter.OpenWindow("PVP_ATTR_ENTER_WINDOW");
		}
		else
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_PVP_ATTRIBUTE_CLOSE);
		return true;
	}
}

//无尽模式 进入窗口
class Button_endless_battle_button : CEvent
{
	public override bool _DoEvent()
	{
		UNLOCK_FUNCTION_TYPE  type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");
		if(GameCommon.ButtonEnbleShowInfo(type))
			return true;

		if((DataCenter.GetData ("PVP_ENDLESS_BATTLE_ENTER_WINDOW") as tWindow).IsOpen ())
			return true;
		
		PVP_SelectEnterWindow.CloseAllWindow ();
		DataCenter.OpenWindow("PVP_ENDLESS_BATTLE_ENTER_WINDOW");

		return true;
	}
}
//巅峰挑战  进入窗口
class Button_top_battle_button : CEvent
{
	public override bool _DoEvent()
	{
		UNLOCK_FUNCTION_TYPE  type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");
		if(GameCommon.ButtonEnbleShowInfo(type))
			return true;

		if((DataCenter.GetData ("PVP_FOUR_VS_FOUR_ENTER") as tWindow ).IsOpen ())
			return true;

		PVP_SelectEnterWindow.CloseAllWindow ();
		DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_ENTER");
		return true;
	}
}

public class Button_speel_pk_button : CEvent
{
	public override bool _DoEvent()
	{
        BaseUI.OpenWindow("PVP_SEARCH_OPPONENT", null, true);
		return true;
	}
}

public class CS_RequestPVP6Score : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        respEvt.Dump();
        //Log("*** Server time>" + CommonParam.NowServerTime().ToString());
        //DataCenter.CloseWindow("PVP_SELECT_ENTER");

        //DataCenter.CloseWindow("PVP_TEAM_ADJUST");

		if(get ("IS_ATTRIBUTE_WAR"))
		{
            if (get("WINDOW_NAME") == "RANK_WINDOW")
            {
                DataCenter.SetData("RANK_WINDOW", "REFRESH_MY_ELEMENT_RANK", respEvt.getData());
            }
			else if(get ("IS_ATTRIBUTE_ENTER"))
				DataCenter.SetData("PVP_ATTR_ENTER_WINDOW", "REFRESH", respEvt);
			else
			{
				tWindow t = DataCenter.GetData ("PVP_ATTR_READY_PLAYER_RANK") as tWindow;
				if(t.IsOpen ())
					DataCenter.SetData("PVP_ATTR_READY_PLAYER_RANK", "REFRESH", respEvt);
				else
					DataCenter.SetData("PVP_ATTR_START_PLAYER_RANK", "REFRESH", respEvt);
			}
		}
		else
		{
            if (get("WINDOW_NAME") == "RANK_WINDOW")
            {
                DataCenter.SetData("RANK_WINDOW", "REFRESH_MY_ARENA_RANK", respEvt.getData());
            }
			else if(get ("PVP_SIX_ENTER"))
				DataCenter.SetData("PVP_SIX_ENTER_WINDOW", "REFRESH", respEvt);
			else
       			DataCenter.SetData("PVP_PLAYER_RANK_WINDOW", "REFRESH", respEvt);
		}
        //BaseUI.OpenWindow("PVP_SEARCH_OPPONENT", respEvt, true);

        //DataCenter.OpenWindow("PVP_SEARCH_OPPONENT", respEvt);
    }
}

//-------------------------------------------------------------------------
public class Button_rank_list_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("PVP_PET_TEAM_WINDOW");
        DataCenter.SetData("PVP_PLAYER_RANK_WINDOW", "SHOW_PET_BUTTON", true);
        DataCenter.OpenWindow("PVP_RANK_WINDOW", false);
        return true;
    }
}

public class Button_battle_teame_button : CEvent
{
    public override bool _DoEvent()
    {
        //BaseUI.OpenWindow("PVP_SEARCH_OPPONENT", null, true);

        DataCenter.CloseWindow("PVP_RANK_WINDOW");
        DataCenter.SetData("PVP_PLAYER_RANK_WINDOW", "SHOW_PET_BUTTON", false);
        DataCenter.OpenWindow("PVP_PET_TEAM_WINDOW");
		DataCenter.OpenWindow("INFO_GROUP_WINDOW");

		DataCenter.SetData("PVP_PET_TEAM_WINDOW", "ADJUST_PET_BUTTON", true);
        return true;
    }
}
//-------------------------------------------------------------------------
// PVP 当前玩家的排行信息 右边
public class PVP_PlayerRankWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_give_battle_button", new DefineFactory<Button_give_battle_button>());
        EventCenter.Self.RegisterEvent("Button_rank_list_button", new DefineFactory<Button_rank_list_button>());
        EventCenter.Self.RegisterEvent("Button_battle_teame_button", new DefineFactory<Button_battle_teame_button>());
		EventCenter.Self.RegisterEvent("Button_give_battle_ready_button", new DefineFactory<Button_battle_teame_button>());
		EventCenter.Self.RegisterEvent("Button_rank_award_button", new DefineFactory<Button_attribute_rank_award_button>());

    }

	public override void OnOpen()
	{
		SetButtonData ();
        tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
        evt.DoEvent();
	}

	public void OnAppendOneTicket(int nowTicketCount)
	{
		SetText("pvp_ticket_number", nowTicketCount.ToString() + "/10");
	}

    public override bool Refresh(object param)
    {
        tEvent respEvt = param as tEvent;
        if (respEvt != null)
        {
            GameCommon.SetUIText(mGameObjUI, "current_integral_number", (string)respEvt["SCORE"]);
            GameCommon.SetUIText(mGameObjUI, "current_rank_number", (string)respEvt["RANKING"]);
            GameCommon.SetUIText(mGameObjUI, "highest_rank_number", (string)respEvt["RANKING_MAX"]);
            GameCommon.SetUIText(mGameObjUI, "highest_streak_number", (string)respEvt["MAX_CONTINUOUS_WIN"]);

            int ticketCount = respEvt["PVP_TICKET"];
            GameCommon.SetUIText(mGameObjUI, "pvp_ticket_number", ticketCount.ToString() + "/10");
            System.UInt64 beginRestoreTime = respEvt["TICKET_RESTORE_TIME"];
            float needTime = 0;

            int ticketTime = (int)DataCenter.mGlobalConfig.GetData("PVP_TICKET_TIME", "VALUE");

            if (beginRestoreTime > 0 && CommonParam.NowServerTime() - (System.Int64)beginRestoreTime < ticketTime)
            {
				GameCommon.SetUIVisiable(mGameObjUI, "battle_recover_time_number", true);
                SetCountdownTime("battle_recover_time_number", (System.Int64)beginRestoreTime + ticketTime, new CallBack(this, "OnAppendOneTicket", ticketCount + 1));
                //needTime = (float)( beginRestoreTime+30*60 - (System.UInt64)CommonParam.NowServerTime());
            }
            else
            {
                GameCommon.SetUIVisiable(mGameObjUI, "battle_recover_time_number", false);
            }

			if(GetSub ("pvp_rank_reset_time") != null && GetSub ("pvp_rank_reset_time").GetComponent<CountdownUI>() == null)
				SetCountdownTime("pvp_rank_reset_time", (Int64)respEvt["PVP_RANK_RESET_TIME"]);
            //string t = GameCommon.FormatTime(needTime);
            //GameCommon.SetUIText(mGameObjUI, "battle_recover_time_number", t);
        }        

        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
           case "SHOW_PET_BUTTON":
				SetButtonVisible ((bool)objVal);
                break;
        }
    }

	protected virtual void SetButtonData()
	{
		NiceData data = GameCommon.GetButtonData (mGameObjUI, "give_battle_button");
		if(data != null )
			data.set ("WINDOW_NAME", "PVP_PET_BAG_WINDOW");
	}

	public virtual void SetButtonVisible(bool b)
	{
		SetVisible("battle_teame_button", b);
		SetVisible("rank_list_button", !b);
		SetVisible("give_battle_button", !b);
		SetVisible("give_battle_ready_button", b);
	}

}

public class CS_RequestPVPRank : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        if (get("WINDOW_NAME") == "RANK_WINDOW")
        {
            if (get("IS_ATTRIBUTE_WAR"))
                DataCenter.SetData("RANK_WINDOW", "REFRESH_ELEMENT_RANK", respEvt.getData());
            else
                DataCenter.SetData("RANK_WINDOW", "REFRESH_ARENA_RANK", respEvt.getData());
        }
		else if (get ("WINDOW_NAME") == "PVP_ENDLESS_BATTLE_WINDOW")
		{
			DataCenter.SetData("PVP_ENDLESS_BATTLE_WINDOW", "REFRESH_OVERALL_RANK", respEvt);
		}
        else
        {
            if (get("IS_ATTRIBUTE_WAR"))
                DataCenter.SetData("PVP_ATTR_READY_RANK_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString(), "REFRESH", respEvt);
            else
                DataCenter.SetData("PVP_RANK_WINDOW", "REFRESH", respEvt);
        }
    }
}

public class CS_RequestFriendRank : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		if (get("WINDOW_NAME") == "PVP_ENDLESS_BATTLE_WINDOW")
		{
			DataCenter.SetData("PVP_ENDLESS_BATTLE_WINDOW", "REFRESH_FRIEND_RANK", respEvt);
		}
		else
		{
			if(get ("IS_ATTRIBUTE_WAR"))
				DataCenter.SetData("PVP_ATTR_READY_RANK_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString (), "REFRESH", respEvt);
			else
				DataCenter.SetData("PVP_RANK_WINDOW", "REFRESH", respEvt);
		}
	}
}

// PVP 排行榜, 左边
public class PVP_RankWindow : tWindow
{
	static public bool mbIsFriendRank = false;
	List<PvpRankData> mList = new List<PvpRankData>();
	public int mRequestNum = 0;
    public override void Init()
    {
        Net.gNetEventCenter.RegisterEvent("CS_RequestPVPRank", new DefineFactory<CS_RequestPVPRank>());
		Net.gNetEventCenter.RegisterEvent("CS_RequestFriendRank", new DefineFactory<CS_RequestFriendRank>());
		EventCenter.Self.RegisterEvent("Button_visit_button", new DefineFactory<Button_visit_friend_button>());
		DataCenter.RegisterData ("PVP_VISIT_FRIEND_BACK", new PVPVisitFriendBack());
    }

//	public override void onChange (string keyIndex, object objVal)
//	{
//		base.onChange (keyIndex, objVal);
//		if(keyIndex == "IS_PVP_FRIEND_RANK")
//		{
//			IsFriendRank(Convert.ToBoolean (objVal));
//		}
//	}

    public override void Open(object param)
    {
        base.Open(param);
		IsFriendRank (Convert.ToBoolean (param));
    }

	public override void OnOpen ()
	{
		if(mbIsFriendRank)
		{
			tEvent evt = Net.StartEvent("CS_RequestFriendRank");
			evt.DoEvent();
		}
		else
		{
			tEvent evt = Net.StartEvent("CS_RequestPVPRank");
			evt.DoEvent();
		}
	}

	public void IsFriendRank(bool b)
	{
		mbIsFriendRank = b;
		GetSub ("overall_rank_button").GetComponent<UIToggle>().value = !mbIsFriendRank;
		GetSub ("friend_rank_button").GetComponent<UIToggle>().value = mbIsFriendRank;
		GetSub ("overall_rank").SetActive (!mbIsFriendRank);
		GetSub ("friend_rank").SetActive (mbIsFriendRank);

		set ("IS_PVP_FRIEND_RANK", mbIsFriendRank);
	}

    public override bool Refresh(object param)
	{
        tEvent respEvt = param as tEvent;

        if (respEvt==null)
            return false;

		if(GetSub ("pvp_rank_reset_time") != null && GetSub ("pvp_rank_reset_time").GetComponent<CountdownUI>() == null)
			SetCountdownTime("pvp_rank_reset_time", (Int64)respEvt["PVP_RANK_RESET_TIME"], new CallBack(this, "RefreshData", ++mRequestNum));

        DataBuffer randData = respEvt.getObject("RANK_DATA") as DataBuffer;

        if (randData == null)
        {
            DEBUG.LogError("get rank data fail");
            return false;
        }

		randData.seek(0);

        int count = 0;
        if (!randData.read(out count))
            return false;

        for (int i = 0; i < count; ++i)
        {
            string name = "";
            if (!randData.readOne(out name))
                break;

            int pvpScore = 0;
            if (!randData.read(out pvpScore))
                break;

			string playerID = "";
			if (!randData.readOne(out playerID))
				break;

			int vipLevel = 0;
			if (!randData.read(out vipLevel))
				break;

			PvpRankData data = new PvpRankData();
			data.mName = name;
			data.mScore = pvpScore;
			data.mPlayerID = playerID;
			data.mVipLevel = vipLevel;
			mList.Add (data);
		}

//		mList = mList.OrderByDescending (d => d.mScore).ToList ();
		mList = GameCommon.SortList(mList, SortListByScore);
		int num = mList.Count;
		UIGridContainer randGrid = mGameObjUI.GetComponentInChildren<UIGridContainer>();
		randGrid.MaxCount = num;
		for(int j = 0; j < num; j++)
		{
			GameObject item = randGrid.controlList[j];
		    GameCommon.SetUIText(item, "rank_label", (j+1).ToString());
			string strVip = "";
//			if(mList[j].mVipLevel != 0) strVip = "  (VIP" + mList[j].mVipLevel.ToString () + ")";
			GameCommon.SetUIText(item, "name_label", mList[j].mName + strVip);
		    GameCommon.SetUIText(item, "pvp_score_label", mList[j].mScore.ToString ());

            SetFlag(item, j);
			SetVisitButtonData(item, mList[j].mPlayerID, mList[j].mName);
		}

		mList.Clear ();
        return true;
    }

	int SortListByScore(PvpRankData a, PvpRankData b)
	{
		return GameCommon.Sort (a.mScore, b.mScore, true);
	}

	public virtual void RefreshData(object obj)
	{
		if(GetSub ("pvp_rank_reset_time") != null && GetSub ("pvp_rank_reset_time").GetComponent<CountdownUI>() != null)
			MonoBehaviour.Destroy (GetSub ("pvp_rank_reset_time").GetComponent<CountdownUI>());
		if(Convert.ToInt32 (obj) == 1)
			DataCenter.OpenWindow ("PVP_SEARCH_OPPONENT", null);
		else
			EventCenter.WaitAction (this, "RefreshDataAgain", obj, 5.0f);
	}

	public virtual void RefreshDataAgain(object obj)
	{
		if((DataCenter.GetData ("PVP_RANK_WINDOW") as tWindow).IsOpen ())
			DataCenter.OpenWindow ("PVP_SEARCH_OPPONENT", null);
	}

	public virtual void SetVisitButtonData(GameObject obj, string playerID, string name)
	{
		NiceData visitButtonData = GameCommon.GetButtonData (obj, "visit_button");
		visitButtonData.set ("FRIEND_ID", playerID);
		visitButtonData.set ("FRIEND_NAME", name);
		visitButtonData.set ("WINDOW_NAME", "PVP_VISIT_FRIEND_BACK");
	}

    private void SetFlag(GameObject item, int index)
    {
        GameObject flag = GameCommon.FindObject(item, "flag");

        if (index < 3)
        {
            flag.SetActive(true);

            switch (index)
            {
                case 0:
                    flag.GetComponent<UISprite>().spriteName = "ui_gonghuizhan_111";
                    break;
                case 1:
                    flag.GetComponent<UISprite>().spriteName = "ui_gonghuizhan_222";
                    break;
                case 2:
                    flag.GetComponent<UISprite>().spriteName = "ui_gonghuizhan_333";
                    break;
            }
        }
        else
        {
            flag.SetActive(false);
        }
    }
}

public class PvpRankData
{
	public string mName = "";
	public int mScore = 0;
	public string mPlayerID = null;
	public int mVipLevel = 0;
}

public class PVP_Secarch_opponent : tWindow
{
    public override void Open(object param)
    {
        DataCenter.Set("PVP_CURRENT_IS_REVENGE", false);

        base.Open(param);
		if(param == null)
		{
//	        DataCenter.OpenWindow("PVP_PET_TEAM_WINDOW");
	        DataCenter.OpenWindow("PVP_PLAYER_RANK_WINDOW");
			DataCenter.OpenWindow ("INFO_GROUP_WINDOW");

	        DataCenter.SetData("PVP_PET_TEAM_WINDOW", "ADJUST_PET_BUTTON", true);
			DataCenter.SetData("PVP_PLAYER_RANK_WINDOW", "SHOW_PET_BUTTON", false);


			DataCenter.SetData("PVP_PLAYER_RANK_WINDOW", "SHOW_PET_BUTTON", true);
			DataCenter.OpenWindow("PVP_RANK_WINDOW", false);
		}

    }
}

//-------------------------------------------------------------------------
public class PVP_PetTeamWindow : tWindow
{
	protected int mPetCount = 6;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_adjust_pet_button", new DefineFactory<Button_adjust_pet_button>());
        EventCenter.Self.RegisterEvent("Button_adjust_over_button", new DefineFactory<Button_adjust_over_button>());
        EventCenter.Self.RegisterEvent("Button_pvp_pet_check_box", new DefineFactory<Button_pvp_pet_check_box>());
		EventCenter.Self.RegisterEvent("Button_pvp_auto_join_button", new DefineFactory<Button_PVPAutoJoinButton>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        ShowClickFlag(false);
		SetButtonData ();
        Refresh(param);

		DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "CLEAR_GRID", true);
    }

    public override bool Refresh(object param)
    {
        RefreshTeamContext();
        return true;
    }

   protected void ShowClickFlag(bool bVisiable)
    {
		for (int i = 0; i < mPetCount; ++i)
        {
            ShowButtonFlag(i, bVisiable);
        }
    }

	protected void SetButtonData()
	{
		NiceData data = GameCommon.GetButtonData (mGameObjUI, "pvp_auto_join_button");
		if(data != null)
			data.set ("WINDOW_NAME", mWinName);
	}

	protected void ShowButtonPetInfo(int pos, bool bVisiable)
    {
        string key = "pvp_pet_check_" + pos.ToString();
        GameObject buttonObj = GameCommon.FindObject(mGameObjUI, key);
        GameCommon.SetUIVisiable(buttonObj, "pvp_pet_check_info", bVisiable);
        GameCommon.SetUIVisiable(buttonObj, "back_ground", !bVisiable);
    }

	protected GameObject GetPVPPetPosObject(int pos)
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

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SHOW_SELECT_POS":
                ShowClickFlag((bool)objVal);
                break;
            case "TEAM_ADJUST_UNLOAD":
                if ((bool)objVal)
                    RefreshTeamContext();
                break;
            case "TEAM_ADJUST_LOAD":
                if ((bool)objVal)
                    ShowClickFlag(false);
                    RefreshTeamContext();
                break;
            case "ADJUST_PET_BUTTON":
                ShowClickFlag(false);
                SetVisible("adjust_over_button", !((bool)objVal));
				SetVisible("pvp_auto_join_button", !((bool)objVal));
                SetVisible("adjust_pet_button", (bool)objVal);
                break;
			case "SET_AUTO_JOIN":
				SetAutoJoin(objVal.ToString ());
				break;
        }
    }

	public bool SetAutoJoin(string bagWindowName)
	{
		DataCenter.SetData(bagWindowName, "SET_AUTO_JOIN", mPetCount);
		return true;
	}

	protected void RefreshTeamContext()
    {
        PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
        int nAttackScore = 0;
		for (int i = 0; i < mPetCount; i++)
        {
            GameObject obj = GetPVPPetPosObject(i);
			PetData data = GetPetData(petLogic, i);
            if (data != null)
            {
                ShowButtonPetInfo(i, true);
                InitPetData(obj, data);
				//SetPetSkill (data.mModelIndex, obj);
                SetPetSkill(data, obj);
                GameCommon.SetUIVisiable(obj, "back_ground", false);

                nAttackScore += TotalAttackScore(data);
            }
            else
            {
                ShowButtonPetInfo(i, false);
            }

            NiceData butData = GameCommon.GetButtonData(obj, "pvp_pet_check_box");
            if (butData != null)
			{
				butData.set("POS_INDEX", i);
				butData.set("WINDOW_NAME", mWinName);
			}
        }
        SetText("battle_number", nAttackScore.ToString());
    }

	public virtual void SetPetSkill(PetData petData, GameObject parentObj){}

	public virtual PetData GetPetData(PetLogicData petLogic, int pos)
	{
		return petLogic.GetPVPPet(pos);
	}

    static public void InitPetData(GameObject o, PetData data)
    {
        GameCommon.SetUISprite(o, "pet_icon_sprite", TableCommon.GetStringFromActiveCongfig(data.tid, "HEAD_ATLAS_NAME"), TableCommon.GetStringFromActiveCongfig(data.tid, "HEAD_SPRITE_NAME"));
        GameCommon.SetUIText(o, "level_label", "Lv." + data.level.ToString());
//        GameCommon.SetUIText(o, "star_level_label", data.mStarLevel.ToString());
		GameCommon.SetStarLevelLabel (o, data.starLevel, "star_level_label" );

		string strStrengThenLevelLabel = data.strengthenLevel.ToString();
		if(strStrengThenLevelLabel == "0")  GameCommon.SetUIText(o, "streng_then_level_label", "");
		else GameCommon.SetUIText(o, "streng_then_level_label", "+" + strStrengThenLevelLabel);

        int iElementIndex = TableCommon.GetNumberFromActiveCongfig(data.tid, "ELEMENT_INDEX");
        string strAtlasName = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        string strSpriteName = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_SPRITE_NAME");

        GameCommon.SetUISprite(o, "element_sprite", strAtlasName, strSpriteName);
    }

	static public int TotalAttackScore(PetData pet)
	{
        return pet == null ? 0 : GameCommon.GetFightingStrength(pet.tid, pet.level, pet.strengthenLevel);
	}
}

public class Button_level_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		string windowName = get ("WINDOW_NAME");
		DataCenter.SetData(windowName, "REFRESH", (int)SORT_TYPE.LEVEL);
//		tWindow t = DataCenter.GetData ("PVP_PET_BAG_WINDOW") as tWindow;
//		tWindow t1 = DataCenter.GetData ("PVP_ATTR_READY_PET_BAG") as tWindow ;
//
//		if(t.IsOpen ())
//			DataCenter.SetData("PVP_PET_BAG_WINDOW", "REFRESH", 2);
//		else if(t1.IsOpen ())
//			DataCenter.SetData("PVP_ATTR_READY_PET_BAG", "REFRESH", 2);
//		else 
//			DataCenter.SetData("PVP_ENDLESS_BATTLE_PET_BAG_WINDOW", "REFRESH", 2);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_property_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		string windowName = get ("WINDOW_NAME");
		DataCenter.SetData(windowName, "REFRESH", (int)SORT_TYPE.ELEMENT_INDEX);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_star_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		string windowName = get ("WINDOW_NAME");
		DataCenter.SetData(windowName, "REFRESH", (int)SORT_TYPE.STAR_LEVEL);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class PVP_PetBagWindow : tWindow
{
	protected SORT_TYPE mSortType = SORT_TYPE.STAR_LEVEL;
	public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_pvp_pet_item", new DefineFactory<Button_pvp_pet_item>());
        EventCenter.Self.RegisterEvent("Button_pvp_pet_info_single_window_close", new DefineFactory<Button_pvp_pet_info_single_window_close>());
        EventCenter.Self.RegisterEvent("Button_pvp_pet_info_single_ok_button", new DefineFactory<Button_pvp_pet_info_single_ok_button>());
		EventCenter.Self.RegisterEvent("Button_level_rank_button", new DefineFactory<Button_level_rank_button>());
		EventCenter.Self.RegisterEvent("Button_property_rank_button", new DefineFactory<Button_property_rank_button>());
		EventCenter.Self.RegisterEvent("Button_star_rank_button", new DefineFactory<Button_star_rank_button>());
    }

    public override void OnOpen()
    {
		SetWindowNameToButtonData ("give_battle_button", "level_rank_button", "property_rank_button", "star_rank_button");

        DataCenter.OpenWindow("PVP_PET_INFO_SINGLE_WINDOW", PET_INFO_WINDOW_TYPE.PET);
        DataCenter.SetData("PVP_PET_INFO_SINGLE_WINDOW", "INIT_WINDOW", true);

		DataCenter.Set ("DESCENDING_ORDER", true);
		GetSub ("order_button").GetComponentInChildren<UISprite>().spriteName = "ui_j";
		GameCommon.GetButtonData (GetSub ("order_button")).set ("WINDOW_NAME", mWinName);
		GameCommon.CloseUIPlayTween (mSortType);

		CS_RequestPetList quest = Net.StartEvent("CS_RequestPetList") as CS_RequestPetList;
		quest.mPvpPetWinName = mWinName;
        quest.DoEvent();
        quest.set("IS_PET_BAG", false);
    }

    public override bool Refresh(object param)
    {
		if(param != null)
			mSortType = (SORT_TYPE)param;
        InitPetBag();
        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
			case "ORDER_RULE":
            case "TEAM_ADJUST_UNLOAD":
            case "TEAM_ADJUST_LOAD":
				InitPetBag();
                break;
			case "SET_AUTO_JOIN":
				SetAutoJoin((int)objVal);
				break;
        }
    }

	protected void SetWindowNameToButtonData(params string[] strButtonNames)
	{
		for(int i = 0; i < strButtonNames.Length; i++)
		{
			NiceData data = GameCommon.GetButtonData(mGameObjUI, strButtonNames[i]);
			if (data != null)
				data.set("WINDOW_NAME", mWinName);
		}
	}

	protected void SetAutoJoin(int iMaxPos)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		if(petLogicData != null)
		{
			List<PetData> petList = new List<PetData>();
			if(petLogicData.mDicPetData.Count > 0)
			{
				petList = petLogicData.mDicPetData.Values.ToList();
				petList = SortAutoJoinList(petList);
			}
			
			int iCurIndex = 0;
			for(int iPos = 0; iPos < iMaxPos; iPos++)
			{
				for(int i = iCurIndex; i < petList.Count; i++)
				{
					PetData pet = petList[i];
					if(pet != null)
					{
						PetData teamPet = GetPVPUsePet(petLogicData, iPos);
						if(teamPet == null || teamPet.itemId != pet.itemId)
						{
							DataCenter.SetData ("PVP_TEAM_ADJUST", "CURRENT_PET_DATA", pet);
							Button_pvp_pet_check_box btnEvent = EventCenter.Start("Button_pvp_pet_check_box") as Button_pvp_pet_check_box;
							btnEvent.set("POS_INDEX", iPos);
							string teamWindowName = "PVP_PET_TEAM_WINDOW";
							if(this is PVP_FourVSFourPetBagWindow)
								teamWindowName = "PVP_FOUR_VS_FOUR_TEAM_WINDOW";
							btnEvent.set ("WINDOW_NAME", teamWindowName);
							btnEvent.DoEvent();
						}
						
						iCurIndex = i + 1;
						break;
					}
				}
			}
		}
	}

	protected List<PetData> SortAutoJoinList(List<PetData> list)
	{
		if(list != null && list.Count > 0)
			list = GameCommon.SortList (list, GameCommon.SortAutoJoinPetDataList);

		return list;
	}

	protected void InitPetBag()
    {
        PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;

        if (petLogic != null)
        {
            UIGridContainer mPetGrid = GameCommon.FindObject(mGameObjUI, "grid").GetComponent<UIGridContainer>();
            mPetGrid.MaxCount = 0;
            mPetGrid.MaxCount = petLogic.mDicPetData.Count;

            int index = 0;
			InitUsedPetIcon(mPetGrid, ref  index);

			InitUnUsedPetIcon(mPetGrid, index);

        }
        else DEBUG.Log("pets data is null");
    }

    protected virtual PetData GetPVPUsePet(PetLogicData petLogic, int teamPos)
    {
        return petLogic.GetPVPPet(teamPos);
    }

    protected virtual bool IsUsePVPPet(PetLogicData petLogic, PetData petData)
    {
		return petLogic.IsPVPUsePet(petData.itemId);
    }

	protected void InitUsedPetIcon(UIGridContainer mPetGrid, ref int index)
	{
		PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
		
		int nAttackScore = 0;
		
		for (int i = 0; i < 6; ++i )
		{
            PetData petData = GetPVPUsePet(petLogic, i);
			if (petData!=null)
			{
				nAttackScore += PVP_PetTeamWindow.TotalAttackScore(petData);
				
				GameObject obj = mPetGrid.controlList[index];
				obj.name = "pvp_pet_item";
				PVP_PetTeamWindow.InitPetData(obj, petData);
				
				NiceData button_data = GameCommon.GetButtonData(obj);
				button_data.set("SINGLE_PET_DATA", petData);
				
				GameCommon.ToggleTrue(obj);
				index++;
			}
		}
		
		SetText("battle_number", nAttackScore.ToString());
	}
	
	protected void InitUnUsedPetIcon(UIGridContainer mPetGrid, int index)
	{
		PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;

		if(petLogic != null)
		{
			List <PetData> petDataList = petLogic.mDicPetData.Values.ToList ();
			//according to the level ranking
			if(mSortType == SORT_TYPE.LEVEL)
				petDataList = GameCommon.SortList (petDataList, GameCommon.SortPetDataByLevel);
			//according to the star ranking
			else if(mSortType == SORT_TYPE.STAR_LEVEL)
				petDataList = GameCommon.SortList (petDataList, GameCommon.SortPetDataByStarLevel);
			//according to the element ranking
			else if(mSortType == SORT_TYPE.ELEMENT_INDEX)
				petDataList = GameCommon.SortList (petDataList, GameCommon.SortPetDataByElement);

			for(int i=0; i<petDataList.Count; i++)
			{
				PetData petData = petDataList[i];
				if (!IsUsePVPPet(petLogic, petData))
				{
					GameObject obj = mPetGrid.controlList[index];
					obj.name = "pvp_pet_item";
					PVP_PetTeamWindow.InitPetData(obj, petData);
					
					NiceData button_data = GameCommon.GetButtonData(obj);
					button_data.set("SINGLE_PET_DATA", petData);
					
					GameCommon.ToggleFalse(obj);
					index ++;
				}
			}
		}
	}
}


public class PVP_Team_Adjust : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_reback_button", new DefineFactory<Button_reback_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        DataCenter.OpenWindow("PVP_PET_TEAM_WINDOW");
        DataCenter.OpenWindow("PVP_PET_BAG_WINDOW");
		DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
    }
}


public class Button_pvp_pet_item : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow("PVP_PET_INFO_SINGLE_WINDOW", PET_INFO_WINDOW_TYPE.PET);

		object val;
		bool b = getData ("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		toggle.value = !toggle.value;

		//NiceData button_data = obj.GetComponent<UIButtonEvent>().mData;

		PetData petData = getObject("SINGLE_PET_DATA") as PetData;

		DataCenter.SetData ("PVP_TEAM_ADJUST", "CURRENT_PET_DATA", petData);
		if(petData != null)
		{
            int iItemId = petData.itemId;
			DataCenter.SetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_BY_ID", iItemId);
			DataCenter.SetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", iItemId);
		}
		else
		{
            DEBUG.LogError("there is not exist petData");
		}

		return true;
	}
}

//pvp_pet_check_box
 public class Button_pvp_pet_check_box : CEvent
{
	public override bool _DoEvent()
	{
        int pvpPos = get("POS_INDEX");
		string teamWindowName = get ("WINDOW_NAME");
		string bagWindowName = "PVP_PET_BAG_WINDOW";

        int team = PetLogicData.mPVPTeam;
		if (teamWindowName == "PVP_PET_TEAM_WINDOW" && pvpPos >= 3)
            team = PetLogicData.mBackTeam;
		else if(teamWindowName == "PVP_FOUR_VS_FOUR_TEAM_WINDOW")
		{
			team = PetLogicData.mFourVsFourTeam;
			bagWindowName = "PVP_FOUR_VS_FOUR_PET_BAG_WINDOW";
		}

		object o;
		DataCenter.GetData ("PVP_TEAM_ADJUST").getData ("CURRENT_PET_DATA", out o);
		PetData petData = o as PetData;

		CS_RequestChangePetUsePos quest = Net.StartEvent("CS_RequestChangePetUsePos") as CS_RequestChangePetUsePos;
		quest.set ("WINDOW", "TEAM_ADJUST_LOAD");
		quest.set ("TEAM_WINDOW_NAME", teamWindowName);
		quest.set ("BAG_WINDOW_NAME", bagWindowName);
        quest.set("PET_ID", petData.itemId);
        quest.set("TEAM_ID", team);
        quest.set("EQUIP", true);
        quest.set("USE_POS", pvpPos%3 + 1);

        quest.DoEvent();

		//DataCenter.SetData ("PVP_TEAM_ADJUST", "SET_WHICH", i);

		return true;
	}
}

public class Button_PVPAutoJoinButton : CEvent
{
	public override bool _DoEvent ()
	{
		string windowName = get ("WINDOW_NAME");
		if(windowName == "PVP_FOUR_VS_FOUR_TEAM_WINDOW")
			DataCenter.SetData(windowName, "SET_AUTO_JOIN", "PVP_FOUR_VS_FOUR_PET_BAG_WINDOW");
		else if(windowName == "PVP_PET_TEAM_WINDOW")
			DataCenter.SetData(windowName, "SET_AUTO_JOIN", "PVP_PET_BAG_WINDOW");

		return true;
	}
}

public class Button_pvp_pet_info_single_window_close : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PVP_PET_INFO_SINGLE_WINDOW");
		DataCenter.CloseWindow("COMMON_TIP_WINDOW");
		return true;
	}
}

public class Button_pvp_pet_info_single_ok_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PVP_PET_INFO_SINGLE_WINDOW");

		object obj;
		DataCenter.GetData ("PVP_TEAM_ADJUST").getData ("CURRENT_PET_DATA", out obj);
		PetData petData = obj as PetData;

		PVP_AttributePetTeamWindow attrPetWindow = DataCenter.GetData("PVP_ATTR_READY_PET_TEAM_" + PVP_AttributeEnterWindow.mCurrentAttributeIndex.ToString ()) as PVP_AttributePetTeamWindow;
        if (attrPetWindow != null && attrPetWindow.IsOpen())
        {
			if(TableCommon.GetNumberFromActiveCongfig (petData.tid, "ELEMENT_INDEX") == PVP_AttributeEnterWindow.mCurrentAttributeIndex)
            	attrPetWindow.OnSelectPet(petData);
			else
			{
				string strAttribute = DataCenter.mPvpAttributeConfig.GetRecord (PVP_AttributeEnterWindow.mCurrentAttributeIndex)["ATTRIBUTE_NAME"];
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_PVP_GET_SAME_ATTRIBUTE_PET, strAttribute.Substring (0,1));
			}
            return true;
        }

        PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
		string teamWindowName = "PVP_PET_TEAM_WINDOW";
		string bagWindowName = "PVP_PET_BAG_WINDOW";
		int pvpTeam = 0;

		PVP_FourVSFourTeamWindow fourVsFourTeamWindow = DataCenter.GetData ("PVP_FOUR_VS_FOUR_TEAM_WINDOW") as PVP_FourVSFourTeamWindow;
		PVP_PetTeamWindow PvpPetTeamWindow = DataCenter.GetData ("PVP_PET_TEAM_WINDOW") as PVP_PetTeamWindow;
		if(fourVsFourTeamWindow != null && fourVsFourTeamWindow.IsOpen ())
		{
			teamWindowName = "PVP_FOUR_VS_FOUR_TEAM_WINDOW";
			bagWindowName = "PVP_FOUR_VS_FOUR_PET_BAG_WINDOW";
			if(petLogic.IsFourVsFourPVPUsePet (petData.itemId))
				pvpTeam = PetLogicData.mFourVsFourTeam;
		}
		else if(PvpPetTeamWindow != null && PvpPetTeamWindow.IsOpen ())
			pvpTeam = petLogic.GetInPVPTeam(petData.itemId);

        if (pvpTeam > 0)
        {
            CS_RequestChangePetUsePos quest = Net.StartEvent("CS_RequestChangePetUsePos") as CS_RequestChangePetUsePos;
            quest.set("WINDOW", "TEAM_ADJUST_UNLOAD");
			quest.set ("TEAM_WINDOW_NAME", teamWindowName);
			quest.set ("BAG_WINDOW_NAME", bagWindowName);
            quest.set("PET_ID", petData.itemId);
            quest.set("TEAM_ID", pvpTeam);
            quest.set("EQUIP", false);
            quest.set("USE_POS", petLogic.GetPosInTeam(pvpTeam, petData.itemId));
            quest.DoEvent();
        }
        else
			DataCenter.SetData(teamWindowName, "SHOW_SELECT_POS", true);

		return true;
	}
}

public class Button_give_battle_button : CEvent
{
    public override bool _DoEvent()
    {
		PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData pet = null;

		string PvpName = get ("WINDOW_NAME");
		 if (PvpName == "PVP_ATTR_READY_PET_BAG")
        {
            tEvent evt = StartEvent("Button_start_attribute_battle_button");
            evt.DoEvent();
            return true;
        }
		else if (PvpName == "PVP_ENDLESS_BATTLE_PET_BAG_WINDOW")
		{

		}
		else if (PvpName == "PVP_FOUR_VS_FOUR_PET_BAG_WINDOW")
		{
			for(int i = 0; i < 3; i++)
			{
                pet = petLogic.GetPetDataByTeamPos(i + 1);//petLogic.GetFourVsFourPetDataByPos (i + 1);

				if(pet != null)
					break;
			}

			if(pet != null)
			{
				int rank = DataCenter.GetData (PvpName).get ("CURRENT_OPPONENT_RANK");
				int DBID = DataCenter.GetData (PvpName).get ("CURRENT_OPPONENT_DBID");
				tEvent evt = Net.StartEvent("CS_Request4v4Competition");
				evt.set ("4v4MASTER", DBID);
				evt.set ("4v4ARENA", rank);
				evt.DoEvent ();
			}
			else
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_NEED_ADJUST_PETS);
		}
		else if(PvpName == "PVP_PET_BAG_WINDOW")
		{
			for (int i = 0; i < 6; ++i)
			{
			    pet = petLogic.GetPVPPet(i);            
			    if (pet != null)
			        break;
			}
	        if (pet != null)
	        {
	            tEvent request = Net.StartEvent("CS_RequestPVP6");
				bool b = DataCenter.Get("PVP_CURRENT_IS_REVENGE");
	            if (b)
				{
					UInt64 id = DataCenter.GetData("PVP_REVENGE_WINDOW", "ATTACKER_DBID");
					request.set("ATTACKER_DBID", id);
				}
	            request.DoEvent();
	        }
	        else
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_NEED_ADJUST_PETS);
		}

        return true;
    }
}

//-----------------------------------------------------------------------------------
public class BackGroupPvpWindow : tWindow
{
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "BACK_WINDOW")
			GameCommon.GetButtonData (GetSub ("reback_button")).set ("BACK_WINDOW", "BACK_ROLE_SELECT_SCENE");
	}
}

public class Button_reback_button : CEvent
{
    public override bool _DoEvent()
    {
		object obj = getObject ("BACK_WINDOW");
		if(obj != null && obj.ToString () == "BACK_ROLE_SELECT_SCENE")
       	 	MainProcess.LoadRoleSelScene();
		else
		{
			string str = "";
			if(GameCommon.bIsWindowOpen ("PVP_PLAYER_FOUR_VS_FOUR_RANK_WINDOW") || GameCommon.bIsWindowOpen("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW"))
				str = "FOUR_VS_FOUR";
			GlobalModule.ClearAllWindow ();
			DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
			DataCenter.OpenWindow ("PVP_SELECT_ENTER", str);
		}
        return true;
    }
}

public class Button_adjust_pet_button : CEvent
{
	public override bool _DoEvent()
	{
        BaseUI.OpenWindow("PVP_TEAM_ADJUST", null, true);
		DataCenter.SetData("PVP_PET_TEAM_WINDOW", "ADJUST_PET_BUTTON", false);

        bool b = DataCenter.Get("PVP_CURRENT_IS_REVENGE");
        if (b)
            DataCenter.CloseWindow("PVP_REVENGE_WINDOW");

		return true;
	}
}

public class Button_adjust_over_button : CEvent
{
	public override bool _DoEvent()
	{
		//DataCenter.CloseWindow ("PVP_TEAM_ADJUST");
		//DataCenter.OpenWindow ("PVP_SEARCH_OPPONENT");

        //tEvent evt = Net.StartEvent("CS_RequestPVP6Score");
        //evt.DoEvent();

        //BaseUI.OpenWindow("PVP_SEARCH_OPPONENT", null, true);

        DataCenter.CloseWindow("PVP_PET_BAG_WINDOW");

        bool b = DataCenter.Get("PVP_CURRENT_IS_REVENGE");
        if (b)
            DataCenter.OpenWindow("PVP_REVENGE_WINDOW");
        else
            DataCenter.OpenWindow("PVP_PLAYER_RANK_WINDOW");

        DataCenter.SetData("PVP_PET_TEAM_WINDOW", "ADJUST_PET_BUTTON", true);
	
		return true;
	}
}
//adjust_over_button
//-------------------------------------------------------------------------

class PVP_VictoryPredictWindow : tWindow
{
    public int[] mPets = new int[6];
    public int[] yPets = new int[6];

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("TM_WaitShowVictoryWindow", new DefineFactory<TM_WaitShowVictoryWindow>());

    }

    public override void Open(object param)
    {
        base.Open(param);
        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        tEvent respEvt = param as tEvent;
        if (respEvt == null)
        {
            EventCenter.Log(LOG_LEVEL.ERROR, "No set response result use pet event when open PVP_VictoryPredictWindow");
            return false;
        }

		int imRoleIconIndex = RoleLogicData.Self.iconIndex;
		int iyRoleIconIndex = respEvt.get ("ROLE_ICON_INDEX");
		int iyRoleModelIndex = respEvt.get ("TARGET_MODEL");
		if(imRoleIconIndex < 1000) imRoleIconIndex = 1000;
		if(iyRoleIconIndex < 1000) iyRoleIconIndex = 1000;
		string strmSpriteName = TableCommon.GetStringFromVipList (imRoleIconIndex, "VIPICON");
		string strySpriteName = TableCommon.GetStringFromVipList (iyRoleIconIndex, "VIPICON");
		if(imRoleIconIndex == 1000) ChangeRoleIconAccordingModelIndex(ref strmSpriteName, RoleLogicData.Self.character.tid);
		if(iyRoleIconIndex  == 1000) ChangeRoleIconAccordingModelIndex(ref strySpriteName, iyRoleModelIndex);
				
		SetUISprite ("my_photo", TableCommon.GetStringFromVipList (imRoleIconIndex, "VIPALATS"), strmSpriteName);
		SetUISprite ("you_photo", TableCommon.GetStringFromVipList (iyRoleIconIndex, "VIPALATS"), strySpriteName);

        // refresh show data info
        RefreshName(respEvt);
        NiceTable usePetData = respEvt.getObject("USE_PET") as NiceTable;
        RefreshTeam(usePetData, respEvt["IS_ATTRIBUTE_WAR"]);

        return true;
    }

	private void ChangeRoleIconAccordingModelIndex(ref string spriteName, int modelIndex)
	{
		string [] names = spriteName.Split ('\\');
		if(modelIndex < 191004)
			spriteName = names[0];
		else 
			spriteName = names[1];
	}

    private void RefreshName(tEvent respEvt)
    {      
		RoleLogicData roleLogicData = RoleLogicData.Self; // RoleLogicData.Self;
		RoleData roleData = RoleLogicData.GetMainRole();
        string myName = roleLogicData.name;
        int myRoleID = roleData.tid;
        string myRoleName = TableCommon.GetStringFromActiveCongfig(myRoleID, "NAME");
        int myRoleLevel = roleData.level;
        string youName = respEvt.get("TARGET_NAME");
        int youRoleID = respEvt.get("TARGET_MODEL");
        string youRoleName = TableCommon.GetStringFromActiveCongfig(youRoleID, "NAME");
        int youRoleLevel = respEvt.get("TARGET_LEVEL");
        GameObject myInfoObj = GameCommon.FindObject(mGameObjUI, "role_bg_top");
        GameObject youInfoObj = GameCommon.FindObject(mGameObjUI, "role_bg_bottom");
        GameCommon.SetUIText(myInfoObj, "momo_name_lable", myName);
        GameCommon.SetUIText(myInfoObj, "role_name_lable", myRoleName + " Lv" + myRoleLevel);
        GameCommon.SetUIText(youInfoObj, "momo_name_lable", youName);
        GameCommon.SetUIText(youInfoObj, "role_name_lable", youRoleName + " Lv" + youRoleLevel);
    }
    
    private void RefreshTeam(NiceTable petTable, bool bAttribute)
    {
        PetInfo[] myPets = GetMyTeamInfo(bAttribute);
        PetInfo[] youPets = GetYouTeamInfo(petTable);

        for (int i = 0; i < 6; ++i)
        {
            RefreshGroupContext(i, myPets[i], youPets[i]);
            //PrintInfo(i, myPets[i]);
            //PrintInfo(i, youPets[i]);
        }

        for (int i = 0; i < 6; ++i)
        {
            mPets[i] = myPets[i] == null ? 0 : myPets[i].mModelIndex;
            yPets[i] = youPets[i] == null ? 0 : youPets[i].mModelIndex;
        }
    }

    //private void PrintInfo(int index, PetInfo info)
    //{
    //    string str = " ##### " + index + " : ";
    //    if (info == null)
    //    {
    //        str += "NULL";
    //    }
    //    else 
    //    {
    //        str += "ID = " + info.mPetID + ", ELEMENT = " + info.mElement;
    //    }
    //    MonoBehaviour.print(str);
    //}

    private void RefreshGroupContext(int index, PetInfo myPet, PetInfo youPet)
    {
        GameObject groupObj = GameCommon.FindObject(mGameObjUI, "predict_compare(Clone)_" + index.ToString());
        GameObject myObj = GameCommon.FindObject(groupObj, "my_srite");
        GameObject youObj = GameCommon.FindObject(groupObj, "you_srite");
        GameObject cmpObj = GameCommon.FindObject(groupObj, "pk_sprite");
        RefreshPetContext(myObj, myPet);
        RefreshPetContext(youObj, youPet);
        RefreshCompareContext(cmpObj, myPet, youPet);
    }

    private void RefreshPetContext(GameObject uiObj, PetInfo petInfo)
    {
        UISprite sprite = uiObj.GetComponent<UISprite>();
        GameObject levelBgObj = GameCommon.FindObject(uiObj, "level_bg");
        GameObject levelLabelObj = GameCommon.FindObject(uiObj, "level_label");
        GameObject starLevelLabelObj = GameCommon.FindObject(uiObj, "star_level_label");
        GameObject strengthenLevelLabelObj = GameCommon.FindObject(uiObj, "streng_then_level_label");
        if (petInfo == null || petInfo.mModelIndex == 0)
        {
            sprite.atlas = null;
            levelBgObj.SetActive(false);
            starLevelLabelObj.SetActive(false);
            strengthenLevelLabelObj.SetActive(false);
        }
        else
        {
            string atlasName = TableCommon.GetStringFromActiveCongfig(petInfo.mModelIndex, "HEAD_ATLAS_NAME");
            string spriteName = TableCommon.GetStringFromActiveCongfig(petInfo.mModelIndex, "HEAD_SPRITE_NAME");
            UIAtlas atlas = GameCommon.LoadUIAtlas(atlasName);
            sprite.atlas = atlas;
            sprite.spriteName = spriteName;
            levelBgObj.SetActive(true);
            starLevelLabelObj.SetActive(true);
            strengthenLevelLabelObj.SetActive(true);
            levelLabelObj.GetComponent<UILabel>().text ="Lv." + petInfo.mLevel.ToString();
//            starLevelLabelObj.GetComponent<UILabel>().text = petInfo.mStarLevel.ToString();
			GameCommon.SetStarLevelLabel (uiObj, petInfo.mStarLevel );

			string strStrengthenLevel = petInfo.mStrengthenLevel.ToString();
			if(strStrengthenLevel == "0") strengthenLevelLabelObj.GetComponent<UILabel>().text = "";
			else strengthenLevelLabelObj.GetComponent<UILabel>().text = "+" + strStrengthenLevel;
        }
    }

    private void RefreshCompareContext(GameObject uiObj, PetInfo myPet, PetInfo youPet)
    {
        ElementCmpResult cmpResult = ElementCmpResult.UNCMP;
        if (myPet != null && myPet.mModelIndex > 0 && youPet != null && youPet.mModelIndex > 0)
        {
            cmpResult = CompareElement(myPet.mElement, youPet.mElement);
        }
        UIAtlas atlas = GameCommon.LoadUIAtlas("PVPUIAtlas");
        UISprite sprite = uiObj.GetComponent<UISprite>();
        switch (cmpResult)
        {
            case ElementCmpResult.UNCMP:
                sprite.atlas = null;
                break;
            case ElementCmpResult.EQUAL:
                sprite.atlas = atlas;
                sprite.spriteName = "ui_dengyu";
                break;
            case ElementCmpResult.WIN:
                sprite.atlas = atlas;
                sprite.spriteName = "ui_dayu";
                break;
            case ElementCmpResult.LOSE:
                sprite.atlas = atlas;
                sprite.spriteName = "ui_xiaoyu";
                break;
        }
    }

    static public PetInfo[] GetMyTeamInfo(bool bAttribute)
    {
        PetInfo[] petInfos = new PetInfo[6];
        PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        //foreach (KeyValuePair<int, PetData> iter in petLogicData.mPetList)
		for (int i=0; i<6; ++i)
        {
			PetData pet;
            if (bAttribute)
                pet = petLogicData.GetAttributePVPPet(i);
            else
                pet = petLogicData.GetPVPPet(i); // iter.Value;
			//int pos = petLogicData.GetPosInTeam(PetLogicData.mPVPTeam, pet.mDBID);
            //if (pos >= 16 && pos<22)
			if ( pet!=null )
            {
                PetInfo petInfo = new PetInfo();
                petInfo.mModelIndex = pet.tid;
                petInfo.mLevel = pet.level;
                petInfo.mStrengthenLevel = pet.strengthenLevel;
				petInfo.mStarLevel=pet.starLevel;
                petInfo.mElement = TableCommon.GetNumberFromActiveCongfig(pet.tid, "ELEMENT_INDEX");
                petInfos[i] = petInfo;
            }
        }
        return petInfos;

        //PetData[] teamPosData = petLogicData.GetPetDatasByTeam(petLogicData.mTeam);
        //PetData[] backTeamData = petLogicData.GetPetDatasByTeam(petLogicData.mBackTeam);
       
        //petInfos[0] = new PetInfo(teamPosData[0]);
        //petInfos[1] = new PetInfo(teamPosData[1]);
        //petInfos[2] = new PetInfo(teamPosData[2]);
        //petInfos[3] = new PetInfo(backTeamData[0]);
        //petInfos[4] = new PetInfo(backTeamData[1]);
        //petInfos[5] = new PetInfo(backTeamData[2]);
        //return petInfos;
    }

    static public PetInfo[] GetYouTeamInfo(NiceTable petTable)
    {
        PetInfo[] petInfos = new PetInfo[6];
        if (petTable == null)
        {
            return petInfos;
        }
        foreach (KeyValuePair<int, DataRecord> pair in petTable.GetAllRecord())
        {
            DataRecord record = pair.Value;
            //int pos = record.getData("COMBAT_SIGN");
            //if (pos >= 16 && pos<22)
            {
                PetInfo petInfo = new PetInfo();
                petInfo.mModelIndex = record.getData("PET_ID");
                petInfo.mLevel = record.getData("PET_LEVEL");
                petInfo.mStrengthenLevel = record.getData("PET_ENHANCE");
//				petInfo.mStarLevel=record.getData ("STAR_LEVEL");
				petInfo.mStarLevel=TableCommon.GetNumberFromActiveCongfig(petInfo.mModelIndex, "STAR_LEVEL");
                petInfo.mElement = TableCommon.GetNumberFromActiveCongfig(petInfo.mModelIndex, "ELEMENT_INDEX");
                int teamPos = (int)record.get(0);
                if (teamPos>5)
                    DEBUG.LogError("PET team pos is error> "+teamPos);
                else
                    petInfos[teamPos] = petInfo;
            }
        }
        return petInfos;
    }

    // 0 火 1 水 2 木 3 阳 4 阴
    private ElementCmpResult CompareElement(int myElement, int youElement)
    {
        switch (myElement)
        {
            case 0:
                if (youElement == 2)
                    return ElementCmpResult.WIN;
                else if (youElement == 1)
                    return ElementCmpResult.LOSE;
                break;
            case 1:
                if (youElement == 0)
                    return ElementCmpResult.WIN;
                else if (youElement == 2)
                    return ElementCmpResult.LOSE;
                break;
            case 2:
                if (youElement == 1)
                    return ElementCmpResult.WIN;
                else if (youElement == 0)
                    return ElementCmpResult.LOSE;
                break;
            case 3:
                if (youElement == 4)
                    return ElementCmpResult.EQUAL;
                break;
            case 4:
                if (youElement == 3)
                    return ElementCmpResult.EQUAL;
                break;
        }
        return ElementCmpResult.EQUAL;
    }

    private enum ElementCmpResult
    {
        EQUAL = 0,
        WIN = 1,
        LOSE = 2,
        UNCMP = 3
    }

    public class PetInfo
    {
        public int mModelIndex = 0;
        public int mLevel = 0;
        public int mStarLevel = 0;
        public int mStrengthenLevel = 0;
        public int mElement = 0;

        public PetInfo()
        { }

        public PetInfo(PetData data)
        {
            if (data != null)
            {
                mModelIndex = data.tid;
                mLevel = data.level;
                mStarLevel = data.starLevel;
                mStrengthenLevel = data.strengthenLevel;
                mElement = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "ELEMENT_INDEX");
            }
        }
    }
}

public class TM_WaitShowVictoryWindow : CEvent
{
    public override bool _DoEvent()
    {
        tEvent respEvt = getObject("SEARCH_RESULT") as tEvent;
        DataCenter.CloseWindow("PVP_SEARCH_OPPONENT");
		DataCenter.CloseWindow ("INFO_GROUP_WINDOW");
        DataCenter.OpenWindow("PVP_VICTORY_PREDICT_WINDOW", respEvt);
		DataCenter.OpenWindow("PVP_BATTLE_LOADING_WINDOW", this);
//        WaitTime(3);
		return true;
    }

    public override void _OnOverTime()
    {
        tEvent respEvt = getObject("SEARCH_RESULT") as tEvent;
        if (respEvt == null)
        {
            EventCenter.Log(LOG_LEVEL.ERROR, "Set open PVP_Secarch_opponent window param error");
            return ;
        }

        if (DataCenter.mPVPBattleList.Length <= 0)
        {
            EventCenter.Log(LOG_LEVEL.ERROR, "Not config PVP6 stage at StageConfig table");
            return ;
        }

        int randIndex = UnityEngine.Random.Range(0, DataCenter.mPVPBattleList.Length);
        int pvpBattleIndex = DataCenter.mPVPBattleList[randIndex];

        MainProcess.ClearBattle();
        DataCenter.Set("CURRENT_STAGE", pvpBattleIndex);
        MainProcess.LoadBattleScene();

        PVP6Battle battle = MainProcess.mStage as PVP6Battle;
        battle.mbIsAttributeWar = respEvt["IS_ATTRIBUTE_WAR"];
        battle.mbIsRevenge = respEvt["IS_REVENGE"];
        battle.mOpponentDBID = respEvt.get("TARGET_DBID");
        battle.mOpponentName = respEvt.get("TARGET_NAME");
        NiceTable usePetData = respEvt.getObject("USE_PET") as NiceTable;
        battle.InitOpponent(usePetData);

        return ;
    }
}
//-------------------------------------------------------------------------

public class PVP_FinishWindow : tWindow
{
    public int mTotalScore = 0;

	public override void Init ()
	{
		EventCenter.Register ("Button_pvp_result_close_button", new DefineFactory<Button_pvp_result_close_button>());
	}

    public override void Open(object param)
    {
        base.Open(param);

        tEvent respEvt = param as tEvent;
        if (respEvt != null)
        {
//            respEvt.Dump();
            bool bWin = (bool)respEvt["RESULT"];

            SetVisible("win", bWin);
            SetVisible("fail", !bWin);

			ResetShowModel(bWin);

            mTotalScore = respEvt["TOTAL_SCORE"];

            int currentScore = respEvt["CURRENT_SCORE"];
            int ranking = respEvt["RANKING"];

            SetText("old_score_number", currentScore.ToString());
            SetText("new_score_number", currentScore.ToString());
            SetText("rank_number", ranking.ToString());
			SetText ("vip_level", RoleLogicData.Self.vipLevel.ToString ());
			SetText ("my_name", RoleLogicData.Self.name);

			GameCommon.SetPalyerIcon (GetComponent<UISprite>("my_photo"), RoleLogicData.Self.iconIndex);

            ChangeNumber(mGameObjUI, "new_score_number", 0.1f, currentScore, mTotalScore);
        }
        else
            mTotalScore = 0;
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

class Button_pvp_result_close_button : CEvent
{
	public override bool _DoEvent()
	{
		tEvent evt = EventCenter.Start("TM_WaitShowFinishResult");
		evt.Finish ();
		evt.DoOverTime ();
		return true;
	}
}

//-------------------------------------------------------------------------

public class PVP_BuyTicketWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_cancle_button", new DefineFactory<Button_cancle_button>());
        EventCenter.Self.RegisterEvent("Button_buy_button", new DefineFactory<Button_buy_button1>());

        Net.gNetEventCenter.RegisterEvent("CS_RequestBuyPVPTicket", new DefineFactory<CS_RequestBuyPVPTicket>());
    }

    public override void Open(object param)
    {
        base.Open(param);

    }
}

public class CS_RequestBuyPVPTicket : tNetEvent
{
    public tWindow mSecarchWindow;
    public override void _OnResp(tEvent respEvent)
    {
        int buyCount = respEvent["TICKET_COUNT"];

		tWindow searchWin = DataCenter.GetData("PVP_PLAYER_RANK_WINDOW") as tWindow;

        if (searchWin != null)
		{
            searchWin.SetText("pvp_ticket_number", buyCount.ToString() + "/10");
			GameCommon.RoleChangeDiamond (-10);
		}
    }
}

public class Button_buy_button1 : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("PVP_BUY_TICKET");

        CS_RequestBuyPVPTicket requestEvt = Net.StartEvent("CS_RequestBuyPVPTicket") as CS_RequestBuyPVPTicket;

        requestEvt.DoEvent();

        return true;
    }
}

public class Button_cancle_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("PVP_BUY_TICKET");
        return true;
    }
}

//-------------------------------------------------------------------------

public class PVPVisitFriendBack : tWindow
{
	public override void Open (object param)
	{
		DataCenter.OpenWindow ("PVP_PLAYER_RANK_WINDOW");
		DataCenter.OpenWindow ("PVP_RANK_WINDOW", param);
		DataCenter.SetData ("PVP_SEARCH_OPPONENT", "OPEN", -1);
	}

	public override void Close ()
	{
		DataCenter.CloseWindow ("PVP_PLAYER_RANK_WINDOW");
		DataCenter.CloseWindow ("PVP_RANK_WINDOW");
		DataCenter.CloseWindow ("PVP_SEARCH_OPPONENT");
		DataCenter.CloseWindow ("PVP_TEAM_ADJUST");
	}
}


//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class GC_RefreshPVPRecord : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("PVP_RECORD_SHOW_WINDOW");
        DataCenter.SetData("PVP_RECORD_SHOW_WINDOW", "RECORD_COUNT", (int)get("RECORD_COUNT"));        

        // Show record
        tWindow pvpWin = DataCenter.GetData("PVP_RECORD_WINDOW") as tWindow;
        if (pvpWin != null && pvpWin.IsOpen())
            DataCenter.OpenWindow("PVP_RECORD_WINDOW");

        return true;
    }
        
}

//-------------------------------------------------------------------------

public class PVP_RecordShowWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_pvp_record", new DefineFactory<Button_pvp_record>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "RECORD_COUNT")
        {
            if (IsOpen())
                SetVisible("record_count_lable", (int)objVal>0);

            SetText("record_count_lable", objVal.ToString());
        }
        base.onChange(keyIndex, objVal);
    }

    public override void OnOpen()
    {
        int count = get("RECORD_COUNT");
        SetVisible("record_count_lable", count > 0);
        SetText("record_count_lable", count.ToString());
    }
}
//-------------------------------------------------------------------------

public class Button_pvp_record : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("PVP_RECORD_WINDOW");

        return true;
    }
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class CS_RequestPVPRecord : BaseNetEvent
{
    public override bool _DoEvent()
    {


        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        // Show record
        DataCenter.LogicActive("PVP_RECORD_WINDOW", "Refresh", respEvent);
    }
}
//-------------------------------------------------------------------------
// PVP Record window
public class PVP_RecordWindow : tWindow
{
    public int mCurrentPage = 0;
    public NiceTable mPVPRecordTable;

    public override void Init()
    {
        Net.Register("CS_RequestPVPRecord", new DefineFactory<CS_RequestPVPRecord>());

        EventCenter.Self.RegisterEvent("Button_pvp_record_button", new DefineFactory<Button_pvp_record_button>());
        EventCenter.Self.RegisterEvent("Button_pvp_record_forward_button", new DefineFactory<Button_pvp_record_forward_button>());
        EventCenter.Self.RegisterEvent("Button_pvp_record_back_button", new DefineFactory<Button_pvp_record_back_button>());
    }

    public override void OnOpen()
    {
        mPVPRecordTable = null;
        mCurrentPage = 0;
        SetVisible("pvp_record_back_button", false);
        SetVisible("pvp_record_forward_button", false);

        tEvent evt = Net.StartEvent("CS_RequestPVPRecord");
        evt.DoEvent();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "PAGE")
        {
            bool bForward = (bool)objVal;
            if (mPVPRecordTable!=null)
            {
                int count = mPVPRecordTable.GetRecordCount();
                if (bForward)
                {
                    if (count > (mCurrentPage+1)*3)
                    {
                        ++mCurrentPage;
                        ShowRecord();
                        SetVisible("pvp_record_back_button", true);
                        SetVisible("pvp_record_forward_button", count > (mCurrentPage + 1) * 3);
                    }
                }                
                else if (mCurrentPage>0)
                {
                     --mCurrentPage;
                     ShowRecord();
                    if (mCurrentPage<=0)
                        SetVisible("pvp_record_back_button", false);

                    SetVisible("pvp_record_forward_button", true);
                }                                                    
            }
        }
        base.onChange(keyIndex, objVal);
    }

    public override bool Refresh(object param) 
    { 
        tEvent evt = param as tEvent;
        if (evt==null)
            return false;

        mPVPRecordTable = evt.getTable("PVP_RECORD");

        int count = 0;

        if (mPVPRecordTable != null)
        {
            foreach (KeyValuePair<int, DataRecord> vRe in mPVPRecordTable.GetAllRecord())
            {                
                if (!vRe.Value["REVENGED"])
                    ++count;
            }
        }
        DataCenter.SetData("PVP_RECORD_SHOW_WINDOW", "RECORD_COUNT", count);        

        ShowRecord();
        if (mPVPRecordTable.GetRecordCount() > 3)
        {
            SetVisible("pvp_record_forward_button", true);
        }
        return false; 
    }

    void InitRecordItem(GameObject item)
    {
        GameCommon.SetUIText(item, "player_name", "");
        GameCommon.SetUIText(item, "battle_result", "");
        item.SetActive(false);
    }

    void ShowRecord()
    {
        NiceTable pvpRecordTable = mPVPRecordTable;
        if (pvpRecordTable != null)
        {
            int i = 0;
			string subKey = "player_info(Clone)_";

            int begin = mCurrentPage * 3;
            int current = -1;
            foreach (KeyValuePair<int, DataRecord> vRe in pvpRecordTable.GetAllRecord())
            {
                ++current;
                if (current<begin)
                    continue;

                if (i>=3)
                    break;

                GameObject item = GetSub(subKey + i.ToString());
                if (item!=null)
                {
                    DataRecord re = vRe.Value;
                    if (re!=null)
                    {
						item.SetActive(true);

                        GameCommon.SetUIText(item, "player_name", re["ATTACKER_NAME"]);
                        int model = re["ATTACKER_MODEL"];
                        bool bWin = re["RESULT"];
                        //GameCommon.SetUIText(item, "battle_result", bWin ? "胜":"负");
                        GameCommon.SetUIVisiable(item, "battle_result_win", bWin);
                        GameCommon.SetUIVisiable(item, "battle_result_lose", !bWin);
						                        
                        NiceData butData = GameCommon.GetButtonData(item, "pvp_record_button");
                        if (butData!=null)
                        {       
                            bool bRevenged = re["REVENGED"];
                            butData.set("REVENGED", bRevenged);
                            butData.set("ATTACKER_DBID", (UInt64)re["ATTACKER_DBID"]);   
                            GameCommon.SetUIText(item, "pvp_record_button", bRevenged ? "已反击":"查看");
                        }
                    }
                    else
                    {
                        InitRecordItem(item);
                    }
                }
                else
                    DEBUG.LogError("Window item no exist >"+subKey+i.ToString());

                ++i;
            }
			for (int n=i; n<3; ++n)
			{
				GameObject item = GetSub(subKey + n.ToString());
				if (item!=null)
				{
                    InitRecordItem(item);
				}
			}            
        }
    }
}

//-------------------------------------------------------------------------
class Button_pvp_record_button : CEvent
{
    public override bool _DoEvent()
    {
        getData().dump();

        if (!get("REVENGED"))
        {
            GlobalModule.ClearAllWindow();
            DataCenter.SetData("PVP_REVENGE_WINDOW", "ATTACKER_DBID", (UInt64)get("ATTACKER_DBID"));
            DataCenter.OpenWindow("PVP_REVENGE_WINDOW");
        }

        return true;
    }
}
// >
class Button_pvp_record_forward_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PVP_RECORD_WINDOW", "PAGE", true);
        return true;
    }
}
// <
class Button_pvp_record_back_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PVP_RECORD_WINDOW", "PAGE", false);
        return true;
    }
}
//-------------------------------------------------------------------------
//-------------------------------------------------------------------------

public class PVP_RevengeWindow : tWindow
{
    public override void Open(object param)
    {
        DataCenter.Set("PVP_CURRENT_IS_REVENGE", true);

        base.Open(param);
        //if (param == null)
        {
            DataCenter.OpenWindow("PVP_PET_TEAM_WINDOW");
            DataCenter.OpenWindow("PVP_ATTACKER_INFO_WINDOW", (UInt64)get("ATTACKER_DBID"));
            DataCenter.OpenWindow("INFO_GROUP_WINDOW");

            DataCenter.SetData("PVP_PET_TEAM_WINDOW", "ADJUST_PET_BUTTON", true);
            DataCenter.SetData("PVP_PLAYER_RANK_WINDOW", "SHOW_PET_BUTTON", false);
        }

    }
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------

public class PVP_AttackerWindow : tWindow
{
    public override void Init()
    {
        Net.Register("CS_AttackerPVPPetData", new DefineFactory<CS_AttackerPVPPetData>());

        //EventCenter.Self.RegisterEvent("Button_start_revenge_button", new DefineFactory<Button_start_revenge_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        tEvent evt = Net.StartEvent("CS_AttackerPVPPetData");
        if (evt != null)
        {
            evt.set("ATTACKER_DBID", (UInt64)param);
            evt.DoEvent();
        }
        else
            DEBUG.LogError("No register [CS_AttackerPVPPetData]");
    }

    public override bool Refresh(object param)
    {
        tEvent resp = param as tEvent;

        if (resp==null)
            return false;

        resp.Dump();

        SetText("rical_name", resp["TARGET_NAME"]);
        SetText("vip_level", resp["VIP_LEVEL"]);
        SetText("battle_number", resp["PVP_TICKET"]);

		int iyRoleIconIndex =resp["VIP_LEVEL"]+1000;
		int iyRoleModelIndex = resp["TARGET_MODEL"];
		if(iyRoleIconIndex < 1000) iyRoleIconIndex = 1000;
		string ySpriteName = TableCommon.GetStringFromVipList (iyRoleIconIndex,"VIPICON");
		if(iyRoleIconIndex == 1000)
		{
			ChangeRoleIconAccordingModelIndex (ref ySpriteName,iyRoleModelIndex );
		}
		SetUISprite ("other_photo",TableCommon.GetStringFromVipList (iyRoleIconIndex,"VIPALATS"),ySpriteName  );


        if ((int)resp["RESULT"] != (int)eRequestPVPResult.PVP_Succeed)
        {
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_OPPONENT_NOE_EQUIP_PETS);
            return false;
        }

        NiceTable petTable = resp.getTable("USE_PET");
        if (petTable!=null)
        {
            UIGridContainer grid = mGameObjUI.GetComponentInChildren<UIGridContainer>();
            if (grid == null)
            {
                DEBUG.LogError("No exist UIGrid ");
                return false;
            }

            grid.MaxCount = 6;
            int attackCost = 0;
            foreach (KeyValuePair<int, DataRecord> r in petTable.GetAllRecord())
            {
                DataRecord re = r.Value;
                PetData pet = new PetData();
                int teamPos = re.getData("ID");
                pet.level = re.getData("PET_LEVEL");
				pet.breakLevel = re.getData("PET_LEVEL_BREAK");
                pet.exp = re.getData("PET_EXP");
                pet.tid = re.getData("PET_ID");
                pet.mFailPoint = re.getData("FAIL_POINT");
                pet.strengthenLevel = re.getData("PET_ENHANCE");
                pet.starLevel = TableCommon.GetNumberFromActiveCongfig(pet.tid, "STAR_LEVEL");
				pet.mMaxLevelNum = TableCommon.GetNumberFromActiveCongfig(pet.tid, "MAX_LEVEL");
				pet.mMaxStrengthenLevel = TableCommon.GetNumberFromActiveCongfig(pet.tid, "MAX_GROW_LEVEL");
				pet.mMaxLevelBreakNum = TableCommon.GetNumberFromActiveCongfig(pet.tid, "MAX_LEVEL_BREAK");

                if (teamPos<grid.controlList.Count)
                {
                    GameObject item = grid.controlList[teamPos];
                    GameCommon.SetUIVisiable(item, "pvp_pet_check_info", true);
                    PVP_PetTeamWindow.InitPetData(item, pet);
                }

                attackCost += PVP_PetTeamWindow.TotalAttackScore(pet);
            }

            SetText("other_battle_number", attackCost.ToString());

        }

        return true;
    }
	private void ChangeRoleIconAccordingModelIndex(ref string spriteName, int modelIndex)
	{
		string [] names = spriteName.Split ('\\');
		if(modelIndex < 191004)
			spriteName = names[0];
		else 
			spriteName = names[1];
	}
}


public class CS_AttackerPVPPetData : BaseNetEvent
{
    public override void _OnResp(tEvent respEvent)
    {       
        DataCenter.SetData("PVP_ATTACKER_INFO_WINDOW", "REFRESH", respEvent);
    }
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
