using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class ActiveLimitBuyGiftBagWindow : ActiveTotalWindow
{
	List<DataRecord> mRecords = new List<DataRecord>();
	List<int> mHadBuyAwardConfigIndex = new List<int>();
	int mMaxItemCount = 5;

	public override void Init ()
	{
		EventCenter.Register ("Button_house_purchase_buy_btn", new DefineFactory<Button_house_purchase_buy_btn>());
		EventCenter.Register ("Button_buy_gift_bag_ok", new DefineFactory<Button_buy_gift_bag_ok>());
		EventCenter.Register ("Button_buy_gift_bag_cancel", new DefineFactory<Button_buy_gift_bag_cancel>());

		Net.gNetEventCenter.RegisterEvent ("CS_GetBuyBagActivityInfo", new DefineFactoryLog<CS_GetBuyBagActivityInfo>());
		Net.gNetEventCenter.RegisterEvent ("CS_BuyBagInActivity", new DefineFactoryLog<CS_BuyBagInActivity>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		mDesLabelName = "tips_label";
		mCountdownLabelName = "active_house_purchase_gift_rest_time";

		mRecords = TableCommon.FindAllRecords (DataCenter.mAwardConfig, lhs => {return lhs["EVENT_ID"] == mConfigIndex;});
		SetVisible ("show_items_info", false);
	}

	public override void OnOpen ()
	{
		base.OnOpen ();

		tEvent evt = Net.StartEvent ("CS_GetBuyBagActivityInfo");
		evt.set ("ACTIVITY_INDEX", mConfigIndex);
		evt.DoEvent ();
//		Refresh (null);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		switch(keyIndex)
		{
		case"SHOW_ITEMS":
				ShowItems ((int)objVal);
				break;
		case"SEND_MESSAGE":
				SendMessage ((int)objVal);
				break;
		case"SEND_MESSAGE_RESULT":
				SendMessageResult ((int)objVal);
				break;
		}
		base.onChange (keyIndex, objVal);
	}

	public override bool Refresh (object param)
	{
		tEvent respEvt = param as tEvent ;
		object obj;
		if(!respEvt.getData ("DATABUFFER", out obj)) return false;
		DataBuffer dataBuffer =obj as DataBuffer;
		dataBuffer.seek (0);
		int count;
		bool b = dataBuffer.read (out count);

		for(int i = 0; i < count; i++)
		{
			int pos;
			if(dataBuffer.read (out pos) && pos != 0) mHadBuyAwardConfigIndex.Add (pos);
		}

		RefreshChangeComponent();

		return true;
	}

	void RefreshChangeComponent()
	{
		UIGridContainer grid = GetComponent<UIGridContainer>("grid");
		grid.MaxCount = mRecords.Count;
		for(int i = 0; i < mRecords.Count; i ++)
		{
			GameObject subCell = grid.controlList[i];
			DataRecord record = mRecords[i];
			int basePrice = record["BASE_PRICE"];
			int realPrice = record["REAL_PRICE"];
			int awardGroupID = record["AWARD_GROUP_1"];
			int awardConfigIndex = record["INDEX"];
			
			GameCommon.GetButtonData (GameCommon.FindObject(subCell, "house_purchase_buy_btn")).set ("AWARD_CONFIG_INDEX", awardConfigIndex);
			GameCommon.SetUIText (subCell, "base_prise_label", basePrice.ToString ());
			GameCommon.SetUIText (subCell, "real_prise_label", realPrice.ToString ());
			GameCommon.SetUIText (subCell, "house_purchase_days_label", "第" + (i + 1).ToString () + "天");
			GameCommon.SetUIVisiable (subCell, "house_purchase_buy_btn", (IsCanBuy (awardConfigIndex) && !mHadBuyAwardConfigIndex.Exists (lhs => {return lhs == awardConfigIndex;})));
			GameCommon.SetUIVisiable (subCell, "house_purchase_buy_btn_gray", (IsCanBuy (awardConfigIndex) && mHadBuyAwardConfigIndex.Exists (lhs => {return lhs == awardConfigIndex;})));
			
			List<DataRecord> groupIDRecords = TableCommon.FindAllRecords (DataCenter.mGroupIDConfig, lhs => {return lhs["GROUP_ID"] == awardGroupID;});
			UIGridContainer itemGrid = GameCommon.FindComponent<UIGridContainer>(subCell, "house_purchase_grid");
			itemGrid.MaxCount = mMaxItemCount;
			for(int j = 0; j < mMaxItemCount; j++)
			{
				GameObject itemSubCell = itemGrid.controlList[j];
				DataRecord itemRecord = groupIDRecords[j];
				GameCommon.SetItemIcon (itemSubCell, new ItemData{mID = itemRecord["ITEM_ID"], mType = itemRecord["ITEM_TYPE"], mNumber = itemRecord["ITEM_COUNT"]});
			}
		}
	}

	bool IsCanBuy(int awardConfigIndex)
	{
		DataRecord record = DataCenter.mAwardConfig.GetRecord (awardConfigIndex);
		string strEndTime = record["EVENT_DATE"];
		string[] strEndTimes = strEndTime.Split ('_');
		DateTime endData = new DateTime(Convert.ToInt32 (strEndTimes[0]), Convert.ToInt32 (strEndTimes[1]), Convert.ToInt32 (strEndTimes[2]), 0, 0, 0);

		Int64 endSeconds = GameCommon.DateTime2TotalSeconds(endData);
		Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ());

		return nowSeconds > endSeconds;
	}

	void ShowItems(int awardConfigIndex)
	{
		SetVisible ("show_items_info", true);
		GameObject parentObj = GetSub ("show_items_info");
		int realPrice = DataCenter.mAwardConfig.GetData (awardConfigIndex, "REAL_PRICE");
		GameCommon.SetUIText (parentObj, "cost_count", realPrice.ToString ());
		GameCommon.GetButtonData (GameCommon.FindObject (parentObj, "buy_gift_bag_ok")).set ("COST_COUNT", realPrice);
		GameCommon.GetButtonData (GameCommon.FindObject (parentObj, "buy_gift_bag_ok")).set ("AWARD_CONFIG_INDEX", awardConfigIndex);
		
		int awardGroupID = DataCenter.mAwardConfig.GetData (awardConfigIndex, "AWARD_GROUP_1");
		List<DataRecord> groupIDRecords = TableCommon.FindAllRecords (DataCenter.mGroupIDConfig, lhs => {return lhs["GROUP_ID"] == awardGroupID;});
		for(int i = 0; i < mMaxItemCount; i++)
		{
			DataRecord itemRecord = groupIDRecords[i];
			GameObject subcell = GameCommon.FindObject (parentObj, "item_info_" + i.ToString ());
			GameCommon.SetItemIcon (subcell, new ItemData{mID = itemRecord["ITEM_ID"], mType = itemRecord["ITEM_TYPE"], mNumber = itemRecord["ITEM_COUNT"]});
		}
	}

	void SendMessage(int awardConfigIndex)
	{
		if(awardConfigIndex == 0)
		{
			SetVisible ("show_items_info", false);
			return;
		}

		tEvent evt = Net.StartEvent ("CS_BuyBagInActivity");
		evt.set ("BAG_INDEX", awardConfigIndex);
		evt.DoEvent ();
	}

	void SendMessageResult(int addAwardConfigIndex)
	{
		SetVisible ("show_items_info", false);

		//reduceCurrency
		int realPrice = DataCenter.mAwardConfig.GetData(addAwardConfigIndex, "REAL_PRICE");
		GameCommon.RoleChangeNumericalAboutRole ((int)ITEM_TYPE.YUANBAO, -realPrice);
		//gaincurrency
		int awardGroupID = DataCenter.mAwardConfig.GetData (addAwardConfigIndex, "AWARD_GROUP_1");
		List<DataRecord> groupIDRecords = TableCommon.FindAllRecords (DataCenter.mGroupIDConfig, lhs => {return lhs["GROUP_ID"] == awardGroupID;});
		for(int i = 0; i < mMaxItemCount; i++)
		{
			DataRecord itemRecord = groupIDRecords[i];
			int itemType = itemRecord["ITEM_TYPE"];
			int itemCount = itemRecord["ITEM_COUNT"];
			GameCommon.RoleChangeNumericalAboutRole (itemType, itemCount);
		}

		mHadBuyAwardConfigIndex.Add (addAwardConfigIndex);
		RefreshChangeComponent ();
	}
	

}

//----------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------
class Button_house_purchase_buy_btn : CEvent
{
	public override bool _DoEvent ()
	{
		int awardConfigIndex = (int)getObject ("AWARD_CONFIG_INDEX");
		DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LIMIT_BUY_GIFT_BAG.ToString () + "_WINDOW", "SHOW_ITEMS", awardConfigIndex);
		return true;
	}
}


class Button_buy_gift_bag_ok : CEvent
{
	public override bool _DoEvent ()
	{
		int costCount = (int)getObject("COST_COUNT");
		if(GameCommon.HaveEnoughCurrency (ITEM_TYPE.YUANBAO, costCount))
		{
			int awardConfigIndex = (int)getObject ("AWARD_CONFIG_INDEX");
			DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LIMIT_BUY_GIFT_BAG.ToString () + "_WINDOW", "SEND_MESSAGE", awardConfigIndex);
		}
		return true;
	}
}

class Button_buy_gift_bag_cancel : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LIMIT_BUY_GIFT_BAG.ToString () + "_WINDOW", "SEND_MESSAGE", 0);
		return true;
	}
}


//----------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------
class CS_GetBuyBagActivityInfo : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			int configIndex = (int)get ("ACTIVITY_INDEX");
			ACTIVE_TYPE type = (ACTIVE_TYPE)((int)DataCenter.mOperateEventConfig.GetData (configIndex, "EVENT_TYPE"));
			DataCenter.SetData (type.ToString () + "_WINDOW", "REFRESH", respEvt);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}


class CS_BuyBagInActivity : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			int awardConfigIndex = get ("BAG_INDEX");
			DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LIMIT_BUY_GIFT_BAG.ToString () + "_WINDOW", "SEND_MESSAGE_RESULT", awardConfigIndex);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}