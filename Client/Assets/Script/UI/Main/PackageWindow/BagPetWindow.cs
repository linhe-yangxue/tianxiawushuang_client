using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System.Linq;
using System;
using DataTable;

public class tSortWindow : tWindow
{
    public SORT_TYPE mSortType = SORT_TYPE.STAR_LEVEL;

    public List<T> SortList<T>(List<T> list)
    {
        if (list != null && list.Count > 0)
        {
            if (typeof(T) == typeof(PetData))
            {
                List<PetData> tempList = list as List<PetData>;
                switch (mSortType)
                {
                    case SORT_TYPE.STAR_LEVEL:
                        tempList = GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByStarLevel);
                        //				list = list.OrderByDescending(p => p.mStarLevel).
                        //					ThenByDescending(p => p.mStrengthenLevel).
                        //						ThenBy(p => p.mModelIndex).
                        //						ThenBy(p => (int)p.mElementType).
                        //						ToList();
                        break;
                    case SORT_TYPE.LEVEL:
                        tempList = GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByLevel);
                        //list = list.OrderByDescending(p => p.mLevel).
                        //    ThenByDescending(p => p.mStarLevel).
                        //    ThenBy(p => p.mModelIndex).
                        //    ThenBy(p => TableCommon.GetNumberFromActiveCongfig(p.mModelIndex, "ELEMENT_INDEX")).
                        //    ToList();
                        break;
                    case SORT_TYPE.ELEMENT_INDEX:
                        tempList = GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByElement);
                        //list = list.OrderBy(p => TableCommon.GetNumberFromActiveCongfig(p.mModelIndex, "ELEMENT_INDEX")).
                        //    ThenByDescending(p => p.mStarLevel).
                        //    ThenByDescending(p => p.mLevel).
                        //    ThenBy(p => p.mModelIndex).
                        //    ToList();
                        break;
                }
            }
            else if (typeof(T) == typeof(EquipData))
            {
                List<EquipData> tempList = list as List<EquipData>;
                switch (mSortType)
                {
                    case SORT_TYPE.STAR_LEVEL:
                        tempList = GameCommon.SortList<EquipData>(tempList, GameCommon.SortEquipDataByStarLevel);
                        //				list = list.OrderByDescending(p => p.mStarLevel).
                        //					ThenByDescending(p => p.mStrengthenLevel).
                        //						ThenBy(p => p.mModelIndex).
                        //						ThenBy(p => (int)p.mElementType).
                        //						ToList();
                        break;
                    case SORT_TYPE.STRENGTHEN_LEVEL:
                        tempList = GameCommon.SortList<EquipData>(tempList, GameCommon.SortEquipDataByStrengthenLevel);
                        //				list = list.OrderByDescending(p => p.mStrengthenLevel).
                        //					ThenByDescending(p => p.mStarLevel).
                        //						ThenBy(p => p.mModelIndex).
                        //						ThenBy(p => (int)p.mElementType).
                        //						ToList();
                        break;
                    case SORT_TYPE.ELEMENT_INDEX:
                        tempList = GameCommon.SortList<EquipData>(tempList, GameCommon.SortEquipDataByElement);
                        //				list = list.OrderBy(p => (int)p.mElementType).
                        //					ThenByDescending(p => p.mStarLevel).
                        //						ThenByDescending(p => p.mStrengthenLevel).
                        //						ThenBy(p => p.mModelIndex).
                        //						ToList();
                        break;
                }
            }
        }

        return list;
    }
}

public class OpenBagObject<T>
{
    public bool isNeedClose=true;
    public BAG_SHOW_TYPE mBagShowType;
    public Func<T, bool> mFilterCondition;
    public Func<List<T>, List<T>> mSortCondition;
    public List<T> mSelectList = new List<T>();
    public Action<T> mSelectAction;
    public Action<List<T>> mMultipleSelectAction;
}

public class BagBaseWindow<T> : tSortWindow where T : ItemDataBase
{
    protected enum SELECT_TYPE
    {
        SINGLE_SELECT = 1,  // 单选
        MULTIPLE_SELECT = 2,  // 多选
    }

    protected BAG_SHOW_TYPE mCurBagShowType = BAG_SHOW_TYPE.USE;
    protected SELECT_TYPE mCurSelectType = SELECT_TYPE.SINGLE_SELECT;
    public List<T> mSelItemDataList = new List<T>();
    public List<T> mShowItemDataList = new List<T>();
    protected Func<T, bool> refreshCondition;
    protected int mMaxNum = 6;
    public List<T> mtempSelList = new List<T>();
    protected OpenBagObject<T> mOpenBagObj;

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        if ("DO_ADD_SELECT_ITEM" == keyIndex)
        {
            AddSelectItem(objVal as T);
            UpdateNumLabel();
        }
        else if ("REMOVE_SELECT_ITEM" == keyIndex)
        {
            RemoveSelectItem(objVal as T);
            UpdateNumLabel();
        }
        else if ("SET_OK_BUTTON_ACTION" == keyIndex)
        {
            SetOkBtnAction((Action)objVal);
        }
        else if ("SET_ITEM_LIST" == keyIndex)
        {
            mSelItemDataList = objVal as List<T>;
        }
        else if ("DO_SELECT_ACTION" == keyIndex)
        {
            DoSelectAction(objVal);
        }
        else if ("DO_MULTIPLE_SELECT_ACTION" == keyIndex)
        {
            DoMultipleSelectAction(mSelItemDataList);
        }
		else if("CHOSE_QUALITY_INFOS" == keyIndex)
		{
			SetChoseQuality();
		}
		else if("CLOSE_QUALITY_MESSAGE" == keyIndex)
		{
			GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "chose_quality_info");
			choseQualityInfoObj.SetActive (false);
		}
    }

	void SetChoseQuality()
	{
		GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "chose_quality_info");
		choseQualityInfoObj.SetActive (true);
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
		grid.MaxCount = 3;

		for(int i = 0; i < grid.MaxCount; i++)
		{
			GameObject obj = grid.controlList[i];
			UILabel qualityLabel = GameCommon.FindObject (obj, "name_label").GetComponent<UILabel>();

			qualityLabel.text = GameCommon.SetQualityName(i + 1);
			qualityLabel.color = GameCommon.SetQualityColor (i + 1);

			GameCommon.GetButtonData(GameCommon.FindObject (obj, "pet_quality_btn").gameObject).set("PET_QUALITY_INDEX", i + 1);
			GameCommon.FindObject (obj, "pet_quality_btn").gameObject.GetComponent<UIToggle>().value = false;
		}
	}
    public override void Open(object param)
    {
        base.Open(param);
        
        mOpenBagObj = param as OpenBagObject<T>;
        if (null == mOpenBagObj)
        {
            DEBUG.LogError("mOpenBagObj is null");
        }
        mCurBagShowType = mOpenBagObj.mBagShowType;
        mCurSelectType = (SELECT_TYPE)((int)mOpenBagObj.mBagShowType / 100);
        SetShowItemDataList();

        //弹经验蛋框
        bool fromTeamInfo = DataCenter.Get("I_AM_FROM_TEAMINO");
        if (mShowItemDataList.Count == 0 && fromTeamInfo == true)
        {
            DataCenter.Set("I_AM_FROM_TEAMINO", false);
            DataCenter.CloseWindow("BAG_PET_WINDOW");
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, 30305);
            return;
        }
        DataCenter.Set("I_AM_FROM_TEAMINO", false);

        mSelItemDataList=new List<T>(mOpenBagObj.mSelectList);
        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        UpdateUI();

        return true;
    }

    public virtual void ShowWindow(BAG_SHOW_TYPE type, Func<T, bool> condition = null, T[] itemDataArr = null, Action btnAction = null)
    {
        refreshCondition = (Func<T, bool>)condition;
        if (null == itemDataArr)
        {
            mSelItemDataList.Clear();
        }
        else
        {
            mSelItemDataList = itemDataArr.ToList();
        }

        if (BAG_SHOW_TYPE.USE == type)
        {
            set("USE_ACTION", btnAction);
        }
        else
        {
            SetOkBtnAction(btnAction);
        }

        Open(type);
    }

    protected virtual void UpdateUI() { }
    protected virtual void UpdateUIByShowType() { }

    protected virtual void UpdateNumLabel()
    {
        UILabel numLabel = GameCommon.FindComponent<UILabel>(mGameObjUI, "num_label");
        if (numLabel != null)
        {   
            numLabel.gameObject.SetActive(mCurSelectType == SELECT_TYPE.MULTIPLE_SELECT);
            numLabel.text = mSelItemDataList.Count + "/" + mShowItemDataList.Count;
			if(mShowItemDataList.Count ==0){
				GameCommon.SetUIText (mGameObjUI ,"Label_no_pet_label",DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_PET_LABEL,"STRING_CN"));
				GameCommon.SetUIVisiable(mGameObjUI ,"Label_no_pet_label",true);
			}
			else {
				GameCommon.SetUIVisiable(mGameObjUI ,"Label_no_pet_label",false);
			}
        }
    }

    public delegate void UpdateItemIcon(GameObject parentObj, T itemData);

    protected virtual bool SetItemIcon(GameObject obj, T itemData) { return true; }

    public override void OnClose()
    {
        base.OnClose();

        //mSelItemDataList.Clear();
    }

    protected virtual tLogicData GetLogicData() { return null; }

    protected virtual GameObject GetOkBtn() { return null; }

    protected virtual GameObject GetUseGroup() { return null; }

    protected virtual void SetOkBtnAction(Action action) { }

    protected virtual void SetShowItemDataList() { }

    protected virtual bool IsItemSel(int iItemid)
    {       
        return mSelItemDataList.Exists(x => x.itemId == iItemid);
    }

    protected virtual void SetToggle(GameObject obj, T itemData)
    {
        UIToggle toggle = obj.GetComponent<UIToggle>();
        toggle.value = IsItemSel(itemData.itemId);
    }

    protected void UpdateItemInfosUI(string strParentObjName, UpdateItemIcon callBack)
    {
        UpdateItemInfosUI2(strParentObjName, callBack);
        return;
    }
    //by chenliang
    //begin

    protected string mUpdateItemParentName = "";
    protected UpdateItemIcon mUpdateItemCallback;
    /// <summary>
    /// 刷新元素网格
    /// </summary>
    /// <param name="strParentObjName"></param>
    /// <param name="callBack"></param>
    protected void UpdateItemInfosUI2(string strParentObjName, UpdateItemIcon callBack)
    {
        GameObject parentObj = GetSub(strParentObjName);
        if (null == parentObj)
            return;

        UIWrapGrid grid = GameCommon.FindComponent<UIWrapGrid>(parentObj, "grid");
        if (grid != null)
        {
            mUpdateItemParentName = strParentObjName;
            mUpdateItemCallback = callBack;
            grid.onInitializeItem = __OnGridUpdate;
            grid.maxIndex = mShowItemDataList.Count - 1;
            grid.ItemsCount = 16;

            UpdateNumLabel();
        }
    }
    /// <summary>
    /// 刷新每个宠物网格
    /// </summary>
    /// <param name="item"></param>
    /// <param name="wrapIndex"></param>
    /// <param name="index"></param>
    protected void __OnGridUpdate(GameObject item, int wrapIndex, int index)
    {
        T itemData = mShowItemDataList[index];
        if (itemData != null)
        {
            GameObject obj = item;
            if (mUpdateItemCallback != null)
                mUpdateItemCallback(obj, itemData);
            if (mUpdateItemParentName == "use_group")
                AddButtonAction(GameCommon.FindObject(obj, "item_icon"), () => GameCommon.SetItemDetailsWindow(itemData.tid));
        }
    }

    //end

    public void UpdateSingleInfo(GameObject parentObj, T itemData)
    {
        if (null == parentObj || null == itemData)
            return;

        GameObject obj = GetSingleBtn(parentObj);
        if (null == obj)
            return;

        UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
        if (buttonEvent != null)
        {
            buttonEvent.mData.set("ITEM_DATA", itemData);
        }

        GameObject iconObj = parentObj.transform.Find("bag_item_icon").gameObject;

        SetItemIcon(iconObj, itemData);
    }

    protected void UpdateMultipleInfo(GameObject parentObj, T itemData)
    {
        if (null == parentObj || null == itemData)
            return;

        GameObject obj = GetMultipleIconBtn(parentObj);
        if (null == obj)
            return;

        UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
        if (buttonEvent != null)
        {
            buttonEvent.mData.set("ITEM_DATA", itemData);
        }

        SetToggle(obj, itemData);
        
        SetItemIcon(obj, itemData);
    }
	protected void UpdateSellInfo(GameObject parentObj, T itemData)
	{
		if (null == parentObj || null == itemData)
			return;
		
		GameObject obj = GetSellIconBtn(parentObj);
		if (null == obj)
			return;
		
		UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
		if (buttonEvent != null)
		{
			buttonEvent.mData.set("QUALITY_ITEM_DATA", itemData);
		}		
		SetToggle(obj, itemData);		
		SetItemIcon(obj, itemData);
	}

    protected virtual GameObject GetMultipleIconBtn(GameObject parentObj) { return null; }
	protected virtual GameObject GetSellIconBtn(GameObject parentObj) { return null; }
    protected virtual GameObject GetSingleBtn(GameObject parentObj) { return null; }
    protected bool GetFilterCondition(T itemData)
    {
        if (null == itemData)
        {
            return false;
        }

        if (mOpenBagObj != null && mOpenBagObj.mFilterCondition != null)
        {
            return mOpenBagObj.mFilterCondition(itemData);
        }

        return true;
    }

    protected List<T> SortItemList(List<T> itemDataList)
    {
        if (null == itemDataList)
        {
            return null;
        }

        if (mOpenBagObj != null && mOpenBagObj.mSortCondition != null)
        {
            return mOpenBagObj.mSortCondition(itemDataList);
        }

        return itemDataList;
    }

    protected void DoSelectAction(object item)
    {
        T itemData = item as T;
        if (null == itemData || null == mOpenBagObj || null == mOpenBagObj.mSelectAction)
        {
            return;
        }

		if (mOpenBagObj.isNeedClose) 
		{
			Close();
			DataCenter.CloseWindow(UIWindowString.master_container);
			TeamInfoWindow.CloseAllWindow();
		}

		mOpenBagObj.mSelectAction(itemData);
    }

    protected void DoMultipleSelectAction(List<T> itemDataList)
    {
        if (null == itemDataList || null == mOpenBagObj || null == mOpenBagObj.mMultipleSelectAction)
        {
            return;
        }

        mOpenBagObj.mMultipleSelectAction(itemDataList.ToList());

        if(mOpenBagObj.isNeedClose) Close();
    }

    protected List<T> FilterAndSortItemList(List<T> itemDataList)
    {
        if (null == itemDataList)
        {
            return null;
        }

        List<T> list = itemDataList.FindAll(x => GetFilterCondition(x) == true);
        return SortItemList(list);
    }

    protected bool AddSelectItem(T itemData)
    {
        if (itemData != null && !mSelItemDataList.Contains(itemData))
        {
            mSelItemDataList.Add(itemData);
            return true;
        }
        return false;
    }

    protected bool RemoveSelectItem(T itemData)
    {
        if (itemData != null && mSelItemDataList.Contains(itemData))
        {
            mSelItemDataList.Remove(itemData); 
            return true;
        }
        return false;
    }
    //by chenliang
    //begin

    /// <summary>
    /// 添加指定品质元素
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    protected bool _AddSelectItemsByQuality(int quality)
    {
        int itemCount = 0;
        return _AddSelectItemsByQuality(quality, out itemCount);
    }
    /// <summary>
    /// 添加指定品质元素
    /// </summary>
    /// <param name="quality"></param>
    /// <param name="itemCount">指定品质元素个数</param>
    /// <returns></returns>
    protected bool _AddSelectItemsByQuality(int quality, out int itemCount)
    {
        itemCount = 0;
        if (mShowItemDataList == null || mSelItemDataList == null)
            return false;

        for (int i = 0, count = mShowItemDataList.Count; i < count; i++)
        {
            T tmpItem = mShowItemDataList[i];
            if (GameCommon.GetItemQuality(tmpItem.tid) != quality)
                continue;
            itemCount += 1;
            if (!mSelItemDataList.Contains(tmpItem))
                mSelItemDataList.Add(tmpItem);
        }
        return true;
    }
    /// <summary>
    /// 移除指定品质元素
    /// </summary>
    /// <param name="quality"></param>
    /// <param name="itemCount">指定品质元素个数</param>
    /// <returns></returns>
    protected bool _RemoveSelectItemsByQuality(int quality)
    {
        int itemCount = 0;
        return _RemoveSelectItemsByQuality(quality, out itemCount);
    }
    /// <summary>
    /// 移除指定品质元素
    /// </summary>
    /// <param name="quality"></param>
    /// <param name="itemCount">指定品质元素个数</param>
    /// <returns></returns>
    protected bool _RemoveSelectItemsByQuality(int quality, out int itemCount)
    {
        itemCount = 0;
        if (mShowItemDataList == null || mSelItemDataList == null)
            return false;

        for (int i = 0, count = mShowItemDataList.Count; i < count; i++)
        {
            T tmpItem = mShowItemDataList[i];
            if (GameCommon.GetItemQuality(tmpItem.tid) != quality)
                continue;
            itemCount += 1;
            if (mSelItemDataList.Contains(tmpItem))
                mSelItemDataList.Remove(tmpItem);
        }
        return true;
    }

    //end
}

public enum BAG_SHOW_TYPE
{
    USE = 101, // 上下阵、重生
    FAIRYLAND = 102,  //仙境
    UPGRADE = 201, // 升级
    RESOLVE = 202, // 分解
	SELL = 203, // 出售
    MULITY=204
}

public class BagPetWindow : BagBaseWindow<PetData>
{
    private PetData mCurItemData = null;
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_chose_quality_info_pet_window_black", new DefineFactory<Button_chose_quality_close_button>());
        EventCenter.Self.RegisterEvent("Button_bag_pet_window_group", new DefineFactory<BagPetCloseButton>());
        EventCenter.Self.RegisterEvent("Button_pet_bag_close_button", new DefineFactory<BagPetCloseButton>());
        EventCenter.Self.RegisterEvent("Button_pet_bag_ok_button", new DefineFactory<BagPetOkButton>());

        EventCenter.Self.RegisterEvent("Button_bag_pet_single_btn", new DefineFactory<Button_BagPetSingleBtn>());
        EventCenter.Self.RegisterEvent("Button_bag_pet_multiple_icon_btn", new DefineFactory<Button_BagPetMultipleIconBtn>());

		EventCenter.Self.RegisterEvent("Button_bag_pet_sell_icon_btn", new DefineFactory<Button_bag_pet_sell_icon_btn>());
		EventCenter.Self.RegisterEvent("Button_pet_sell_ok_button", new DefineFactory<Button_pet_sell_ok_button>());
		EventCenter.Self.RegisterEvent("Button_quality_chose_button", new DefineFactory<Button_quality_chose_button>());
		EventCenter.Self.RegisterEvent("Button_chose_quality_close_button", new DefineFactory<Button_chose_quality_close_button>());
		EventCenter.Self.RegisterEvent("Button_pet_quality_btn", new DefineFactory<Button_pet_quality_btn>());
		EventCenter.Self.RegisterEvent("Button_chose_quality_ok_button", new DefineFactory<Button_chose_quality_close_button>());

    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        if ("ADD_SELECT_ITEM" == keyIndex)
        {
            if (mCurBagShowType == BAG_SHOW_TYPE.UPGRADE || mCurBagShowType == BAG_SHOW_TYPE.RESOLVE)
            {
                bool bIsToggleState = true;
                if (mSelItemDataList.Count >= mMaxNum)
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_UPGRADE_MAX_NUM, mMaxNum.ToString());
                    bIsToggleState = false;
                }
                else
                {
                    if (mCurItemData != null && mOpenBagObj.mBagShowType == BAG_SHOW_TYPE.UPGRADE) //升级的时候才做这个判断
                    {
                        int mMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, mCurItemData.level);
                        if (PetLevelUpInfoWidnow.eatExp > 0 || ((mMaxExp - mCurItemData.exp == 1) && (mCurItemData.level == RoleLogicData.GetMainRole().level)))
                        {
                            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_UPGRADE_PET_HIGHER_THAN_CHARACTER);
                            bIsToggleState = false;
                            GameObject obj = getObject("BUTTON") as GameObject;
                            if (obj != null)
                            {
                                UIToggle toggle = obj.GetComponent<UIToggle>();
                                toggle.value = bIsToggleState;
                            }
                            return;
                        }
                        int iDExp = GetSelItemTotalExp() + GetItemProvideExp(objVal as PetData);
                        int iAimExp = mCurItemData.exp;
                        int iAimLevel = mCurItemData.level;
                        int iMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, iAimLevel);

                        PerAddExp(iDExp, ref iAimLevel, ref iAimExp);
                        if (iAimLevel > RoleLogicData.GetMainRole().level)
                        {
                            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_UPGRADE_PET_HIGHER_THAN_CHARACTER);
                            bIsToggleState = false;
                        }
                    }
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
			else if(mCurBagShowType == BAG_SHOW_TYPE.SELL)
			{
				bool bIsToggleState = true;

				if (!bIsToggleState)
				{
					GameObject obj = getObject("BUTTON") as GameObject;
					if (obj != null)
						obj.GetComponent<UIToggle>().value = bIsToggleState;
					return;
				}
			}
            mtempSelList.Add(objVal as PetData);
            base.onChange("DO_ADD_SELECT_ITEM", objVal);
        }
        else if ("SET_UPGRADE_PET_DATA" == keyIndex)
        {
            mCurItemData = objVal as PetData;
        }
        else if ("REMOVE_SELECT" == keyIndex)
        {
            if (mCurItemData != null)
            {
                PetLevelUpInfoWidnow.eatExp = 0;
                int iDExp = GetSelItemTotalExp();
                int iAimExp = mCurItemData.exp;
                int iAimLevel = mCurItemData.level;
                int iMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, iAimLevel);
                PerAddExp(iDExp, ref iAimLevel, ref iAimExp);
            }
        }
        else if ("SELECT_THEN_CLOSE" == keyIndex)
        {
            if ((object)DataCenter.Get("I_AM_FROM_PET_LEVEL_UP") != null && DataCenter.Get("I_AM_FROM_PET_LEVEL_UP"))
            {
                //      List<PetData> ShowList = FilterAndSortItemList(PetLogicData.Self.mDicPetData.Values.ToList());
                List<PetData> mdeleteList = new List<PetData>();
                foreach (PetData mtemp in mSelItemDataList)
                {
                    if (mtempSelList.Exists((x) => mtemp.itemId == x.itemId))
                    {
                        mdeleteList.Add(mtemp);
                    }
                }
                foreach (PetData mtemp in mdeleteList)
                {
                    mSelItemDataList.Remove(mtemp);
                }
                mtempSelList.Clear();
                List<PetData> temp = mSelItemDataList;
                PetLevelUpInfoWidnow.eatExp = 0;
                int iDExp = GetSelItemTotalExp();
                int iAimExp = mCurItemData.exp;
                int iAimLevel = mCurItemData.level;
                int iMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, iAimLevel);
                PerAddExp(iDExp, ref iAimLevel, ref iAimExp);
                DataCenter.Set("I_AM_FROM_PET_LEVEL_UP", false);
            }
        }
        else if ("CLEAR" == keyIndex)
        {
            mtempSelList.Clear();
        }
		else if("ADD_SELECT_QUALITY" == keyIndex)
		{
            //by chenliang
            //begin

//            int i = (int)objVal;
//			UpdateQualityItemInfosUI("sell_group", UpdateQualitySellInfo, i);
//------------------
            int tmpQuality = (int)objVal;
            int tmpItemsCount = 0;
            _AddSelectItemsByQuality(tmpQuality, out tmpItemsCount);
            if (tmpItemsCount == 0)
            {
                DataCenter.ErrorTipsLabelMessage(TableCommon.getStringFromStringList(STRING_INDEX.ERROR_NO_THIS_LEVEL_PET));
                GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "chose_quality_info");
                UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
                _grid.MaxCount = 3;

                for (int i = 0; i < _grid.MaxCount; i++)
                {
                    GameObject obj = _grid.controlList[i];
                    UIToggle objToggle = GameCommon.FindObject(obj, "pet_quality_btn").GetComponent<UIToggle>();
                    if (i + 1 == tmpQuality)
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
//            int i = (int)objVal;
//			UpdateQualityItemInfosUI("sell_group", RemoveQualitySellInfo, i);
//---------------------
            int tmpQuality = (int)objVal;
            int tmpItemsCount = 0;
            _RemoveSelectItemsByQuality(tmpQuality, out tmpItemsCount);
            if (tmpItemsCount == 0)
            {
                DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的宠物");
                GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "chose_quality_info");
                UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
                _grid.MaxCount = 3;

                for (int i = 0; i < _grid.MaxCount; i++)
                {
                    GameObject obj = _grid.controlList[i];
                    UIToggle objToggle = GameCommon.FindObject(obj, "pet_quality_btn").GetComponent<UIToggle>();
                    if (i + 1 == tmpQuality)
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
		else if("UPDATE_SELL_REWARDS" == keyIndex)
		{
			SetGetMoney();
		}
    }

	void RemoveQualitySellInfo(GameObject parentObj, PetData itemData)
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
	void UpdateQualitySellInfo(GameObject parentObj, PetData itemData)
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
				PetData itemData = mShowItemDataList[i];
				if (itemData != null)
				{
					GameObject obj = grid.controlList[iIndex];
					int starLevel=TableCommon.GetNumberFromActiveCongfig(itemData.tid,"STAR_LEVEL");
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
				DataCenter.ErrorTipsLabelMessage ("您没有任何相应品质的宠物");
				GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "chose_quality_info");
				UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
				_grid.MaxCount = 3;
				
				for(int i = 0; i < _grid.MaxCount; i++)
				{
					GameObject obj = _grid.controlList[i];
					UIToggle objToggle = GameCommon.FindObject (obj, "pet_quality_btn").GetComponent<UIToggle>();
					if(i + 1 == iQualityIndex)
						objToggle.value = false;
				}
			}
		}
	}

    private int GetSelItemTotalExp()
    {
        int iExp = 0;
        foreach (PetData itemData in mSelItemDataList)
        {
            iExp += GetItemProvideExp(itemData);
        }

        return iExp;
    }

    private int GetItemProvideExp(PetData itemData)
    {
        if (itemData != null)
        {
            DataRecord dataRecord = DataCenter.mPetLevelExpTable.GetRecord(itemData.level);
            if (dataRecord != null)
            {
                return TableCommon.GetNumberFromActiveCongfig(itemData.tid, "DROP_EXP") + dataRecord["TOTAL_EXP_" + itemData.starLevel];
            }
        }
        return 0;
    }

    public void PerAddExp(int dExp, ref int iAimLevel, ref int iAimExp)
    {
        if (iAimLevel >= mCurItemData.mMaxLevelNum)
        {
            iAimLevel = mCurItemData.mMaxLevelNum;
            return;
        }

        iAimExp += dExp;

        int iMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, iAimLevel);
        if (iAimExp >= iMaxExp)
        {
            PerLevelUp(iAimExp,ref iAimLevel);
            if (PetLevelUpInfoWidnow.eatExp > 0)
                return;
            int iDExp = iAimExp - iMaxExp;

            iAimExp = 0;
            PerAddExp(iDExp, ref iAimLevel, ref iAimExp);
        }
    }

    private void PerLevelUp(int iAimExp,ref int iAimLevel)
    {
        if (iAimLevel >= mCurItemData.mMaxLevelNum)
            return;
        if (iAimLevel == RoleLogicData.Self.character.level)
        {
            int iMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, iAimLevel);
            PetLevelUpInfoWidnow.eatExp = iAimExp - iMaxExp + 1;
            return;
        }
        iAimLevel += 1;
    }

    protected override tLogicData GetLogicData()
    {
        return PetLogicData.Self;
    }

    protected override void SetShowItemDataList()
    {
        mShowItemDataList = FilterAndSortItemList(PetLogicData.Self.mDicPetData.Values.ToList());
    }

    protected override GameObject GetMultipleIconBtn(GameObject parentObj)
    {
        if (parentObj != null)
        {
            return parentObj.transform.Find("bag_pet_multiple_icon_btn").gameObject;
        }
        return null;
    }   
	protected override GameObject GetSellIconBtn(GameObject parentObj)
	{
		if (parentObj != null)
		{
			return parentObj.transform.Find("bag_pet_sell_icon_btn").gameObject;
		}
		return null;
	}
	protected override GameObject GetSingleBtn(GameObject parentObj)
    {
        if (parentObj != null)
        {
            return parentObj.transform.Find("bag_pet_single_btn").gameObject;
        }
        return null;
    }


    protected override void UpdateUI()
    {
        GameObject useGroup = GameCommon.FindObject(mGameObjUI, "use_group");
        GameObject upGroup = GameCommon.FindObject(mGameObjUI, "up_group");
        GameObject fairylandGroup = GameCommon.FindObject(mGameObjUI, "fairyland_group");
		GameObject sellGroup = GameCommon.FindObject (mGameObjUI, "sell_group");
        GameObject okBtn = GameCommon.FindObject(mGameObjUI, "pet_bag_ok_button");
		GameObject noButtonSprite = GameCommon.FindObject (mGameObjUI, "no_button_sprite");
		GameObject haveButtonSprite = GameCommon.FindObject (mGameObjUI, "have_button_sprite");
		if (null == useGroup || null == upGroup || null == fairylandGroup || null == sellGroup)
            return;

        if (null == okBtn)
            return;

//        useGroup.SetActive(mCurSelectType == SELECT_TYPE.SINGLE_SELECT);
//        fairylandGroup.SetActive(mCurSelectType == SELECT_TYPE.SINGLE_SELECT);
		useGroup.SetActive(mCurBagShowType == BAG_SHOW_TYPE.USE);
		fairylandGroup.SetActive(mCurBagShowType == BAG_SHOW_TYPE.FAIRYLAND);
//        upGroup.SetActive(mCurSelectType == SELECT_TYPE.MULTIPLE_SELECT);
		upGroup.SetActive (mCurBagShowType == BAG_SHOW_TYPE.UPGRADE || mCurBagShowType == BAG_SHOW_TYPE.RESOLVE);
		sellGroup.SetActive (mCurBagShowType == BAG_SHOW_TYPE.SELL);
//        okBtn.SetActive(mCurSelectType == SELECT_TYPE.MULTIPLE_SELECT);
		okBtn.SetActive(mCurBagShowType == BAG_SHOW_TYPE.UPGRADE || mCurBagShowType == BAG_SHOW_TYPE.RESOLVE);
		if(mCurBagShowType == BAG_SHOW_TYPE.USE || mCurBagShowType == BAG_SHOW_TYPE.FAIRYLAND)
		{
			noButtonSprite.SetActive (true);
			haveButtonSprite.SetActive (false);
		}else
		{
			noButtonSprite.SetActive (false);
			haveButtonSprite.SetActive (true);
		}
		SetGetMoney();
        UpdateUIByShowType();
    }

    protected override void UpdateUIByShowType()
    {
        switch (mCurBagShowType)
        {
            case BAG_SHOW_TYPE.USE:
                UpdateItemInfosUI("use_group", UpdateSingleInfo);
                break;
            case BAG_SHOW_TYPE.UPGRADE:
            case BAG_SHOW_TYPE.RESOLVE:
                UpdateItemInfosUI("up_group", UpdateMultipleInfo);
                break;
            case BAG_SHOW_TYPE.FAIRYLAND:
                UpdateItemInfosUI("fairyland_group", UpdateSingleInfo);
                break;
			case BAG_SHOW_TYPE.SELL:
				UpdateItemInfosUI("sell_group", UpdateSellInfo);
				break;
        }
    }

    protected override bool SetItemIcon(GameObject obj, PetData itemData)
    {
        if (null == obj || null == itemData)
            return false;

        // 设置宠物头像
        GameCommon.SetItemIcon(obj, "item_icon", itemData.tid);

        // 设置名称
        GameCommon.SetUIText(obj, "name_label", TableCommon.GetStringFromActiveCongfig(itemData.tid, "NAME"));

        // 设置等级
        GameCommon.SetUIText(obj, "level_label", itemData.level.ToString());

        //设置碎片数

        int mpieceNum = 0;
        foreach (KeyValuePair<int, PetFragmentData> temp in PetFragmentLogicData.Self.mDicPetFragmentData)
        {
            int mitemId = DataCenter.mFragment.GetData(temp.Value.tid, "ITEM_ID");
            if (mitemId == itemData.tid)
            {
                mpieceNum = temp.Value.itemNum;
            }
        }
        GameCommon.SetUIText(obj, "mpiece_num", mpieceNum.ToString()); 
        
        // 设置突破等级
        GameCommon.SetUIText(obj, "break_number_label", GameCommon.ShowAddNumUI(itemData.breakLevel));

        // 设置产生的经验
        int iDropExp = TableCommon.GetNumberFromActiveCongfig(itemData.tid, "DROP_EXP");
        DataRecord dataRecord = DataCenter.mPetLevelExpTable.GetRecord(itemData.level);
        int iAddExp = iDropExp + dataRecord.get("TOTAL_EXP_" + itemData.starLevel);
        GameCommon.SetUIText(obj, "exp_number_label", iAddExp.ToString());

        // 获取缘分数量
        int teamPos = TeamManager.mAddPetPos > 0 ? TeamManager.mAddPetPos : (int)TeamManager.mCurTeamPos;
		int iCount = GetRelateNewActivateCount(itemData.tid, teamPos);

        // 设置缘分
        GameCommon.SetUIVisiable(obj, "relate_label", iCount > 0 && mCurBagShowType == BAG_SHOW_TYPE.USE);
        GameCommon.SetUIVisiable(obj, "relate_number_label", iCount > 0);
        GameCommon.SetUIText(obj, "relate_number_label", iCount.ToString());

        return true;
    }

    protected override void SetOkBtnAction(Action action)
    {
        base.SetOkBtnAction(action);
        UIButtonEvent btnEvent = GameCommon.FindComponent<UIButtonEvent>(mGameObjUI, "pet_bag_ok_button");
        if (btnEvent != null)
        {
            //btnEvent.mAction = action;
            btnEvent.mData.set("ACTION", action);

        }
        mSelItemDataList.Clear();
        mShowItemDataList.Clear();
        
    }
	int CalculateMoney(List<PetData> list) 
	{
		int totalExp=0;
		int breakStoneMoney=0;
		int skillMoney=0;
		int baseMoney=0;
		list.ForEach(pet => {
			var starLevel=TableCommon.GetNumberFromActiveCongfig(pet.tid,"STAR_LEVEL");
			string basemoney = TableCommon.GetStringFromActiveCongfig(pet.tid, "SELL_PRICE");
			String[] kind_value=basemoney.Split('#');
			baseMoney += int.Parse(kind_value[1]);
			totalExp+=(int)DataCenter.mPetLevelExpTable.GetRecord(pet.level).get("TOTAL_EXP_"+starLevel)+pet.exp;
			
			breakStoneMoney+=(TableCommon.GetNumberFromBreakLevelConfig(pet.breakLevel,"TOTAL_ACTIVE_NUM")*(int)DataCenter.mPetRecoverConfig.GetRecord(starLevel).get("MONEY"));
			
			for(int i=0;i<pet.skillLevel.Length;i++) {
				var skillLevel=pet.skillLevel[i];
				skillMoney+=TableCommon.GetNumberFromSkillCost(skillLevel,"TOTAL_MONEY_COST");
			}
		});
		return totalExp + breakStoneMoney + skillMoney + baseMoney;
	}

	void SetGetMoney()
	{
		int money=CalculateMoney(mSelItemDataList );
		if(mSelItemDataList.Count == 0)
			money = 0;
		int tid=(int)ITEM_TYPE.GOLD;		
		GameObject sellGroup = GameCommon.FindObject (mGameObjUI, "sell_group");
		GameCommon.SetResIcon (sellGroup, "sell_rewards_icon", tid, false, true);
		GameCommon.SetUIText(sellGroup, "sell_rewards_num", "x" + money);
	}
    public static int GetRelateNewActivateCount(int upTid, int teamPos)
    {
        var targetPosData = TeamManager.GetActiveDataByTeamPos(teamPos);
        int downTid = targetPosData == null ? 0 : targetPosData.tid;
        int total = 0;

        for (int i = 0; i < (int)TEAM_POS.MAX; ++i)
        {
            var oldContext = Relationship.GetCachedContextTidSet(i);
            var newContext = Relationship.AlterContextTidSet(oldContext, upTid, downTid);

            if (i == teamPos)
            {
                var relateSet = Relationship.GetRelateTidSet(upTid);
                var addRelateSet = Relationship.FilterRelateByAlteration(new HashSet<int>(), newContext, relateSet, RelateAlteration.Inactive2Active);
                total += addRelateSet.Count;
            }
            else 
            {
                var relateSet = Relationship.GetCachedRelateTidSet(i);
                var addRelateSet = Relationship.FilterRelateByAlteration(oldContext, newContext, relateSet, RelateAlteration.Inactive2Active);
                total += addRelateSet.Count;
            }            
        }

        return total;
    }
}

public class BagPetCloseButton : CEvent
{
    public override bool _DoEvent()
    {
        base._DoEvent();
        DataCenter.SetData("BAG_PET_WINDOW", "SELECT_THEN_CLOSE", true);
        DataCenter.CloseWindow("BAG_PET_WINDOW");
        return false;
    }
}

public class BagPetOkButton : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("BAG_PET_WINDOW", "CLEAR", true);
        DataCenter.SetData("BAG_PET_WINDOW", "DO_MULTIPLE_SELECT_ACTION", true);
        return false;
    }
}

public class Button_BagPetSingleBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("BAG_PET_WINDOW", "DO_SELECT_ACTION", getObject("ITEM_DATA"));
        return true;
    }
}

public class Button_BagPetMultipleIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        object val;
        bool b = getData("BUTTON", out val);
        GameObject obj = val as GameObject;
        UIToggle toggle = obj.GetComponent<UIToggle>();
        if (toggle.value)
        {
            DataCenter.SetData("BAG_PET_WINDOW", "BUTTON", obj);
            DataCenter.SetData("BAG_PET_WINDOW", "ADD_SELECT_ITEM", getObject("ITEM_DATA"));
        }
        else
        {
            DataCenter.SetData("BAG_PET_WINDOW", "REMOVE_SELECT_ITEM", getObject("ITEM_DATA"));
            DataCenter.SetData("BAG_PET_WINDOW", "REMOVE_SELECT", getObject("ITEM_DATA"));
        }

        return true;
    }
}

public class Button_pet_sell_ok_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_PET_WINDOW", "CLEAR", true);
		DataCenter.SetData("BAG_PET_WINDOW", "DO_MULTIPLE_SELECT_ACTION", true);
		return false;
	}
}
public class Button_quality_chose_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_PET_WINDOW", "CHOSE_QUALITY_INFOS", true);
		return true;
	}
}
public class Button_bag_pet_sell_icon_btn : CEvent
{
	public override bool _DoEvent()
	{
		object val;
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		if (toggle.value)
		{
			DataCenter.SetData("BAG_PET_WINDOW", "BUTTON", obj);
			DataCenter.SetData("BAG_PET_WINDOW", "ADD_SELECT_ITEM", getObject("QUALITY_ITEM_DATA"));
			DataCenter.SetData("BAG_PET_WINDOW", "UPDATE_SELL_REWARDS", true);
		}
		else
		{
			DataCenter.SetData("BAG_PET_WINDOW", "REMOVE_SELECT_ITEM", getObject("QUALITY_ITEM_DATA"));
			DataCenter.SetData("BAG_PET_WINDOW", "REMOVE_SELECT", getObject("QUALITY_ITEM_DATA"));
			DataCenter.SetData("BAG_PET_WINDOW", "UPDATE_SELL_REWARDS", true);
		}	
		return true;
	}
}
public class Button_pet_quality_btn : CEvent
{
	public override bool DoEvent ()
	{
		int petQualityIndex = get ("PET_QUALITY_INDEX");
		object val;
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		if (toggle.value)
		{
			DataCenter.SetData("BAG_PET_WINDOW", "ADD_SELECT_QUALITY", petQualityIndex);
		}else
		{
			DataCenter.SetData("BAG_PET_WINDOW", "REMOVE_SELECT_QUALITY", petQualityIndex);		
		}
		return true;
	}
}
public class Button_chose_quality_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_PET_WINDOW", "CLOSE_QUALITY_MESSAGE", true);
		return true;
	}
}