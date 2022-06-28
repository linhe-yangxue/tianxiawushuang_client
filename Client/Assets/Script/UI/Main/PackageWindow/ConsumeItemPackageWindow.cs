using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System.Linq;
using System;
using DataTable;

public class ConsumeItemPackageWindow : ItemPackageBaseWindow {
	
	public ConsumeItemLogicData mConsumeItemLogicData;
	public SORT_TYPE mSortType;
	
	public ConsumeItemData mCurSelConsumeItemData = null;
	int mSaleNum = 1;
	bool mbSaleAll = true;
	bool mbNeedCostItem = false;
	bool mbShowTreasureContext = false;
	
	public override void Init ()
	{
		base.Init ();

		EventCenter.Self.RegisterEvent("Button_package_consume_item_star_btn", new DefineFactory<Button_PackageConsumeItemStarBtn>());
		EventCenter.Self.RegisterEvent("Button_package_consume_item_strengthen_level_btn", new DefineFactory<Button_PackageConsumeItemStrengthenLevelBtn>());
		EventCenter.Self.RegisterEvent("Button_package_consume_item_attribute_btn", new DefineFactory<Button_PackageConsumeItemAttributeBtn>());

		EventCenter.Self.RegisterEvent("Button_package_consume_item_icon_btn", new DefineFactory<Button_PackageConsumeItemIconBtn>());
		EventCenter.Self.RegisterEvent("Button_consume_item_package_sale_button", new DefineFactory<Button_ConsumeItemPackageSaleButton>());
		EventCenter.Self.RegisterEvent("Button_consume_item_package_use_button", new DefineFactory<Button_ConsumeItemPackageUseButton>());
		EventCenter.Self.RegisterEvent("Button_sale_consume_num_ok_button", new DefineFactory<Button_sale_consume_num_ok_button>());
		EventCenter.Self.RegisterEvent("Button_sale_consume_num_cancel_button", new DefineFactory<Button_sale_consume_num_cancel_button>());
		EventCenter.Self.RegisterEvent("Button_reduce_sale_consume_num_button", new DefineFactory<Button_reduce_sale_consume_num_button>());
		EventCenter.Self.RegisterEvent("Button_add_sale_consume_num_button", new DefineFactory<Button_add_sale_consume_num_button>());
		EventCenter.Self.RegisterEvent("Button_close_use_treasure_box_context_button", new DefineFactory<Button_close_use_treasure_box_context_button>());

		Net.gNetEventCenter.RegisterEvent ("CS_SellConsum", new DefineFactoryLog<CS_SellConsum>());
		Net.gNetEventCenter.RegisterEvent ("CS_UseConsum", new DefineFactoryLog<CS_UseConsum>());
	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SORT_CONSUME_ITEM_ICONS")
		{
//			SortConsumeItemIcons((SORT_TYPE)objVal);
		}
		else if(keyIndex == "SHOW_SALE")
		{
			ShowOrHiddenSaleContext (Convert.ToBoolean (objVal));
		}
		else if(keyIndex == "REDUCE_OR_ADD_SALE_NUM")
		{
			ReduceOrAddSaleNum (Convert.ToInt32 (objVal));
		}
		else if(keyIndex == "SALE")
		{
            string strValue = GetSub("Input").GetComponent<UIInput>().value;
            if (JudgeStringConvertInt(strValue))
            {
                int count = Convert.ToInt32(strValue);
                if (count > mCurSelConsumeItemData.itemNum)
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CONSUME_BRYOND_NUM_INPUT_AGAIN, true);
                else
                {
                    mSaleNum = count;
                    int iPrice = GetCurSelConsumeItemFieldData("ITEM_SELL_PRICE");
                    DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_CONSUME_SALE, (mSaleNum * iPrice).ToString(), () => SaleOK());
                }
            }
            else
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CONSUME_INPUT_NUM, true);
                mSaleNum = 1;
                SetSaleNum();
            }
		}
		else if(keyIndex == "USE_CONSUME_ITEM")
		{
			UseConsumeItem();
		}
		else if(keyIndex == "USE_CONSUME_ITEM_RESULT")
		{
			UseConsumeItemResult ((tEvent)objVal);
		}
		else if(keyIndex == "CLOSE_TREASURE_CONTEXT")
		{
			mbShowTreasureContext = false;
			ShowOrHiddenUseTreasureBoxContext ();
		}
	}
	
	public override void OnOpen ()
	{
		base.OnOpen ();
	}
	
	public override void InitVariable()
	{
		mConsumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;

		int count =  mConsumeItemLogicData.mConsumeItemList.Count;
		for(int i = 0; i < count; i++)
		{
			ConsumeItemData consumeItemData = mConsumeItemLogicData.mConsumeItemList[i];
			GameCommon.SplitConsumeData(ref consumeItemData, mConsumeItemLogicData);
		}

		GameCommon.AddConsumeData (1001, RoleLogicData.Self.mResetNum, mConsumeItemLogicData);
		GameCommon.AddConsumeData (1002, RoleLogicData.Self.mSweepNum, mConsumeItemLogicData);
		GameCommon.AddConsumeData (1003, RoleLogicData.Self.mLockNum, mConsumeItemLogicData);

		mConsumeItemLogicData.mConsumeItemList = GameCommon.SortList (mConsumeItemLogicData.mConsumeItemList, SortList);

		mSortType = SORT_TYPE.STAR_LEVEL;
		InitPackageIcons();
		RefreshBagNum();

		if(mbSaleAll)
			Refresh(0);
		else 
			Refresh (mCurSelGridIndex);

		mSaleNum = 1;
		ShowOrHiddenSaleContext (false);

		ShowOrHiddenUseTreasureBoxContext ();
	}
	
	int SortList(ConsumeItemData a, ConsumeItemData b)
	{
		SortListParam sUseable = new SortListParam{mParam1 = TableCommon.GetNumberFromConsumeConfig (a.tid, "USEABLE"),
			mParam2 = TableCommon.GetNumberFromConsumeConfig (b.tid, "USEABLE"), bIsDescending = true};
		SortListParam sIndex = new SortListParam{mParam1 = a.tid, mParam2 = b.tid, bIsDescending = false};
		SortListParam sCount = new SortListParam{mParam1 = a.itemNum, mParam2 = b.itemNum, bIsDescending = false};

		return GameCommon.Sort (sUseable, sIndex, sCount);
	}

	public override bool Refresh (object param)
	{
		base.Refresh (param);
		mCurSelGridIndex = Convert.ToInt32 (param);
		mCurSelConsumeItemData = mConsumeItemLogicData.GetDataByGridIndex (mCurSelGridIndex);
		RefreshInfoWindow();
		return true;
	}

	public override void InitPackageIcons()
	{
		int count = mConsumeItemLogicData.mConsumeItemList.Count;
		int iEmptyCount = 0 /*(3 - count % 3)%3*/;
        mGrid.MaxCount = count + iEmptyCount;
        for (int i = 0; i < mGrid.MaxCount; i++)
        {
            GameObject obj = mGrid.controlList[i];
            if (i < count)
            {
                mConsumeItemLogicData.mConsumeItemList[i].mGridIndex = i;
                ConsumeItemData data = mConsumeItemLogicData.mConsumeItemList[i];
                SetConsumeIcon(obj, data);
                GameCommon.SetUIText(obj, "num_label", data.itemNum.ToString());

                GameCommon.GetButtonData(obj, "package_consume_item_icon_btn").set("GRID_INDEX", i);

                if (i == 0)
                    mCurSelConsumeItemData = data;
            }
            else
            {
                GameObject background = GameCommon.FindObject(obj, "icon_sprite");
                GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");
                background.SetActive(false);
                emptyBG.SetActive(true);

                UIToggle toggle = obj.GetComponent<UIToggle>();
                if (toggle != null)
                    toggle.value = false;
            }
		}
	}

	public override void RefreshBagNum()
	{
		UILabel ConsumeItemNumLabel = mGameObjUI.transform.Find("num_label").GetComponent<UILabel>();
		if(ConsumeItemNumLabel != null)
		{
			ConsumeItemNumLabel.text = mConsumeItemLogicData.mConsumeItemList.Count.ToString () ; //+ " / " + RoleLogicData.Self.mMaxConsumeItemNum.ToString();
		}
	}
	
	public override void RefreshInfoWindow()
	{
		GameObject franmentGroup = mGameObjUI.transform.Find("info_window/group").gameObject;
		GameObject franmentEmptyGroup = mGameObjUI.transform.Find("info_window/empty_bg").gameObject;
		if (mCurSelConsumeItemData == null)
		{
			franmentGroup.SetActive(false);
			franmentEmptyGroup.SetActive(true);
		}
		else
		{
			franmentGroup.SetActive(true);
			franmentEmptyGroup.SetActive(false);
			SetConsumeIcon(franmentGroup, mCurSelConsumeItemData);
			UILabel name = GameCommon.FindObject(franmentGroup, "props_name").GetComponent<UILabel>();
			UILabel descrition = GameCommon.FindObject(franmentGroup, "introduce_label").GetComponent<UILabel>();

			name.text = TableCommon.GetStringFromConsumeConfig(mCurSelConsumeItemData.tid, "ITEM_NAME");
			descrition.text = TableCommon.GetStringFromConsumeConfig(mCurSelConsumeItemData.tid, "DESCRIBE");

			GameCommon.GetUIButton (mGameObjUI, "consume_item_package_use_button").isEnabled = 
				JudgeConsumeItemIsUseable(mCurSelConsumeItemData.tid );
		}
	}
	
//	public void SortConsumeItemIcons(SORT_TYPE sortType)
//	{
//		mSortType = sortType;
//		
//		InitPackageIcons();
//	}
	
	public void SaleOK()
	{
		if(mCurSelConsumeItemData == null)
			return;
		
		if(CommonParam.bIsNetworkGame)
		{
            SetSaleNum();

            CS_SellConsum quest = Net.StartEvent("CS_SellConsum") as CS_SellConsum;
            quest.set("CONSUM_ID", mCurSelConsumeItemData.tid);
            quest.set("COUNT", mSaleNum);
            quest.mAction = () => SaleResult();
            quest.DoEvent();
		}
		else
			SaleResult();
	}
	
	public void SaleResult()
	{
		if(mConsumeItemLogicData.RemoveConsumeItemData (mCurSelGridIndex, mSaleNum))
		{
			int iPrice = GetCurSelConsumeItemFieldData ( "ITEM_SELL_PRICE");
			GameCommon.RoleChangeGold (mSaleNum * iPrice);
			mbSaleAll = mSaleNum == mCurSelConsumeItemData.itemNum ? true : false;

			DataCenter.OpenWindow ("CONSUME_ITEM_PACKAGE_WINDOW");
		}
	}

	public void UseConsumeItem()
	{
		int index = mCurSelConsumeItemData.tid;
		int count = mCurSelConsumeItemData.itemNum;

		int costTypeIndex = GetCostTypeIndex ();
		if(costTypeIndex != 0)
		{
			int costCount = GetCostCount ();
			if(JudgeHaveEnoughCostItem (costTypeIndex, costCount))
			{
				mbNeedCostItem = true;
				CS_UseConsum quest = Net.StartEvent("CS_UseConsum") as CS_UseConsum;
				quest.set("CONSUM_ID", mCurSelConsumeItemData.tid);
				quest.DoEvent();
			}
			else
				ShowNotEnoughCostItemMessage(costTypeIndex);
		}
		else
		{
			mbNeedCostItem = false;
			CS_UseConsum quest = Net.StartEvent("CS_UseConsum") as CS_UseConsum;
			quest.set("CONSUM_ID", mCurSelConsumeItemData.tid);
			quest.DoEvent();
		}
	}

	public void UseConsumeItemResult(tEvent respEvt)
	{
		int type = GetCurSelConsumeItemFieldData ( "ITEM_TYPE");
		int time = GetCurSelConsumeItemFieldData( "ITEM_EFFECT_TIME");

		if(type == (int)CONSUME_ITEM_TYPE.BUFFER)
		{
			int bufferID = GetCurSelConsumeItemFieldData( "ITEM_BUFF");
			string buffDes = TableCommon.GetStringFromAffectBuffer (bufferID, "NAME");

			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CONSUME_GET_BUFF_RESULT, buffDes, (time/60).ToString (), true);

			ConsumeItemLogicStatus logic = DataCenter.GetData ("CONSUME_ITEM_STATUS") as ConsumeItemLogicStatus;
			logic.AddConsumeItemStatus (mCurSelConsumeItemData.tid);
		}
		else if(type == (int)CONSUME_ITEM_TYPE.TREASURE_BOX)
		{
			object resultData;
			if (!respEvt.getData("RESULT_TABLE", out resultData))
				return;
			NiceTable dataTable = resultData as NiceTable;

			int num = 0;
			mbShowTreasureContext = true;
			ShowOrHiddenUseTreasureBoxContext ();
			UIGridContainer grid = GameCommon.FindObject (GetSub ("use_treasure_box_context"), "grid").GetComponent<UIGridContainer>();
            grid.MaxCount = 0;
            grid.MaxCount = dataTable.GetRecordCount ();
			foreach(KeyValuePair<int, DataRecord> re in dataTable.GetAllRecord ())
			{
				DataRecord record = re.Value;
				SetTreasureItem (record, grid.controlList[num]);
				GainItemFromTreasure (record);
				num++;
			}
		}
		else if(type == (int)CONSUME_ITEM_TYPE.CHANGE_MODEL)
		{
			int changeModelID = GetCurSelConsumeItemFieldData( "ITEM_PET_ID");
			string changeModelDes = TableCommon.GetStringFromActiveCongfig (changeModelID, "NAME");

			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CONSUME_CHANGE_MODEL_RESULT, changeModelDes, (time/60).ToString (), true);

			ConsumeItemLogicStatus logic = DataCenter.GetData ("CONSUME_ITEM_STATUS") as ConsumeItemLogicStatus;
			logic.AddConsumeItemStatus (mCurSelConsumeItemData.tid);
		}
		else if(type == (int)CONSUME_ITEM_TYPE.NUMERICAL)
		{

		}

		mbSaleAll = mCurSelConsumeItemData.itemNum == 1 ? true : false;
		mConsumeItemLogicData.RemoveConsumeItemData (mCurSelGridIndex, 1);
		if(mbNeedCostItem)
			GameCommon.RoleChangeFunctionalProp (GetCostTypeIndex (), -GetCostCount ());
		
		DataCenter.OpenWindow ("CONSUME_ITEM_PACKAGE_WINDOW");
	}

	public void SetTreasureItem(DataRecord dataRecord, GameObject obj)
	{
		int iItemType = dataRecord.getData("ITEM_TYPE");
		int iItemID = dataRecord.getData("ITEM_ID");
		int iItemCount = dataRecord.getData("ITEM_COUNT");
		int iRoleEquipElement = dataRecord.getData ("ELEMENT");

		GameCommon.SetUIText (obj, "num_label", "x" + iItemCount.ToString ());
		UISprite itemSprite = GameCommon.FindObject (obj , "item_icon").GetComponent<UISprite>();
		GameCommon.SetUIVisiable (obj, "star_level_label", false);
		GameCommon.SetUIVisiable (obj, "role_equip_element", false);
		GameCommon.SetUIVisiable (obj, "element", false);
		GameCommon.SetUIVisiable (obj, "fragment_sprite", false);
        GameCommon.SetUIVisiable(obj, "material_fragment_sprite", iItemType == (int)ITEM_TYPE.MATERIAL_FRAGMENT);

		switch(iItemType)
		{
		case (int)ITEM_TYPE.PET:
			//GameCommon.SetUIVisiable (obj, "star_level_label", true);
			GameCommon.SetUIVisiable (obj, "element", true);
			GameCommon.SetPetIconWithElementAndStar (obj, "item_icon", "element", "star_level_label", iItemID);
			break;
		case (int)ITEM_TYPE.PET_FRAGMENT:
			//GameCommon.SetUIVisiable (obj, "star_level_label", true);
			GameCommon.SetUIVisiable (obj, "element", true);
			GameCommon.SetUIVisiable (obj, "fragment_sprite", true);
			iItemID = TableCommon.GetNumberFromFragment(iItemID, "ITEM_ID");
			int elementIndex = TableCommon.GetNumberFromActiveCongfig(iItemID, "ELEMENT_INDEX");
			GameCommon.SetElementFragmentIcon (obj, "fragment_sprite", elementIndex);
			GameCommon.SetPetIconWithElementAndStar (obj, "item_icon", "element", "star_level_label", iItemID);
			break;
		case (int)ITEM_TYPE.EQUIP:
			//GameCommon.SetUIVisiable (obj, "star_level_label", true);
			GameCommon.SetUIVisiable (obj, "role_equip_element", true);
			GameCommon.SetEquipElementBgIcons(obj, "small_element", "role_equip_element", iItemID, iRoleEquipElement);
			GameCommon.SetEquipIcon (itemSprite, iItemID);
			iItemCount = TableCommon.GetNumberFromRoleEquipConfig (iItemID, "STAR_LEVEL");
			//GameCommon.SetUIText (obj, "star_level_label", iItemCount.ToString ());
            GameCommon.SetStarLevelLabel(obj, iItemCount, "star_level_label");
			break;
		default :
			GameCommon.SetItemIcon (itemSprite, iItemType, iItemID);
			break;
		}
	}

	void GainItemFromTreasure(DataRecord dataRecord)
	{
		int iItemType = dataRecord.getData("ITEM_TYPE");
		int iItemID = dataRecord.getData("ITEM_ID");
		int iItemCount = dataRecord.getData("ITEM_COUNT");
		int iRoleEquipElement = dataRecord.getData ("ELEMENT");

        ItemDataBase itemData = new ItemDataBase();
        itemData.itemId = dataRecord.getData("DBID");
        itemData.tid = dataRecord.getData("ITEM_ID");
        itemData.itemNum = dataRecord.getData("ITEM_COUNT");
        PackageManager.AddItem(itemData);
        //switch(iItemType)
        //{
        //case (int)ITEM_TYPE.SILVER:
        //case (int)ITEM_TYPE.YUANBAO:
        //case (int)ITEM_TYPE.FRIEND_POINT:
        //case (int)ITEM_TYPE.POWER:
        //case (int)ITEM_TYPE.SAODANG_POINT:
        //case (int)ITEM_TYPE.RESET_POINT:
        //case (int)ITEM_TYPE.LOCK_POINT:
        //    GameCommon.RoleChangeNumericalAboutRole (iItemType, iItemCount);
        //    break;
        //case (int)ITEM_TYPE.PET_FRAGMENT:
        //    PetFragmentLogicData.Self.SetDataByItemID (iItemID, iItemCount);
        //    break;
        //case (int)ITEM_TYPE.EQUIP:
        //    RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
        //    NiceTable t = new NiceTable();
        //    t.SetField ("ID", FIELD_TYPE.FIELD_INT, 0);
        //    t.SetField ("ITEM_ID", FIELD_TYPE.FIELD_INT, 1);
        //    t.SetField ("ITEM_ELEMENT", FIELD_TYPE.FIELD_INT, 2);
        //    t.SetField ("NEW_SIGN", FIELD_TYPE.FIELD_BOOL, 3);
        //    DataRecord re = t.CreateRecord(0);
        //    re.set ("ID",(int)dataRecord.getData ("DBID"));
        //    re.set ("ITEM_ID", iItemID);
        //    re.set ("ITEM_ELEMENT", iRoleEquipElement);
        //    re.set ("ITEM_ID", true);
        //    roleEquipLogicData.AttachRoleEquip (re);
        //    break;
        //case(int)ITEM_TYPE.CONSUME_ITEM:
        //    ConsumeItemLogicData tempConsumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        //    tempConsumeItemLogicData.SetDataByItemID (iItemID, iItemCount);
        //    break;
        //case(int)ITEM_TYPE.GEM:
        //    GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
        //    gemLogicData.SetGetDataByType (iItemID, iItemCount);
        //    break;
        //case (int)ITEM_TYPE.MATERIAL:
        //    MaterialLogicData materiallogicData = DataCenter.GetData("MATERIAL_DATA") as MaterialLogicData;
        //    materiallogicData.ChangeMaterialDataNum(iItemID, iItemCount);
        //    break;
        //case (int)ITEM_TYPE.MATERIAL_FRAGMENT:
        //    MaterialFragmentLogicData materialFragmentlogicData = DataCenter.GetData("MATERIAL_FRAGMENT_DATA") as MaterialFragmentLogicData;
        //    materialFragmentlogicData.ChangeMaterialFragmentDataNum(iItemID, iItemCount);
        //    break;
        //}
	}

	void SetConsumeIcon (GameObject obj, ConsumeItemData data)
	{
		string strAtlas = TableCommon.GetStringFromConsumeConfig (data.tid, "ITEM_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromConsumeConfig (data.tid, "ITEM_SPRITE_NAME");
		GameCommon.SetIcon (obj, "icon_sprite", strSpriteName, strAtlas);
	}
	
	void ShowOrHiddenUseTreasureBoxContext()
	{
		SetVisible ("use_treasure_box_context", mbShowTreasureContext);
		SetVisible ("black_background", mbShowTreasureContext);
	}
	
	void ShowOrHiddenSaleContext(bool bVisible)
	{
		SetVisible ("sale_context", bVisible);
		SetVisible ("sale_context_black_background", bVisible);
		if(bVisible)
		{
			mSaleNum = 1;
			SetSaleNum();
		}
	}
	
	void ReduceOrAddSaleNum(int num)
	{
		string strValue = GetSub ("Input").GetComponent<UIInput>().value;
		if(JudgeStringConvertInt (strValue))
			mSaleNum = Convert.ToInt32 (strValue);
		else 
			mSaleNum = 0;
	
		mSaleNum += num;
		
		if(mSaleNum < 1)
		{
			mSaleNum = 1;
			return;
		}
		
		if(mSaleNum > mCurSelConsumeItemData.itemNum && num > 0)
		{
			mSaleNum = mCurSelConsumeItemData.itemNum;
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_CONSUME_BRYOND_NUM , true);
			return;
		}
		
		SetSaleNum ();
	}
	
	void SetSaleNum()
	{
		GetSub ("Input").GetComponent<UIInput>().value = mSaleNum.ToString ();
	}
	
	bool JudgeStringConvertInt(string str)
	{
		try{
			Convert.ToInt32 (str);
			return true;
		}
		catch{
			return false;
		}
	}

	int GetCurSelConsumeItemFieldData (string strField)
	{
		return TableCommon.GetNumberFromConsumeConfig (mCurSelConsumeItemData.tid, strField);
	}

	bool JudgeConsumeItemIsUseable(int index)
	{
		return TableCommon.GetNumberFromConsumeConfig (index, "USEABLE") == 1 ? true : false;
	}

	int GetCostTypeIndex()
	{
		return GetCurSelConsumeItemFieldData ("COST_ITEM_ID");
	}

	int GetCostCount()
	{
		return GetCurSelConsumeItemFieldData ("COST_ITEM_NUM");
	}

	bool JudgeHaveEnoughCostItem(int costIndex, int costCount)
	{
		if(costIndex == 1001)
			return costCount >= RoleLogicData.Self.mResetNum ? false : true;
		else if(costIndex == 1002)
			return costCount >= RoleLogicData.Self.mSweepNum ? false : true;
		else if(costIndex == 1003)
			return costCount >= RoleLogicData.Self.mLockNum ? false : true;

		return false;
	}

	void ShowNotEnoughCostItemMessage(int costIndex)
	{
		if(costIndex == 1001)
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_CONSUME_NOT_ENOUGH_RESET_POINT, true);
		else if(costIndex == 1002)
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_CONSUME_NOT_ENOUGH_SAODANG_POINT, true);
		else if(costIndex == 1003)
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_CONSUME_NOT_ENOUGH_LOCK_POINT, true);
	}
}

public class Button_PackageConsumeItemStarBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.STAR_LEVEL);
		return true;
	}
}

public class Button_PackageConsumeItemStrengthenLevelBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.STRENGTHEN_LEVEL);
		return true;
	}
}

public class Button_PackageConsumeItemAttributeBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.ELEMENT_INDEX);
		return true;
	}
}

public class Button_PackageConsumeItemIconBtn : CEvent
{
	public override bool _DoEvent()
	{
		int gridIndex = Convert.ToInt32 (getObject ("GRID_INDEX"));
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "REFRESH", gridIndex);
		return true;
	}
}

public class Button_ConsumeItemPackageSaleButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "SHOW_SALE", true);
		return true;
	}
}

public class Button_ConsumeItemPackageUseButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("CONSUME_ITEM_PACKAGE_WINDOW", "USE_CONSUME_ITEM", true);

		return true;
	}
}

public class Button_sale_consume_num_ok_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "SALE", true);
		return true;
	}
}

public class Button_sale_consume_num_cancel_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "SHOW_SALE", false);
		return true;
	}
}

public class Button_reduce_sale_consume_num_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "REDUCE_OR_ADD_SALE_NUM", -1);
		return true;
	}
}

public class Button_add_sale_consume_num_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "REDUCE_OR_ADD_SALE_NUM", 1);
		return true;
	}
}

public class Button_close_use_treasure_box_context_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("CONSUME_ITEM_PACKAGE_WINDOW", "CLOSE_TREASURE_CONTEXT", true);
		return true;
	}
}

public class CS_SellConsum : BaseNetEvent
{
	public Action mAction = null;
	public override bool _DoEvent ()
	{
		return true;
	}

	public override void _OnResp (tEvent respEvent)
	{
		if(respEvent == null)
			return;

		int bResult = respEvent.get ("RESULT");
		if((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			mAction();
			mAction = null;
		}
		else 
			DataCenter.OpenMessageWindow ((STRING_INDEX)bResult);
	}
}

public class CS_UseConsum : BaseNetEvent
{
	public override bool _DoEvent ()
	{
		return true;
	}
	public override void _OnResp (tEvent respEvent)
	{
		if(respEvent == null)
			return;

		int bResult = respEvent.get ("RESULT");
		if((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData ("CONSUME_ITEM_PACKAGE_WINDOW", "USE_CONSUME_ITEM_RESULT", respEvent);
		}
	}
}

