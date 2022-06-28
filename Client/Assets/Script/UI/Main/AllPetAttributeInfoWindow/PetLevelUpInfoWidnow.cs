using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using System;
using System.Linq;
using DataTable;

public class PetLevelUpInfoWidnow : tWindow {

    public List<PetData> mSelItemDataList = new List<PetData>();
    private PetData mCurItemData;
    private List<AFFECT_TYPE> mAttributeTypeList = new List<AFFECT_TYPE>();

    public static int eatExp = 0;

    private int mAimExp = 0;
    private int mAimLevel = 0;

    //added by xuke
    // 预加经验，实际没有增加
    private int mPreAimLevel = 0;
    private int mPreAimExp = 0;
    //end

	GameObject aimPetUpgradeObj;
	GameObject petUpgradeObj;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_add_upgrade_pet_btn", new DefineFactory<Button_AddUpgradePetBtn>());
        EventCenter.Self.RegisterEvent("Button_delete_upgrade_pet_btn", new DefineFactory<Button_DeleteUpgradePetBtn>());
        EventCenter.Self.RegisterEvent("Button_auto_add_upgrade_pet_btn", new DefineFactory<Button_AutoAddUpgradePetBtn>());
        EventCenter.Self.RegisterEvent("Button_pet_level_up_btn", new DefineFactory<Button_PetLevelUpBtn>());

        mAttributeTypeList.Clear();
        mAttributeTypeList.Add(AFFECT_TYPE.ATTACK);
        mAttributeTypeList.Add(AFFECT_TYPE.HP);
        mAttributeTypeList.Add(AFFECT_TYPE.PHYSICAL_DEFENCE);
        mAttributeTypeList.Add(AFFECT_TYPE.MAGIC_DEFENCE);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "OPEN")
        {
            mSelItemDataList.Clear();
            eatExp = 0;
        }
        base.onChange(keyIndex, objVal);       

        if ("SET_ITEM_LIST" == keyIndex)
        {
            mSelItemDataList = objVal as List<PetData>;

            Refresh(null);
        }
        else if ("AUTO_ADD" == keyIndex)
        {
            int mMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, mCurItemData.level);
            if (mCurItemData.level > RoleLogicData.GetMainRole().level || (mMaxExp - mCurItemData.exp == 1 && mCurItemData.level == RoleLogicData.GetMainRole().level))
            {
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_UPGRADE_PET_HIGHER_THAN_CHARACTER);
                return;
            }

            UIGridContainer grid = GetSub("pet_icon_grid").GetComponent<UIGridContainer>();
            if (grid != null)
            {
                List<PetData> itemDataList = PetLogicData.Self.mDicPetData.Values.ToList();
                itemDataList.RemoveAll(x => x.teamPos >= 0 || x.itemId == mCurItemData.itemId || (x.starLevel >= 3 && x.tid != 30293 && x.tid != 30299&&x.tid != 30305));
                DataCenter.Set("DESCENDING_ORDER", false);
                itemDataList = GameCommon.SortList<PetData>(itemDataList, GameCommon.SortPetDataByStarLevel);
                int iIndex = mSelItemDataList.Count;
				if( itemDataList.Count == 0)
				{
                    DataCenter.OpenWindow(UIWindowString.access_to_res_window, 30305);
                    return;
                }
                for (int i = 0; i < itemDataList.Count; i++)
                {
                    PetData itemData = itemDataList[i];
                    if (itemData != null)
                    {
                        if (!itemData.IsInTeam() && iIndex < grid.MaxCount && !IsItemSel(itemData.itemId))
                        {
             
                            itemData.mGridIndex = iIndex;
                            GameObject obj = grid.controlList[iIndex];
                            NiceData data = GameCommon.GetButtonData(obj, "delete_upgrade_pet_btn");
                            if (data != null)
                            {
                                data.set("ITEM_ID", itemData.itemId);
                            }
                            mSelItemDataList.Add(itemData);
                            iIndex++;
                            if (eatExp > 0)
                            {
                                mSelItemDataList.Remove(itemData);
                                iIndex--;
                                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_UPGRADE_PET_HIGHER_THAN_CHARACTER);
                                ResetPreLevelInfo();
                                break;
                            }
                            //added by xuke
                            //判断是否超过了角色等级
                            int _totalExp = GetSelItemTotalExp();
                            ResetPreLevelInfo();
                            eatExp = 0;
                            PreAddExp(_totalExp);
                            ResetPreLevelInfo();
                            if (eatExp > 0)
                            {
                               // mPreAimExp = mCurItemData.exp;
                                break;
                            }
                            //if (mPreAimLevel > RoleLogicData.Self.character.level)
                            //{
                            //    mSelItemDataList.Remove(itemData);
                            //    iIndex--;
                            //    ResetPreLevelInfo();
                            //    break;
                            //}
                            
                            //end

                            if (iIndex >= itemDataList.Count)
                                break;
                        }
                    }
                }
            }

            Refresh(null);
        }
        else if ("REQUEST_UPGRADE" == keyIndex)
        {
            if (!UpgradeCondition())
                return;

            CS_PetUpgrade petUpgrade = new CS_PetUpgrade();
			aimPetUpgradeObj.SetActive (true);
			petUpgradeObj.SetActive (true);
			GameCommon.FindObject (mGameObjUI, "box_colloder_tips").gameObject.SetActive (true);
			GlobalModule.DoLater (() => GameCommon.FindObject (mGameObjUI, "box_colloder_tips").gameObject.SetActive (false), 0.6f);

            ItemDataBase[] list = new ItemDataBase[mSelItemDataList.Count];
            for (int i = 0; i < mSelItemDataList.Count; i++)
            {
                PetData itemData = mSelItemDataList[i];
                ItemDataBase item = new ItemDataBase();
                item.itemId = itemData.itemId;
                item.tid = itemData.tid;
                item.itemNum = itemData.itemNum;
                list[i] = item;
            }
            petUpgrade.arr = list;
            petUpgrade.itemId = mCurItemData.itemId;
            petUpgrade.tid = mCurItemData.tid;
            petUpgrade.pt = "CS_PetUpgrade";
            HttpModule.Instace.SendGameServerMessage(petUpgrade, x => RequestPetUpgradeSuccess(petUpgrade, x), RequestPetUpgradeFail);
        }
        else if ("REMOVE_SELECT_ITEM" == keyIndex)
        {
            DeleteSelPetUI((int)objVal);
        }
        else if ("OPEN_PET_BAG" == keyIndex)
        {
            OpenPetBag();
        }
    }

    private bool UpgradeCondition()
    {
        if (mSelItemDataList.Count <= 0)
        {
            DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_SELECT);
            return false;
        }
        if (mCurItemData.level >= mCurItemData.mMaxLevelNum)
        {
			DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_LEVEL_MAX);
            return false;
        }
        if (mCurItemData.level > RoleLogicData.GetMainRole().level)
        {
			DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_HIGHER_THAN_CHARACTER);
            return false;
        }
     //   if (GetSelItemTotalExp()-eatExp> RoleLogicData.Self.gold)
        if (GetSelItemTotalExp() > RoleLogicData.Self.gold)
        {
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
			//DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_PET_OPERATE_NEED_COIN);
            return false;
        }
        return true;
    }

    private void OpenPetBag()
    {
        if (mCurItemData.level > RoleLogicData.GetMainRole().level)
        {
			DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_HIGHER_THAN_CHARACTER);
            return;
        }

        OpenBagObject<PetData> openObj = new OpenBagObject<PetData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.UPGRADE;
        openObj.mFilterCondition = (itemData) =>
        {
            return !itemData.IsInTeam() && itemData.itemId != mCurItemData.itemId && itemData.inFairyland == 0;
        };

        openObj.mSortCondition = (tempList) =>
        {
            DataCenter.Set("DESCENDING_ORDER", false);
            return GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByStarLevel);
        };

        openObj.mMultipleSelectAction = (itemDataList) =>
        {
            if (null == itemDataList)
                return;

            DataCenter.SetData("PET_LEVEL_UP_INFO_WINDOW", "SET_ITEM_LIST", itemDataList.ToList());
        };

        openObj.mSelectList = mSelItemDataList.ToList();

        DataCenter.SetData("BAG_PET_WINDOW", "OPEN", openObj);
        DataCenter.SetData("BAG_PET_WINDOW", "SET_UPGRADE_PET_DATA", mCurItemData);
        
    }

    private int mAddLevel = 0;  //> 符灵升级增加等级
     private void RequestPetUpgradeSuccess(CS_PetUpgrade petUpgrade, string text)
    {
        SC_PetUpgrade upgrade = JCode.Decode<SC_PetUpgrade>(text);

		GlobalModule.DoLater (() => aimPetUpgradeObj.SetActive (false), 0.55f);
		GlobalModule.DoLater (() => petUpgradeObj.SetActive (false), 0.55f);
		
		PackageManager.RemoveItem(petUpgrade.arr.ToList<ItemDataBase>());

        PetData petData = PetLogicData.Self.GetPetDataByItemId(petUpgrade.itemId);

        if (petData.level < mAimLevel)
        {
            GameCommon.PlaySound("Sound/uisound/Pet Lv Up Sound", GameCommon.GetMainCamera().transform.position);
        }

        if (petData != null)
        {
            AddPetUpgradeChangeTip(petData, mAimLevel);
            mAddLevel = mAimLevel - petData.level;
            if (petData.level != mAimLevel) 
            {
                CommonParam.mIsRefreshPetUpgradeInfo = false;
            }
            petData.level = mAimLevel;
            petData.exp = mAimExp;
        }
		int _totalAddedExp = GetSelItemTotalExp ()-eatExp;
        //by chenliang
        //begin

//         mSelItemDataList.Clear();
// 
//         PackageManager.RemoveItem((int)ITEM_TYPE.GOLD, -1, GetSelItemTotalExp());
//        // PackageManager.RemoveItem((int)ITEM_TYPE.GOLD, -1, _totalAddedExp);
//-----------------
        //在调用GetSelItemTotal之前不能清除mSelItemDataList.Clear()
        PackageManager.RemoveItem((int)ITEM_TYPE.GOLD, -1, GetSelItemTotalExp());
        mSelItemDataList.Clear();

        //end

        GlobalModule.DoLater (() => Refresh(null), 0.3f);
        
        //刷新符灵背包界面
        tWindow window = DataCenter.GetData("TEAM_PET_PACKAGE_WINDOW") as tWindow;
        if (window != null && window.IsOpen())
        {
            DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "REFRESH", 0);
            DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "SET_TOGGLE_BY_CURRENT_ITEM", null);
        }

		GlobalModule.DoLater(() => DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_PET_ADD_EXP, _totalAddedExp.ToString()), 0.6f);

		TeamInfoWindow _tWindow = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
		if(_tWindow != null)
		{
			ActiveData _tmp = TeamManager.GetActiveDataByTeamPos (mCurItemData.teamPos);
//			GlobalModule.DoCoroutine(_tWindow.SetTipsAttribute(_tmp));
            DataCenter.Set("CHANGE_TIP_PANEL_TYPE", ChangeTipPanelType.PET_LEVEL_UP_INFO_WINDOW);
           
			_tWindow.SetTipsAttribute(_tmp);
			_tWindow.ChangeFitting();
            ChangeTipManager.Self.PlayAnim();
		}
        //added by xuke 红点相关
        TeamNewMarkManager.Self.CheckPetLevelUp();
        TeamNewMarkManager.Self.RefreshTeamNewMark();
        //end
//        DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_ADD_EXP, mAimExp.ToString());
    }

    private void RequestPetUpgradeFail(string text)
    {
		GlobalModule.DoLater (() => GameCommon.FindObject (mGameObjUI, "box_colloder_tips").gameObject.SetActive (false), 0.6f);
    }
    /// <summary>
    /// 添加符灵升级等级变化反馈文字
    /// </summary>
    /// <param name="kPetData"></param>
    /// <param name="kAimLevel"></param>
    private void AddPetUpgradeChangeTip(PetData kPetData,int kAimLevel) 
    {
        if (kAimLevel == kPetData.level)
            return;
        string _levelShowInfo = "灵将等级 +" + (kAimLevel - kPetData.level).ToString();
        ChangeTip _levelTip = new ChangeTip() { Content = _levelShowInfo,TargetValue = kAimLevel};
        _levelTip.SetTargetObj(ChangeTipPanelType.PET_LEVEL_UP_INFO_WINDOW,ChangeTipValueType.PET_UPGRADE_LEVEL);
        ChangeTipManager.Self.Enqueue(_levelTip, (int)ChangeTipPriority.PET_LEVEL);
    }

    protected virtual bool IsItemSel(int iItemid)
    {
        return mSelItemDataList.Exists(x => x.itemId == iItemid);
    }

    public override void OnOpen()
    {
        base.OnOpen();

		petUpgradeObj = GameCommon.FindObject (mGameObjUI, "pet_upgrade_tips").gameObject;
		aimPetUpgradeObj = GameCommon.FindObject (mGameObjUI, "ui_shengji_bao").gameObject;
		aimPetUpgradeObj.SetActive (false);
		petUpgradeObj.SetActive (false);

        Refresh(null);
    }

    public override void OnClose()
    {
        mSelItemDataList.Clear();
        if (ChangeTipManager.Self != null)
        {
            ChangeTipManager.Self.CheckChangeTipShowState(mGameObjUI);
        }
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        mCurItemData = TeamPosInfoWindow.mCurActiveData as PetData;
        
        UpdateUI();

        return true;
    }

    /// <summary>
    /// 删除所选物品UI的刷新
    /// </summary>
    private void DeleteSelPetUI(int iItemId)
    {
        DeleteSelItemIcon(iItemId);
        RemoveSelectItem(iItemId);
        UpdateLevel();
        UpdateNeedGoldUI();
    }

    /// <summary>
    /// 删除所选物品icon
    /// </summary>
    /// <param name="iItemId"></param>
    private void DeleteSelItemIcon(int iItemId)
    {
        if (null == mCurItemData)
            return;

        UIGridContainer grid = GetSub("pet_icon_grid").GetComponent<UIGridContainer>();
        PetData itemData = mSelItemDataList.Find(x => x.itemId == iItemId);
        if (grid != null && itemData != null)
        {
            int iIndex = itemData.mGridIndex;
            GameObject obj = grid.controlList[itemData.mGridIndex];
            if (obj != null)
            {
                GameObject addUpgradeBtn = GameCommon.FindObject(obj, "add_upgrade_pet_btn");
                GameObject delUpgradeBtn = GameCommon.FindObject(obj, "delete_upgrade_pet_btn");

                delUpgradeBtn.SetActive(false);
                addUpgradeBtn.SetActive(true);
            }
			GameObject petObj = petUpgradeObj.transform.Find ("ui_shengji_line" + itemData.mGridIndex.ToString()).gameObject;
			petObj.SetActive (false);
        }
    }

    /// <summary>
    /// 删除所选法器
    /// </summary>
    /// <param name="iItemId">itemId</param>
    /// <returns>删除结果</returns>
    protected bool RemoveSelectItem(int iItemId)
    {
        PetData itemData = PetLogicData.Self.GetPetDataByItemId(iItemId);
        if (itemData != null && mSelItemDataList.Contains(itemData))
        {
            mSelItemDataList.Remove(itemData);
            int _totalExp = GetSelItemTotalExp();
            ResetPreLevelInfo();
            eatExp = 0;
            PreAddExp(_totalExp);
            ResetPreLevelInfo();
            return true;
        }
        return false;
    }

    private void UpdateUI()
    {
        UpdateItemIconInfo();
        UpdateSelItemlistIcon();
        UpdateLevel();        

        UpdateNeedGoldUI();
    }

    private void UpdateItemIconInfo()
    {
        if (null == mCurItemData)
            return;

        UISprite sprite = mGameObjUI.transform.Find("pet_level_up_info/aim_pet_icon/Background").GetComponent<UISprite>();
        GameCommon.SetItemIcon(sprite, PackageManager.GetItemTypeByTableID(mCurItemData.tid), mCurItemData.tid);

        UILabel nameLabel = mGameObjUI.transform.Find("pet_level_up_info/label_info/pet_name_label").GetComponent<UILabel>();
        if (nameLabel != null)
        {
            nameLabel.text = TableCommon.GetStringFromActiveCongfig(mCurItemData.tid, "NAME");
        }

        UILabel breakLevelLabel = mGameObjUI.transform.Find("pet_level_up_info/label_info/pet_name_label/pet_break_number_label").GetComponent<UILabel>();
        if (breakLevelLabel != null)
        {
            if (mCurItemData.breakLevel != 0)
            {
                breakLevelLabel.text = "+" + mCurItemData.breakLevel.ToString();
            }
            else
            {
                breakLevelLabel.text = "";
            }
        }
    }

    private void UpdateSelItemlistIcon()
    {
        if (null == mCurItemData)
            return;

        UIGridContainer grid = GetSub("pet_icon_grid").GetComponent<UIGridContainer>();
        if(grid != null)
        {
            for (int i = 0; i < grid.MaxCount; i++)
            {
                GameObject obj = grid.controlList[i];
                if (obj != null)
                {
                    GameObject addUpgradeBtn = GameCommon.FindObject(obj, "add_upgrade_pet_btn");
                    GameObject delUpgradeBtn = GameCommon.FindObject(obj, "delete_upgrade_pet_btn");

                    delUpgradeBtn.SetActive(i < mSelItemDataList.Count);
                    addUpgradeBtn.SetActive(i >= mSelItemDataList.Count);

                    if (delUpgradeBtn != null && addUpgradeBtn != null)
                    {
                        if (i < mSelItemDataList.Count)
                        {
                            PetData itemData = mSelItemDataList[i];
                            itemData.mGridIndex = i;
                            NiceData data = GameCommon.GetButtonData(obj, "delete_upgrade_pet_btn");
                            if (data != null)
                            {
                                data.set("ITEM_ID", itemData.itemId);
                            }

                            GameCommon.SetItemIcon(delUpgradeBtn.GetComponent<UISprite>(), PackageManager.GetItemTypeByTableID(itemData.tid), itemData.tid);
                        }
                    }
                }
				GameObject petObj = petUpgradeObj.transform.Find ("ui_shengji_line" + i.ToString()).gameObject;
				petObj.SetActive (i < mSelItemDataList.Count);
            }
        }
    }

    private void UpdateLevel()
    {
        if(null == mCurItemData)
            return;

        UIProgressBar curLevelUpBar = mGameObjUI.transform.Find("level_stuff_info/Level_up_bar/cur_level_up_bar").GetComponent<UIProgressBar>();
        UIProgressBar AimLevelUpBar = mGameObjUI.transform.Find("level_stuff_info/Level_up_bar/aim_level_up_bar").GetComponent<UIProgressBar>();
        
        // cue exp bar
        int curExp = mCurItemData.exp;
        int maxExp = mCurItemData.GetMaxExp();
        float percentage = curExp / (float)maxExp;
        int iPercentage = (int)(percentage * 100);
        curLevelUpBar.value = percentage;
        
        // aim exp bar
        int iDExp = GetSelItemTotalExp();

		mAimExp = curExp;
        mAimLevel = mCurItemData.level;
        AddExp(iDExp-eatExp);
        float aimPercentage = mAimExp / (float)maxExp;
        AimLevelUpBar.value = mAimLevel > mCurItemData.level ? 1 : aimPercentage;
        AimLevelUpBar.gameObject.SetActive(iDExp-eatExp > 0);

        // exp
        UILabel getExplabel = GameCommon.FindComponent<UILabel>(curLevelUpBar.gameObject, "get_exp_label");
        if (getExplabel != null)
        {
            getExplabel.text = curExp.ToString();
        }
        UILabel needExplabel = GameCommon.FindComponent<UILabel>(curLevelUpBar.gameObject, "need_exp_label");
        if (needExplabel != null)
        {
            needExplabel.text = maxExp.ToString();
        }

        // level
        UILabel levelLabel = GameCommon.FindComponent<UILabel>(curLevelUpBar.gameObject, "level_label");
        if (levelLabel != null)
        {
            if (CommonParam.mIsRefreshPetUpgradeInfo)
            {
                levelLabel.text = mCurItemData.level.ToString();
            }
            else 
            {
                levelLabel.text = (mCurItemData.level - mAddLevel).ToString();
            }
            string str = "pet_pool_info_window/pet_introduce/team_pet_info_card/card_group_window_army(Clone)/info/level_label";
			if(GameObject.Find(str)!=null)
			{GameObject.Find(str).GetComponent<UILabel>().text = mCurItemData.level.ToString();}
        }
       // GameObject team_info_grid=mGameObjUI.transform.Find("team_info_window/team_info_grid").gameObject;
        for (int i = 0; i <= 3; i++) 
        {
            string path="team_info_window/team_info_grid/team_info(Clone)_"+i.ToString();
            if (GameObject.Find(path) != null)
            {
                path = path + "/team_body_info_card" + i.ToString() + "/card_group_window_army(Clone)/info/level_label";
                GameObject.Find(path).GetComponent<UILabel>().text = mCurItemData.level.ToString();
            }
                
        }

        UILabel addLabel = GameCommon.FindComponent<UILabel>(curLevelUpBar.gameObject, "add_label");
        if (addLabel != null)
        {
            addLabel.text = GameCommon.ShowAddNumUI(mAimLevel - mCurItemData.level);
        }

        UpdateAttribute();
    }

    private void UpdateAttribute()
    {
        if (null == mCurItemData)
            return;

        UIGridContainer grid = mGameObjUI.transform.Find("level_stuff_info/Level_up_bar/grid").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            for (int i = 0; i < grid.MaxCount; i++)
            {
                GameObject obj = grid.controlList[i];
                if (obj != null)
                {
                    UILabel baseNumberLabel = GameCommon.FindComponent<UILabel>(obj, "pet_base_number_label");
                    UILabel addNumberLabel = GameCommon.FindComponent<UILabel>(obj, "pet_add_number_label");
					UILabel attributeNameLabel = GameCommon.FindComponent<UILabel>(obj, "attribute_name_label");

                    int iDLevel = mAimLevel - mCurItemData.level;
                    int iCurNum = mCurItemData.GetAttribute(mAttributeTypeList[i]);
                    mCurItemData.level += iDLevel;
                    int iAimNum = mCurItemData.GetAttribute(mAttributeTypeList[i]);
                    mCurItemData.level -= iDLevel;
                    if (CommonParam.mIsRefreshPetUpgradeInfo)
                    {
                        baseNumberLabel.text = iCurNum.ToString();
                    }
                    else 
                    {
                        mCurItemData.level -= mAddLevel;
                        int _iCurNum = mCurItemData.GetAttribute(mAttributeTypeList[i]);
                        baseNumberLabel.text = _iCurNum.ToString();
                        mCurItemData.level += mAddLevel;
                    }
                    attributeNameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig(Convert.ToInt32(mAttributeTypeList[i]), "NAME");

                    DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(mCurItemData.tid);
                    if (dataRecord == null)
                        return;

                    int addValue = dataRecord["ADD_" + mAttributeTypeList[i].ToString()];
                    int breakValue = dataRecord["BREAK_" + mAttributeTypeList[i].ToString()];
                    int baseBreakValue = dataRecord["BASE_BREAK_" + mAttributeTypeList[i].ToString()];
                    int addnum = (mAimLevel - mCurItemData.level) * (addValue + mCurItemData.breakLevel * breakValue);

                    addNumberLabel.text = GameCommon.ShowAddNumUI(iAimNum - iCurNum);
                }
            }
        }
        CommonParam.mIsRefreshPetUpgradeInfo = true;
    }


    // 预加经验值，实际不增加
    private void PreAddExp(int dExp)
    {
        if (mPreAimLevel >= mCurItemData.mMaxLevelNum) 
        {
            mPreAimLevel = mCurItemData.mMaxLevelNum;
            return;
        }
        mPreAimExp += dExp;
        int iMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, mPreAimLevel);
        if (mPreAimExp >= iMaxExp) 
        {
            PreLevelUp();
            if (eatExp > 0)
                return;
            int iDExp = mPreAimExp - iMaxExp;

            mPreAimExp = 0;
            PreAddExp(iDExp);
        }
    }

    private void PreLevelUp() 
    {
        if (mPreAimLevel >= mCurItemData.mMaxLevelNum)
            return;
        if (mPreAimLevel == RoleLogicData.Self.character.level)
        {
            int iMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, mPreAimLevel);
            eatExp = mPreAimExp - iMaxExp + 1;
            return;
        }
        mPreAimLevel += 1;
    }

    private void ResetPreLevelInfo() 
    {
        mPreAimLevel = mCurItemData.level;
        mPreAimExp = mCurItemData.exp;
    }

    public virtual void AddExp(int dExp)
    {
        if (mAimLevel >= mCurItemData.mMaxLevelNum)
        {
            mAimLevel = mCurItemData.mMaxLevelNum;
            return;
        }

        mAimExp += dExp;

        int iMaxExp = TableCommon.GetMaxExp(mCurItemData.starLevel, mAimLevel);
        if (mAimExp >= iMaxExp)
        {
            LevelUp();

            int iDExp = mAimExp - iMaxExp;

            mAimExp = 0;
            AddExp(iDExp);
        }
    }

    public virtual void LevelUp()
    {
        if (mAimLevel >= mCurItemData.mMaxLevelNum)
            return;
        mAimLevel += 1;
    }

    private void UpdateNeedGoldUI()
    {
        if (null == mCurItemData)
            return;

        UILabel needGoldLabel = mGameObjUI.transform.Find("need_gold/need_gold_num").GetComponent<UILabel>();
        if (needGoldLabel != null)
        {
            needGoldLabel.text = GameCommon.CheckCostNumColor(GetSelItemTotalExp()-eatExp, RoleLogicData.Self.gold);
        }
    }

    private int GetSelItemTotalExp()
    {
        int iExp = 0;
        foreach (PetData itemData in mSelItemDataList)
        {
            DataRecord dataRecord = DataCenter.mPetLevelExpTable.GetRecord(itemData.level);
            if (dataRecord != null)
            {
                iExp += TableCommon.GetNumberFromActiveCongfig(itemData.tid, "DROP_EXP") + dataRecord["TOTAL_EXP_" + itemData.starLevel];
            }
        }

        return iExp;
    }

}

class Button_AddUpgradePetBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.Set("I_AM_FROM_TEAMINO", true);
        DataCenter.SetData("PET_LEVEL_UP_INFO_WINDOW", "OPEN_PET_BAG", true);
        DataCenter.Set("I_AM_FROM_PET_LEVEL_UP",true);
        return true;
    }
}

class Button_DeleteUpgradePetBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PET_LEVEL_UP_INFO_WINDOW", "REMOVE_SELECT_ITEM", (int)get("ITEM_ID"));
        return true;
    }
}

class Button_AutoAddUpgradePetBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PET_LEVEL_UP_INFO_WINDOW", "AUTO_ADD", true);
        return true;

    }
}

class Button_PetLevelUpBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PET_LEVEL_UP_INFO_WINDOW", "REQUEST_UPGRADE", true);
        return true;
    }
}