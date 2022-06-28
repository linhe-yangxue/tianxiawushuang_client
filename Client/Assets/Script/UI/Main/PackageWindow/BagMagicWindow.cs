using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System.Linq;
using System;

public class BagMagicWindow : BagEquipBaseWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_chose_quality_info_bg_btn", new DefineFactory<Button_magic_chose_quality_close_button>());

        EventCenter.Self.RegisterEvent("Button_bag_magic_window", new DefineFactory<MagicBagCloseButton>());
        EventCenter.Self.RegisterEvent("Button_magic_bag_close_button", new DefineFactory<MagicBagCloseButton>());
        EventCenter.Self.RegisterEvent("Button_magic_bag_ok_button", new DefineFactory<MagicBagOkButton>());

        EventCenter.Self.RegisterEvent("Button_bag_magic_single_btn", new DefineFactory<Button_BagMagicSingleBtn>());
        EventCenter.Self.RegisterEvent("Button_bag_magic_multiple_icon_btn", new DefineFactory<Button_BagMagicMultipleIconBtn>());

		EventCenter.Self.RegisterEvent("Button_bag_magic_sell_icon_btn", new DefineFactory<Button_bag_magic_sell_icon_btn>());
		EventCenter.Self.RegisterEvent("Button_magic_sell_ok_button", new DefineFactory<Button_magic_sell_ok_button>());
		EventCenter.Self.RegisterEvent("Button_magic_quality_chose_button", new DefineFactory<Button_magic_quality_chose_button>());
		EventCenter.Self.RegisterEvent("Button_equip_bag_ok_button", new DefineFactory<EquipBagOkButton>());
		EventCenter.Self.RegisterEvent("Button_magic_chose_quality_close_button", new DefineFactory<Button_magic_chose_quality_close_button>());
		EventCenter.Self.RegisterEvent("Button_magic_chose_quality_ok_button", new DefineFactory<Button_magic_chose_quality_close_button>());
		EventCenter.Self.RegisterEvent("Button_magic_quality_btn", new DefineFactory<Button_magic_quality_btn>());
    }

    public override void OnOpen()
    {
        base.OnOpen();
        GameObject _resolveGroup = GameCommon.FindObject(mGameObjUI, "resolve_group");
        UIScrollView _scrollView = GameCommon.FindComponent<UIScrollView>(_resolveGroup, "ScrollView");
        _scrollView.ResetPosition();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        if ("ADD_SELECT_ITEM" == keyIndex)
        {
            if (mCurBagShowType == BAG_SHOW_TYPE.UPGRADE)
            {
                bool bIsToggleState = true;
                if (mSelItemDataList.Count >= mMaxNum)
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_UPGRADE_MAX_NUM, mMaxNum.ToString());
                    bIsToggleState = false;
                }

                if (!bIsToggleState)
                {
                    GameObject obj = getObject("BUTTON") as GameObject;
                    if (obj != null)
                    {
                        UIToggle toggle = obj.GetComponent<UIToggle>();
                        toggle.value = bIsToggleState;
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
                DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的神器");
                GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "chose_quality_info");
                UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
                _grid.MaxCount = 1;

                for (int i = 0; i < _grid.MaxCount; i++)
                {
                    GameObject obj = _grid.controlList[i];
                    UIToggle objToggle = GameCommon.FindObject(obj, "magic_quality_btn").GetComponent<UIToggle>();
                    if (i + 3 == tmpQuality)
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
                DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的神器");
                GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "chose_quality_info");
                UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
                _grid.MaxCount = 1;

                for (int i = 0; i < _grid.MaxCount; i++)
                {
                    GameObject obj = _grid.controlList[i];
                    UIToggle objToggle = GameCommon.FindObject(obj, "magic_quality_btn").GetComponent<UIToggle>();
                    if (i + 3 == tmpQuality)
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
	protected override GameObject GetSellIconBtn(GameObject parentObj)
	{
		if (parentObj != null)
		{
			return parentObj.transform.Find("bag_magic_sell_icon_btn").gameObject;
		}
		return null;
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
                DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的神器");
				GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "chose_quality_info");
				UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
				_grid.MaxCount = 1;
				
				for(int i = 0; i < _grid.MaxCount; i++)
				{
					GameObject obj = _grid.controlList[i];
					UIToggle objToggle = GameCommon.FindObject (obj, "magic_quality_btn").GetComponent<UIToggle>();
					if(i + 3 == iQualityIndex)
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
			total += DataCenter.mMagicEquipRefineConfig.GetRecord(equip.refineLevel).get("TOTAL_REFINE_EQUIP_MONEY");
			int qualityLevel = TableCommon.GetNumberFromRoleEquipConfig(equip.tid, "QUALITY");
			total += DataCenter.mMagicEquipLvConfig.GetRecord(equip.strengthenLevel).get("TOTAL_EXP_" + qualityLevel) + equip.strengthenExp + itemData.itemNum;
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
		grid.MaxCount = 1;
		
		for(int i = 0; i < grid.MaxCount; i++)
		{
			GameObject obj = grid.controlList[i];
			UILabel qualityLabel = GameCommon.FindObject (obj, "name_label").GetComponent<UILabel>();
			qualityLabel.text = GameCommon.SetQualityName(i + 3);
			qualityLabel.color = GameCommon.SetQualityColor (i + 3);
			GameCommon.GetButtonData(GameCommon.FindObject (obj, "magic_quality_btn").gameObject).set("EQUIP_QUALITY_INDEX", i + 3);
			GameCommon.FindObject (obj, "magic_quality_btn").gameObject.GetComponent<UIToggle>().value = false;
		}
	}

    protected override tLogicData GetLogicData()
    {
        return MagicLogicData.Self;
    }

    protected override GameObject GetSingleBtn(GameObject parentObj)
    {
        if (parentObj != null)
        {
            return parentObj.transform.Find("bag_magic_single_btn").gameObject;
        }
        return null;
    }

    protected override GameObject GetMultipleIconBtn(GameObject parentObj)
    {
        if (parentObj != null)
        {
            return parentObj.transform.Find("bag_magic_multiple_icon_btn").gameObject;
        }
        return null;
    }

    protected override void UpdateUI()
    {
        GameObject useGroup = GameCommon.FindObject(mGameObjUI, "use_group");
        GameObject resolveGroup = GameCommon.FindObject(mGameObjUI, "resolve_group");
        GameObject okBtn = GameCommon.FindObject(mGameObjUI, "magic_bag_ok_button");

		GameObject sellGroup = GameCommon.FindObject(mGameObjUI, "sell_group");
		GameObject noButtonSprite = GameCommon.FindObject (mGameObjUI, "no_button_sprite");
		GameObject haveButtonSprite = GameCommon.FindObject (mGameObjUI, "have_button_sprite");

        if (null == useGroup || null == resolveGroup || null == sellGroup) {
			return;
		}

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
    }

    protected override void UpdateUIByShowType()
    {
        switch (mCurBagShowType)
        {
            case BAG_SHOW_TYPE.USE:
                UpdateItemInfosUI("use_group", UpdateSingleInfo);
				UIGridContainer container = GetComponent<UIGridContainer>("grid");
                if (mShowItemDataList != null && mShowItemDataList.Count == 0)
                {
				string str=DataCenter.mStringList.GetData ((int)STRING_INDEX.ERROR_NO_MAGIC_INFORMATION_TIPS,"STRING_CN");
				GameObject .Find ("Label_nomass_information_tips").GetComponent <UILabel >().text =str;
				}
				else{
				GameObject .Find ("Label_nomass_information_tips").GetComponent <UILabel >().text ="";
				}
                break;
            case BAG_SHOW_TYPE.UPGRADE:
                UpdateItemInfosUI("resolve_group", UpdateMultipleInfo);
				UILabel num=GetComponent <UILabel >("num_label");
				if(num.text =="0/0"){
				string str=DataCenter.mStringList.GetData ((int)STRING_INDEX.ERROR_NO_MAGIC_INFORMATION_TIPS,"STRING_CN");
				GameObject .Find ("Label_nomass_information_tips").GetComponent <UILabel >().text =str;
				}
				else{
				GameObject .Find ("Label_nomass_information_tips").GetComponent <UILabel >().text ="";
				}
                break;
            case BAG_SHOW_TYPE.RESOLVE:
                UpdateItemInfosUI("resolve_group", UpdateMultipleInfo);
                break;
			case BAG_SHOW_TYPE.SELL:
				UpdateItemInfosUI("sell_group", UpdateSellInfo);
				UILabel numLabel=GetComponent <UILabel >("num_label");
				if(numLabel.text =="0/0"){
					string str=DataCenter.mStringList.GetData ((int)STRING_INDEX.ERROR_NO_MAGIC_INFORMATION_TIPS,"STRING_CN");
					GameObject .Find ("Label_nomass_information_tips").GetComponent <UILabel >().text =str;
				}
				else{
					GameObject .Find ("Label_nomass_information_tips").GetComponent <UILabel >().text ="";
				}
				break;
		}
	}
	
	protected override void SetOkBtnAction(Action action)
    {
        base.SetOkBtnAction(action);
        UIButtonEvent btnEvent = GameCommon.FindComponent<UIButtonEvent>(mGameObjUI, "magic_bag_ok_button");
        if (btnEvent != null)
			btnEvent.mData.set ("ACTION", action);
    }
}

public class MagicBagCloseButton : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("BAG_MAGIC_WINDOW");
        return true;
    }
}

public class MagicBagOkButton : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("BAG_MAGIC_WINDOW", "DO_MULTIPLE_SELECT_ACTION", true);
        return true;
    }
}

public class Button_BagMagicSingleBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("BAG_MAGIC_WINDOW", "DO_SELECT_ACTION", getObject("ITEM_DATA"));
        return true;
    }
}

public class Button_BagMagicMultipleIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        object val;
        bool b = getData("BUTTON", out val);
        GameObject obj = val as GameObject;
        UIToggle toggle = obj.GetComponent<UIToggle>();
        if (toggle.value)
        {
            DataCenter.SetData("BAG_MAGIC_WINDOW", "BUTTON", obj);
            DataCenter.SetData("BAG_MAGIC_WINDOW", "ADD_SELECT_ITEM", getObject("ITEM_DATA"));
        }
        else
            DataCenter.SetData("BAG_MAGIC_WINDOW", "REMOVE_SELECT_ITEM", getObject("ITEM_DATA"));

        return true;
    }
}

public class Button_bag_magic_sell_icon_btn : CEvent
{
	public override bool _DoEvent()
	{
		object val;
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		if (toggle.value)
		{
			DataCenter.SetData("BAG_MAGIC_WINDOW", "BUTTON", obj);
			DataCenter.SetData("BAG_MAGIC_WINDOW", "ADD_SELECT_ITEM", getObject("QUALITY_ITEM_DATA"));
			DataCenter.SetData("BAG_MAGIC_WINDOW", "EQUIP_UPDATE_SELL_REWARDS", true);
		}
		else
		{
			DataCenter.SetData("BAG_MAGIC_WINDOW", "REMOVE_SELECT_ITEM", getObject("QUALITY_ITEM_DATA"));
			DataCenter.SetData("BAG_MAGIC_WINDOW", "EQUIP_UPDATE_SELL_REWARDS", true);
		}
		
		return true;
	}
}
public class Button_magic_quality_btn : CEvent
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
			DataCenter.SetData("BAG_MAGIC_WINDOW", "ADD_SELECT_QUALITY", equipQualityIndex);
		}else
		{
			DataCenter.SetData("BAG_MAGIC_WINDOW", "REMOVE_SELECT_QUALITY", equipQualityIndex);		
		}
		return true;
	}
}
public class Button_magic_sell_ok_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_MAGIC_WINDOW", "DO_MULTIPLE_SELECT_ACTION", true);
		return true;
	}
}
public class Button_magic_quality_chose_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_MAGIC_WINDOW", "EQUIP_CHOSE_QUALITY_INFOS", true);
		return true;
	}
}
public class Button_magic_chose_quality_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_MAGIC_WINDOW", "CLOSE_QUALITY_MESSAGE", true);
		return true;
	}
}
