using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class EquipData : TeamPosItemDataBase
{
    public int strengthenLevel;     // 强化等级
    public int strengthenExp;       // 强化经验
    public bool isNew;              // 是否是新得到
    public int refineLevel;         // 精炼等级
    public int refineExp;           // 精炼经验值
    public int strengCostGold;      // 强化消耗的银币
    public int mGridIndex;
    public int mMaxStrengthenLevel = 100;
    public int mMaxRefineLevel=80;
    public int mStarLevel;
    public int mUserID;
    public int mReset;
    public int mExp;
    public ITEM_TYPE mItemType;
    public EQUIP_QUALITY_TYPE mQualityType = EQUIP_QUALITY_TYPE.LOW;
    public ELEMENT_TYPE mElementType = ELEMENT_TYPE.RED;
    public List<EquipAttachAttribute> mAttachAttributeList = new List<EquipAttachAttribute>();
    public List<EquipBaseAttribute> mBaseAttributeList = new List<EquipBaseAttribute>();
    private int mAttributeNum = 2;

    public EquipData()
    {

    }

    public EquipData(ItemDataBase itemData)
    {
        if (null == itemData)
            return;

        tid = itemData.tid;
        itemId = itemData.itemId;
        itemNum = itemData.itemNum;
        strengthenLevel = 1;
        DataRecord record = DataCenter.mRoleEquipConfig.GetRecord(tid);
        mStarLevel = record["STAR_LEVEL"];
        mMaxStrengthenLevel = record["MAX_GROW_LEVEL"];
        mMaxRefineLevel = record["MAX_REFINE_LEVEL"];
        mQualityType = (EQUIP_QUALITY_TYPE)(int)record["QUALITY"];
        mElementType = (ELEMENT_TYPE)UnityEngine.Random.Range(0, 5);
        mUserID = 0;
        isNew = true;
        mReset = 0;
        for (int i = 0; i < 2; i++)
        {
            float fCurBaseAttributeValue = RoleEquipData.GetBaseAttributeValue(i, tid, strengthenLevel);
            AddEquipBaseAttriblute(i, (AFFECT_TYPE)(int)record["ATTRIBUTE_TYPE_" + i.ToString()], fCurBaseAttributeValue);
        }
    }

    public void Init()
    {
        mStarLevel = TableCommon.GetNumberFromRoleEquipConfig(tid, "STAR_LEVEL");
        mMaxStrengthenLevel = TableCommon.GetNumberFromRoleEquipConfig(tid, "MAX_GROW_LEVEL");
        mQualityType = (EQUIP_QUALITY_TYPE)TableCommon.GetNumberFromRoleEquipConfig(tid, "QUALITY");
        mMaxRefineLevel =TableCommon.GetNumberFromRoleEquipConfig(tid, "MAX_REFINE_LEVEL");
    }

    public bool AddEquipBaseAttriblute(int iIndex, AFFECT_TYPE type, float fValue)
    {
        if (fValue != 0 && mBaseAttributeList.Count >= iIndex && iIndex < 2)
        {
            EquipBaseAttribute attr = new EquipBaseAttribute();
            attr.mType = type;
            attr.mValue = fValue;
            attr.mIndex = iIndex;
            mBaseAttributeList.Add(attr);
            return true;
        }
        return false;
    }

    public bool AddEquipAttachAttriblute(int iIndex, AFFECT_TYPE type, float fValue)
    {
        if (fValue != 0 && mAttachAttributeList.Count >= iIndex && iIndex < 5)
        {
            EquipAttachAttribute attr = new EquipAttachAttribute();
            attr.mType = type;
            attr.mValue = fValue;
            mAttachAttributeList.Add(attr);
            return true;
        }
        return false;
    }

    public virtual bool ApplyAffect() { return true; }

    public virtual float ApplyAffect(AFFECT_TYPE affectType, float fValue)
    {
        float rate = 0;
        float total = 0;

        string strRateType = "";
        string strType = "";

        if (GameCommon.IsAffectTypeRate(affectType))
        {
            strRateType = strType;
            strType.Replace("_RATE", "");
        }
        else
        {
            strRateType = strType + "_RATE";
        }

        AFFECT_TYPE type = GameCommon.ToAffectTypeEnum(strType);
        AFFECT_TYPE rateType = GameCommon.ToAffectTypeEnum(strRateType);






        for (int i = 0; i < mAttributeNum; i++)
        {
            EquipBaseAttribute attribute = mBaseAttributeList[i];
            if (attribute != null)
            {
                total += attribute.Affect(type);
                rate += attribute.AffectRate(rateType);
            }
        }

        for (int i = 0; i < mAttachAttributeList.Count; i++)
        {
            EquipAttachAttribute attribute = mAttachAttributeList[i];
            if (attribute != null)
            {
                total += attribute.Affect(type);
                rate += attribute.AffectRate(rateType);
            }
        }

        float nowValue = total / 10000 + fValue * (1.0f + rate / 10000);
        return nowValue;
    }

    public virtual float ApplyAffect(AFFECT_TYPE affectType, int nValue)
    {
        return ApplyAffect(affectType, nValue * 1.0f);
    }

    public static float GetBaseAttributeValue(int iIndex, int iModelIndex, int iStrengthenLevel)
    {
        float fBaseValue = (float)TableCommon.GetNumberFromRoleEquipConfig(iModelIndex, "BASE_ATTRIBUTE_" + iIndex.ToString());
        float fLevelAddValue = (float)TableCommon.GetNumberFromRoleEquipConfig(iModelIndex, "STRENGTHEN_" + iIndex.ToString());
        // 1星基础属性数值 + 强化等级 * 强化属性加成值
        return fBaseValue + iStrengthenLevel * fLevelAddValue;
    }

    //equip base attribute value 
    public static float GetEquipBaseAttributeValue(int iIndex, int iModelIndex, int iStrengthenLevel)
    {
        float fBaseValue = (float)TableCommon.GetNumberFromRoleEquipConfig(iModelIndex, "BASE_ATTRIBUTE_" + iIndex.ToString());
        float fLevelAddValue = (float)TableCommon.GetNumberFromRoleEquipConfig(iModelIndex, "STRENGTHEN_" + iIndex.ToString());
        // 基础属性数值 + (强化等级-1) * 强化属性加成值
        return fBaseValue + (iStrengthenLevel - 1.0f) * fLevelAddValue;
    }

    public virtual int GetMaxExp()
    {
        return TableCommon.GetNumberFromMagicEquipLvConfig(strengthenLevel,"LEVEL_UP_EXP_" + (int)mQualityType);
    }

    //--------------------------------------------------------------------------------------------------
    // affect attribute
    public float GetAttribute(int iIndex)
    {
        float fValue = 0;

        DataRecord dataRecord = DataCenter.mRoleEquipConfig.GetRecord(tid);
        if (dataRecord != null)
        {
            ITEM_TYPE type = PackageManager.GetItemTypeByTableID(tid);
            if (ITEM_TYPE.MAGIC == type || ITEM_TYPE.EQUIP == type)
            {
                fValue += GetStrengthenAttribute(iIndex);
                //fValue += GetRefineAttribute(iIndex);
            }
        }
        return fValue;
    }

    /// <summary>
    /// 获得强化属性
    /// </summary>
    /// <param name="iIndex">索引值</param>
    /// <returns>强化属性值</returns>
    public float GetStrengthenAttribute(int iIndex)
    {
        DataRecord dataRecord = DataCenter.mRoleEquipConfig.GetRecord(tid);
        if (dataRecord != null)
        {
            int iBaseValue = dataRecord["BASE_ATTRIBUTE_" + iIndex];
            int iAddStrengthenValue = dataRecord["STRENGTHEN_" + iIndex];
            return iBaseValue + iAddStrengthenValue * (strengthenLevel - 1.0f);
        }
        return 0;
    }
    /// <summary>
    /// 获得特定强化等级的强化属性
    /// </summary>
    /// <param name="iIndex"></param>
    /// <param name="iStrengthenLevel"></param>
    /// <returns></returns>
    public float GetSpecificStrengthenAttribute(int iIndex, int iStrengthenLevel) 
    {
        DataRecord dataRecord = DataCenter.mRoleEquipConfig.GetRecord(tid);
        if (dataRecord != null)
        {
            int iBaseValue = dataRecord["BASE_ATTRIBUTE_" + iIndex];
            int iAddStrengthenValue = dataRecord["STRENGTHEN_" + iIndex];
            return iBaseValue + iAddStrengthenValue * (iStrengthenLevel - 1.0f);
        }
        return 0;
    }

    /// <summary>
    /// 获得精炼属性
    /// </summary>
    /// <param name="iIndex">索引值</param>
    /// <returns>精炼属性值</returns>
    public float GetRefineAttribute(int iIndex)
    {
        DataRecord dataRecord = DataCenter.mRoleEquipConfig.GetRecord(tid);
        if (dataRecord != null)
        {
            AFFECT_TYPE affectType = (AFFECT_TYPE)((int)dataRecord["REFINE_TYPE_" + iIndex]);
            int iAddValue = dataRecord["REFINE_VALUE_" + iIndex];

            return iAddValue * refineLevel;
        }
        return 0;
    }

    /// <summary>
    /// 计算增加一定精炼经验值后的精炼等级和经验
    /// 注意：调用此方法并不会真正改变装备精炼数据，只是用作参考
    /// </summary>
    /// <param name="exp"> 增加的经验值 </param>
    /// <param name="level"> 增加经验值后的精炼等级 </param>
    /// <param name="exp"> 增加经验值后的当前精炼经验 </param>
    /// <returns> 当前已满级或增加经验后满级 </returns>
    string mtemp;
    public bool TryAddRefineExp(int exp, out int finalLevel, out int finalExp)
    {
       // UIButtonEvent[] mtempObj = new UIButtonEvent[4];
        if (refineLevel > mMaxRefineLevel)
        {
            finalLevel = refineLevel;
            finalExp = 0;
            return true;
        }

        finalLevel = refineLevel;
        finalExp = refineExp;
        int cur = exp;
        string fieldName = "LEVEL_UP_EXP_" + (int)mQualityType;
        
        while (cur > 0 && finalLevel < mMaxRefineLevel)
        {
            int needExp = DataCenter.mEquipRefineLvConfig.GetData(finalLevel, fieldName) - finalExp;

            if (needExp > cur)
            {
                finalExp += cur;
                cur = 0;
                UIButtonEvent.can_press = true;
            }
            else 
            {
                ++finalLevel;
                GameObject wumaotexiao = GameCommon.FindObject(GameObject.Find("equip_refine_info_window"), "ui_weapon_lvup");
                string mtip = DataCenter.mStringList.GetData(2509, "STRING_CN");
                mtip = mtip.Replace("{0}", finalLevel.ToString());
                if (mtemp != mtip)
                {
                    DataCenter.OnlyTipsLabelMessage(mtip);
                    wumaotexiao.SetActive(false);
                    wumaotexiao.SetActive(true);
                    GlobalModule.DoLater(() => { wumaotexiao.SetActive(false);}, 0.8f);
                }
                GlobalModule.DoLater(() => { UIButtonEvent.can_press = true; }, 0.8f);
                mtemp = mtip;
                for (int j = 0; j < 4; j++)
                {
                    EquipRefineInfoWindow.mtempObj[j].BanPress(false);
                    UIButtonEvent.can_press = false;
                }
                
                finalExp = 0;
                cur -= needExp;
            }
        }

        return finalLevel > mMaxRefineLevel;
    }


    /// <summary>
    /// 增加精炼经验
    /// </summary>
    /// <param name="exp"> 增加的经验值 </param>
    public void AddRefineExp(int exp)
    {
        int finalLevel = 0;
        int finalExp = 0;
        TryAddRefineExp(exp, out finalLevel, out finalExp);
        refineLevel = finalLevel;
        refineExp = finalExp;
    }

    public bool IsStrengthenLevelUp()
    {
        return strengthenLevel > 1;
    }

    public bool IsRefineLevelUp()
    {
        return refineLevel > 1;
    }
}

public class RoleEquipData : EquipData
{
    public RoleEquipData()
    {

    }

    public override bool ApplyAffect()
    {
        for (int i = 0; i < mBaseAttributeList.Count; i++)
        {
            EquipBaseAttribute temp = mBaseAttributeList[i];
            if (temp != null)
            {
                temp.SetEquipData(this);
                temp.ApplyAffect();
            }
        }


        for (int i = 0; i < mAttachAttributeList.Count; i++)
        {
            EquipAttachAttribute temp = mAttachAttributeList[i];
            if (temp != null)
                temp.ApplyAffect();
        }

        return true;
    }
}

public class NetEquipLogicData : RespMessage
{
    public Dictionary<string, EquipData> itemList = new Dictionary<string, EquipData>();
}

public class NetRoleEquipLogicData : NetEquipLogicData
{
}

public class NetMagicLogicData : NetEquipLogicData
{
}

public class EquipLogicData : tLogicData
{
    /// <summary>
    /// key: DBID
    /// value: EquipData
    /// </summary>
    public Dictionary<int, EquipData> mDicEquip = new Dictionary<int, EquipData>();

    public bool AddItemData(EquipData itemData)
    {
        if (itemData != null)
        {
            if (!mDicEquip.ContainsKey(itemData.itemId))
            {
                itemData.Init();
                mDicEquip.Add(itemData.itemId, itemData);
            }
        }
        return false;
    }

    public bool UpdateItemData(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            if (itemData.itemNum <= 0)
            {
                RemoveItemData(itemData.itemId);
            }
            else
            {
                if (mDicEquip.ContainsKey(itemData.itemId))
                {
                    mDicEquip[itemData.itemId].itemId = itemData.itemId;
                    mDicEquip[itemData.itemId].tid = itemData.tid;
                    mDicEquip[itemData.itemId].itemNum = itemData.itemNum;
                }
                else
                {
                    AddItemData(new EquipData(itemData));
                }
            }
        }
        return false;
    }

    public bool RemoveItemData(int iItemId)
    {
        if (mDicEquip.ContainsKey(iItemId))
        {
            if (mDicEquip[iItemId].IsInTeam())
                return false;

            mDicEquip.Remove(iItemId);
            return true;
        }

        return false;
    }

    public EquipData GetEquipDataByItemId(int iItemId)
    {
        if (mDicEquip.ContainsKey(iItemId))
            return mDicEquip[iItemId];
        return null;
    }

    public EquipData GetEquipDataByGridIndex(int iGridIndex)
    {
        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            if (pair.Value.mGridIndex == iGridIndex)
            {
                return pair.Value;
            }
        }
        return null;
    }

    public EquipData GetEquipDataByGridIndex(int iGridIndex, int iType)
    {
        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            if (pair.Value.mGridIndex == iGridIndex && TableCommon.GetNumberFromRoleEquipConfig(pair.Value.tid, "ROLEEQUIP_TYPE") == iType)
            {
                return pair.Value;
            }
        }
        return null;
    }

    public EquipData GetEquipDataByModelIndex(int iModelIndex)
    {
        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            if (pair.Value.tid == iModelIndex)
            {
                return pair.Value;
            }
        }
        return null;
    }

    public int GetNumByIndex(int iIndex)
    {
        int iCount = 0;
        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            if (pair.Value.tid == iIndex)
            {
                iCount++;
            }
        }
        return iCount;
    }

    public bool RemoveRoleEquipByModelIndex(int iModelIndex)
    {
        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            if (pair.Value.tid == iModelIndex)
            {
                mDicEquip.Remove(pair.Key);
                return true;
            }
        }
        return false;
    }

    public bool GetEquipIdListByTeamPos(List<int> equipIdList, int iTeamPos, int iTypeMaxNum)
    {
        for (int i = 0; i < iTypeMaxNum; i++)
        {
            EquipData equipData = GetEquipDataByTeamPosAndType(iTeamPos, i);
            int iItemId = 0;
            if (equipData != null)
            {
                iItemId = equipData.itemId;
            }
            equipIdList.Add(iItemId);
        }

        return true;
    }

    public EquipData[] GetEquipDatasByTeamPos(int iTeamPos)
    {
        List<EquipData> datas = new List<EquipData>();

        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            if (iTeamPos == pair.Value.teamPos)
            {
                datas.Add(pair.Value);
            }
        }

        return datas.ToArray();
    }

    public EquipData GetEquipDataByTeamPosAndType(int TeamPos, int iType)
    {
        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            int tid = pair.Value.tid;
            if (TeamPos == pair.Value.teamPos && iType == PackageManager.GetSlotPosByTid(tid))
            {
                return pair.Value;
            }
        }
        return null;
    }

    public int GetEquipDataNumByType(int iType)
    {
        int iCount = 0;
        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            if (PackageManager.GetSlotPosByTid(pair.Value.tid) == iType)
            {
                iCount++;
            }
        }
        return iCount;
    }

    public void ChangeTeamPos(TeamPosChangeData teamPosChangeData)
    {
        if (null == teamPosChangeData)
            return;

        ITEM_TYPE downItemType = PackageManager.GetItemTypeByTableID(teamPosChangeData.downTid);
        ITEM_TYPE upItemType = PackageManager.GetItemTypeByTableID(teamPosChangeData.upTid);

        if (ITEM_TYPE.EQUIP == downItemType && ITEM_TYPE.MAGIC == downItemType)
            return;
        if (ITEM_TYPE.MAGIC == downItemType && ITEM_TYPE.EQUIP == downItemType)
            return;

        if (ITEM_TYPE.EQUIP == downItemType || ITEM_TYPE.MAGIC == downItemType
            || ITEM_TYPE.EQUIP == upItemType || ITEM_TYPE.MAGIC == upItemType)
        {
            // down
            ChangeTeamPos(teamPosChangeData.downItemId, teamPosChangeData.downTid, -1);
            // up
            ChangeTeamPos(teamPosChangeData.upItemId, teamPosChangeData.upTid, teamPosChangeData.teamPos);
        }
    }

    public void ChangeTeamPos(int iItemId, int iTid, int iTeamPos)
    {
        ITEM_TYPE itemType = PackageManager.GetItemTypeByTableID(iTid);
        if (itemType != ITEM_TYPE.EQUIP && itemType != ITEM_TYPE.MAGIC)
            return;

        EquipData data = GetEquipDataByItemId(iItemId);
        if (data != null)
        {
            data.teamPos = iTeamPos;
        }
    }
}

public class RoleEquipLogicData : EquipLogicData
{
    public Dictionary<int, Dictionary<int, EquipData>> mDicRoleUseEquip = new Dictionary<int, Dictionary<int, EquipData>>();

    public static RoleEquipLogicData Self;
    public RoleEquipLogicData()
    {
        Self = this;
    }

    public Dictionary<int, EquipData> GetCurRoleAllUseEquips()
    {
        int iUserID = RoleLogicData.GetMainRole().mIndex;
        if (mDicRoleUseEquip.ContainsKey(iUserID))
            return mDicRoleUseEquip[iUserID];
        return null;
    }

    // fa qi
    public EquipData GetUseEquip()
    {
        return GetUseEquip((int)EQUIP_TYPE.ELEMENT_EQUIP);
    }

    public EquipData GetUseEquip(int type)
    {
        Dictionary<int, EquipData> dicUseEquip = GetCurRoleAllUseEquips();

        if (dicUseEquip != null)
        {
            if (dicUseEquip.ContainsKey(type))
            {
                return dicUseEquip[type];
            }
        }
        return null;
    }

    public void SetUseEquip(int type, EquipData roleEquipData)
    {
        int iUserID = 0;
        if (roleEquipData != null)
            iUserID = roleEquipData.mUserID;
        else
            iUserID = RoleLogicData.GetMainRole().mIndex;

        if (mDicRoleUseEquip.ContainsKey(iUserID))
        {
            Dictionary<int, EquipData> dicEquipData = mDicRoleUseEquip[iUserID];
            if (roleEquipData != null)
            {
                if (dicEquipData.ContainsKey(type))
                {
                    dicEquipData[type].mUserID = 0;
                    dicEquipData[type] = roleEquipData;
                }
                else
                {
                    dicEquipData.Add(type, roleEquipData);
                }
            }
            else
            {
                if (dicEquipData.ContainsKey(type))
                {
                    dicEquipData.Remove(type);

                    if (dicEquipData.Count <= 0)
                        mDicRoleUseEquip.Remove(iUserID);
                }
            }
        }
        else
        {
            if (roleEquipData != null)
            {
                Dictionary<int, EquipData> dicEquipData = new Dictionary<int, EquipData>();
                dicEquipData.Add(type, roleEquipData);
                mDicRoleUseEquip.Add(iUserID, dicEquipData);
            }
        }
    }

    public bool RemoveRoleEquip(EquipData roleEquipData)
    {
        if (roleEquipData != null)
        {
            int iUserID = roleEquipData.mUserID;
            if (mDicRoleUseEquip.ContainsKey(iUserID))
            {
                Dictionary<int, EquipData> dicEquipData = mDicRoleUseEquip[iUserID];
                int type = TableCommon.GetNumberFromRoleEquipConfig(roleEquipData.tid, "ROLEEQUIP_TYPE");
                if (dicEquipData.ContainsKey(type) && roleEquipData.itemId == dicEquipData[type].itemId)
                {
                    dicEquipData.Remove(type);
                }

                if (dicEquipData.Count <= 0)
                    mDicRoleUseEquip.Remove(iUserID);
            }

            if (mDicEquip.ContainsKey(roleEquipData.itemId))
            {
                mDicEquip.Remove(roleEquipData.itemId);

                return true;
            }
        }

        return false;
    }

    public bool AttachRoleEquip(DataRecord re)
    {
        if (re == null)
            return false;

        bool bIsAdd = false;
        int iItemId = re.getData("ID");

        RoleEquipData roleEquip = GetEquipDataByItemId(iItemId) as RoleEquipData;
        if (roleEquip == null)
        {
            roleEquip = new RoleEquipData();
            bIsAdd = true;
        }

        roleEquip.itemId = re.getData("ID");
        roleEquip.tid = re.getData("ITEM_ID");
        roleEquip.strengthenLevel = re.getData("ITEM_ENHANCE");
        roleEquip.mMaxStrengthenLevel = TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "MAX_GROW_LEVEL");
        roleEquip.mStarLevel = TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "STAR_LEVEL");
        roleEquip.mElementType = (ELEMENT_TYPE)((int)re.getData("ITEM_ELEMENT"));
        roleEquip.mQualityType = (EQUIP_QUALITY_TYPE)TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "QUALITY");
        roleEquip.mUserID = re.getData("USER_ID");
        roleEquip.isNew = re.getData("NEW_SIGN");
        roleEquip.mReset = re.getData("ITEM_RESET_NUM");
        roleEquip.mExp = re.getData("ITEM_EXP");

        roleEquip.mBaseAttributeList.Clear();
        for (int i = 0; i < 2; i++)
        {
            float fCurBaseAttributeValue = RoleEquipData.GetBaseAttributeValue(i, roleEquip.tid, roleEquip.strengthenLevel);
            roleEquip.AddEquipBaseAttriblute(i, (AFFECT_TYPE)TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "ATTRIBUTE_TYPE_" + i.ToString()), fCurBaseAttributeValue);
        }

        roleEquip.mAttachAttributeList.Clear();
        for (int i = 0; i < 5; i++)
        {
            roleEquip.AddEquipAttachAttriblute(i, (AFFECT_TYPE)((int)re.getData("EXTRA_TYPE_" + (i + 1).ToString())), re.getData("EXTRA_NUM_" + (i + 1).ToString()));
        }

        roleEquip.ApplyAffect();

        if (bIsAdd)
            mDicEquip.Add(roleEquip.itemId, roleEquip);

        if (roleEquip.mUserID != 0)
            SetUseEquip(TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "ROLEEQUIP_TYPE"), roleEquip);

        return true;
    }

    public int GetRoleEquipCount()
    {
        return mDicRoleUseEquip.Count;
    }

	/// <summary>
	/// 判断是否有未装备的空位或高于已装备的未上阵的装备
	/// </summary>
	/// <returns><c>true</c> if this instance has free equip; otherwise, <c>false</c>.</returns>
	public bool HasFreeEquipAndFreeEquipPos()
	{		
		for (int i = (int)TEAM_POS.CHARACTER; i <= (int)TEAM_POS.PET_3; i++) 
		{
			if(TeamManager.GetPetDataByTeamPos(i) != null || i == 0)
			{
				for(int j = (int)EQUIP_TYPE.ARM_EQUIP; j <= (int)EQUIP_TYPE.DEFENCE_EQUIP2; j++)
				{
					EquipData posEquipData = TeamManager.GetRoleEquipDataByTeamPos (i, j);
					foreach (KeyValuePair<int, EquipData> pair in mDicEquip) 
					{
						if(pair.Value.IsInTeam())
							continue;

						int packageEquipType = TableCommon.GetNumberFromRoleEquipConfig(pair.Value.tid, "EQUIP_TYPE");

						if(packageEquipType == j + 1 && posEquipData == null)
						{
							return true;
						}else if(posEquipData != null && packageEquipType == j + 1 )
						{
							if((int)posEquipData.mQualityType < (int)pair.Value.mQualityType)
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}
}

public class MagicLogicData : EquipLogicData
{
    public static MagicLogicData Self;
    public MagicLogicData()
    {
        Self = this;
    }

    public bool GetRefineStuffPetList(out List<EquipData> itemDataList, EquipData itemData)
    {
        itemDataList = new List<EquipData>();
        if (null == itemData)
            return false;

        foreach (KeyValuePair<int, EquipData> pair in mDicEquip)
        {
            if (pair.Value.tid == itemData.tid && pair.Value.itemId != itemData.itemId)
            {
                EquipData item = pair.Value;
                if (item.IsStrengthenLevelUp() || item.IsRefineLevelUp() /*added by xuke*/ || item.IsInTeam()/*end*/)
                    continue;

                itemDataList.Add(pair.Value);
            }
        }
        return itemDataList.Count > 0;
    }

    public int GetRefineStuffMagicCount(EquipData itemData)
    {
        List<EquipData> itemDataList;
        GetRefineStuffPetList(out itemDataList, itemData);

        return itemDataList.Count;
    }
}

//-------------------------------------------------
// equip fragment
//-------------------------------------------------
public class EquipFragmentData : FragmentBaseData
{
    public EquipFragmentData()
    {

    }

    public EquipFragmentData(ItemDataBase itemData)
        :base(itemData)
    {
    }
}

public class RoleEquipFragmentData : EquipFragmentData
{
}

public class MagicFragmentData : EquipFragmentData
{
}


public class NetEquipFragmentLogicData : RespMessage
{
    public Dictionary<string, EquipFragmentData> itemList = new Dictionary<string, EquipFragmentData>();
}

public class NetRoleEquipFragmentLogicData : NetEquipFragmentLogicData
{
}
public class NetMagicFragmentLogicData : NetEquipFragmentLogicData
{
}

public class EquipFragmentLogicData : tLogicData
{
    //public static EquipFragmentLogicData self;
    public Dictionary<int, EquipFragmentData> mDicEquipFragmentData = new Dictionary<int, EquipFragmentData>();

    public EquipFragmentLogicData() {
    }
    public EquipFragmentData AddItemData(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            EquipFragmentData data;
            if (mDicEquipFragmentData.TryGetValue(itemData.tid, out data))
            {
                data.itemId = itemData.itemId;
                data.itemNum += itemData.itemNum;
                data.itemNum = Mathf.Clamp(data.itemNum, 0, CommonParam.packageOverlapLimit);

                if (data.itemNum <= itemData.itemNum)
                {
                    data.mComposeItemTid = TableCommon.GetNumberFromFragment(itemData.tid, "ITEM_ID");
                }

                return data;
            }

            data = new EquipFragmentData(itemData);
            mDicEquipFragmentData.Add(itemData.tid, data);
            return data;
        }
        return null;
    }

    public EquipFragmentData AddItemData(int itemID, int tid, int count)
    {
        ItemDataBase item = new ItemDataBase();
        item.itemId = itemID;
        item.tid = tid;
        item.itemNum = count;
        return AddItemData(item);
    }

    public void UpdateItemData(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            EquipFragmentData data;
            if (mDicEquipFragmentData.TryGetValue(itemData.tid, out data))
            {
                data.itemId = itemData.itemId;
                data.itemNum = itemData.itemNum;
            }
            else
            {
                data = new EquipFragmentData(itemData);
                mDicEquipFragmentData.Add(itemData.tid, data);
            }

            if (itemData.itemNum <= 0)
            {
                mDicEquipFragmentData.Remove(itemData.tid);
            }
        }
    }

    public EquipFragmentData GetItemDataByTid(int iTid)
    {
        if (mDicEquipFragmentData.ContainsKey(iTid))
        {
            return mDicEquipFragmentData[iTid];
        }
        else
        {
            return AddItemData(-1, iTid, 0);
        }
    }

    public void SetDataByItemID(int itemID, int count)
    {
        if (mDicEquipFragmentData.ContainsKey(itemID))
            mDicEquipFragmentData[itemID].itemNum += count;
        else
        {
            EquipFragmentData data = new EquipFragmentData();
            data.itemNum = count;
            data.tid = itemID;
            data.mComposeItemTid = TableCommon.GetNumberFromFragment(itemID, "ITEM_ID");
            mDicEquipFragmentData.Add(itemID, data);
        }
    }

    public EquipFragmentData GetEquipFragmentDataByGridIndex(int iGridIndex)
    {
        foreach (KeyValuePair<int, EquipFragmentData> pair in mDicEquipFragmentData)
        {
            if (pair.Value.mGridIndex == iGridIndex)
            {
                return pair.Value;
            }
        }
        return null;
    }

    public EquipFragmentData GetEquipFragmentDataByIndex(int iIndex)
    {
        if (mDicEquipFragmentData.ContainsKey(iIndex))
            return mDicEquipFragmentData[iIndex];
        return null;
    }

    public bool ChangeItemDataNum(int iItemId, int iTid, int dCount)
    {
        ItemDataBase item = new ItemDataBase();
        item.itemId = iItemId;
        item.tid = iTid;
        item.itemNum = dCount;
        return ChangeItemDataNum(item);
    }

    public bool ChangeItemDataNum(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            if (mDicEquipFragmentData.ContainsKey(itemData.tid))
            {
                EquipFragmentData data = mDicEquipFragmentData[itemData.tid];
                data.itemNum += itemData.itemNum;
                if (data.itemNum <= 0)
                {
                    mDicEquipFragmentData.Remove(itemData.tid);
                }
            }
            else
            {
                if (itemData.itemNum < 0)
                    return false;

                EquipFragmentData data = new EquipFragmentData(itemData);
                mDicEquipFragmentData.Add(itemData.tid, data);
            }
        }        
        return false;
    }

    public int GetCountByTid(int iTid)
    {
        int count = 0;
        foreach (KeyValuePair<int, EquipFragmentData> pair in mDicEquipFragmentData)
        {
            if (pair.Key == iTid)
                return pair.Value.itemNum;
        }
        return count;
    }
}

public class RoleEquipFragmentLogicData : EquipFragmentLogicData
{
    static public RoleEquipFragmentLogicData Self = null;

    public RoleEquipFragmentLogicData()
    {
        Self = this;
    }
}

public class MagicFragmentLogicData : EquipFragmentLogicData
{
    static public MagicFragmentLogicData Self = null;

    public MagicFragmentLogicData()
    {
        Self = this;
    }
}