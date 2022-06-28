using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using DataTable;
using Logic;

public enum ACTIVE_RANK_TYPE
{
	PVP_4v4 = 1,
	PVP_6v6,
	RECHARGE,
	CONSUME,
	LEVEL,
	COLLECT,
}

public class ActiveRankWindow : ActiveTotalWindow
{
	List<DataRecord> mRecords = new List<DataRecord>();
	int mCurrRankType;
	const int mMaxRankCount = 100;

	public override void Init ()
	{
		EventCenter.Register ("Button_active_rank_child_window_button", new DefineFactory<Button_active_rank_child_window_button>());
		EventCenter.Register ("Button_active_rank_can_get_award", new DefineFactory<Button_active_rank_can_get_award>());

		Net.gNetEventCenter.RegisterEvent ("CS_GetActivityRank", new DefineFactoryLog<CS_GetActivityRank>());
		Net.gNetEventCenter.RegisterEvent ("CS_ReceiveActivityRankAward", new DefineFactoryLog<CS_ReceiveActivityRankAward>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);
		mDesLabelName = "tips_label";
		mCountdownLabelName = "active_rank_reward_rest_time";

		mRecords = TableCommon.FindAllRecords (DataCenter.mActiveRankConfig, lhs => {return lhs["EVENT_ID"] == mConfigIndex;});
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
		RefreshTab ();

		mCurrRankType = GetFirstChildActiveType ();
		RequestRankChildActiveData ();
	}

	void RefreshTab()
	{
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(GetSub ("button_scrollview_context"), "grid");
		grid.MaxCount  = GetChildActive ();
		int index = 0;
		List<int> ls = new List<int>();
		foreach(var v in mRecords)
		{
			if(v["INDEX"] == 0) continue;
			
			int rankType = v["RANK_TYPE"];
			if(ls.Exists (lhs => {return lhs == rankType;})) continue;
			else ls.Add (rankType);
			
			GameObject subCell = grid.controlList[index];
			subCell.transform.name = "active_rank_child_window_button";
			
			if(index == 0) GameCommon.ToggleTrue (subCell);
			
			string title = v["TITLE"];
			foreach (UILabel l in subCell.GetComponentsInChildren<UILabel>())
			{
				if(l != null) l.text = title;
			}
			GameCommon.GetButtonData (subCell).set ("RANK_TYPE", rankType);
			
			index ++;
		}
	}
	
	public override void onChange (string keyIndex, object objVal)
	{
		switch(keyIndex)
		{
		case "REQUSET_CHILD_ACTIVE_DATA":
			mCurrRankType = (int)objVal;
			RequestRankChildActiveData ();
			break;
		case"GET_AWARD":
			GetAward ((int)objVal);
			break;
		case"GET_AWARD_RESULT":
			SetGetAwardButtonVisible (1);
			break;
		}
		base.onChange (keyIndex, objVal);
	}

	public override bool Refresh (object param)
	{
		tEvent respEvt = param as tEvent;

		int rankType = respEvt.get ("RANK_TYPE");
		object obj;
		if(!respEvt.getData ("RANK_DATA", out obj)) return false;
		NiceTable dataTable = obj as NiceTable;

		List<DataRecord> dataRecords  = new List<DataRecord>();
		foreach(var v in dataTable.Records ())
		{
			dataRecords.Add (v);
		}

		RefreshRankInfo(GetSub ("my_active_rank_info"), rankType, GetMyRankPos (dataRecords), dataRecords[0]);
		GameCommon.GetButtonData (GetSub ("active_rank_can_get_award")).set ("RANK_TYPE", mCurrRankType);
		SetGetAwardButtonVisible (respEvt.get ("AWARD_GOT"));

		UIGridContainer activeRankGrid = GameCommon.FindComponent<UIGridContainer>(GetSub ("active_rank_infos"), "grid");
		int maxCount = dataRecords.Count - 1;
		maxCount = Mathf.Min (10, maxCount);
		activeRankGrid.SetMaxCountAsync (maxCount, 1, i => RefreshRankInfo (activeRankGrid.controlList[i], rankType, i + 1, dataRecords[i + 1]), null);
//		activeRankGrid.MaxCount = dataRecords.Count - 1;
//		for(int i = 1; i < dataRecords.Count; i++)
//		{
//			RefreshRankInfo (activeRankGrid.controlList[i - 1], rankType, i, dataRecords[i]);
//		}
		return true;
	}

	int GetMyRankPos(List<DataRecord> dataRecords)
	{
		if(dataRecords == null) return mMaxRankCount + 1;

		DataRecord d = dataRecords[0];
		string name = d["NAME"];
		for(int i = 1; i < dataRecords.Count; i++)
		{
			if(name == dataRecords[i]["NAME"])
				return i;
		}

		return mMaxRankCount + 1;
	}

	void RefreshRankInfo(GameObject parentObj, int rankType, int rankPos, DataRecord record)
	{
		string name = record["NAME"];
		int roleIconIndex = record["ICON"];
		int num = record["VALUE"];

		UIGridContainer mAwardGrid = GameCommon.FindComponent<UIGridContainer>(parentObj, "Grid");

		string strRankPos = "0";
		strRankPos =  rankPos > mMaxRankCount ? "未上榜" : rankPos.ToString ();
		GameCommon.SetUIText (parentObj, "active_rank_num", strRankPos);
		GameCommon.SetUIText (parentObj, "role_name", name);
		GameCommon.SetPalyerIcon (GameCommon.FindComponent<UISprite>(parentObj, "role_photo_icon"), roleIconIndex);

//		int collectItemType = 0; 
//		int collectItemID = 0;
		string collectItemAtlasName = "";
		string collectItemSpriteName = "";
		if(rankType == (int)ACTIVE_RANK_TYPE.COLLECT)
		{
			GetCollectItemTypeAndIDByRankType(rankType, /*ref collectItemType, ref collectItemType,*/ ref collectItemAtlasName, ref collectItemSpriteName);
		}
		SetRankTypeTitle (parentObj, rankType, num, /*collectItemType, collectItemType,*/ collectItemAtlasName, collectItemSpriteName);

		SetAwardIconByGroupID (mAwardGrid, GetAwardGroupIDByRankTypeAndPos (rankType, rankPos));
	}

	void SetGetAwardButtonVisible(int awardGot)
	{
		SetVisible ("active_rank_can_not_get_award", !IsAwardTime ());
		SetVisible ("active_rank_can_get_award", IsAwardTime () && awardGot != 1);
		SetVisible ("active_rank_got_award", IsAwardTime () && awardGot == 1);
	}

	void GetCollectItemTypeAndIDByRankType(int rankType, /*ref int collectItemType, ref int collectItemID,*/ ref string collectItemAtlasName, ref string collectItemSpriteName)
	{
		foreach(var v in DataCenter.mActiveRankConfig.Records ())
		{
			if(v["RANK_TYPE"] == rankType)
			{
//				collectItemID = v["COLLECT_ITEM_ID"];
//				collectItemType = v["COLLECT_ITEM_TYPE"];
				collectItemAtlasName = v["ICON_ATLAS_NAME"];
				collectItemSpriteName = v["ICON_SPRITE_NAME"];
				return ;
			}
		}
	}

	int GetAwardGroupIDByRankTypeAndPos(int rankType, int rankPos)
	{
		foreach(var v in TableCommon.FindAllRecords (DataCenter.mActiveRankConfig, lhs => {return lhs["RANK_TYPE"] == rankType;}))
		{
			int rankNumMax = v["RANK_NUM_MAX"];
			int rankNumMin = v["RANK_NUM_MIN"];
			if(rankPos <= rankNumMin && rankPos >= rankNumMax) return v["AWARD_GROUPID"];
		}

		return 0;
	}

	void SetAwardIconByGroupID(UIGridContainer grid, int groupID)
	{
		List<DataRecord> records = TableCommon.FindAllRecords (DataCenter.mGroupIDConfig, lhs => {return groupID == lhs["GROUP_ID"];});
		grid.MaxCount = records.Count;
		for(int i = 0; i < records.Count; i++)
		{
			DataRecord record = records[i];
			GameCommon.SetItemIcon (grid.controlList[i], new ItemData{mID = record["ITEM_ID"], mType = record["ITEM_TYPE"], mNumber = record["ITEM_COUNT"]});
		}
	}

	void SetRankTypeTitle(GameObject parentObj, int rankType, int num, /*int collectItemType, int collectItemID,*/ string collectItemAtlasName, string collectItemSpriteName)
	{
		GameObject typeTitlesObj = GameCommon.FindObject (parentObj, "type_titles");

		for(int i = 1; i < 7; i++)
		{
			GameCommon.SetUIVisiable (typeTitlesObj, "title_type_" + i.ToString (), i == rankType);
		}

		GameObject typeTitleObj = GameCommon.FindObject (typeTitlesObj, "title_type_" + rankType.ToString ());
		GameCommon.SetUIText (typeTitleObj, "num_label", num.ToString ());

		if(rankType == (int)ACTIVE_RANK_TYPE.COLLECT)
		{
			GameCommon.SetIcon (typeTitleObj, "have_icon", collectItemSpriteName, collectItemAtlasName);
		}
	}

	int GetChildActive()
	{
		List<int> childs = new List<int>();
		foreach(var v in mRecords)
		{
			int type = v["RANK_TYPE"];
			if(!childs.Exists (lhs => {return lhs == type;}))
			{
				childs.Add (type);
			}
		}

		return  childs.Count;
	}

	int GetFirstChildActiveType()
	{
		foreach(var v in mRecords)
		{
			if(v["RANK_TYPE"] != 0) return v["RANK_TYPE"];
		}

		return 0;
	}

	void RequestRankChildActiveData()
	{
		tEvent evt = Net.StartEvent ("CS_GetActivityRank");
		evt.set ("RANK_TYPE", mCurrRankType);
		evt.DoEvent ();
	}

	void GetAward(int rankType)
	{
		tEvent evt = Net.StartEvent ("CS_ReceiveActivityRankAward");
		evt.set ("ACTIVITY_INDEX", mConfigIndex);
		evt.set ("RANK_TYPE", mCurrRankType);
		evt.DoEvent ();
	}
	
	public override void CanGetAwardThenRefresh()
	{
		RequestRankChildActiveData();
	}
}


public class Button_active_rank_child_window_button : CEvent
{
	public override bool _DoEvent()
	{
		int rankType = (int)getObject ("RANK_TYPE");
		DataCenter.SetData ("ACTIVE_RANK_WINDOW", "REQUSET_CHILD_ACTIVE_DATA", rankType);
		return true;
	}
}

public class Button_active_rank_can_get_award : CEvent
{
	public override bool _DoEvent()
	{
		int rankType = (int)getObject ("RANK_TYPE");
		DataCenter.SetData ("ACTIVE_RANK_WINDOW", "GET_AWARD", rankType);
		return true;
	}
}


//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
class CS_GetActivityRank : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			respEvt.set ("RANK_TYPE", (int)get ("RANK_TYPE"));
			DataCenter.SetData ("ACTIVE_RANK_WINDOW", "REFRESH", respEvt);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}


class CS_ReceiveActivityRankAward : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData ("ACTIVE_RANK_WINDOW", "GET_AWARD_RESULT", true);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}
