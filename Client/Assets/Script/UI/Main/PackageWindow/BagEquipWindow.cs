using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System.Linq;
using System;
using DataTable;

public class BagEquipBaseWindow : BagBaseWindow<EquipData>
{
    protected override void SetShowItemDataList()
    {
        EquipLogicData logicData = GetLogicData() as EquipLogicData;
        if (logicData != null){
            mShowItemDataList = FilterAndSortItemList(logicData.mDicEquip.Values.ToList());
		}
    }

    protected override void SetOkBtnAction(Action action)
    {
        base.SetOkBtnAction(action);
    }

    protected override bool SetItemIcon(GameObject obj, EquipData itemData)
    {
        if (null == obj || null == itemData)
            return false;

        // 设置图标
        GameCommon.SetItemIcon(obj, "item_icon", itemData.tid);

        // 设置名称
        GameCommon.SetUIText(obj, "name_label", TableCommon.GetStringFromRoleEquipConfig(itemData.tid, "NAME"));

        // 设置精炼等级
        GameCommon.SetUIText(obj, "refine_level_label", GameCommon.ShowAddNumUI(itemData.refineLevel));

        // 设置强化等级
        GameCommon.SetUIText(obj, "level_label", itemData.strengthenLevel.ToString());

        // 设置产生的经验
        int iDropExp = TableCommon.GetNumberFromRoleEquipConfig(itemData.tid, "SUPPLY_EXP");
        int iAddExp = TableCommon.GetNumberFromMagicEquipLvConfig(itemData.strengthenLevel, "TOTAL_EXP_" + (int)itemData.mQualityType);
        GameCommon.SetUIText(obj, "exp_number_label", (iDropExp + iAddExp).ToString());

        // 获取缘分数量
        int iCount = GetRelateNewActivateCount(itemData.tid);
        // 设置缘分
        GameCommon.FindObject(obj, "relate_label").SetActive(iCount > 0);
        GameCommon.SetUIVisiable(obj, "relate_number_label", iCount > 0);
        GameCommon.SetUIText(obj, "relate_number_label", iCount.ToString());

        return true;
    }

    private int GetRelateNewActivateCount(int upTid)
    {
        int teamPos = (int)TeamManager.mCurTeamPos;
        ITEM_TYPE type = PackageManager.GetItemTypeByTableID(upTid);
        int slotPos = PackageManager.GetSlotPosByTid(upTid);
        var equip = type == ITEM_TYPE.EQUIP ? RoleEquipLogicData.Self.GetEquipDataByTeamPosAndType(teamPos, slotPos) : MagicLogicData.Self.GetEquipDataByTeamPosAndType(teamPos, slotPos);
        var oldContext = Relationship.GetCachedContextTidSet(teamPos);
        var newContext = Relationship.AlterContextTidSet(oldContext, upTid, equip == null ? 0 : equip.tid);
        var relateSet = Relationship.GetCachedRelateTidSet(teamPos);
        var newActive = Relationship.FilterRelateByAlteration(oldContext, newContext, relateSet, RelateAlteration.Inactive2Active);
        return newActive.Count;
    }
}

public class BagEquipWindow : BagEquipBaseWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_chose_bag_equip_window_black_btn", new DefineFactory<Button_equip_chose_quality_close_button>());
        EventCenter.Self.RegisterEvent("Button_bag_equip_window_black_btn", new DefineFactory<EquipBagCloseButton>());
        EventCenter.Self.RegisterEvent("Button_equip_bag_close_button", new DefineFactory<EquipBagCloseButton>());
        EventCenter.Self.RegisterEvent("Button_equip_bag_ok_button", new DefineFactory<EquipBagOkButton>());

        EventCenter.Self.RegisterEvent("Button_bag_equip_single_btn", new DefineFactory<Button_BagEquipSingleBtn>());
        EventCenter.Self.RegisterEvent("Button_bag_equip_multiple_icon_btn", new DefineFactory<Button_BagEquipMultipleIconBtn>());

		EventCenter.Self.RegisterEvent("Button_bag_equip_sell_icon_btn", new DefineFactory<Button_bag_equip_sell_icon_btn>());
		EventCenter.Self.RegisterEvent("Button_equip_sell_ok_button", new DefineFactory<Button_equip_sell_ok_button>());
		EventCenter.Self.RegisterEvent("Button_equip_quality_chose_button", new DefineFactory<Button_equip_quality_chose_button>());
		EventCenter.Self.RegisterEvent("Button_equip_bag_ok_button", new DefineFactory<EquipBagOkButton>());
		EventCenter.Self.RegisterEvent("Button_equip_chose_quality_close_button", new DefineFactory<Button_equip_chose_quality_close_button>());
		EventCenter.Self.RegisterEvent("Button_equip_chose_quality_ok_button", new DefineFactory<Button_equip_chose_quality_close_button>());
		EventCenter.Self.RegisterEvent("Button_equip_quality_btn", new DefineFactory<Button_equip_quality_btn>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        if ("ADD_SELECT_ITEM" == keyIndex)
        {
            if (mCurBagShowType == BAG_SHOW_TYPE.RESOLVE)
            {
                if (mSelItemDataList.Count >= mMaxNum)
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_UPGRADE_MAX_NUM, mMaxNum.ToString());

                    GameObject obj = getObject("BUTTON") as GameObject;
                    if (obj != null)
                    {
                        UIToggle toggle = obj.GetComponent<UIToggle>();
                        toggle.value = false;
                    }
                    return;
                }
            }
            base.onChange("DO_ADD_SELECT_ITEM", objVal);            
        }
		else if("CLOSE_QUALITY_MESSAGE" == keyIndex)
		{
			GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "chose_quality_info");
			choseQualityInfoObj.SetActive (false);
		}
		else if("EQUIP_CHOSE_QUALITY_INFOS" == keyIndex)
		{
			SetChoseQuality();
		}
		else if("EQUIP_UPDATE_SELL_REWARDS" == keyIndex)
		{
			SetGetMoney();
		}
		else if("ADD_SELECT_QUALITY" == keyIndex)
		{
            //by chenliang
            //begin

// 			int i = (int)objVal;
// 			UpdateQualityItemInfosUI("sell_group", UpdateQualitySellInfo, i);
//---------------------
            int tmpQuality = (int)objVal;
            int tmpItemsCount = 0;
            _AddSelectItemsByQuality(tmpQuality, out tmpItemsCount);
            if (tmpItemsCount == 0)
            {
                DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的装备");
                GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "chose_quality_info");
                UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
                _grid.MaxCount = 2;

                for (int i = 0; i < _grid.MaxCount; i++)
                {
                    GameObject obj = _grid.controlList[i];
                    UIToggle objToggle = GameCommon.FindObject(obj, "equip_quality_btn").GetComponent<UIToggle>();
                    if (i + 2 == tmpQuality)
                        objToggle.value = false;
                }
            }
            else
            {
                UpdateItemInfosUI("sell_group", UpdateSellInfo);
                UpdateNumLabel();
                SetGetMoney();
            }

            //end
		}else if("REMOVE_SELECT_QUALITY" == keyIndex)
		{
            //by chenliang
            //begin

// 			int i = (int)objVal;
// 			UpdateQualityItemInfosUI("sell_group", RemoveQualitySellInfo, i);
//---------------------
            int tmpQuality = (int)objVal;
            int tmpItemsCount = 0;
            _RemoveSelectItemsByQuality(tmpQuality, out tmpItemsCount);
            if (tmpItemsCount == 0)
            {
                DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的装备");
                GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "chose_quality_info");
                UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
                _grid.MaxCount = 2;

                for (int i = 0; i < _grid.MaxCount; i++)
                {
                    GameObject obj = _grid.controlList[i];
                    UIToggle objToggle = GameCommon.FindObject(obj, "equip_quality_btn").GetComponent<UIToggle>();
                    if (i + 2 == tmpQuality)
                        objToggle.value = false;
                }
            }
            else
            {
                UpdateItemInfosUI("sell_group", UpdateSellInfo);
                UpdateNumLabel();
                SetGetMoney();
            }

            //end
		}
    }
	void RemoveQualitySellInfo(GameObject parentObj, EquipData itemData)
	{
		if (null == parentObj || null == itemData)
			return;		
		GameObject obj = GetSellIconBtn(parentObj);
		if (null == obj)
			return;
		RemoveSelectItem(itemData);
		UpdateNumLabel();
		SetGetMoney();
		SetToggle(obj, itemData);		
	}
	void UpdateQualitySellInfo(GameObject parentObj, EquipData itemData)
	{
		if (null == parentObj || null == itemData)
			return;
		
		GameObject obj = GetSellIconBtn(parentObj);
		if (null == obj)
			return;
		AddSelectItem(itemData);
		UpdateNumLabel();
		SetGetMoney();
		SetToggle(obj, itemData);		
	}
	void UpdateQualityItemInfosUI(string strParentObjName, UpdateItemIcon callBack, int iQualityIndex)
	{
		GameObject parentObj = GetSub(strParentObjName);
		if (null == parentObj)
			return;
		
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(parentObj, "grid");
		if (grid != null)
		{
			grid.MaxCount = mShowItemDataList.Count;			
			int iIndex = 0;
			int iMaxCount = 0;
			for (int i = 0; i < mShowItemDataList.Count; i++)
			{
				EquipData itemData = mShowItemDataList[i];
				if (itemData != null)
				{
					GameObject obj = grid.controlList[iIndex];
					int starLevel=TableCommon.GetNumberFromRoleEquipConfig(itemData.tid,"QUALITY");
					if(starLevel == iQualityIndex)
					{
						callBack(obj, itemData);
						iMaxCount++;
					}
					iIndex++;
				}
			}
			if(iMaxCount == 0)
			{
				DataCenter.ErrorTipsLabelMessage ("您没有任何相应品质的装备");
				GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "chose_quality_info");
				UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
				_grid.MaxCount = 2;
				
				for(int i = 0; i < _grid.MaxCount; i++)
				{
					GameObject obj = _grid.controlList[i];
					UIToggle objToggle = GameCommon.FindObject (obj, "equip_quality_btn").GetComponent<UIToggle>();
					if(i + 2 == iQualityIndex)
						objToggle.value = false;
				}
			}
		}
	}

	void SetGetMoney()
	{
		int total = 0;
		int tid = (int)ITEM_TYPE.GOLD;
		mSelItemDataList.ForEach(equip =>
		             {
			string sellPrice = (string)DataCenter.mRoleEquipConfig.GetRecord(equip.tid).getData("SELL_PRICE");
			var itemData = GameCommon.ParseItemList(sellPrice)[0];
			tid = itemData.tid;
			total += equip.strengCostGold + itemData.itemNum;
		});

		if(mSelItemDataList.Count == 0)
			total = 0;
		ItemDataBase _item = new ItemDataBase();
		_item.itemNum = total;
		_item.tid = tid;	
		GameObject sellGroup = GameCommon.FindObject (mGameObjUI, "sell_group");
		GameCommon.SetResIcon (sellGroup, "sell_rewards_icon", _item.tid, false, true);
		GameCommon.SetUIText(sellGroup, "sell_rewards_num", "x" + _item.itemNum);
	}
	void SetChoseQuality()
	{
		GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "chose_quality_info");
		choseQualityInfoObj.SetActive (true);
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
		grid.MaxCount = 2;
		
		for(int i = 0; i < grid.MaxCount; i++)
		{
			GameObject obj = grid.controlList[i];
			UILabel qualityLabel = GameCommon.FindObject (obj, "name_label").GetComponent<UILabel>();
			qualityLabel.text = GameCommon.SetQualityName(i + 2);
			qualityLabel.color = GameCommon.SetQualityColor (i + 2);		
			GameCommon.GetButtonData(GameCommon.FindObject (obj, "equip_quality_btn").gameObject).set("EQUIP_QUALITY_INDEX", i + 2);
			GameCommon.FindObject (obj, "equip_quality_btn").gameObject.GetComponent<UIToggle>().value = false;
		}
	}
    protected override tLogicData GetLogicData()
    {
        return RoleEquipLogicData.Self;
    }

    protected override GameObject GetSingleBtn(GameObject parentObj)
    {
        if (parentObj != null)
        {
            return parentObj.transform.Find("bag_equip_single_btn").gameObject;
        }
        return null;
    }

    protected override GameObject GetMultipleIconBtn(GameObject parentObj)
    {
        if (parentObj != null)
        {
            return parentObj.transform.Find("bag_equip_multiple_icon_btn").gameObject;
        }
        return null;
    }
	protected override GameObject GetSellIconBtn(GameObject parentObj)
	{
		if (parentObj != null)
		{
			return parentObj.transform.Find("bag_equip_sell_icon_btn").gameObject;
		}
		return null;
	}

    protected override void UpdateUI()
    {
        GameObject useGroup = GameCommon.FindObject(mGameObjUI, "use_group");
        GameObject resolveGroup = GameCommon.FindObject(mGameObjUI, "resolve_group");
        GameObject okBtn = GameCommon.FindObject(mGameObjUI, "equip_bag_ok_button");
		GameObject sellGroup = GameCommon.FindObject(mGameObjUI, "sell_group");
		GameObject noButtonSprite = GameCommon.FindObject (mGameObjUI, "no_button_sprite");
		GameObject haveButtonSprite = GameCommon.FindObject (mGameObjUI, "have_button_sprite");

        if (null == useGroup || null == resolveGroup || null == sellGroup)
            return;

        if (null == okBtn)
            return;

        useGroup.SetActive(mCurSelectType == SELECT_TYPE.SINGLE_SELECT);
        resolveGroup.SetActive(mCurSelectType == SELECT_TYPE.MULTIPLE_SELECT);

//        okBtn.SetActive(mCurSelectType == SELECT_TYPE.MULTIPLE_SELECT);
		okBtn.SetActive(mCurBagShowType == BAG_SHOW_TYPE.UPGRADE || mCurBagShowType == BAG_SHOW_TYPE.RESOLVE);
		sellGroup.SetActive (mCurBagShowType == BAG_SHOW_TYPE.SELL);
		if(mCurBagShowType == BAG_SHOW_TYPE.USE || mCurBagShowType == BAG_SHOW_TYPE.FAIRYLAND)
		{
			noButtonSprite.SetActive (true);
			haveButtonSprite.SetActive (false);
		}else
		{
			noButtonSprite.SetActive (false);
			haveButtonSprite.SetActive (true);
		}

        UpdateUIByShowType();
        UIScrollView _resolveView = GameCommon.FindComponent<UIScrollView>(resolveGroup, "ScrollView");
        if (_resolveView != null)
            _resolveView.ResetPosition();
		UIScrollView _useView = GameCommon.FindComponent<UIScrollView>(useGroup, "ScrollView");
		if (_useView != null)
			_useView.ResetPosition();
    }

    protected override void UpdateUIByShowType()
    {
        switch (mCurBagShowType)
        {
            case BAG_SHOW_TYPE.USE:
                UpdateItemInfosUI("use_group", UpdateSingleInfo);
                //by chenliang
                //begin

// 				UIGridContainer container = GetComponent<UIGridContainer>("grid");
// 				if (container.MaxCount == 0){
// 				string str=DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_EQUIP_INFORMATION_TIPS,"STRING_CN");
// 				GameObject .Find ("Label_noequip_information_tips").GetComponent <UILabel >().text =str;
// 				}
// 				else{
// 					GameObject .Find ("Label_noequip_information_tips").GetComponent <UILabel >().text ="";
// 				}
//-------------------------
				if (mShowItemDataList != null && mShowItemDataList.Count == 0){
				string str=DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_EQUIP_INFORMATION_TIPS,"STRING_CN");
				GameObject .Find ("Label_noequip_information_tips").GetComponent <UILabel >().text =str;
				}
				else{
					GameObject .Find ("Label_noequip_information_tips").GetComponent <UILabel >().text ="";
				}

                //end
                break;
            case BAG_SHOW_TYPE.RESOLVE:
				UpdateItemInfosUI("resolve_group", UpdateMultipleInfo);
				UILabel num=GetComponent <UILabel >("num_label");
				if(num.text =="0/0"){
				string str=DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_EQUIP_INFORMATION_TIPS,"STRING_CN");
				GameObject .Find ("Label_noequip_information_tips").GetComponent <UILabel >().text =str;
				}
				else{
				GameObject .Find ("Label_noequip_information_tips").GetComponent <UILabel >().text ="";
				}
			break;
		case BAG_SHOW_TYPE.SELL:
			UpdateItemInfosUI("sell_group", UpdateSellInfo);
			UILabel numLabel=GetComponent <UILabel >("num_label");
			if(numLabel.text =="0/0"){
				string str=DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_EQUIP_INFORMATION_TIPS,"STRING_CN");
				GameObject .Find ("Label_noequip_information_tips").GetComponent <UILabel >().text =str;
			}
			else{
				GameObject .Find ("Label_noequip_information_tips").GetComponent <UILabel >().text ="";
			}
			break;
        }
    }

    protected override void SetOkBtnAction(Action action)
    {
        base.SetOkBtnAction(action);
        UIButtonEvent btnEvent = GameCommon.FindComponent<UIButtonEvent>(mGameObjUI, "equip_bag_ok_button");
        if (btnEvent != null) btnEvent.mData.set("ACTION", action);
    }
}

public class EquipBagCloseButton : CEvent
{
    public override bool _DoEvent()
    {        
        DataCenter.CloseWindow("BAG_EQUIP_WINDOW");
        return true;
    }
}

public class EquipBagOkButton : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("BAG_EQUIP_WINDOW", "DO_MULTIPLE_SELECT_ACTION", true);
        return true;
    }
}

public class Button_BagEquipSingleBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("BAG_EQUIP_WINDOW", "DO_SELECT_ACTION", getObject("ITEM_DATA"));
        return true;
    }
}

public class Button_BagEquipMultipleIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        object val;
        bool b = getData("BUTTON", out val);
        GameObject obj = val as GameObject;
        UIToggle toggle = obj.GetComponent<UIToggle>();
        if (toggle.value)
        {
            DataCenter.SetData("BAG_EQUIP_WINDOW", "BUTTON", obj);
            DataCenter.SetData("BAG_EQUIP_WINDOW", "ADD_SELECT_ITEM", getObject("ITEM_DATA"));
        }
        else
            DataCenter.SetData("BAG_EQUIP_WINDOW", "REMOVE_SELECT_ITEM", getObject("ITEM_DATA"));

        return true;
    }
}


public class Button_bag_equip_sell_icon_btn : CEvent
{
	public override bool _DoEvent()
	{
		object val;
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		if (toggle.value)
		{
			DataCenter.SetData("BAG_EQUIP_WINDOW", "BUTTON", obj);
			DataCenter.SetData("BAG_EQUIP_WINDOW", "ADD_SELECT_ITEM", getObject("QUALITY_ITEM_DATA"));
			DataCenter.SetData("BAG_EQUIP_WINDOW", "EQUIP_UPDATE_SELL_REWARDS", true);
		}
		else
		{
			DataCenter.SetData("BAG_EQUIP_WINDOW", "REMOVE_SELECT_ITEM", getObject("QUALITY_ITEM_DATA"));
			DataCenter.SetData("BAG_EQUIP_WINDOW", "EQUIP_UPDATE_SELL_REWARDS", true);
		}
		
		return true;
	}
}
public class Button_equip_quality_btn : CEvent
{
	public override bool DoEvent ()
	{
		int equipQualityIndex = get ("EQUIP_QUALITY_INDEX");
		object val;
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		if (toggle.value)
		{
			DataCenter.SetData("BAG_EQUIP_WINDOW", "ADD_SELECT_QUALITY", equipQualityIndex);
		}else
		{
			DataCenter.SetData("BAG_EQUIP_WINDOW", "REMOVE_SELECT_QUALITY", equipQualityIndex);		
		}
		return true;
	}
}
public class Button_equip_sell_ok_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_EQUIP_WINDOW", "DO_MULTIPLE_SELECT_ACTION", true);
		return true;
	}
}
public class Button_equip_quality_chose_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_EQUIP_WINDOW", "EQUIP_CHOSE_QUALITY_INFOS", true);
		return true;
	}
}
public class Button_equip_chose_quality_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_EQUIP_WINDOW", "CLOSE_QUALITY_MESSAGE", true);
		return true;
	}
}