using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;


public class EquipRefineInfoWindow : tWindow
{
    private EquipData equipData;
    private EquipRefineInfo refineInfo;
    private float mLastAddTime = 0f;    

	List<EquipData> equipList = new List<EquipData>();
	int curMasterLevel = 0;

    public override void Open(object param)
    {
        base.Open(param);
        equipData = param as EquipData;
        refineInfo = new EquipRefineInfo(equipData);
		equipList.Clear();
		for (int i = 0; i < 4; i++) 
		{
			var equip = TeamManager.GetRoleEquipDataByCurTeamPos(i);
			if (equip != null) equipList.Add(equip);
		}
		curMasterLevel =GameCommon.GetMinMosterLevel("EQUIP_REFINE_LEVEL", equipList, GetMinStrengthLevel(equipList));
        InitButtons();
        InitIcon();
        RefreshRefineInfo();
        RefreshAttrChangeInfo();
    }
	int GetMinStrengthLevel(List<EquipData> _equipList)
	{
		int iMinLevel = 100;
		foreach(var v in _equipList)
		{
			if(v.refineLevel < iMinLevel)
				iMinLevel = v.refineLevel;
		}
		return iMinLevel;
	}
    private void InitButtons()
    {
        AddButtonAction("do_equip_refine_button", OnSend);

        UIGridContainer container = GetComponent<UIGridContainer>("refine_stuff_icon_grid");

        for (int i = 0; i < 4; ++i)
        {
            UIButtonEvent evt = container.controlList[i].GetComponent<UIButtonEvent>();
            mtempObj[i] = evt;// container.controlList[i];
            int index = i;
            evt.SetOnPressingAction(() => OnPressStone(index));
            UIButtonEvent covEvt = GameCommon.FindComponent<UIButtonEvent>(container.controlList[i], "StoneIconCoverBtn");
            covEvt.AddAction(OnClickStoneCover);
        }
    }

    private void InitIcon()
    {
        string _colorPrefix = GameCommon.GetEquipTypeColor(TableCommon.GetNumberFromRoleEquipConfig(equipData.tid, "QUALITY"));
        string equipName = TableCommon.GetStringFromRoleEquipConfig(equipData.tid, "NAME");
        SetText("equip_name_label", _colorPrefix + equipName + "[-]");
        SetText("type_label", _colorPrefix + GameCommon.GetEquipTypeDesc(TableCommon.GetNumberFromRoleEquipConfig(equipData.tid, "EQUIP_TYPE")) + "[-]");
        SetText("point_label", _colorPrefix + "·" + "[-]");
        GameCommon.SetItemIcon(GetSub("equip_icon_window"), equipData);

    }

    private void RefreshRefineInfo()
    {
        UIGridContainer container = GetComponent<UIGridContainer>("refine_stuff_icon_grid");
        UIGridContainer stuffs = GetComponent<UIGridContainer>("stuff_label_grid");

        for (int i = 0; i < 4; ++i)
        {
            RefreshRefineStone(container.controlList[i], stuffs.controlList[i], i);
        }

        GameCommon.SetUIButtonEnabled(mGameObjUI, "do_equip_refine_button", refineInfo.addExp > 0);
        //by chenliang
        //begin

//        SetVisible("click_stone_label", refineInfo.addExp == 0);
//---------------
        //不显示，之前代码废弃

        //end
    }

    private void RefreshRefineStone(GameObject obj, GameObject stuff, int stoneIndex)
    {
        ItemDataBase store = refineInfo.storeStones[stoneIndex];
        ItemDataBase cost = refineInfo.costStones[stoneIndex];
        DataRecord record = DataCenter.mEquipRefineStoneConfig.GetRecord((int)ITEM_TYPE.LOW_REFINE_STONE + stoneIndex);

        GameCommon.SetIcon(obj, "stone_icon", record["ICON_SPRITE_NAME"], record["ICON_ATLAS_NAME"]);
        GameCommon.SetUIText(obj, "exp_number_Label", "经验+" + ((int)record["REFINE_STONE_EXP"]).ToString());
        GameCommon.SetUIText(obj, "StoneNumLabel", (store.itemNum - cost.itemNum).ToString());
        GameCommon.SetUIText(stuff, "number_label", store.itemNum.ToString());
        GameCommon.SetUIVisiable(stuff, "number_label", store.itemNum > 0);
        GameCommon.SetUIVisiable(obj, "Checkmark", cost.itemNum > 0);
        GameCommon.SetUIVisiable(obj, "StoneIconCoverBtn", store.itemNum - cost.itemNum <= 0);
        obj.GetComponent<BoxCollider>().enabled = equipData.refineLevel < equipData.mMaxRefineLevel;
    }

    private int mCurShowRefineLevel = 0;    //> 当前显示精炼等级
    private void RefreshAttrChangeInfo()
    {
        GameObject changeObj = GetSub("equip_info_window");
        // 经验条
        UIProgressBar barBase = GameCommon.FindComponent<UIProgressBar>(changeObj, "cur_refine_up_bar");
        barBase.value = refineInfo.curExpRate;

        UIProgressBar barDest = GameCommon.FindComponent<UIProgressBar>(changeObj, "aim_refine_up_bar");
        barDest.value = refineInfo.finalLevel > refineInfo.curLevel ? 1f : refineInfo.finalExpRate;

        EquipRefineInfo.mneedexp = DataCenter.mEquipRefineLvConfig.GetData(equipData.refineLevel, "LEVEL_UP_EXP_" + (int)equipData.mQualityType);
        GameCommon.SetUIText(barBase.gameObject, "get_exp_label", equipData.refineExp.ToString() + "/" + EquipRefineInfo.mneedexp.ToString());

        // 等级
        GameObject levelObj = GameCommon.FindObject(changeObj, "refine_level_label");

        // 属性
        GameObject attrObj0 = GetSub("base_attuibute_label");
        GameObject attrObj1 = GetSub("subjoin_attuibute_label");

        string strName = TableCommon.GetStringFromEquipAttributeIconConfig((int)refineInfo.attrType0, "NAME");
        attrObj0.GetComponent<UILabel>().text = strName;
        GameCommon.SetUIText(attrObj0, "label", strName);

        strName = TableCommon.GetStringFromEquipAttributeIconConfig((int)refineInfo.attrType1, "NAME");
        attrObj1.GetComponent<UILabel>().text = strName;
        GameCommon.SetUIText(attrObj1, "label", strName);

        if (CommonParam.mIsRefreshEquipRefineInfo) 
        {
            GameCommon.SetUIText(levelObj, "base_number_label", refineInfo.curLevel.ToString());
            GameCommon.SetUIText(levelObj, "add_number_label", (refineInfo.curLevel + 1).ToString());
            mCurShowRefineLevel = refineInfo.curLevel;

            bool isRate = GameCommon.IsAffectTypeRate((AFFECT_TYPE)refineInfo.attrType0);
            if (!isRate)
            {
                GameCommon.SetUIText(attrObj0, "base_number_label", __GetEquipStrengthValue(refineInfo.baseValue0));
                GameCommon.SetUIText(attrObj0, "add_number_label", __GetEquipStrengthValue(refineInfo.addValue0));
            }
            else
            {
                GameCommon.SetUIText(attrObj0, "base_number_label", (_GetEquipStrengthValue(refineInfo.baseValue0) + (isRate ? "%" : "")));
                GameCommon.SetUIText(attrObj0, "add_number_label", (_GetEquipStrengthValue(refineInfo.addValue0) + (isRate ? "%" : "")));
            }

            isRate = GameCommon.IsAffectTypeRate((AFFECT_TYPE)refineInfo.attrType1);
            if (!isRate)
            {
                GameCommon.SetUIText(attrObj1, "base_number_label", __GetEquipStrengthValue(refineInfo.baseValue1));
                GameCommon.SetUIText(attrObj1, "add_number_label", __GetEquipStrengthValue(refineInfo.addValue1));
            }
            else
            {
                GameCommon.SetUIText(attrObj1, "base_number_label", (_GetEquipStrengthValue(refineInfo.baseValue1) + (isRate ? "%" : "")));
                GameCommon.SetUIText(attrObj1, "add_number_label", (_GetEquipStrengthValue(refineInfo.addValue1) + (isRate ? "%" : "")));
            }
        }
        else
        {
            // 刷新上一个精炼等级的信息
            // 等级
            GameCommon.SetUIText(levelObj, "base_number_label", (refineInfo.curLevel - 1).ToString());
            GameCommon.SetUIText(levelObj, "add_number_label", refineInfo.curLevel.ToString());
            mCurShowRefineLevel = refineInfo.curLevel - 1;
            // 属性
            bool isRate = GameCommon.IsAffectTypeRate((AFFECT_TYPE)refineInfo.attrType0);
            string _suffix = isRate ? "%" : "";

            GameCommon.SetUIText(attrObj0, "base_number_label", GetRealValue(GetValueByRefineLevel(equipData.tid,refineInfo.curLevel - 1),isRate) + _suffix);
            GameCommon.SetUIText(attrObj0, "add_number_label", GetRealValue(GetValueByRefineLevel(equipData.tid, refineInfo.curLevel), isRate) + _suffix);          
        }
        UpdateMaxRefineLvUI();
    }
    //更新精炼是否达到顶级的UI
    private void UpdateMaxRefineLvUI() 
    {
        bool _isMaxRefineLevel = equipData.refineLevel >= equipData.mMaxRefineLevel;
        GameObject changeObj = GetSub("equip_info_window");
        //箭头
        GameCommon.SetUIVisiable(changeObj, "arrow_sprite",!_isMaxRefineLevel);
        // 等级
        GameObject levelObj = GameCommon.FindObject(changeObj, "refine_level_label");
        GameCommon.SetUIVisiable(levelObj, "label",!_isMaxRefineLevel);
        GameCommon.SetUIVisiable(levelObj, "add_number_label",!_isMaxRefineLevel);
        GameCommon.SetUIVisiable(levelObj, "max_tips_label",_isMaxRefineLevel);
        // 属性0
        GameObject attrObj0 = GetSub("base_attuibute_label");
        GameCommon.SetUIVisiable(attrObj0, "label", !_isMaxRefineLevel);
        GameCommon.SetUIVisiable(attrObj0, "add_number_label", !_isMaxRefineLevel);
        // 属性1
        GameObject attrObj1 = GetSub("subjoin_attuibute_label");
        GameCommon.SetUIVisiable(attrObj1, "label", !_isMaxRefineLevel);
        GameCommon.SetUIVisiable(attrObj1, "add_number_label", !_isMaxRefineLevel);
    }

    private float GetValueByRefineLevel(int kTid,int kRefineLevel) 
    {
        DataRecord r = DataCenter.mRoleEquipConfig.GetRecord(kTid);

        AFFECT_TYPE attrType = (AFFECT_TYPE)(int)r["REFINE_TYPE_0"];
        float attrValue = (float)(int)r["REFINE_VALUE_0"];
        float baseValue = attrValue * kRefineLevel;

        return baseValue;
        //if (GameCommon.IsAffectTypeRate(attrType))
        //{
        //    attrValue /= 100f;
        //}
        //float addValue = baseValue + attrValue;
        //return addValue;
    }
    /// <summary>
    /// 刷新下一精炼等级的信息
    /// </summary>
    private void UpdateNextRefineLevelInfo() 
    {
        // 等级
        GameObject changeObj = GetSub("equip_info_window");
        GameObject levelObj = GameCommon.FindObject(changeObj, "refine_level_label");
        GameCommon.SetUIText(levelObj, "add_number_label", (refineInfo.curLevel + 1).ToString());

        mCurShowRefineLevel = refineInfo.curLevel;
        // 属性
        GameObject attrObj0 = GetSub("base_attuibute_label");
        bool isRate = GameCommon.IsAffectTypeRate((AFFECT_TYPE)refineInfo.attrType0);
        if (!isRate)
        {
            GameCommon.SetUIText(attrObj0, "add_number_label", __GetEquipStrengthValue(refineInfo.addValue0));
        }
        else
        {
            GameCommon.SetUIText(attrObj0, "add_number_label", (_GetEquipStrengthValue(refineInfo.addValue0) + (isRate ? "%" : "")));
        }
        GameObject attrObj1 = GetSub("subjoin_attuibute_label");
        isRate = GameCommon.IsAffectTypeRate((AFFECT_TYPE)refineInfo.attrType1);
        if (!isRate)
        {
            GameCommon.SetUIText(attrObj1, "add_number_label", __GetEquipStrengthValue(refineInfo.addValue1));
        }
        else
        {
            GameCommon.SetUIText(attrObj1, "add_number_label", (_GetEquipStrengthValue(refineInfo.addValue1) + (isRate ? "%" : "")));
        }

    }

	private string __GetEquipStrengthValue(float kValue)
	{
		return (kValue / 10000).ToString ();
	}
    
    private string _GetEquipStrengthValue(float kValue)
    {
        return (kValue / 100).ToString();
    }

    private void AddCostStone(int index)
    {
        if (refineInfo.AddCostStone(index))
        {
            //RefreshRefineInfo();
            //RefreshAttrChangeInfo();
            DoCoroutine(DoSend());
        }
        else 
        {
            if (refineInfo.overflow)
            {
                DataCenter.OpenMessageWindow("经验已满");
            }
            else 
            {
                DataCenter.OpenMessageWindow("精炼石不足");
                SC_Back = true;
            }
        }
    }

    private void OnPressStone(int index)
    {
        float eat_time = (float)DataCenter.mEquipRefineStoneConfig.GetData(index, "SPEED");
        if ((Time.time - mLastAddTime > eat_time) && (refineInfo.curLevel == refineInfo.finalLevel)&&(SC_Back)&&UIButtonEvent.can_press)
        {          
            SC_Back = false;
            AddCostStone(index);
            mLastAddTime = Time.time;
        }
    }  

    private void OnClickStoneCover()
    {
        DataCenter.OpenMessageWindow("精炼石不足");
        SC_Back = true;
    }

    private void OnSend()
    {
        DoCoroutine(DoSend());
    }
    //public GameObject[] mtempObj=new GameObject[4];
    public static UIButtonEvent[] mtempObj = new UIButtonEvent[4];
    bool SC_Back = true;

    private bool mNeedPlayAnim = false;
    private IEnumerator DoSend()
    {
        GameCommon.SetUIButtonEnabled(mGameObjUI, "do_equip_refine_button", false);
        List<ItemDataBase> list = new List<ItemDataBase>();

        foreach (ItemDataBase item in refineInfo.costStones)
        {
            if (item.itemNum > 0)
            {
                list.Add(item);
            }
        }

        RefineEquipRequester req = new RefineEquipRequester(equipData.itemId, equipData.tid, list.ToArray());
        yield return req.Start();

        if (req.success)
        {
            //SC_Back = true;
            GameObject changeObj = GetSub("equip_info_window");
            UIProgressBar barBase = GameCommon.FindComponent<UIProgressBar>(changeObj, "cur_refine_up_bar");
          //  GameObject wumaotexiao = GetSub("ui_weapon_lvup");

            //if (refineInfo.curLevel < refineInfo.finalLevel)
            //{
            //    string mtip = DataCenter.mStringList.GetData(2509, "STRING_CN");
            //    mtip = mtip.Replace("{0}", refineInfo.finalLevel.ToString());
            //    DataCenter.OnlyTipsLabelMessage(mtip);
            //    for (int i = 0; i < 4; i++)
            //    {
            //        mtempObj[i].BanPress(false);
            //        UIButtonEvent.can_press = false;
            //    }
            //    wumaotexiao.SetActive(false);
            //    wumaotexiao.SetActive(true);
            //    GlobalModule.DoLater(() => { wumaotexiao.SetActive(false); UIButtonEvent.can_press = true; }, 0.8f);
            //}
            yield return DoCoroutine(UIKIt.PushLevelBar(this, barBase, null, refineInfo.curLevel, refineInfo.curExpRate, refineInfo.finalLevel, refineInfo.finalExpRate, null));          
            if (refineInfo.finalLevel != refineInfo.curLevel)
            {
                CommonParam.mIsRefreshEquipRefineInfo = false;
                mNeedPlayAnim = true;
            }
            if (refineInfo.finalLevel - mCurShowRefineLevel >= 1) 
            {
                CommonParam.mIsRefreshEquipRefineInfo = false;
            }
            refineInfo = new EquipRefineInfo(equipData);        
            RefreshRefineInfo();
            RefreshAttrChangeInfo();
            //added by xuke 刷新装备背包
            DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "REFRESH_EQUIP_BAG_GROUP",null);
          
            //end
			TeamInfoWindow _tWindow = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
			if(_tWindow != null)
			{
				ActiveData _tmp = TeamManager.GetActiveDataByTeamPos (equipData.teamPos);
//				GlobalModule.DoCoroutine(_tWindow.SetTipsAttribute(_tmp));
				if(equipList.Count >= 4)
				{
					int aa = GameCommon.GetMinMosterLevel("EQUIP_REFINE_LEVEL", equipList, GetMinStrengthLevel(equipList));
					if (aa > curMasterLevel)
					{
						string str = "[ff9900]装备精炼大师[-] [99ff66]{0}[-] 级达成！" ;
						_tWindow.SetTipsMaster(str, aa);
						curMasterLevel = aa;					
					}
				}

                AddRefineChangeTip(refineInfo);
				_tWindow.SetTipsAttribute(_tmp);
				_tWindow.ChangeFitting();
                if (mNeedPlayAnim) 
                {
                    ChangeTipManager.Self.PlayAnim(() => { UpdateNextRefineLevelInfo(); });                
                }
                mNeedPlayAnim = false;
                CommonParam.mIsRefreshEquipRefineInfo = true;
			}
            SC_Back = true;
        }
    }

    private void AddRefineChangeTip(EquipRefineInfo kRefineInfo) 
    {
        if (kRefineInfo == null)
            return;

        if (!mNeedPlayAnim)
            return;
        // 精炼等级
        string _addLevelInfo = "精炼等级 +1";
        ChangeTip _levelTip = new ChangeTip() {Content = _addLevelInfo,TargetValue = kRefineInfo.finalLevel };
        _levelTip.SetTargetObj(ChangeTipPanelType.EQUIP_REFINE_INFO_WINDOW,ChangeTipValueType.EQUIP_REFINE_LEVEL);
        ChangeTipManager.Self.Enqueue(_levelTip,(int)ChangeTipPriority.STRENGTHEN_LEVEL);
        
        // 精炼属性
        AddRefineAttrChangeTip(kRefineInfo.attrValue0,kRefineInfo.baseValue0,kRefineInfo.attrType0,ChangeTipPanelType.EQUIP_REFINE_INFO_WINDOW,ChangeTipValueType.EQUIP_REFINE_BASE_ATTR);
        AddRefineAttrChangeTip(kRefineInfo.attrValue1, kRefineInfo.baseValue1, kRefineInfo.attrType1, ChangeTipPanelType.EQUIP_REFINE_INFO_WINDOW, ChangeTipValueType.EQUIP_REFINE_SUB_ATTR);
    }

    private float GetRealValue(float kValue, bool kIsRate) 
    {
        return !kIsRate ? kValue / 10000 : kValue / 100;
    }
    private void AddRefineAttrChangeTip(float kAttrValue, float kBaseValue, AFFECT_TYPE attrType, ChangeTipPanelType kPanelType, ChangeTipValueType kValueTpye) 
    {
        bool isRate = false;
        string _attrName = TableCommon.GetStringFromEquipAttributeIconConfig((int)attrType, "NAME");
        float _targetValue = kBaseValue;
        float _diffValue = kAttrValue;
        isRate = GameCommon.IsAffectTypeRate(attrType);
        if (isRate)
            _diffValue *= 100;
        string _suffix = isRate ? "%" : "";
        int _precision = isRate ? 1 : 0;
        string _showAttrInfo = _attrName + " +" + GetRealValue(_diffValue, isRate) + _suffix;
        ChangeTip _tip = new ChangeTip() { Content = _showAttrInfo,TargetValue = GetRealValue(_targetValue,isRate),Precision = _precision};
        _tip.SetTargetObj(kPanelType,kValueTpye);
        ChangeTipManager.Self.Enqueue(_tip,(int)ChangeTipPriority.STRENGTHEN_ATTR);
    }

    private class EquipRefineInfo
    {
        public static int mneedexp = 0;
        public int curLevel { get; private set; }
        public static int curExp { get; private set; }
        public float curExpRate { get; private set; }

        public AFFECT_TYPE attrType0 { get; private set; }
        public float attrValue0 { get; private set; }
        public AFFECT_TYPE attrType1 { get; private set; }
        public float attrValue1 { get; private set; }

        public ItemDataBase[] storeStones { get; private set; }
        public ItemDataBase[] costStones { get; private set; }

        public int addExp { get; private set; }
        public int finalLevel { get; private set; }
        public static int finalExp { get; private set; }
        public float finalExpRate { get; private set; }
        public bool overflow { get; private set; }
        public float baseValue0 { get; private set; }
        public float baseValue1 { get; private set; }
        public float addValue0 { get; private set; }
        public float addValue1 { get; private set; }

        private EquipData mEquipData;

        public EquipRefineInfo(EquipData data)
        {                   
            mEquipData = data;
            curLevel = data.refineLevel;
            curExp = data.refineExp;
            int curNeedExp = DataCenter.mEquipRefineLvConfig.GetData(curLevel, "LEVEL_UP_EXP_" + (int)data.mQualityType);
            curExpRate = (float)curExp / curNeedExp;

            finalLevel = curLevel;
            finalExp = curExp;
            finalExpRate = curExpRate;

            DataRecord r = DataCenter.mRoleEquipConfig.GetRecord(data.tid);

            attrType0 = (AFFECT_TYPE)(int)r["REFINE_TYPE_0"];
            attrValue0 = (float)(int)r["REFINE_VALUE_0"];
            baseValue0 = attrValue0 * curLevel;

            if (GameCommon.IsAffectTypeRate(attrType0))
            {
                attrValue0 /= 100f;
            }

            attrType1 = (AFFECT_TYPE)(int)r["REFINE_TYPE_1"];
            attrValue1 = (float)(int)r["REFINE_VALUE_1"];
            baseValue1 = attrValue1 * curLevel;


            addValue0 = baseValue0 + attrValue0;
            addValue1 = baseValue1 + attrValue1;

            if (GameCommon.IsAffectTypeRate(attrType1))
            {
                attrValue1 /= 100f;
            }

            storeStones = new ItemDataBase[4];

            storeStones[0] = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.LOW_REFINE_STONE);
            storeStones[1] = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.MIDDLE_REFINE_STONE);
            storeStones[2] = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.HIGH_REFINE_STONE);
            storeStones[3] = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.BEST_REFINE_STONE);

            costStones = new ItemDataBase[4];

            for (int i = 0; i < 4; ++i)
            {
                costStones[i] = new ItemDataBase();
                costStones[i].itemId = storeStones[i].itemId;
                costStones[i].tid = storeStones[i].tid;
                costStones[i].itemNum = 0;
            }
        }

        public bool AddCostStone(int index)
        {
            if (costStones[index].itemNum >= storeStones[index].itemNum || overflow)
                return false;

            costStones[index].itemNum++;
            addExp += DataCenter.mEquipRefineStoneConfig.GetData(storeStones[index].tid, "REFINE_STONE_EXP");
            int lv, exp;
            overflow = mEquipData.TryAddRefineExp(addExp, out lv, out exp);
            finalLevel = lv;
            finalExp = exp;
            int finalNeedExp = DataCenter.mEquipRefineLvConfig.GetData(finalLevel, "LEVEL_UP_EXP_" + (int)mEquipData.mQualityType);
            mneedexp = DataCenter.mEquipRefineLvConfig.GetData(mEquipData.refineLevel, "LEVEL_UP_EXP_" + (int)mEquipData.mQualityType); 
            finalExpRate = (float)finalExp / finalNeedExp;

            return true;
        }
    }
    public override void OnClose()
    {
        if (ChangeTipManager.Self != null)
        {
            ChangeTipManager.Self.CheckChangeTipShowState(mGameObjUI);
        }
        base.OnClose();
    }

}