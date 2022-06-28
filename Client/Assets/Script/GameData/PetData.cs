using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using System;
using System.Linq;
public class ActiveData : TeamPosItemDataBase
{
	public int level;
	public int starLevel;
	public int exp;	
	public int mMaxLevelNum = 120;
	public int mMaxLevelBreakNum = 100;
	public int fateLevel; // 天命等级
    public int mMaxFateLevel; // 天命等级上限
    public int fateExp; // 天命经验
    public int fateStoneCost; // 天命石消耗
	public int breakLevel; // 突破等级
    public int[] skillLevel; // 技能等级

    public ActiveData()
    {
    }

    public ActiveData(ItemDataBase itemData)
    {
        if (null == itemData)
            return;

        itemId = itemData.itemId;
        tid = itemData.tid;
        itemNum = itemData.itemNum;
        level = 1;
        breakLevel = 0;
        exp = 0;
        fateLevel = 0;
        fateExp = 0;
        fateStoneCost = 0;
        mMaxLevelNum = DataCenter.mActiveConfigTable.GetData(itemData.tid, "MAX_LEVEL");
        skillLevel = new int[4];
        for (int i = 0; i < 4; i++ )
        {
            skillLevel[i] = 1;
        }
    }

    public virtual void Init()
    {
        starLevel = TableCommon.GetNumberFromActiveCongfig(tid, "STAR_LEVEL");
        mMaxLevelBreakNum = TableCommon.GetNumberFromActiveCongfig(tid, "MAX_LEVEL_BREAK");
        mMaxFateLevel = TableCommon.GetNumberFromActiveCongfig(tid, "MAX_STRENGTH");
    }

    private DataRecord GetDateRecord()
    {
        ITEM_TYPE type = PackageManager.GetItemTypeByTableID(tid);
        if (ITEM_TYPE.MONSTER == type)
        {
            return DataCenter.mMonsterObject.GetRecord(tid);
        }
        else if(ITEM_TYPE.PET == type || ITEM_TYPE.CHARACTER == type)
        {
            return DataCenter.mActiveConfigTable.GetRecord(tid);
        }
        return null;
    }
    //--------------------------------------------------------------------------------------------------
    // affect attribute
    public int GetAttribute(AFFECT_TYPE affectType)
    {
        int fValue = 0;

        fValue = Convert.ToInt32(GetBaseAttribute(affectType));
        affectType = GameCommon.GetAffactType(affectType);
        AFFECT_TYPE rateAffectType = GameCommon.GetAffactRateType(affectType);

        switch (affectType)
        {
            case AFFECT_TYPE.ATTACK:
                return GameCommon.GetTotalAttack(this);
            case AFFECT_TYPE.HP:
                return GameCommon.GetTotalMaxHP(this);
            case AFFECT_TYPE.PHYSICAL_DEFENCE:
                return GameCommon.GetTotalPhysicalDefence(this);
            case AFFECT_TYPE.MAGIC_DEFENCE:
                return GameCommon.GetTotalMagicDefence(this);
        }
        return fValue;
    }

    public float GetBaseAttribute(AFFECT_TYPE affectType)
    {
        string strAffectType = affectType.ToString();
        DataRecord dataRecord = GetDateRecord();
        if (dataRecord != null)
        {
            // 基础属性+等级×属性成长+等级×突破等级×突破属性成长
            float fBaseValue = (float)dataRecord["BASE_" + strAffectType];
            float fAddValue = (float)dataRecord["ADD_" + strAffectType];
            int iBreakValue = (int)dataRecord["BREAK_" + strAffectType];
            int baseBreakValue = dataRecord["BASE_BREAK_" + strAffectType];
            return fBaseValue + fAddValue * (level - 1) + breakLevel * baseBreakValue + (level - 1) * breakLevel * iBreakValue;
        }
        return 0;
    }
       

    // break
    private float GetBreakAddRate(AFFECT_TYPE affectType)
    {
        return GetBreakAddValue(affectType) * 0.01f;
    }

    private float GetBreakAddValue(AFFECT_TYPE affectType)
    {
        int value = 0;
        for (int i = 1; i <= breakLevel; i++)
        {
            int bufferId = TableCommon.GetNumberFromBreakBuffConfig(tid, "BREAK_" + i);
            if (bufferId > 0)
            {
                AFFECT_TYPE tempAffectType = AffectBuffer.GetAffectType(bufferId);
                if (tempAffectType == affectType)
                {
                    DataRecord record = DataCenter.mAffectBuffer.GetRecord(bufferId);
                    if (record != null)
                    {
                        value += record["AFFECT_VALUE"];
                    }
                }
            }
        }

        return value;
    }

    // fate
    private float GetFateAddRate(AFFECT_TYPE affectType)
    {
        return GetFateAddValue(affectType) * 0.01f;
    }

    private float GetFateAddValue(AFFECT_TYPE affectType)
    {
        int value = 0;
        for (int i = 1; i <= fateLevel; i++)
        {
            int bufferId = TableCommon.GetNumberFromFateConfig(tid, "BREAK_" + i);
            if (bufferId > 0)
            {
                AFFECT_TYPE tempAffectType = AffectBuffer.GetAffectType(bufferId);
                if(tempAffectType == affectType)
                {
                    DataRecord record = DataCenter.mAffectBuffer.GetRecord(bufferId);
                    if(record != null)
                    {
                        value += record["AFFECT_VALUE"];
                    }
                }
            }
        }
        
        return value;
    }
    
    private int GetRoleSkinAttack()
    {
        return (int)TableCommon.GetNumberFromRoleSkinConfig(tid, "ROLE_SKIN_ATTACK"); ;
    }













    //-------------------------------------------------
    // max hp
    //-------------------------------------------------
    // base 
    public float GetBaseHp()
    {
        return GameCommon.GetBaseMaxHP(tid, level, breakLevel);
    }

    public int GetRoleSkinHP()
    {
        return (int)TableCommon.GetNumberFromRoleSkinConfig(tid, "ROLE_SKIN_HP"); ;
    }

    //-------------------------------------------------
    // physical defence
    //-------------------------------------------------
    // base 
    public float GetBasePhysicalDefence()
    {
        return GameCommon.GetBasePhysicalDefence(tid, level, breakLevel);
    }

    //-------------------------------------------------
    // magic defence
    //-------------------------------------------------
    // base 
    public float GetBaseMagicDefence()
    {
        return GameCommon.GetBaseMagicDefence(tid, level, breakLevel);
    }
}

public class PetSkillData
{
    public int index;
    public int level;
}

public class PetData : ActiveData
{
    public int strengthenLevel;     // 强化等级  // 弃用
    public int mMaxStrengthenLevel; // 弃用
    public int mFailPoint; // 弃用
    public int mGridIndex; // 索引——背包、升级  // 弃用
    public Dictionary<int, PetSkillData> mDicPetSkill = new Dictionary<int, PetSkillData>();// 弃用
	//by chenliang
	//begin

    public int inFairyland;         //目标仙境Id

	//end

    public PetData()
    {
    }

    public PetData(ItemDataBase itemData)
    {
        if (null == itemData)
            return;

        itemId = itemData.itemId;
        tid = itemData.tid;
        itemNum = itemData.itemNum;
        level = 1;
        breakLevel = 0;
        exp = 0;
        fateLevel = 0;
        fateExp = 0;
        fateStoneCost = 0;
        skillLevel = new int[4];
        for (int i = 0; i < 4; i++ )
        {
            skillLevel[i] = 1;
        }
    }

    public PetData(int tid) {
        itemId=0;
        this.tid=tid;
        itemNum=1;
        level=1;
        breakLevel=0;
        exp=0;
        fateLevel=0;
        fateExp=0;
        fateStoneCost=0;
        skillLevel=new int[4];
        for(int i=0;i<4;i++) {
            skillLevel[i]=1;
        }
    }

    public override void Init()
    {
        base.Init();
        mMaxStrengthenLevel = TableCommon.GetNumberFromActiveCongfig(tid, "MAX_GROW_LEVEL");
    }

    public virtual int GetMaxExp()
    {
        return TableCommon.GetMaxExp(starLevel, level);
    }

    public PetSkillData GetPetSkillDataByIndex(int iKey)
    {
        if (mDicPetSkill.ContainsKey(iKey) && GameCommon.GetPassiveSkillNum(this) >= iKey)
            return mDicPetSkill[iKey];

        return null;
    }

    public int GetSkillIndexByIndex(int iKey)
    {
        if (mDicPetSkill.ContainsKey(iKey) && GameCommon.GetPassiveSkillNum(this) >= iKey)
            return mDicPetSkill[iKey].index + mDicPetSkill[iKey].level - 1;

        return 0;
    }

    public int GetSkillLevelByIndex(int iKey)
    {
        if (mDicPetSkill.ContainsKey(iKey) && GameCommon.GetPassiveSkillNum(this) >= iKey)
            return mDicPetSkill[iKey].level;

        return 1;
    }

    public void SetSkillData()
    {
        PetSkillData petSkillData = new PetSkillData();
        petSkillData.index = TableCommon.GetNumberFromActiveCongfig(tid, "PET_SKILL_1");
        petSkillData.level = 1;
        mDicPetSkill.Add(0, petSkillData);

        for (int iIndex = 1; iIndex <= 2; iIndex++)
        {
            petSkillData = new PetSkillData();
            petSkillData.index = GameCommon.GetPetPassiveSkillIndex(this, iIndex);
            petSkillData.level = 1;
            mDicPetSkill.Add(iIndex, petSkillData);
        }
    }

    public bool IsLevelUp()
    {
        return level > 1;
    }
    public bool IsBreakLevelUp()
    {
        return breakLevel > 0;
    }
    public bool IsFateLevelUp()
    {
        return fateLevel > 0;
    }
    public bool IsSkillLevelUp()
    {
        for (int i = 0; i < skillLevel.Length; i++)
        {
            if (skillLevel[i] > 1)
                return true;
        }
        return false;
    }

    public bool IsInCommonTeam()
    {
        return teamPos >= 0 && teamPos <= 3;
    }

    public bool IsInRelateTeam()
    {
        return teamPos >= 11 && teamPos <= 16;
    }
}

public class NetPetLogicData : RespMessage
{
    public Dictionary<string, PetData> itemList = new Dictionary<string, PetData>();
}

public class PetLogicData : tLogicData
{
    public static PetLogicData Self = null;
    public Dictionary<int, PetData> mDicPetData = new Dictionary<int, PetData>();
    public NiceTable mTeamTable;

    static public NiceData mAttributePVPTeam;

    public int mCurrentTeam = 0;

    static public int mPVPTeam = 5;
    static public int mBackTeam = 6;
    static public int mFourVsFourTeam = 7;

	static public PetData mFreindPetData;

    public PetLogicData()
    {
        Self = this;
    }

    public int GetPetItemIdByTeamPos(int iTeamPos)
    {
        PetData petData = GetPetDataByTeamPos(iTeamPos);
        if (petData != null)
        {
            return petData.itemId;
        }
        return 0;
    }

    public PetData GetPetDataByTeamPos(int iTeamPos)
    {
        foreach (KeyValuePair<int, PetData> pair in mDicPetData)
        {
            if (pair.Value.teamPos == iTeamPos)
            {
                return pair.Value;
            }
        }
        return null;
    }

    public bool GetBreakStuffPetList(out List<PetData> petDataList, ActiveData itemData)
    {
        petDataList = new List<PetData>();

        if(itemData == null)
            return false;

        foreach (KeyValuePair<int, PetData> pair in mDicPetData)
        {
            if (pair.Value.tid == itemData.tid && itemData.itemId != pair.Value.itemId)
            {
                PetData pet = pair.Value;
                if (pet.IsLevelUp() || pet.IsBreakLevelUp() || pet.IsFateLevelUp() || pet.IsSkillLevelUp())
                    continue;

                petDataList.Add(pair.Value);
            }
        }
        return petDataList.Count > 0;
    }

	/// <summary>
	/// 判断是否有未上阵的符灵
	/// </summary>
	/// <returns><c>true</c> if this instance has free pet; otherwise, <c>false</c>.</returns>
	public bool HasFreePet()
	{
		// 得到当前上阵的符灵数量
		int _onTeamPetCount = TeamManager.GetOnTeamPetCount ();
		foreach (KeyValuePair<int,PetData> pair in mDicPetData) 
		{
			// 如果当前符灵已经上阵，则继续
			if(pair.Value.IsInTeam())
				continue;

			int _difCount  = 0;
			// 对于未上阵的符灵，遍历上阵符灵，如果tid不一样则有可上阵
			for(int i = (int)TEAM_POS.PET_1;i < (int)TEAM_POS.MAX;i++)
			{
				PetData _petData = TeamManager.GetPetDataByTeamPos(i);
				if(_petData == null)
					continue;
				if(_petData.tid != pair.Value.tid)
					_difCount++;
			}

			if(_difCount == _onTeamPetCount)
				return true;
		}
		return false;
	}

    public int GetBreakStuffPetCount(ActiveData itemData)
    {
        if (itemData == null)
            return 0;

        List<PetData> petDataList;
        GetBreakStuffPetList(out petDataList, itemData);

        return petDataList.Count;
    }

    public PetData GetPetDataByItemId(int iItemId)
    {
        PetData pet;
        if (mDicPetData.TryGetValue(iItemId, out pet))
            return pet;

        return null;
    }

    public void ChangeTeamPos(TeamPosChangeData teamPosChangeData)
    {
        if (null == teamPosChangeData)
            return;

        if (PackageManager.GetItemTypeByTableID(teamPosChangeData.downTid) == ITEM_TYPE.PET
            || PackageManager.GetItemTypeByTableID(teamPosChangeData.upTid) == ITEM_TYPE.PET)
        {
            // down
            ChangeTeamPos(teamPosChangeData.downItemId, teamPosChangeData.downTid, -1);
            // up
            ChangeTeamPos(teamPosChangeData.upItemId, teamPosChangeData.upTid, teamPosChangeData.teamPos);
        }
    }

    public void ChangeTeamPos(int iItemId, int iTid, int iTeamPos)
    {
        if (PackageManager.GetItemTypeByTableID(iTid) != ITEM_TYPE.PET)
            return;

        PetData data = GetPetDataByItemId(iItemId);
        if (data != null)
        {
            data.teamPos = iTeamPos;
        }
    }

    //-------------------------------------------------
    // old
    //-------------------------------------------------
    public PetData GetAttributePVPPet(int teamPos)
    {
        if (mAttributePVPTeam != null)
        {
            int petID = mAttributePVPTeam.get(teamPos.ToString());
            return GetPetDataByItemId(petID);
        }
        return null;
    }

    public bool InAttributePVPTeam(PetData pet)
    {
        for (int i=0; i<6; ++i)
        {
            if (pet==GetAttributePVPPet(i))
                return true;
        }
        return false;
    }

    public PetData GetPetDataByPos(int iTeam, int iPos)
    {
        if (iPos < 0 || iPos > 3)
        {
            DEBUG.LogError("Set team pos error > " + iPos.ToString());
            return null;
        }
        DataRecord t = mTeamTable.GetRecord(iTeam);
        if (t != null)
        {
            int iPetID = (int)(t.get(iPos));
            if (iPetID > 0)
            {
                PetData result;
                if (mDicPetData.TryGetValue(iPetID, out result))
                    return result;
            }
        }
        return null;
    }

    public PetData GetPetDataByPos(int iPos)
    {
        //return GetPetDataByPos(mCurrentTeam, iPos);
        return TeamManager.GetPetDataByTeamPos(iPos);
    }

    public int GetPosInTeam(int iTeam, int iItemId)
    {
        DataRecord t = mTeamTable.GetRecord(iTeam);
        if (t != null)
        {
            for (int iPos = 1; iPos <= 3; iPos++)
            {
                int iPetID = (int)(t.get(iPos));
                if (iPetID != 0 && iPetID == iItemId)
                    return iPos;
            }
        }
        return 0;
    }

    public int GetPosInCurTeam(int iItemId)
    {
        return GetPosInTeam(mCurrentTeam, iItemId);
    }

    public void SetPosInTeam(int iTeam, int iPos, int iItemId)
    {
        DataRecord t = mTeamTable.GetRecord(iTeam);
        if (t != null)
        {
            t.set(iPos, iItemId);
        }
    }

    public bool IsPetUsedInTeam(int iTeam, int iItemId)
    {
        return GetPosInTeam(iTeam, iItemId) != 0;
    }

	public bool IsPetUsedInCurTeam(int iItemId)
	{
		return IsPetUsedInTeam(mCurrentTeam, iItemId);
	}

	public bool IsPetUsed(int iItemId)
	{
		for(int iTeam = 0; iTeam < 5; iTeam++)
		{
			if(IsPetUsedInTeam(iTeam, iItemId))
				return true;
		}

		return false;
	}

    public bool IsPVPUsePet(int petDBID)
    {
        DataRecord t = mTeamTable.GetRecord(mPVPTeam);
        if (t != null)
        {
            for (int iPos = 1; iPos <= 3; iPos++)
            {
                int iPetID = (int)(t.get(iPos));
                if (iPetID == petDBID)
                    return true;
            }
        }
        t = mTeamTable.GetRecord(mBackTeam);
        if (t != null)
        {
            for (int iPos = 1; iPos <= 3; iPos++)
            {
                int iPetID = (int)(t.get(iPos));
				if (iPetID == petDBID)
                    return true;
            }
        }
        return false;
    }

    public int GetInPVPTeam(int petDBID)
    {
        DataRecord t = mTeamTable.GetRecord(mPVPTeam);
        if (t != null)
        {
            for (int iPos = 1; iPos <= 3; iPos++)
            {
                int iPetID = (int)(t.get(iPos));
                if (iPetID == petDBID)
                    return mPVPTeam;
            }
        }
        t = mTeamTable.GetRecord(mBackTeam);
        if (t != null)
        {
            for (int iPos = 1; iPos <= 3; iPos++)
            {
                int iPetID = (int)(t.get(iPos));
                if (iPetID == petDBID)
                    return mBackTeam;
            }
        }
        return -1;
    }

    // 0 ~ 5
    public PetData GetPVPPet(int pos)
    {
        //??? 
        if (pos >= 0 && pos < 3)
            return GetPetDataByPos(mPVPTeam, pos+1);
        else
            return GetPetDataByPos(mBackTeam, pos - 3+1);
    }

	//1~3
	//ublic PetData GetFourVsFourPetDataByPos(int pos)
	//
	//	if(pos <1 || pos > 3)
	//		return null;
	//	return GetPetDataByPos(mFourVsFourTeam, pos);
	//

	public bool IsFourVsFourPVPUsePet(int petDBID)
	{
		DataRecord t = mTeamTable.GetRecord(mFourVsFourTeam);
		if (t != null)
		{
			for (int iPos = 1; iPos <= 3; iPos++)
			{
				int iPetID = (int)(t.get(iPos));
				if (iPetID == petDBID)
					return true;
			}
		}

		return false;
	}

    public PetData GetPetDataByModelIndex(int iModelIndex)
    {
        foreach (KeyValuePair<int, PetData> petV in mDicPetData)
        {
            if (petV.Value.tid == iModelIndex)
            {
                return petV.Value;
            }
        }

        Logic.EventCenter.Log(LOG_LEVEL.WARN, "there is not exist pet.mModelIndex == mModelIndex");
        return null;
    }

    public PetData[] GetPetDatasByTeam(int iTeam)
    {
        PetData[] petDatas = new PetData[3];
        petDatas[0] = GetPetDataByPos(iTeam, 1);
        petDatas[1] = GetPetDataByPos(iTeam, 2);
        petDatas[2] = GetPetDataByPos(iTeam, 3);
        return petDatas;
    }

    public bool AddItemData(PetData itemData)
    {
        if (itemData != null)
        {
            if (!mDicPetData.ContainsKey(itemData.itemId))
            {
                itemData.Init();
                mDicPetData.Add(itemData.itemId, itemData);
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
                if (mDicPetData.ContainsKey(itemData.itemId))
                {
                    mDicPetData[itemData.itemId].itemId = itemData.itemId;
                    mDicPetData[itemData.itemId].tid = itemData.tid;
                    mDicPetData[itemData.itemId].itemNum = itemData.itemNum;
                }
                else
                {
                    AddItemData(new PetData(itemData));
                }
            }
        }
        return false;
    }

    public bool RemoveItemData(PetData petData)
    {
        if (petData != null)
        {
            if (mDicPetData.ContainsKey(petData.itemId))
            {
                mDicPetData.Remove(petData.itemId);
                return true;
            }
        }

        return false;
    }

	public bool RemoveItemData(int iItemId)
	{
		if (mDicPetData.ContainsKey(iItemId))
		{
            if (mDicPetData[iItemId].IsInTeam())
                return false;

			mDicPetData.Remove(iItemId);
			return true;
		}
		
		return false;
	}

	public bool RemoveDBIDByPosInTeam(int iTeam, int iPos)
	{
		if (iPos < 0 || iPos > 3)
		{
			DEBUG.LogError("Set team pos error > " + iPos.ToString());
			return false;
		}
		DataRecord t = mTeamTable.GetRecord(iTeam);
		if (t != null)
		{
			int iPetID = (int)(t.get(iPos));
			if (iPetID > 0)
			{
				t.set(iPos, 0);
				return true;
			}
		}
		return false;
	}

	public bool RemoveDBIDByPos(int iPos)
	{
		return RemoveDBIDByPosInTeam(mCurrentTeam, iPos);
	}

    public int GetTeamElement(int iTeam)
    {
        int teamElement = -1;

        for (int i = 1; i <= 3; ++i)
        {
            PetData pet = GetPetDataByPos(i);

            if (pet == null)
                return -1;

            int iElement = TableCommon.GetNumberFromActiveCongfig(pet.tid, "ELEMENT_INDEX");

            if (iElement < 0 || iElement >= 5)
            {
                return -1;
            }
            else
            {
                if (teamElement == -1)
                    teamElement = iElement;
                else if (iElement != teamElement)
                    return -1;
            }
        }

        return teamElement;
    }

    public int GetCurTeamElement()
    {
        return GetTeamElement(mCurrentTeam);
    }

    //by chenliang
    //begin

// 	public int GetCountByModelID(int modelID)
// 	{
// 		int count = 0;
// 		int levelBreakFlag = TableCommon.GetNumberFromActiveCongfig (modelID, "LEVEL_BERAK_FLAG");
// 		foreach(KeyValuePair<int, PetData> d in mDicPetData)
// 		{
// 			if(levelBreakFlag == TableCommon.GetNumberFromActiveCongfig (d.Value.tid, "LEVEL_BERAK_FLAG"))
// 				count++;
// 		}
// 
// 		return count;
// 	}
//-------------------
    public int GetCountByTid(int tid)
    {
        int count = 0;
        foreach (KeyValuePair<int, PetData> d in mDicPetData)
        {
            if (tid == d.Value.tid)
                count++;
        }

        return count;
    }

    //end

}

//-------------------------------------------------
// 碎片基类
//-------------------------------------------------
public class FragmentBaseData : ItemDataBase
{
    public int mComposeItemTid;
    public int mGridIndex;

    public FragmentBaseData()
    {

    }

    public FragmentBaseData(ItemDataBase itemData)
    {
        if (null == itemData)
            return;

        itemNum = itemData.itemNum;
        itemId = itemData.itemId;
        tid = itemData.tid;
        mComposeItemTid = TableCommon.GetNumberFromFragment(itemData.tid, "ITEM_ID");
    }
}

//-------------------------------------------------
// pet fragment
//-------------------------------------------------
public class PetFragmentData : FragmentBaseData
{
    public PetFragmentData()
    {

    }

    public PetFragmentData(ItemDataBase itemData)
        : base(itemData)
    {
    }
}

public class NetPetFragmentLogicData : RespMessage
{
    public Dictionary<string, PetFragmentData> itemList = new Dictionary<string, PetFragmentData>();
}

public class PetFragmentLogicData : tLogicData
{
    static public PetFragmentLogicData Self = null;
    public Dictionary<int, PetFragmentData> mDicPetFragmentData = new Dictionary<int, PetFragmentData>();

    public PetFragmentLogicData()
    {
        Self = this;
    }

    public PetFragmentData AddItemData(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            PetFragmentData data;
            if (mDicPetFragmentData.TryGetValue(itemData.tid, out data))
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

            data = new PetFragmentData(itemData);
            mDicPetFragmentData.Add(itemData.tid, data);
            return data;
        }
        return null;
    }

    public void UpdateItemData(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            PetFragmentData data;
            if (mDicPetFragmentData.TryGetValue(itemData.tid, out data))
            {
                data.itemId = itemData.itemId;
                data.itemNum = itemData.itemNum;
            }
            else
            {
                data = new PetFragmentData(itemData);
                mDicPetFragmentData.Add(itemData.tid, data);
            }

            if (itemData.itemNum <= 0)
            {
                mDicPetFragmentData.Remove(itemData.tid);
            }
        }
    }

    public PetFragmentData AddItemData(int itemID, int tid, int count)
    {
        ItemDataBase item = new ItemDataBase();
        item.itemId = itemID;
        item.tid = tid;
        item.itemNum = count;
        return AddItemData(item);
    }

    public PetFragmentData GetItemDataByTid(int iTid)
    {
        if (mDicPetFragmentData.ContainsKey(iTid))
        {
            return mDicPetFragmentData[iTid];
        }
        else
        {
            return AddItemData(-1, iTid, 0);
        }
    }

    public PetFragmentData GetPetFragmentDataByGridIndex(int iGridIndex)
    {
        foreach (KeyValuePair<int, PetFragmentData> pair in mDicPetFragmentData)
        {
            if (pair.Value.mGridIndex == iGridIndex)
            {
                return pair.Value;
            }
        }
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
            if (mDicPetFragmentData.ContainsKey(itemData.tid))
            {
                PetFragmentData data = mDicPetFragmentData[itemData.tid];
                data.itemNum += itemData.itemNum;
                if (data.itemNum <= 0)
                {
                    mDicPetFragmentData.Remove(itemData.tid);
                }
            }
            else
            {
                if (itemData.itemNum < 0)
                    return false;

                PetFragmentData data = new PetFragmentData(itemData);
                mDicPetFragmentData.Add(itemData.tid, data);
            }
        }
        return false;
    }

    public int GetCountByTid(int iTid)
    {
        int count = 0;
        foreach (KeyValuePair<int, PetFragmentData> pair in mDicPetFragmentData)
        {
            if (pair.Key == iTid)
                return pair.Value.itemNum;
        }
        return count;
    }

    public int GetItemDataCount()
    {
        int count = 0;
        foreach (KeyValuePair<int, PetFragmentData> pair in mDicPetFragmentData)
        {
            if (pair.Value.itemNum > 0)
                count++;
        }
        return count;
    }

    public List<int> mCanComposeFragItemIDList = new List<int>();  //>可以合成的碎片的Tid列表
    //added by xuke 红点逻辑
    /// <summary>
    /// 检测是否有符灵碎片可以合成
    /// </summary>
    public bool CheckHasPetFragmentToCompose() 
    {
        mCanComposeFragItemIDList.Clear();
        int _maxCount = PetFragmentLogicData.Self.GetItemDataCount();
        List<PetFragmentData> itemDataList = PetFragmentLogicData.Self.mDicPetFragmentData.Values.ToList();
        PetFragmentData _fragData = null;
        for (int i = 0, count = itemDataList.Count; i < count; i++) 
        {
            _fragData = itemDataList[i];
            int costNum = TableCommon.GetNumberFromFragment(_fragData.tid, "COST_NUM");
            if (_fragData.itemNum >= costNum) 
            {
                mCanComposeFragItemIDList.Add(_fragData.itemId);
            }              
        }
        if (mCanComposeFragItemIDList.Count > 0)
            return true;
        return false;
    }
    //end
}