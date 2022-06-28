using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class MysteriousShopWindow : tWindow
{
	MysterousItemData mCurrentMysterousData;
	public override void Init ()
	{
		EventCenter.Register("Button_mysterious_commodity_refresh_button", new DefineFactory<Button_mysterious_commodity_refresh_button>());
		EventCenter.Register("Button_get_mysterious_item_button", new DefineFactory<Button_get_mysterious_item_button>());
		EventCenter.Register("Button_close_sure_buy_item_button", new DefineFactory<Button_close_sure_buy_item_button>());
		EventCenter.Register("Button_sure_buy_item_button", new DefineFactory<Button_sure_buy_item_button>());

		Net.gNetEventCenter.RegisterEvent ("CS_RequestShopItemList", new DefineFactoryLog<CS_RequestShopItemList>());
		Net.gNetEventCenter.RegisterEvent ("CS_RefreshShopItemList", new DefineFactoryLog<CS_RefreshShopItemList>());
	}

	public override void OnOpen ()
	{
		SetVisible ("sure_buy_item_window", false);
		tEvent quest = Net.StartEvent("CS_RequestShopItemList");
		quest.DoEvent();
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case "ITEM_INFO":
			ShowItemInfo(objVal);
			break;
		case "SURE_BUY":
			tEvent evt = Net.StartEvent("CS_BuyItemResult");
			evt.set ("IS_FREE", false);
			evt.set ("IS_DISCOUNT", false);
			evt.set("SHOP_SLOT_INDEX", (int)objVal);
			evt.set("IS_MYSTERIOUS", true);
			evt.DoEvent();
			break;
		case "CANCEL_BUY":
			SetVisible ("sure_buy_item_window", false);
			break;
		case "BUY_RESULT":
			BuyResult ();
			break;
		case"REFRESH_DATA":
			tEvent quest = Net.StartEvent("CS_RefreshShopItemList");
			quest.set ("COST_COUNT", (int)objVal);
			quest.DoEvent();
			break;
		}
	}
	
	public override bool Refresh (object param)
	{
		tEvent respEvt = param as tEvent;
		SetText ("my_honor_point_number", RoleLogicData.Self.mHonorPoint.ToString ());

		int refeshTimes = respEvt.get ("SHOP_DATA_REFRESH_TIMES");
		int baseRefreshCount = DataCenter.mGlobalConfig.GetData ("REFRESH_MYSTERIOUS_SHOP", "VALUE");
		int refreshCostCount = (refeshTimes + 1) * baseRefreshCount;
		SetText ("mysterious_commodity_refresh_need_number", "X" + refreshCostCount.ToString ());
		GameCommon.GetButtonData (GetSub ("mysterious_commodity_refresh_button")).set ("COUNT", refreshCostCount);

		object tableData;
		if (!respEvt.getData("SHOP_DATA", out tableData))
		{
			return false;
		}
		NiceTable table = tableData as NiceTable;


		UIGridContainer grid = GetComponent<UIGridContainer>("grid");
		int maxCount = table.GetRecordCount ();
		grid.MaxCount = maxCount;
		int index = 0;
		foreach(DataRecord r in table.Records ())
		{
			GameObject subCell = grid.controlList[index];
			index++;
			bool bIsSoldOut = r["SHOP_KIND_STATE"] == 0 ? false : true;
			GameCommon.SetUIVisiable (subCell, "sold_out", bIsSoldOut);
			GameCommon.SetUIVisiable (subCell, "get_mysterious_item_button", !bIsSoldOut);
			if(bIsSoldOut) continue;

			int shopIndex = r["SHOP_KIND_INDEX"];
			int costType = TableCommon.GetNumberFromShopSlotBase (shopIndex, "COST_ITEM_TYPE");
			int costNum = TableCommon.GetNumberFromShopSlotBase (shopIndex, "COST_ITEM_COUNT");

			string atlasName = DataCenter.mItemIcon.GetData(costType, "MYSTERIOUS_SHOP_ATLAS");
			string spriteName = DataCenter.mItemIcon.GetData(costType, "MYSTERIOUS_SHOP_SPRITE");
			GameCommon.SetIcon (subCell, "cost_icon", spriteName, atlasName);

			int equipElement = 1;
			int itemType = r["ITEM_TYPE"];
			int itemID = r["ITEM_ID"];
			int itemCount = r["ITEM_COUNT"];
//			int itemCount = TableCommon.GetNumberFromShopSlotBase (shopIndex,"EXTRACT_TIMES");
			bool bTodayIsFree = false;
			bool bNewTitle = false;
			
			string name = "";
			string des = "";
			GetNameAndDescribeByTypeAndID ((ITEM_TYPE)itemType, itemID, ref name, ref des);
			GameCommon.SetUIText (subCell, "cost_num", "X" + costNum.ToString ());
			GameCommon.SetUIText (subCell, "item_name_and_num", name + "X" + itemCount.ToString());
			GameCommon.SetUIVisiable (subCell, "today_free_label", bTodayIsFree);
			GameCommon.SetUIVisiable (subCell, "new_title", bNewTitle);
			
			ItemData data = new ItemData() { mType = itemType, mID = itemID, mEquipElement = equipElement, mNumber = itemCount};
			GameCommon.SetItemIcon(subCell, data);
			
			MysterousItemData mysterousData = new MysterousItemData{mData = data, mCostType = costType, mCostNum = costNum, mShopIndex = shopIndex};
			GameCommon.GetButtonData (subCell, "get_mysterious_item_button").set ("ITEM_DATA", mysterousData);
		}

		return true;
	}

	void ShowItemInfo(object objVal)
	{
		GameObject obj = GetSub ("sure_buy_item_window");
		obj.SetActive (true);
		mCurrentMysterousData = (MysterousItemData)objVal;
		GameCommon.SetItemIcon(obj, mCurrentMysterousData.mData);
		GameCommon.GetButtonData(GetSub ("sure_buy_item_button")).set ("SHOP_INDEX", mCurrentMysterousData.mShopIndex);
		GameCommon.GetButtonData(GetSub ("sure_buy_item_button")).set ("ITEM_DATA", mCurrentMysterousData.mData);

		ITEM_TYPE type = (ITEM_TYPE)mCurrentMysterousData.mData.mType;
		int ID = mCurrentMysterousData.mData.mID;
		int coutn = mCurrentMysterousData.mData.mNumber;
		string name = "";
		string des = "";
		GetNameAndDescribeByTypeAndID (type, ID, ref name, ref des);

		GameCommon.SetUIText (obj, "name", name);
		GameCommon.SetUIText (obj, "describe_label", des);
		GameCommon.SetUIText (obj, "have_num", GetHadCountByTypeAndID ((int)type, ID).ToString ());
		GameCommon.SetUIVisiable (obj, "have_num", false);
		GameCommon.SetUIText (obj, "cost_num", mCurrentMysterousData.mCostNum.ToString ());
	}

	void GetNameAndDescribeByTypeAndID(ITEM_TYPE type, int modelID, ref string name, ref string des)
	{
		switch(type)
		{
		case ITEM_TYPE.PET:
			name = TableCommon.GetStringFromActiveCongfig (modelID, "NAME");
			des = TableCommon.GetStringFromActiveCongfig (modelID, "DESCRIBE");
			break;
		case ITEM_TYPE.EQUIP:
			name = TableCommon.GetStringFromRoleEquipConfig (modelID, "NAME");
			des = TableCommon.GetStringFromRoleEquipConfig (modelID, "DESCRIPTION");
			break;
		case ITEM_TYPE.PET_FRAGMENT:
			name = TableCommon.GetStringFromFragment (modelID, "NAME");
			des = TableCommon.GetStringFromFragment (modelID, "DESCRIPTION");
			break;
		case ITEM_TYPE.MATERIAL:
			name = TableCommon.GetStringFromMaterialConfig (modelID, "NAME");
			des = TableCommon.GetStringFromMaterialConfig (modelID, "DESCRIPTION");
			break;
		case ITEM_TYPE.MATERIAL_FRAGMENT:
			name = TableCommon.GetStringFromMaterialFragment (modelID, "NAME");
			des = TableCommon.GetStringFromMaterialFragment (modelID, "DESCRIPTION");
			break;
		case ITEM_TYPE.GEM:
			name = TableCommon.GetStringFromStoneTypeIconConfig (modelID, "NAME");
			des = TableCommon.GetStringFromStoneTypeIconConfig (modelID, "DESCRIPTION");
			break;
		case ITEM_TYPE.GOLD:
		case ITEM_TYPE.YUANBAO:
		case ITEM_TYPE.SPIRIT:
		case ITEM_TYPE.POWER:
		case ITEM_TYPE.LOCK_POINT:
		case ITEM_TYPE.RESET_POINT:
		case ITEM_TYPE.SAODANG_POINT:
			name = DataCenter.mItemIcon.GetRecord ((int)type)["NAME"];
			des = name;
			break;
		case ITEM_TYPE.PET_EQUIP:
		default:
			name = type.ToString ();
			des = "not find";
			break;
		}
	}

	int GetHadCountByTypeAndID(int type ,int iTid)
	{
		int count = 0;
		switch((ITEM_TYPE)type)
		{
		case ITEM_TYPE.CONSUME_ITEM:
			ConsumeItemLogicData consumData = DataCenter.GetData ("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
			count = consumData.GetCountByID (iTid);
			break; 
		case ITEM_TYPE.GEM:
			GemLogicData gemData = DataCenter.GetData ("GEM_DATA") as GemLogicData;
			count = gemData.GetCountByID (iTid);
			break;
		case ITEM_TYPE.EQUIP:
			RoleEquipLogicData roleEquipData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
			count = roleEquipData.GetNumByIndex(iTid);
			break;
		case ITEM_TYPE.PET_FRAGMENT:
			count = PetFragmentLogicData.Self.GetCountByTid (iTid);
			break;
		case ITEM_TYPE.MATERIAL:
			count = MaterialLogicData.Self.GetNumByIndex (iTid);
			break;
		case ITEM_TYPE.MATERIAL_FRAGMENT:
			count = MaterialFragmentLogicData.Self.GetNumByIndex (iTid);
			break;
		case ITEM_TYPE.PET:
			count = PetLogicData.Self.GetCountByTid (iTid);
			break;
		case ITEM_TYPE.PET_EQUIP:
			break;
		case ITEM_TYPE.YUANBAO:
			count = RoleLogicData.Self.diamond;
			break;
		case ITEM_TYPE.SPIRIT:
			count = RoleLogicData.Self.spirit;
			break;
		case ITEM_TYPE.GOLD:
			count = RoleLogicData.Self.gold;
			break;
		case ITEM_TYPE.HONOR_POINT:
			count = RoleLogicData.Self.mHonorPoint;
			break;
		case ITEM_TYPE.LOCK_POINT:
			count = RoleLogicData.Self.mLockNum;
			break;
		case ITEM_TYPE.POWER:
			count = RoleLogicData.Self.stamina;
			break;
		case ITEM_TYPE.RESET_POINT:
			count = RoleLogicData.Self.mResetNum;
			break;
		case ITEM_TYPE.SAODANG_POINT:
			count = RoleLogicData.Self.mSweepNum;
			break;
		}

		return count;
	}


	void BuyResult()
	{
		GameCommon.RoleChangeNumericalAboutRole (mCurrentMysterousData.mData.mType, mCurrentMysterousData.mData.mNumber);
		GameCommon.RoleChangeNumericalAboutRole (mCurrentMysterousData.mCostType, -mCurrentMysterousData.mCostNum);
		OnOpen ();
	}

	class MysterousItemData
	{
		public ItemData mData;
		public int mCostNum;
		public int mCostType;
		public int mShopIndex;
	}
}


public class Button_mysterious_commodity_refresh_button : CEvent
{
	public override bool _DoEvent()
	{
		int count = (int)getObject ("COUNT");
		DataCenter.OpenMessageOkWindow (STRING_INDEX.ERROR_MYSTERIOUS_REFRESH, count.ToString (),
		                                () => {DataCenter.SetData ("MYSTERIOUS_SHOP_WINDOW", "REFRESH_DATA", count);});
		return true;
	}
}

public class Button_get_mysterious_item_button : CEvent
{
	public override bool _DoEvent()
	{
		object dataObj = getObject ("ITEM_DATA");
		DataCenter.SetData ("MYSTERIOUS_SHOP_WINDOW", "ITEM_INFO", dataObj);
		return true;
	}
}

public class Button_close_sure_buy_item_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("MYSTERIOUS_SHOP_WINDOW", "CANCEL_BUY", true);
		return true;
	}
}

public class Button_sure_buy_item_button : CEvent
{
	public override bool _DoEvent()
	{
		ItemData data = (ItemData)getObject ("ITEM_DATA");
		int addPetCount = 0;
		int addRoleEquipCount = 0;
		switch(data.mType)
		{
		case(int)ITEM_TYPE.PET:
			addPetCount = data.mNumber;
			break;
		case (int)ITEM_TYPE.EQUIP:
			addRoleEquipCount = data.mNumber;
			break;
		}
		if(!JudgeMailWillFull (addPetCount, addRoleEquipCount))
		{
			int shopIndex = (int)getObject ("SHOP_INDEX");
			DataCenter.SetData ("MYSTERIOUS_SHOP_WINDOW", "SURE_BUY", shopIndex);
		}

		return true;
	}

	bool JudgeMailWillFull(int addPetCount, int addRoleEquipCount)
	{
		int maxMailCount = DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE");
//		if(RoleLogicData.Self.mMailNum + (addPetCount - RoleLogicData.Self.GetFreeSpaceInPetBag ()) > maxMailCount 
//		   || RoleLogicData.Self.mMailNum + (addRoleEquipCount - RoleLogicData.Self.GetFreeSpaceInRoleEquipBag ()) > maxMailCount)
		//不考虑背包的剩余空间（背包数据不是实时同步）
		if(RoleLogicData.Self.mMailNum + addPetCount +addRoleEquipCount > maxMailCount) 
		{
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_FULL_MAIL);
			return true;
		}
		
		return false;
	}
}



class CS_RequestShopItemList : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData ("MYSTERIOUS_SHOP_WINDOW", "REFRESH", respEvt);
		}
	}
}


class CS_RefreshShopItemList : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			int costCount = get("COST_COUNT");
			GameCommon.RoleChangeDiamond (-costCount);
			DataCenter.OpenWindow ("MYSTERIOUS_SHOP_WINDOW");
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}
