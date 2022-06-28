using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;
using System.Linq;
using System;

public class MagicStrengthenInfoWindow : tWindow
{
    public List<EquipData> mSelItemDataList = new List<EquipData>();
    private EquipData mCurItemData;
    
    private int mAimStrengthenExp = 0;
    private int mAimStrengthenLevel = 0;

	List<EquipData> equipList = new List<EquipData>();
	int curMasterLevel = 0;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_add_strengthen_magic_btn", new DefineFactory<Button_AddStrengthenMagicBtn>());
        EventCenter.Self.RegisterEvent("Button_delete_strengthen_magic_btn", new DefineFactory<Button_DeleteStrengthenMagicBtn>());
        EventCenter.Self.RegisterEvent("Button_auto_add_strengthen_magic_btn", new DefineFactory<Button_AutoAddStrengthenMagicBtn>());
        EventCenter.Self.RegisterEvent("Button_magic_strengthen_btn", new DefineFactory<Button_MagicStrengthenBtn>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        if ("SET_ITEM_LIST" == keyIndex)
        {
            mSelItemDataList = objVal as List<EquipData>;

            Refresh(null);
        }
        else if ("AUTO_ADD" == keyIndex)
        {
            if (mAimStrengthenLevel >= mCurItemData.mMaxStrengthenLevel)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_STRENGTHEN_AIM_LEVEL_MAX);
                return;
            }

            if (mCurItemData.strengthenLevel >= mCurItemData.mMaxStrengthenLevel)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_STRENGTHEN_MAX_LEVEL);
                return;
            }

            UIGridContainer grid = GetSub("magic_icon_grid").GetComponent<UIGridContainer>();
            if (grid != null)
            {
                List<EquipData> itemDataList = MagicLogicData.Self.mDicEquip.Values.ToList();
                itemDataList.RemoveAll(x => x.teamPos >= 0 || x.itemId == mCurItemData.itemId);
                List<EquipData> exp_magic=new List<EquipData>();
                List<EquipData> unexp_magic=new List<EquipData>();
                foreach(EquipData temp in itemDataList)
                {
                   int magic_type=DataCenter.mRoleEquipConfig.GetData(temp.tid,"EQUIP_TYPE");
                   if (magic_type == 5)
                   {
                       exp_magic.Add(temp);
                   }
                   else if(temp.mStarLevel<4)
                   {
                       unexp_magic.Add(temp);
                   }
                }
                itemDataList.Clear();
                exp_magic.Sort((left,right)=>{
                    if ((int)left.mQualityType < (int)right.mQualityType)
                    {
                        return -1;
                    }
                    else if ((int)left.mQualityType < (int)right.mQualityType)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                });
                foreach(EquipData temp in exp_magic)
                {
                    itemDataList.Add(temp);
                }
                unexp_magic.Sort((left,right)=>{
                    if ((int)left.mQualityType < (int)right.mQualityType)
                    {
                        return -1;
                    }
                    else if ((int)left.mQualityType < (int)right.mQualityType)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                });
                foreach (EquipData temp in unexp_magic)
                {
                    itemDataList.Add(temp);
                }                 
                int iIndex = mSelItemDataList.Count;
                for (int i = 0; i < itemDataList.Count; i++)
                {
                    EquipData itemData = itemDataList[i];
                    if (itemData != null)
                    {
                        if (!itemData.IsInTeam() && iIndex < grid.MaxCount && !IsItemSel(itemData.itemId))
                        {
                            itemData.mGridIndex = iIndex;
                            GameObject obj = grid.controlList[iIndex];
                            NiceData data = GameCommon.GetButtonData(obj, "delete_strengthen_magic_btn");
                            if (data != null)
                            {
                                data.set("ITEM_ID", itemData.itemId);
                            }
                            mSelItemDataList.Add(itemData);
                            iIndex++;

                            if(iIndex >= itemDataList.Count)
                                break;
                        }
                    }
                }
            }

            Refresh(null);
        }
        else if ("REQUEST_UPGRADE" == keyIndex)
        {
            if (!StrengthenCondition())
                return;

            CS_StrengthenMagic petUpgrade = new CS_StrengthenMagic();

            ItemDataBase[] list = new ItemDataBase[mSelItemDataList.Count];
            for (int i = 0; i < mSelItemDataList.Count; i++)
            {
                EquipData itemData = mSelItemDataList[i];
                ItemDataBase item = new ItemDataBase();
                item.itemId = itemData.itemId;
                item.tid = itemData.tid;
                item.itemNum = itemData.itemNum;
                list[i] = item;
            }
            petUpgrade.arr = list;
            petUpgrade.itemId = mCurItemData.itemId;
            petUpgrade.tid = mCurItemData.tid;
            petUpgrade.pt = "CS_StrengthenMagic";
            HttpModule.Instace.SendGameServerMessage(petUpgrade, x => RequestMaigcStrengthenSuccess(petUpgrade, x), RequestMaigcStrengthenFail);
        }
        else if ("REMOVE_SELECT_ITEM" == keyIndex)
        {
            DeleteSelMaigcUI((int)objVal);
        }
        else if ("OPEN_MAGIC_BAG" == keyIndex)
        {
            OpenMagicBag();
        }
    }

    private bool StrengthenCondition()
    {
        if (mSelItemDataList.Count <= 0)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_STRENGTHEN_NEED_MAGIC);
            return false;
        }
        if (mCurItemData.strengthenLevel >= mCurItemData.mMaxStrengthenLevel)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_STRENGTHEN_MAX_LEVEL);
            return false;
        }
        if (GetSelItemTotalExp() > RoleLogicData.Self.gold)
        {
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
            return false;
        }
        return true;
    }

    private void OpenMagicBag()
    {
        if (mAimStrengthenLevel >= mCurItemData.mMaxStrengthenLevel)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_STRENGTHEN_AIM_LEVEL_MAX);
            return;
        }

        if (mCurItemData.strengthenLevel >= mCurItemData.mMaxStrengthenLevel)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_STRENGTHEN_MAX_LEVEL);
            return;
        }

        OpenBagObject<EquipData> openObj = new OpenBagObject<EquipData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.UPGRADE;
        openObj.mFilterCondition = (itemData) =>
        {
            return !itemData.IsInTeam() && itemData.itemId != mCurItemData.itemId;
        };

        openObj.mMultipleSelectAction = (itemDataList) =>
        {
            if (itemDataList != null)
            {
                DataCenter.SetData("MAGIC_STRENGTHEN_INFO_WINDOW", "SET_ITEM_LIST", itemDataList.ToList());
            }
        };

        openObj.mSelectList = mSelItemDataList.ToList();
        DataCenter.SetData("BAG_MAGIC_WINDOW", "OPEN", openObj);
    }

    /// <summary>
    /// 删除所选法器
    /// </summary>
    /// <param name="iItemId">itemId</param>
    /// <returns>删除结果</returns>
    protected bool RemoveSelectItem(int iItemId)
    {
        EquipData itemData = MagicLogicData.Self.GetEquipDataByItemId(iItemId);
        if (itemData != null && mSelItemDataList.Contains(itemData))
        {
            mSelItemDataList.Remove(itemData);
            return true;
        }
        return false;
    }

    private int mStrengthenAddLevel = 0;    //> 强化增加的等级
    private void RequestMaigcStrengthenSuccess(CS_StrengthenMagic magicStrengthen, string text)
    {
        SC_StrengthenMagic upgrade = JCode.Decode<SC_StrengthenMagic>(text);

        PackageManager.RemoveItem(magicStrengthen.arr.ToList<ItemDataBase>());

        EquipData itemData = MagicLogicData.Self.GetEquipDataByItemId(magicStrengthen.itemId);
        if (itemData != null)
        {
            //added by xuke 属性变化反馈文字效果
            mStrengthenAddLevel = mAimStrengthenLevel - itemData.strengthenLevel;
            AddMagicStrengthenChangeTip(itemData,mStrengthenAddLevel);
            CommonParam.mIsRefreshMagicStrengthenInfo = false;
            //end
            itemData.strengthenLevel = mAimStrengthenLevel;
            itemData.strengthenExp = mAimStrengthenExp;
        }

        int _totalAddedExp = GetSelItemTotalExp();

        mSelItemDataList.Clear();

        PackageManager.RemoveItem((int)ITEM_TYPE.GOLD, -1, _totalAddedExp);

        Refresh(null);      

        tWindow window = DataCenter.GetData("PACKAGE_EQUIP_WINDOW") as tWindow;
        if (window != null && window.IsOpen())
		    DataCenter.SetData ("PACKAGE_EQUIP_WINDOW","REFRESH_MAGIC_BAG_GROUP",null);

        GlobalModule.DoLater(() => DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_MAGIC_ADD_EXP, _totalAddedExp.ToString()), 0);
        if (GameObject.Find("team_info_window") != null)
        {
            GameObject mteamInfoWindow = GameCommon.FindObject(GameObject.Find("UI Root"), "team_info_window");
            UIGridContainer mTeamInfoGrid = GameCommon.FindComponent<UIGridContainer>(mteamInfoWindow, "team_info_grid");
            GameObject obj = mTeamInfoGrid.controlList[itemData.teamPos];
            UIGridContainer itemIconGrid = GameCommon.FindComponent<UIGridContainer>(obj, "magic_info_grid");
            int packageEquipType = TableCommon.GetNumberFromRoleEquipConfig(itemData.tid, "EQUIP_TYPE");
            GameObject itemObj = itemIconGrid.controlList[packageEquipType - 6];   //

            GameObject mlevel = GameCommon.FindObject(itemObj, "level_bg");

            GameCommon.FindObject(mlevel, "level_label").SetActive(true);
            GameCommon.FindObject(mlevel, "level_label").GetComponent<UILabel>().text = itemData.strengthenLevel.ToString();
            GameCommon.FindObject(mlevel, "level_label").GetComponent<UILabel>().fontSize = 16;
        }
		TeamInfoWindow _tWindow = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
		if(_tWindow != null)
		{
			ActiveData _tmp = TeamManager.GetActiveDataByTeamPos (mCurItemData.teamPos);
			//			GlobalModule.DoLater (() => _tWindow.SetTipsAttribute(_tmp), 0.9f);
//			GlobalModule.DoCoroutine(_tWindow.SetTipsAttribute(_tmp));
			if(equipList.Count >= 2)
			{
				int aa = GameCommon.GetMinMosterLevel("MAGIC_STERNG_LEVEL", equipList, GetMinStrengthLevel(equipList));
				if (aa > curMasterLevel)
				{
                    string str = "[ff9900]神器强化大师[-] [99ff66]{0}[-] 级达成！";
					_tWindow.SetTipsMaster(str, aa);
					curMasterLevel = aa;					
				}
			}
			_tWindow.SetTipsAttribute(_tmp);
			_tWindow.ChangeFitting();
            ChangeTipManager.Self.PlayAnim(() => 
            {
                //UpdateLevel_ChangeTip();
                //UpdateAttribute(); 
            });
		}
        //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_ADD_EXP, mAimStrengthenExp.ToString());
        //added by xuke 法器强化刷新红点
        TeamNewMarkManager.Self.CheckMagicEquipStrengthen();
        TeamNewMarkManager.Self.RefreshTeamNewMark();
        //end
    }

    private void RequestMaigcStrengthenFail(string text)
    {

    }
    /// <summary>
    /// 添加法器强化反馈文字
    /// </summary>
    private void AddMagicStrengthenChangeTip(EquipData kEquipData,int kAddLevel) 
    {
        if (kAddLevel == 0)
            return;

        UIGridContainer grid = GetSub("attribute_label_Grid").GetComponent<UIGridContainer>();
        string _addLevelInfo = "强化等级 +" + kAddLevel;
        ChangeTip _levelTip = new ChangeTip() { Content = _addLevelInfo, TargetValue = mAimStrengthenLevel };
        _levelTip.SetTargetObj(ChangeTipPanelType.MAGIC_STRENGTHEN_INFO_WINDOW, ChangeTipValueType.MAGIC_STRENGTHEN_LEVEL);
        ChangeTipManager.Self.Enqueue(_levelTip, (int)ChangeTipPriority.STRENGTHEN_LEVEL);
        
        for (int i = 0; i < grid.MaxCount; i++)
        {
            GameObject obj = grid.controlList[i];
            if (obj != null)
            {
                int iAttributeType = TableCommon.GetNumberFromRoleEquipConfig(kEquipData.tid, "ATTRIBUTE_TYPE_" + i.ToString());
                string _attrName = TableCommon.GetStringFromEquipAttributeIconConfig(iAttributeType, "NAME");
                float _targetValue = RoleEquipData.GetEquipBaseAttributeValue(i, kEquipData.tid, mAimStrengthenLevel);
                float _beforeValue = RoleEquipData.GetEquipBaseAttributeValue(i, kEquipData.tid, kEquipData.strengthenLevel);
                float _addAttrValue = _targetValue - _beforeValue;

                string strAttributeType = GameCommon.ToAffectTypeString((AFFECT_TYPE)iAttributeType);
                bool bIsRate = strAttributeType.LastIndexOf("_RATE") > 0;

                _addAttrValue = _addAttrValue / 10000;
                string strValue = "";
                if (bIsRate)
                {
                    _addAttrValue *= 100;
                    strValue = (_addAttrValue + 0.001f).ToString("f2")+"%";
                }
                else
                {
                    _addAttrValue = (float)((int)_addAttrValue);
                    strValue = _addAttrValue.ToString();
                }

                float _showTargetValue = bIsRate ? _targetValue / 100 : _targetValue / 10000;

                string _addAttrInfo = _attrName + " +" + strValue;
                int _precision = bIsRate ? 2 : 0;
                ChangeTip _strengthAttrTip = new ChangeTip() { Content = _addAttrInfo, TargetValue = _showTargetValue ,Precision = _precision};
                _strengthAttrTip.SetTargetObj(ChangeTipPanelType.MAGIC_STRENGTHEN_INFO_WINDOW, ChangeTipValueType.MAGIC_STRENGTHEN_ATTR_BASE + i + 1);
                ChangeTipManager.Self.Enqueue(_strengthAttrTip, (int)ChangeTipPriority.STRENGTHEN_ATTR);

            }
        }
    }

    protected virtual bool IsItemSel(int iItemid)
    {
        return mSelItemDataList.Exists(x => x.itemId == iItemid);
    }

    public override void Open(object param)
    {
        base.Open(param);

        mSelItemDataList.Clear();

        mCurItemData = param as EquipData;

		for (int i = 0; i < 2; i++) 
		{
			var equip = TeamManager.GetMagicDataByCurTeamPos(i);
			if (equip != null) equipList.Add(equip);
		}
		curMasterLevel =GameCommon.GetMinMosterLevel("MAGIC_STERNG_LEVEL", equipList, GetMinStrengthLevel(equipList));

        Refresh(null);
    }
	int GetMinStrengthLevel(List<EquipData> _equipList)
	{
		int iMinLevel = 100;
		foreach(var v in _equipList)
		{
			if(v.strengthenLevel < iMinLevel)
				iMinLevel = v.strengthenLevel;
		}
		return iMinLevel;
	}

    public override void OnClose()
    {
        if (ChangeTipManager.Self != null)
        {
            ChangeTipManager.Self.CheckChangeTipShowState(mGameObjUI);
        }
        mSelItemDataList.Clear();
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);

        UpdateUI();

        return true;
    }

    /// <summary>
    /// 删除所选物品UI的刷新
    /// </summary>
    private void DeleteSelMaigcUI(int iItemId)
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

        UIGridContainer grid = GetSub("magic_icon_grid").GetComponent<UIGridContainer>();
        EquipData itemData = mSelItemDataList.Find(x => x.itemId == iItemId);
        if (grid != null && itemData != null)
        {
            int iIndex = itemData.mGridIndex;
            GameObject obj = grid.controlList[itemData.mGridIndex];
            if (obj != null)
            {
                GameObject addUpgradeBtn = GameCommon.FindObject(obj, "add_strengthen_magic_btn");
                GameObject delUpgradeBtn = GameCommon.FindObject(obj, "delete_strengthen_magic_btn");

                delUpgradeBtn.SetActive(false);
                addUpgradeBtn.SetActive(true);
            }
        }
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

        UISprite sprite = GetSub("magic_icon_sprite").GetComponent<UISprite>();
        GameCommon.SetItemIcon(sprite, PackageManager.GetItemTypeByTableID(mCurItemData.tid), mCurItemData.tid);

        UILabel nameLabel = GetSub("magic_name_label").GetComponent<UILabel>();
        if (nameLabel != null)
        {
            nameLabel.text = TableCommon.GetStringFromRoleEquipConfig(mCurItemData.tid, "NAME");
        }

        UILabel refineLevelLabel = GetSub("magic_refine_number_label").GetComponent<UILabel>();
        if (refineLevelLabel != null)
        {
            if (mCurItemData.refineLevel != 0)
            {
                refineLevelLabel.text = "+" + mCurItemData.refineLevel.ToString();
            }
            else
            {
                refineLevelLabel.text = "";
            }
        }
    }

    private void UpdateSelItemlistIcon()
    {
        if (null == mCurItemData)
            return;

        UIGridContainer grid = GetSub("magic_icon_grid").GetComponent<UIGridContainer>();
        if(grid != null)
        {
            for (int i = 0; i < grid.MaxCount; i++)
            {
                GameObject obj = grid.controlList[i];
                if (obj != null)
                {
                    GameObject addUpgradeBtn = GameCommon.FindObject(obj, "add_strengthen_magic_btn");
                    GameObject delUpgradeBtn = GameCommon.FindObject(obj, "delete_strengthen_magic_btn");

                    delUpgradeBtn.SetActive(i < mSelItemDataList.Count);
                    addUpgradeBtn.SetActive(i >= mSelItemDataList.Count);

                    if (delUpgradeBtn != null && addUpgradeBtn != null)
                    {
                        if (i < mSelItemDataList.Count)
                        {
                            EquipData itemData = mSelItemDataList[i];
                            itemData.mGridIndex = i;
                            NiceData data = GameCommon.GetButtonData(obj, "delete_strengthen_magic_btn");
                            if (data != null && itemData != null)
                            {
                                data.set("ITEM_ID", itemData.itemId);
                            }

                            GameCommon.SetItemIcon(delUpgradeBtn.GetComponent<UISprite>(), PackageManager.GetItemTypeByTableID(itemData.tid), itemData.tid);
                        }
                    }
                }
            }
        }
    }

    private void UpdateLevel()
    {
        if(null == mCurItemData)
            return;

        UIProgressBar curStrengthenBar = GetSub("cur_strengthen_bar").GetComponent<UIProgressBar>();
        UIProgressBar AimStrengthenBar = GetSub("aim_strengthen_bar").GetComponent<UIProgressBar>();        

        // cur exp bar
        int curExp = mCurItemData.strengthenExp;
        int maxExp = mCurItemData.GetMaxExp();
        float percentage = curExp / (float)maxExp;
        int iPercentage = (int)(percentage * 100);
        curStrengthenBar.value = percentage;
        
        // aim exp bar
        int iDExp = GetSelItemTotalExp();
        mAimStrengthenExp = curExp;
        mAimStrengthenLevel = mCurItemData.strengthenLevel;
        AddExp(iDExp);
		float aimPercentage = (iDExp + curExp)/ (float)maxExp;
        AimStrengthenBar.value = mAimStrengthenLevel > mCurItemData.strengthenLevel ? 1 : aimPercentage;
        AimStrengthenBar.gameObject.SetActive(iDExp > 0);

        // exp
        UILabel getExplabel = GameCommon.FindComponent<UILabel>(curStrengthenBar.gameObject, "get_exp_label");
        if (getExplabel != null)
        {
            getExplabel.text = curExp.ToString();
        }

        UILabel needExplabel = GameCommon.FindComponent<UILabel>(curStrengthenBar.gameObject, "need_exp_label");
        if (needExplabel != null)
        {
            needExplabel.text = maxExp.ToString();
        }

        // level
        //modified by xuke 属性变化反馈文字相关
        //UILabel levelLabel = GameCommon.FindComponent<UILabel>(curStrengthenBar.gameObject, "level_label");
        //if (levelLabel != null)
        //{
        //    levelLabel.text = "Lv." + mCurItemData.strengthenLevel.ToString();
        //}
        UpdateLevel_ChangeTip();
        //end
        UILabel addLabel = GameCommon.FindComponent<UILabel>(curStrengthenBar.gameObject, "add_label");
        if (addLabel != null)
        {
            addLabel.text = GameCommon.ShowAddNumUI(mAimStrengthenLevel - mCurItemData.strengthenLevel);
        }

        UpdateAttribute();
    }

    private void UpdateLevel_ChangeTip() 
    {
        UILabel levelLabel = GameCommon.FindComponent<UILabel>(GetSub("cur_strengthen_bar"), "level_label");
        if (levelLabel == null)
            return;
        if (CommonParam.mIsRefreshMagicStrengthenInfo)
        {         
            levelLabel.text = "Lv." + mCurItemData.strengthenLevel.ToString();          
        }
        else 
        {
            levelLabel.text = "Lv." + (mCurItemData.strengthenLevel - mStrengthenAddLevel).ToString();    
        }
    }

    private void UpdateAttribute()
    {
        if (null == mCurItemData)
            return;

        UIGridContainer grid = GetSub("attribute_label_Grid").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            for (int i = 0; i < grid.MaxCount; i++)
            {
                GameObject obj = grid.controlList[i];
                if (obj != null)
                {
                    UILabel baseNumberLabel = GameCommon.FindComponent<UILabel>(obj, "base_number_label");
                    UILabel addNumberLabel = GameCommon.FindComponent<UILabel>(obj, "add_number_label");
//                    baseNumberLabel.text = Convert.ToInt32(mCurItemData.GetAttribute(i)).ToString();

                    int addnum = (mAimStrengthenLevel - mCurItemData.strengthenLevel) * TableCommon.GetNumberFromRoleEquipConfig(mCurItemData.tid, "STRENGTHEN_" + i);

                    //addNumberLabel.text = GameCommon.ShowAddNumUI(addnum);

                    DataRecord dataRecord = DataCenter.mRoleEquipConfig.GetRecord(mCurItemData.tid);
                    if (dataRecord != null)
                    {
                        AFFECT_TYPE affectType = (AFFECT_TYPE)((int)dataRecord["ATTRIBUTE_TYPE_" + i]);
                        GameCommon.SetAttributeName(obj, (int)affectType, "attribute_name_label");

                        if (CommonParam.mIsRefreshMagicStrengthenInfo == true)
                        {
                            baseNumberLabel.text = GetRealEquipValueInfo(affectType, mCurItemData.GetAttribute(i));
                        }
                        else 
                        {
                            baseNumberLabel.text = GetRealEquipValueInfo(affectType, RoleEquipData.GetEquipBaseAttributeValue(i, mCurItemData.tid, (mAimStrengthenLevel - mStrengthenAddLevel)));
                        }
						if(addnum > 0){
							addNumberLabel.text = "+" + GetRealEquipValueInfo(affectType,addnum);
						}else {addNumberLabel.text = "";}
                    }

                }
            }
        }
        CommonParam.mIsRefreshMagicStrengthenInfo = true;
    }

	private string GetRealEquipValueInfo(AFFECT_TYPE kAffectType,float kValue)
	{
		string _finalInfo = string.Empty;
		float _realValue = 0f;
		string _affectStr = kAffectType.ToString ();
		if (_affectStr.EndsWith ("_RATE")) {
			_realValue = kValue / 100f;
			_finalInfo = _realValue.ToString ("f2") + "%";
		} else {
			_realValue = kValue / 10000f;
			_finalInfo = _realValue.ToString("f0");
		}
		return _finalInfo;

	}

    public virtual void AddExp(int dExp)
    {
        if (mAimStrengthenLevel >= mCurItemData.mMaxStrengthenLevel)
        {
            mAimStrengthenLevel = mCurItemData.mMaxStrengthenLevel;
            return;
        }

        mAimStrengthenExp += dExp;

        int iMaxExp = DataCenter.mMagicEquipLvConfig.GetRecord(mAimStrengthenLevel).get("LEVEL_UP_EXP_" + (int)mCurItemData.mQualityType);
        if (mAimStrengthenExp >= iMaxExp)
        {
            LevelUp();

            int iDExp = mAimStrengthenExp - iMaxExp;

            mAimStrengthenExp = 0;
            AddExp(iDExp);
        }
    }

    public virtual void LevelUp()
    {
        if (mAimStrengthenLevel >= mCurItemData.mMaxStrengthenLevel)
            return;
        mAimStrengthenLevel += 1;
    }

    private void UpdateNeedGoldUI()
    {
        if (null == mCurItemData)
            return;

        UILabel needGoldLabel = mGameObjUI.transform.Find("need_gold/need_gold_num").GetComponent<UILabel>();
        if (needGoldLabel != null)
        {
           // needGoldLabel.text = GameCommon.CheckShowCostNumUI(GetSelItemTotalExp(), RoleLogicData.Self.gold);
			needGoldLabel.text = GameCommon.CheckCostNumColor(GetSelItemTotalExp(),RoleLogicData.Self.gold);
			GameCommon.GetButtonData(mGameObjUI,"magic_strengthen_btn").set("COST_GOLD_NUM",GetSelItemTotalExp());
        }
    }

    private int GetSelItemTotalExp()
    {
        int iExp = 0;
        foreach (EquipData itemData in mSelItemDataList)
        {
            iExp += TableCommon.GetNumberFromRoleEquipConfig(itemData.tid, "SUPPLY_EXP")
                    + TableCommon.GetNumberFromMagicEquipLvConfig(itemData.strengthenLevel, "TOTAL_EXP_" + (int)itemData.mQualityType);
        }

        return iExp;
    }
}

class Button_AddStrengthenMagicBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MAGIC_STRENGTHEN_INFO_WINDOW", "OPEN_MAGIC_BAG", true);
        return true;

    }
}

class Button_DeleteStrengthenMagicBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MAGIC_STRENGTHEN_INFO_WINDOW", "REMOVE_SELECT_ITEM", (int)get("ITEM_ID"));

        return true;
    }
}

class Button_AutoAddStrengthenMagicBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MAGIC_STRENGTHEN_INFO_WINDOW", "AUTO_ADD", true);
        return true;

    }
}

class Button_MagicStrengthenBtn : CEvent
{
    public override bool _DoEvent()
    {
		int _goldNum = (int)getObject ("COST_GOLD_NUM");
		if (_goldNum > RoleLogicData.Self.gold) 
		{
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
			return true;
		}
			
        DataCenter.SetData("MAGIC_STRENGTHEN_INFO_WINDOW", "REQUEST_UPGRADE", true);
        return true;
    }
}
