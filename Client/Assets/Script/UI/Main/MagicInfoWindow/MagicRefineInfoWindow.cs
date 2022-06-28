using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;
using System.Linq;
using System;

public class MagicRefineInfoWindow : tWindow
{
    private EquipData mCurItemData;
	List<EquipData> equipList = new List<EquipData>();
	int curMasterLevel = 0;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_magic_refine_btn", new DefineFactory<Button_MagicRefineBtn>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        mCurItemData = param as EquipData;

		equipList.Clear();
		for (int i = 0; i < 2; i++) 
		{
			var equip = TeamManager.GetMagicDataByCurTeamPos(i);
			if (equip != null) equipList.Add(equip);
		}
		curMasterLevel =GameCommon.GetMinMosterLevel("MAGIC_REFINE_LEVEL", equipList, GetMinStrengthLevel(equipList));

        Refresh(null);
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
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if ("REQUEST_REFINE" == keyIndex)
        {
            if (!RefineCondition())
                return;

            if (mCurItemData.refineLevel == GameCommon.getMagicEquipRefineMaxLevel())
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_REFINE_FULL);
                return;
            }
            CS_RefineMagic magicRefine = new CS_RefineMagic();
            int iNeedNum = DataCenter.mMagicEquipRefineConfig.GetData(mCurItemData.refineLevel, "REFINE_EQUIP_NUM");
            List<EquipData> itemDataList;
			MagicLogicData.Self.GetRefineStuffPetList(out itemDataList, mCurItemData);

			//ItemDataBase[] list = new ItemDataBase[iNeedNum];
            //added by xuke  需要额外传送精炼石的数据
            ItemDataBase[] list = new ItemDataBase[iNeedNum + 1];
            //end
            for (int i = 0; i < iNeedNum; i++)
			{
				EquipData itemData = itemDataList[i];
				
				ItemDataBase item = new ItemDataBase();
				item.itemId = itemData.itemId;
				item.tid = itemData.tid;
				item.itemNum = itemData.itemNum;
				list[i] = item;
			}
            //added by xuke  获取精炼石的数据
            ItemDataBase _refineStoneData = new ItemDataBase();
            int _needStoneNum = DataCenter.mMagicEquipRefineConfig.GetRecord(mCurItemData.refineLevel).getData("REFINESTONE_NUM");
            int _itemTid = (int)ITEM_TYPE.MAGIC_REFINE_STONE;
            int _itemID = GameCommon.GetItemId(_itemTid);
            _refineStoneData.itemId = _itemID;
            _refineStoneData.tid = _itemTid;
            _refineStoneData.itemNum = _needStoneNum;
            list[list.Length - 1] = _refineStoneData;
			//end
            magicRefine.arr = list;
			magicRefine.itemId = mCurItemData.itemId;
			magicRefine.tid = mCurItemData.tid;
			magicRefine.pt = "CS_RefineMagic";
			HttpModule.Instace.SendGameServerMessage(magicRefine, x => RequestMaigcRefineSuccess(magicRefine, x), RequestMaigcStrengthenFail);
        }
    }

    private bool RefineCondition()
    {
        // 精炼石是否充足
        int iNeedNum = DataCenter.mMagicEquipRefineConfig.GetData(mCurItemData.refineLevel, "REFINESTONE_NUM");
        int iCurNum = PackageManager.GetItemLeftCount((int)ITEM_TYPE.MAGIC_REFINE_STONE);
        if (iNeedNum > iCurNum)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_REFINE_NEED_REFINE_STONE);
            return false;
        }
        
        // 材料是否充足
        iNeedNum = DataCenter.mMagicEquipRefineConfig.GetData(mCurItemData.refineLevel, "REFINE_EQUIP_NUM");
        iCurNum = MagicLogicData.Self.GetRefineStuffMagicCount(mCurItemData);
        if (iNeedNum > iCurNum)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAGIC_REFINE_NEED_REFINE_MAGIC);
            return false;
        }

        // 银币是否充足
        iNeedNum = DataCenter.mMagicEquipRefineConfig.GetData(mCurItemData.refineLevel, "REFINE_EQUIP_MONEY");
        if (iNeedNum > RoleLogicData.Self.gold)
        {
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
            return false;
        }
        return true;
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        UpdateItemIcon();
        UpdateItemAttributeUI();
        UpdateStuffStoneIcon();
        UpdateStuffMagicIcon();
        UpdateNeedGoldUI();
    }

    private void UpdateItemIcon()
    {
        if (mCurItemData != null && mGameObjUI != null)
        {
            GameObject parentObj = GetSub("magic_info");
            if (parentObj != null)
            {
                string _colorPrefix = GameCommon.GetEquipTypeColor(TableCommon.GetNumberFromRoleEquipConfig(mCurItemData.tid, "QUALITY"));
                GameCommon.SetItemIcon(parentObj, "icon_sprite", mCurItemData.tid);
                GameCommon.SetUIText(parentObj, "magic_name_label",_colorPrefix +  GameCommon.GetItemStringField(mCurItemData.tid, GET_ITEM_FIELD_TYPE.NAME) + "[-]");

                GameCommon.FindComponent<UILabel>(parentObj, "type_label").text = _colorPrefix + GameCommon.GetEquipTypeDesc(TableCommon.GetNumberFromRoleEquipConfig(mCurItemData.tid, "EQUIP_TYPE")) + "[-]";
                GameCommon.FindComponent<UILabel>(parentObj, "point_label").text = _colorPrefix + "·" + "[-]";
            }
        }
    }

    private void UpdateItemAttributeUI()
    {
        if (mCurItemData != null && mGameObjUI != null)
        {
            UIGridContainer grid = GetSub("attribute_change_grid").GetComponent<UIGridContainer>();
            if (grid != null)
            {
                for (int i = 0; i < grid.MaxCount; i++)
                {
                    DataRecord r = DataCenter.mRoleEquipConfig.GetRecord(mCurItemData.tid);
                    string strType = "REFINE_TYPE_" + i;
                    string strValue = "REFINE_VALUE_" + i;
                    SetRefineAttribute(grid.controlList[i], r[strType], r[strValue]);
                }
            }
        }

		//added by xuke begin
		// 添加等级描述
        GameObject _curLvObj = GameCommon.FindObject(mGameObjUI, "attribute_change_label");
        GameObject _aimLvObj = GameCommon.FindObject(mGameObjUI, "aim_label");
        
        int _showRefineLevel = mCurItemData.refineLevel;
        if (!CommonParam.mIsRefreshMagicRefineInfo)
        {
            _showRefineLevel--;           
        }
        GameCommon.SetUIText(_curLvObj, "num", _showRefineLevel.ToString());
        int _nextLv = Mathf.Min(_showRefineLevel + 1, mCurItemData.mMaxRefineLevel);
        if (_showRefineLevel != GameCommon.getMagicEquipRefineMaxLevel())
        {
            GameCommon.SetUIVisiable(_curLvObj, "max_tips_label", false);
            _aimLvObj.SetActive(true);
            GameCommon.SetUIText(_aimLvObj, "num", _nextLv.ToString());
        }
        else
        {
            GameCommon.SetUIVisiable(_curLvObj, "max_tips_label", true);
            _aimLvObj.SetActive(false);
            GameCommon.SetUIText(_aimLvObj, "num", "满级");
        }
      
		//end
        CommonParam.mIsRefreshMagicRefineInfo = true;
    }
    /// <summary>
    /// 刷新下一精炼等级属性
    /// </summary>
    private void UpdateNextReineInfo() 
    {
        // 设置精炼等级
        GameObject _curLvObj = GameCommon.FindObject(mGameObjUI, "attribute_change_label");
        GameObject _aimLvObj = GameCommon.FindObject(mGameObjUI, "aim_label");
        GameCommon.SetUIText(_aimLvObj, "num", mCurItemData.refineLevel.ToString());

        int _nextLv = Mathf.Min(mCurItemData.refineLevel + 1, mCurItemData.mMaxRefineLevel);
        if (mCurItemData.refineLevel != GameCommon.getMagicEquipRefineMaxLevel())
        {
            GameCommon.SetUIVisiable(_curLvObj, "max_tips_label", false);
            _aimLvObj.SetActive(true);
            GameCommon.SetUIText(_aimLvObj, "num", _nextLv.ToString());
        }
        else
        {
            GameCommon.SetUIVisiable(_curLvObj, "max_tips_label", true);
            _aimLvObj.SetActive(false);
            GameCommon.SetUIText(_aimLvObj, "num", "满级");
        }

        // 设置属性
        if (mCurItemData != null && mGameObjUI != null)
        {
            UIGridContainer grid = GetSub("attribute_change_grid").GetComponent<UIGridContainer>();
            if (grid != null)
            {
                for (int i = 0; i < grid.MaxCount; i++)
                {
                    DataRecord r = DataCenter.mRoleEquipConfig.GetRecord(mCurItemData.tid);
                    string strType = "REFINE_TYPE_" + i;
                    string strValue = "REFINE_VALUE_" + i;
                    int attrType = r[strType];
                    int attrValue = r[strValue];
                    bool isRate = GameCommon.IsAffectTypeRate((AFFECT_TYPE)attrType);
                    float value = isRate ? attrValue / 100f : attrValue;
                    int _refineLevel = mCurItemData.refineLevel;

                    if (_refineLevel != GameCommon.getMagicEquipRefineMaxLevel())
                    {
                        GameCommon.SetUIVisiable(grid.controlList[i], "label", true);
                        GameCommon.SetUIVisiable(grid.controlList[i], "add_number_label", true);
                        GameCommon.SetUIText(grid.controlList[i], "add_number_label", GameCommon.ShowAddNumUI(value * (_refineLevel + 1)) + (isRate ? "%" : ""));
                    }
                    else
                    {
                        GameCommon.SetUIVisiable(grid.controlList[i], "label", false);
                        GameCommon.SetUIVisiable(grid.controlList[i], "add_number_label", false);
                        GameCommon.SetUIText(grid.controlList[i], "add_number_label", "满级");
                    }
                    
                }
            }
        }
    }
    private void SetRefineAttribute(GameObject obj, int attrType, int attrValue)
    {
        GameCommon.SetAttributeName(obj, attrType, "attribute_name_label");
        GameCommon.SetAttributeName(obj, attrType, "label");
        bool isRate = GameCommon.IsAffectTypeRate((AFFECT_TYPE)attrType);
        float value = isRate ? attrValue / 100f : attrValue;
        int _refineLevel = mCurItemData.refineLevel;
        if (!CommonParam.mIsRefreshMagicRefineInfo)
        {
            _refineLevel--;
        }
        GameCommon.SetUIText(obj, "cur_number_label", (value * _refineLevel).ToString() + (isRate ? "%" : ""));
        if (_refineLevel != GameCommon.getMagicEquipRefineMaxLevel())
        {
            GameCommon.SetUIVisiable(obj, "label", true);
            GameCommon.SetUIVisiable(obj, "add_number_label", true);
            GameCommon.SetUIText(obj, "add_number_label", GameCommon.ShowAddNumUI(value * (_refineLevel + 1)) + (isRate ? "%" : ""));
        }
        else
        {
            GameCommon.SetUIVisiable(obj, "label", false);
            GameCommon.SetUIVisiable(obj, "add_number_label", false);
            GameCommon.SetUIText(obj, "add_number_label", "满级");
        }
    }

    private void UpdateStuffStoneIcon()
    {
        if (mCurItemData != null && mGameObjUI != null)
        {
            GameObject parentObj = GetSub("stuff_stone_icon");
            if (parentObj != null)
            {
                // need num
                int iNeedNum = DataCenter.mMagicEquipRefineConfig.GetData(mCurItemData.refineLevel, "REFINESTONE_NUM");
                // 拥有数量
                int iCurNum = PackageManager.GetItemLeftCount((int)ITEM_TYPE.MAGIC_REFINE_STONE);
				GameCommon.SetUIText(parentObj, "stone_number_label", GameCommon.CheckOwnNumColor(iNeedNum, iCurNum) + "/" + iNeedNum.ToString());
                GameCommon.SetUIText(parentObj, "stone_stuff_label", GameCommon.GetItemName(2000003));

                GameCommon.BindItemDescriptionEvent(parentObj, (int)ITEM_TYPE.MAGIC_REFINE_STONE);
            }
        }
    }

    private void UpdateStuffMagicIcon()
    {
        if (mCurItemData != null && mGameObjUI != null)
        {
            GameObject parentObj = GetSub("stuff_magic_icon_group");
            if (parentObj != null)
            {
                // need num
                int iNeedNum = DataCenter.mMagicEquipRefineConfig.GetData(mCurItemData.refineLevel, "REFINE_EQUIP_NUM");
                // 拥有数量
                int iCurNum = MagicLogicData.Self.GetRefineStuffMagicCount(mCurItemData);
                GameCommon.SetUIText(parentObj, "need_magic_num", GameCommon.CheckOwnNumColor(iNeedNum, iCurNum) + "/" + iNeedNum.ToString());
                GameCommon.SetUIText(parentObj, "magic_name_stuff_label", GameCommon.GetItemStringField(mCurItemData.tid, GET_ITEM_FIELD_TYPE.NAME));
                GameCommon.SetItemIcon(parentObj, "icon_sprite", mCurItemData.tid);
                parentObj.SetActive(iNeedNum > 0);

                GameObject _magicIcon = GameCommon.FindObject(mGameObjUI, "magic_stuff_icon");
                GameCommon.BindItemDescriptionEvent(_magicIcon, mCurItemData.tid);
            }
        }
    }

    private void UpdateNeedGoldUI()
    {
        if (mCurItemData != null && mGameObjUI != null)
        {
            GameObject parentObj = GetSub("need_gold");
            if (parentObj != null)
            {
                // need num
                int iNeedNum = DataCenter.mMagicEquipRefineConfig.GetData(mCurItemData.refineLevel, "REFINE_EQUIP_MONEY");
                GameCommon.SetUIText(parentObj, "need_gold_num", GameCommon.CheckCostNumColor(iNeedNum, PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD)));
            }
        }
    }

    private void RequestMaigcRefineSuccess(CS_RefineMagic magicRefine, string text)
    {
        SC_RefineMagic upgrade = JCode.Decode<SC_RefineMagic>(text);

        PackageManager.RemoveItem(magicRefine.arr.ToList<ItemDataBase>());
     
        int iNeedNum = DataCenter.mMagicEquipRefineConfig.GetData(mCurItemData.refineLevel, "REFINE_EQUIP_MONEY");

        PackageManager.RemoveItem((int)ITEM_TYPE.GOLD, -1, iNeedNum);

        mCurItemData.refineLevel += 1;
        CommonParam.mIsRefreshMagicRefineInfo = false;
        Refresh(null);
        tWindow window = DataCenter.GetData("PACKAGE_EQUIP_WINDOW") as tWindow;
        if (window != null && window.IsOpen())
            DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "REFRESH_MAGIC_BAG_GROUP", null);
        
        DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_MAGIC_ADD_REFINE_LEVEL, mCurItemData.refineLevel.ToString());
		TeamInfoWindow _tWindow = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
		if(_tWindow != null)
		{
			ActiveData _tmp = TeamManager.GetActiveDataByTeamPos (mCurItemData.teamPos);
//			GlobalModule.DoCoroutine(_tWindow.SetTipsAttribute(_tmp));
			if(equipList.Count >= 2)
			{
				int aa = GameCommon.GetMinMosterLevel("MAGIC_REFINE_LEVEL", equipList, GetMinStrengthLevel(equipList));
				if (aa > curMasterLevel)
				{
                    string str = "[ff9900]神器精炼大师[-] [99ff66]{0}[-] 级达成！";
					_tWindow.SetTipsMaster(str, aa);
					curMasterLevel = aa;					
				}
			}
            AddRefineChangeTip(mCurItemData);
			_tWindow.SetTipsAttribute(_tmp);
			_tWindow.ChangeFitting();
            ChangeTipManager.Self.PlayAnim(()=>{UpdateNextReineInfo();});
		}
        //added by xuke 刷新红点逻辑
        TeamNewMarkManager.Self.CheckCoin();
        TeamNewMarkManager.Self.RefreshTeamNewMark();
        //end
    }

    private void RequestMaigcStrengthenFail(string text)
    {

    }

    private void AddRefineChangeTip(EquipData kEquipData) 
    {
        // 精炼等级
        string _refineInfo = "精炼等级 +1";
        ChangeTip _levelTip = new ChangeTip() { Content = _refineInfo,TargetValue = kEquipData.refineLevel};
        _levelTip.SetTargetObj(ChangeTipPanelType.MAGIC_REFINE_INFO_WINDOW,ChangeTipValueType.MAGIC_REFINE_LEVEL);
        ChangeTipManager.Self.Enqueue(_levelTip,(int)ChangeTipPriority.STRENGTHEN_LEVEL);

        DataRecord r = DataCenter.mRoleEquipConfig.GetRecord(mCurItemData.tid);
        if (r == null)
            return;
        // 精炼属性
        UIGridContainer grid = GetSub("attribute_change_grid").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            for (int i = 0; i < grid.MaxCount; i++)
            {
             
                string strType = "REFINE_TYPE_" + i;
                string strValue = "REFINE_VALUE_" + i;
                int attrValue = r[strValue];
                int attrType = r[strType];

                bool isRate = GameCommon.IsAffectTypeRate((AFFECT_TYPE)attrType);
                float value = isRate ? attrValue / 100f : attrValue;

                string _attrName = TableCommon.GetStringFromEquipAttributeIconConfig(attrType, "NAME");

                float _targetValue = value * kEquipData.refineLevel;
                float _beforeValue = value * (kEquipData.refineLevel - 1);
                float _addAttrValue = _targetValue - _beforeValue;

                string _suffix = isRate ? "%" : "";
                string _addAttrInfo = _attrName + " +" + _addAttrValue.ToString() + _suffix;
                ChangeTip _strengthAttrTip = new ChangeTip() { Content = _addAttrInfo, TargetValue = _targetValue };
                _strengthAttrTip.SetTargetObj(ChangeTipPanelType.MAGIC_REFINE_INFO_WINDOW, ChangeTipValueType.MAGIC_REFINE_ATTR_BASE + i + 1);
                ChangeTipManager.Self.Enqueue(_strengthAttrTip, (int)ChangeTipPriority.STRENGTHEN_ATTR);
            }
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


class Button_MagicRefineBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MAGIC_REFINE_INFO_WINDOW", "REQUEST_REFINE", true);
        return true;
    }
}