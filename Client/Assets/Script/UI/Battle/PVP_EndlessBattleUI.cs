using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;
using System.Linq;

public class PVP_EndlessBattleUI : MonoBehaviour 
{

}

public class PVP_EndlessBattleEnterWindow :tWindow 
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_endless_battle_enter_ready_button", new DefineFactory<Button_endless_battle_enter_ready_button>());
	}
}

public class Button_endless_battle_enter_ready_button : CEvent
{
	public override bool _DoEvent()
	{
		GlobalModule.ClearAllWindow();
		DataCenter.OpenWindow("PVP_ENDLESS_BATTLE_READY_WINDOW", false);
		return true;
	}
}

public class PVP_EndlessReadyWindow : tWindow
{
	public override void Open(object param)
	{
		base.Open(param);
		DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
		DataCenter.OpenWindow("PVP_ENDLESS_BATTLE_WINDOW");
		DataCenter.SetData ("PVP_ENDLESS_BATTLE_WINDOW", "ENDLESS_OVERALL", true);
		DataCenter.OpenWindow("PVP_PLAYER_ENDLESS_BATTLE_RANK_WINDOW");
		GameCommon.ToggleTrue (GameCommon.FindUI ("endless_battle_overall_rank_button"));
	}
	
	public override void Close ()
	{
		base.Close ();
		DataCenter.CloseWindow("PVP_PLAYER_ENDLESS_BATTLE_RANK_WINDOW");
		DataCenter.CloseWindow("PVP_ENDLESS_BATTLE_WINDOW");
	}
}

//无尽  排行   按钮  
public class PVP_EndlessBattleWindow : tWindow
{
	List<PvpRankData> mList = new List<PvpRankData>();
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_endless_battle_overall_rank_button", new DefineFactory<Button_endless_battle_overall_rank_button>());
		EventCenter.Self.RegisterEvent("Button_endless_battle_friend_rank_button", new DefineFactory<Button_endless_battle_friend_rank_button>());
		EventCenter.Self.RegisterEvent("Button_endless_battle_shop_button", new DefineFactory<Button_endless_battle_shop_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		GetSub ("endless_battle_overall_rank").SetActive (true);
		GetSub ("endless_battle_friend_rank").SetActive (false );
		GetSub ("endless_battle_commodity").SetActive (false);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex)
		{
		case "ENDLESS_OVERALL":
			GetSub ("endless_battle_overall_rank").SetActive (true);
			GetSub ("endless_battle_friend_rank").SetActive (false );
			GetSub ("endless_battle_commodity").SetActive (false);
			GetSub ("pvp_endless_battle_rank_reset_time").SetActive (true);	
			
			tEvent evt = Net.StartEvent("CS_RequestPVPRank");
			evt.set ("WINDOW_NAME", "PVP_ENDLESS_BATTLE_WINDOW");
			evt.DoEvent();
	
			break;
		case "ENDLESS_FRIEND":
			GetSub ("endless_battle_overall_rank").SetActive (false);
			GetSub ("endless_battle_friend_rank").SetActive (true  );
			GetSub ("endless_battle_commodity").SetActive (false);
			GetSub ("pvp_endless_battle_rank_reset_time").SetActive (true);

			tEvent e = Net.StartEvent("CS_RequestFriendRank");
			e.set ("WINDOW_NAME", "PVP_ENDLESS_BATTLE_WINDOW");
			e.DoEvent();
	
			break;
		case "ENDLESS_SHOP":
			GetSub ("endless_battle_overall_rank").SetActive (false);
			GetSub ("endless_battle_friend_rank").SetActive (false  );
			GetSub ("endless_battle_commodity").SetActive (true);
			GetSub ("pvp_endless_battle_rank_reset_time").SetActive (false);
			EndlessBattleCommodityInfo();
			break;
		case "REFRESH_OVERALL_RANK":
			RefreshOverallRank(objVal);
			break;
		case "REFRESH_FRIEND_RANK":
			RefreshFriendRank(objVal);
			break;
		}
	}

	bool RefreshOverallRank(object param)
	{
		tEvent respEvt = param as tEvent;
		
		if (respEvt==null)
			return false;
		
//		if(GetSub ("pvp_rank_reset_time") != null && GetSub ("pvp_rank_reset_time").GetComponent<CountdownUI>() == null)
//			SetCountdownTime("pvp_rank_reset_time", (Int64)respEvt["PVP_RANK_RESET_TIME"], new CallBack(this, "RefreshData", ++mRequestNum));
		
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
		UIGridContainer randGrid = GameCommon.FindObject (GetSub ("endless_battle_overall_rank"), "grid").GetComponent<UIGridContainer>();
		randGrid.MaxCount = num;
		for(int j = 0; j < num; j++)
		{
			GameObject item = randGrid.controlList[j];
			GameCommon.SetUIText(item, "rank_label", (j+1).ToString());
			string strVip = "";
			//			if(mList[j].mVipLevel != 0) strVip = "  (VIP" + mList[j].mVipLevel.ToString () + ")";
			GameCommon.SetUIText(item, "name_label", mList[j].mName + strVip);
			GameCommon.SetUIText(item, "pvp_score_label", mList[j].mScore.ToString ());
			
//			SetFlag(item, j);
//			SetVisitButtonData(item, mList[j].mPlayerID, mList[j].mName);
		}
		
		mList.Clear ();
		return true;
	}

	int SortListByScore(PvpRankData a, PvpRankData b)
	{
		return GameCommon.Sort (a.mScore, b.mScore, true);
	}

	bool RefreshFriendRank(object param)
	{
		tEvent respEvt = param as tEvent;
		
		if (respEvt==null)
			return false;
		
		//		if(GetSub ("pvp_rank_reset_time") != null && GetSub ("pvp_rank_reset_time").GetComponent<CountdownUI>() == null)
		//			SetCountdownTime("pvp_rank_reset_time", (Int64)respEvt["PVP_RANK_RESET_TIME"], new CallBack(this, "RefreshData", ++mRequestNum));
		
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
		UIGridContainer randGrid = GameCommon.FindObject (GetSub ("endless_battle_friend_rank"), "grid").GetComponent<UIGridContainer>();
		randGrid.MaxCount = num;
		for(int j = 0; j < num; j++)
		{
			GameObject item = randGrid.controlList[j];
			GameCommon.SetUIText(item, "rank_label", (j+1).ToString());
			string strVip = "";
			//			if(mList[j].mVipLevel != 0) strVip = "  (VIP" + mList[j].mVipLevel.ToString () + ")";
			GameCommon.SetUIText(item, "name_label", mList[j].mName + strVip);
			GameCommon.SetUIText(item, "pvp_score_label", mList[j].mScore.ToString ());
			
			//			SetFlag(item, j);
			//			SetVisitButtonData(item, mList[j].mPlayerID, mList[j].mName);
		}
		
		mList.Clear ();
		return true;
	}

	//无尽  商品
	void EndlessBattleCommodityInfo()
	{
		UIGridContainer mEndlessBattleCommodityGrid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
		mEndlessBattleCommodityGrid.MaxCount = 6;
		
		int iIndex = 1001 ;
		
		for(int i = 0; i < mEndlessBattleCommodityGrid.MaxCount ; i++)
		{	
			string mCommodityName = TableCommon.GetStringFromGuildShop (i + iIndex, "NAME");
			string mNeedEmblemNumber = TableCommon.GetStringFromGuildShop (i + iIndex, "SINGEROFFER");
			GameCommon.SetUIVisiable (mEndlessBattleCommodityGrid.controlList[i], "ensure_buy_it", false  );
			
			
			string strAtlasName = TableCommon.GetStringFromGuildShop (i + iIndex, "ICONATLAS");
			string strSpriteName = TableCommon.GetStringFromGuildShop (i + iIndex, "PICTURE");
			GameCommon.SetUISprite (mEndlessBattleCommodityGrid.controlList[i], "commodity_icon", strAtlasName, strSpriteName);
			
			GameCommon.SetUIText (mEndlessBattleCommodityGrid.controlList[i], "commodity_name", mCommodityName);
			GameCommon.SetUIText (mEndlessBattleCommodityGrid.controlList[i], "need_emblem_number", mNeedEmblemNumber);
		}
	}
}
//无尽 个人信息 页面
public class PVP_PlayerEndlessBattleRankWindow : tWindow
{

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_endless_battle_prepare_button", new DefineFactory<Button_endless_battle_prepare_button>());
		EventCenter.Self.RegisterEvent("Button_endless_battle_rank_award_button", new DefineFactory<Button_attribute_rank_award_button>());
		EventCenter.Self.RegisterEvent("Button_endless_battle_battle_teame_button", new DefineFactory<Button_endless_battle_prepare_button>());
	}

//	public override void onChange(string keyIndex, object objVal)
//	{
//		base.onChange(keyIndex, objVal);
//		switch (keyIndex)
//		{
////		case "SHOW_START_ENDLESS_BATTLE_BUTTON":
////			GetSub("endless_battle_prepare_button").SetActive (false);
//////			GetSub("start_endless_battle_button").SetActive( true);
////			break;
////		case "SHOW_ENDLESS_RANK_LIST_BUTTON":
////			GetSub ("endless_battle_rank_award_button").SetActive (true );
////			GetSub ("endless_battle_rank_list_button").SetActive (false );
////			break;
//		}
//	}
}

//无尽模式 Button 事件
public class Button_endless_battle_overall_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PVP_ENDLESS_BATTLE_WINDOW","ENDLESS_OVERALL",true);

		return true;
	}
}

public class Button_endless_battle_friend_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PVP_ENDLESS_BATTLE_WINDOW","ENDLESS_FRIEND",true );

		return true;
	}
}

public class Button_endless_battle_shop_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PVP_ENDLESS_BATTLE_WINDOW","ENDLESS_SHOP",true);
		return true;
	}
}

//个人信息  button 事件
public class Button_endless_battle_prepare_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("PVP_ENDLESS_BATTLE_PET_TEAM_WINDOW");
		DataCenter.OpenWindow("PVP_ENDLESS_BATTLE_DIFFICULTY_WINDOW");
		DataCenter.CloseWindow ("PVP_ENDLESS_BATTLE_WINDOW");
		DataCenter.CloseWindow ("PVP_PLAYER_ENDLESS_BATTLE_RANK_WINDOW");
//		DataCenter.SetData ("PVP_PLAYER_ENDLESS_BATTLE_RANK_WINDOW","SHOW_START_ENDLESS_BATTLE_BUTTON",true );
		return true;
	}
}

//无尽模式   调整宠物
public class PVP_EndlessBattlePetTeamWindow : tWindow
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_endless_battle_select_pet_button", new DefineFactory<Button_endless_battle_select_pet_button>());
		EventCenter.Self.RegisterEvent("Button_endless_battle_finish_select_pet_button", new DefineFactory<Button_endless_battle_finish_select_pet_button>());
	}

	public override void Open(object param)
	{
		base.Open(param);
		ShowClickFlag(false);
		DataCenter.SetData ("PVP_ENDLESS_BATTLE_PET_TEAM_WINDOW","ENDLESS_SELECT_BUTTON",false );

	}

	void ShowClickFlag(bool bVisiable)
	{
		for (int i = 0; i < 3; ++i)
		{
			ShowButtonFlag(i, bVisiable);
		}
	}

	void ShowButtonFlag(int pos, bool bVisiable)
	{
		string key = "pvp_endless_battle_pet_check_" + pos.ToString();
		GameObject buttonObj = GameCommon.FindObject(mGameObjUI, key);
		GameCommon.SetUIVisiable(buttonObj, "pvp_pet_check_info", bVisiable);
		GameCommon.SetUIVisiable(buttonObj, "check_mark_1", bVisiable);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex)
		{
		case "ENDLESS_SELECT_BUTTON":
			GetSub("endless_battle_select_pet_button").SetActive ( true );
			GetSub("endless_battle_finish_select_pet_button").SetActive ( false );
			break;
		case "ENDLESS_FINISH_BUTTON":
			GetSub("endless_battle_select_pet_button").SetActive (false );
			GetSub("endless_battle_finish_select_pet_button").SetActive ( true );
			break;
		}
	}
}

public class Button_endless_battle_select_pet_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PVP_ENDLESS_BATTLE_DIFFICULTY_WINDOW");
		DataCenter.SetData("PVP_ENDLESS_BATTLE_PET_TEAM_WINDOW", "ENDLESS_FINISH_BUTTON", true);
		DataCenter.OpenWindow("PVP_ENDLESS_BATTLE_PET_BAG_WINDOW");

		return true;
	}
}

public class Button_endless_battle_finish_select_pet_button : CEvent
{
	public override bool _DoEvent()
	{
		
		DataCenter.CloseWindow("PVP_ENDLESS_BATTLE_PET_BAG_WINDOW");
		DataCenter.OpenWindow("PVP_ENDLESS_BATTLE_DIFFICULTY_WINDOW");
		
		DataCenter.SetData("PVP_ENDLESS_BATTLE_PET_TEAM_WINDOW", "ENDLESS_SELECT_BUTTON", true);
		
		return true;
	}
}


//无尽   选择难度
public class PVP_EndlessBattleDifficultyWindow : tWindow
{
	int MaxEndlessBattleDifficultyNum = 6 ;

	public override void Open(object param)
	{
		base.Open(param);
		UnionShopInfo();
	}
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_endless_battle_rank_list_button", new DefineFactory<Button_endless_battle_rank_list_button>());
		EventCenter.Self.RegisterEvent("Button_start_endless_battle_button", new DefineFactory<Button_give_battle_button>());
	}
		
	void UnionShopInfo()
	{
		UIGridContainer mEndlessBattleDifficultyGrid = GameCommon.FindObject (mGameObjUI, "endless_battle_difficulty_grid").GetComponent<UIGridContainer>();
		mEndlessBattleDifficultyGrid.MaxCount = MaxEndlessBattleDifficultyNum;
		
		int iIndex = 1001 ;
		
		for(int i = 0; i < mEndlessBattleDifficultyGrid.MaxCount ; i++)
		{	
			string mEndlessBattleDifficultyName = TableCommon.GetStringFromEndlessDifficult (i + iIndex, "NAME");
			string mEndlessBattleDifficultyNumber = TableCommon.GetStringFromEndlessDifficult (i + iIndex, "DESC");
			if(i==0)
			{
				GameCommon.SetUIVisiable (mEndlessBattleDifficultyGrid.controlList[i],"not_active_icon",false );
			}

			GameCommon.SetUIText (mEndlessBattleDifficultyGrid.controlList[i], "endless_battle_difficulty_name", mEndlessBattleDifficultyName);
			GameCommon.SetUIText (mEndlessBattleDifficultyGrid.controlList[i], "endless_battle_difficulty_level", mEndlessBattleDifficultyNumber);
		}
	}
}

public class Button_endless_battle_rank_list_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PVP_ENDLESS_BATTLE_PET_TEAM_WINDOW");
		DataCenter.CloseWindow ("PVP_ENDLESS_BATTLE_DIFFICULTY_WINDOW");
		DataCenter.SetData("PVP_ENDLESS_BATTLE_WINDOW","ENDLESS_OVERALL",true );
		DataCenter.OpenWindow ("PVP_ENDLESS_BATTLE_WINDOW",true );
		DataCenter.OpenWindow ("PVP_PLAYER_ENDLESS_BATTLE_RANK_WINDOW");
		
		return true;
	}
}

//无尽   宠物背包
public class PVP_EndlessBattlePetBagWindow : PVP_PetBagWindow
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











