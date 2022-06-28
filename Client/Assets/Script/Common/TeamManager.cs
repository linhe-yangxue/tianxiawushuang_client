using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

//public enum EQUIP_TYPE
//{
//    ARM_EQUIP,
//    ORNAMENT_EQUIP,
//    DEFENCE_EQUIP,
//    DEFENCE_EQUIP2,
//    ELEMENT_EQUIP,
//    MAX,
//}

//public enum MAGIC_TYPE
//{
//    MAGIC1,
//    MAGIC2,
//    MAX,
//}

//public enum TEAM_POS
//{
//    POS_CHARACTER,
//    POS_PET_1,
//    POS_PET_2,
//    POS_PET_3,
//    POS_MAX
//}

public class TeamManager
{
    public static TEAM_POS mCurTeamPos = TEAM_POS.CHARACTER;
    public static int mAddPetPos = -1;          // 当前添加符灵位置
    public static Dictionary<TEAM_POS, TeamPosData> mDicTeamPosData = new Dictionary<TEAM_POS, TeamPosData>();

    public static DelegateList<TeamPosChangeData> OnChangeTeamPos = new DelegateList<TeamPosChangeData>();
    public static DelegateList<TeamPosChangeData> OnChangeBodyTeamPos = new DelegateList<TeamPosChangeData>();
    public static DelegateList<TeamPosChangeData> OnChangeEquipTeamPos = new DelegateList<TeamPosChangeData>();
    public static DelegateList<TeamPosChangeData> OnChangeMagicTeamPos = new DelegateList<TeamPosChangeData>();

    public static void InitTeamListData()
    {
        mDicTeamPosData.Clear();
        OnChangeTeamPos.Clear();
        OnChangeBodyTeamPos.Clear();
        OnChangeEquipTeamPos.Clear();
        OnChangeMagicTeamPos.Clear();

        PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
        MagicLogicData mgicLogicData = DataCenter.GetData("MAGIC_DATA") as MagicLogicData;
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++)
        {
            TeamPosData teamPosData = new TeamPosData(i);
            // role
            if (i == (int)TEAM_POS.CHARACTER)
            {
                teamPosData.bodyId = 0;
            }
            else
            {
                teamPosData.bodyId = petLogicData.GetPetItemIdByTeamPos(i);
            }

            // equip
            if (roleEquipLogicData.GetEquipIdListByTeamPos(teamPosData.mEquipIdList, i, (int)EQUIP_TYPE.MAX))
            {

            }

            // magic
            if (mgicLogicData.GetEquipIdListByTeamPos(teamPosData.mMagicIdList, i, (int)MAGIC_TYPE.MAX))
            {

            }

            mDicTeamPosData.Add((TEAM_POS)i, teamPosData);
        }

        for (int i = (int)TEAM_POS.RELATE_1; i < (int)TEAM_POS.RELATE_6 + 1; i++)
        {
            TeamPosData teamPosData = new TeamPosData(i);
            teamPosData.bodyId = petLogicData.GetPetItemIdByTeamPos(i);
            mDicTeamPosData.Add((TEAM_POS)i, teamPosData);
        }

        if(!CommonParam.isTeamManagerInit)
            CommonParam.isTeamManagerInit = true;
    }

    public static TeamPosData GetTeamPosData(int iTeamPos)
    {
        TeamPosData teamPosData;
        if (mDicTeamPosData.TryGetValue((TEAM_POS)iTeamPos, out teamPosData))
        {
            return teamPosData;
        }
        return null;
    }

	/// <summary>
	/// 判断是否有空闲的上阵位置
	/// </summary>
	/// <returns><c>true</c> if this instance has free team position; otherwise, <c>false</c>.</returns>
	public static bool HasFreeTeamPos()
	{
		for (int i = (int)TEAM_POS.PET_1; i <= (int)TEAM_POS.PET_3; i++) 
		{
			if(GetPetDataByTeamPos(i) == null)
				return true;
		}
		return false;
	}

	/// <summary>
	/// 得到当前已经上阵的符灵数量,不包括角色
	/// </summary>
	/// <returns>The on team pet count.</returns>
	public static int GetOnTeamPetCount()
	{
		int _onTeamPosCount = 0;
		for (int i = (int)TEAM_POS.PET_1; i < (int)TEAM_POS.MAX; i++) 
		{
			if(GetPetDataByTeamPos(i) != null)
				_onTeamPosCount++;
		}
		return _onTeamPosCount;
	}

    public static int GetPetItemIdByTeamPos(int iTeamPos)
    {
        TeamPosData teamPosData;
        if (mDicTeamPosData.TryGetValue((TEAM_POS)iTeamPos, out teamPosData))
        {
            return teamPosData.bodyId;
        }
        return 0;
    }

    public static int GetPetItemIdByCurTeamPos()
    {
        return GetPetItemIdByTeamPos((int)mCurTeamPos);
    }

    public static PetData GetPetDataByTeamPos(int iTeamPos)
    {
        int iItemId = GetPetItemIdByTeamPos(iTeamPos);
        if (iItemId > 0)
        {
            return PetLogicData.Self.GetPetDataByItemId(iItemId);
        }
        return null;
    }
#region 红点相关
    //----------------------------
    //符灵相关
    //---------------------------

    /// <summary>
    /// 判断当前位置是否有符灵
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public static bool CheckHasPetInThisPos(int kTeamPos)
    {
        if (kTeamPos == (int)TEAM_POS.CHARACTER)
            return true;
        return GetPetDataByTeamPos(kTeamPos) != null;
    }
    /// <summary>
    /// 判断该符灵是否与上阵符灵重复,包括缘分站位
    /// </summary>
    /// <param name="kTid"></param>
    /// <returns></returns>
    public static bool CheckPetRepeatInTeam(int kTid) 
    {
        PetData _petData = null;
        for (int i = (int)TEAM_POS.PET_1; i <= (int)TEAM_POS.PET_3; i++)
        {
            _petData = GetPetDataByTeamPos(i);
            if (_petData == null)
                continue;
            if (_petData.tid == kTid)
                return true;
        }
        for (int j = (int)TEAM_POS.RELATE_1; j <= (int)TEAM_POS.RELATE_6; j++) 
        {
            _petData = GetPetDataByTeamPos(j);
            if (_petData == null)
                continue;
            if (_petData.tid == kTid)
                return true;
        }
            return false;
    }

    private static int[] mExpPetTid = { 30289, 30293, 30299, 30305 };
    /// <summary>
    /// 判断是否经验蛋
    /// </summary>
    /// <returns></returns>
    public static bool CheckIsExpPet(int kTid) 
    {
        return mExpPetTid.ToList<int>().IndexOf(kTid) != -1;
    }

    /// <summary>
    /// 判断指定位置是否有可上阵的符灵,该位置必须为空,非空则返回false
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public static bool CheckHasPetCanSetUp(int kTeamPos) 
    {
        //1.如果是主角则不能
        if (TEAM_POS.CHARACTER == (TEAM_POS)kTeamPos) 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckOpenLevel = true;
            return false;        
        }
        //2.如果当前位置，当前等级未开放
        TeamPosData _teamPosData = TeamManager.GetTeamPosData(kTeamPos);
        if (_teamPosData == null)
            return false;
        if (RoleLogicData.GetMainRole() == null)
            return false;
        if (_teamPosData.openLevel > RoleLogicData.GetMainRole().level)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckOpenLevel = false;
            return false;
        }
        else 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckOpenLevel = true;        
        }
        //3.判断当前位置是否为空
        if (_teamPosData.bodyId > 0)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckEmptyPetSetUp = false;   
            return false;
        }
        //4.判断当前位置是否有不重复的符灵可以上阵,经验蛋不能上阵
        foreach (KeyValuePair<int, PetData> pair in PetLogicData.Self.mDicPetData)
        {
            if (CheckPetRepeatInTeam(pair.Value.tid))
                continue;
            else 
            {
                //5.如果是经验蛋则不能上阵
                if (CheckIsExpPet(pair.Value.tid))
                    continue;
                TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckEmptyPetSetUp = true;             
                return true;
            }
        }
        TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckEmptyPetSetUp = false;            
        return false;
    }

    /// <summary>
    /// 判断是否有更高品质的符灵可以上阵，当前位置必须有符灵
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public static bool CheckHasHighQualityPetCanSetUp(int kTeamPos)
    {
        //1.如果是主角的话则不能
        if (TEAM_POS.CHARACTER == (TEAM_POS)kTeamPos) 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckHighQuality = false;
            return false;        
        }
        //2.如果当前位置未开放
        TeamPosData _teamPosData = TeamManager.GetTeamPosData(kTeamPos);
        if (_teamPosData == null)
            return false;
        if (_teamPosData.openLevel > RoleLogicData.GetMainRole().level)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckOpenLevel = false;
            return false;        
        }
        else 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckOpenLevel = true;        
        }
        //3.如果当前位置为空
        if (_teamPosData.bodyId <= 0) 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckHighQuality = false;
            return false;        
        }
        PetData _petData = TeamManager.GetPetDataByTeamPos(kTeamPos);
        if (_petData == null) 
        {
            return false;
        }
        int _curPetQuality = GameCommon.GetItemQuality(_petData.tid);
        //4.判断当前位置是否有更高级的非重复符灵可以上阵
        foreach (KeyValuePair<int, PetData> pair in PetLogicData.Self.mDicPetData)
        {
            if (CheckPetRepeatInTeam(pair.Value.tid))
                continue;
            else if (GameCommon.GetItemQuality(pair.Value.tid) > _curPetQuality)
            {
                //5.如果是经验蛋则不能上阵
                if (CheckIsExpPet(pair.Value.tid))
                    continue;
                TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckHighQuality = true;
                return true;
            }
        }
        TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckHighQuality = false;
        return false;
    }

    /// <summary>
    /// 获得突破时需要得到的自身卡片数量
    /// </summary>
    public static int GetBreakNeedSelfCount(int kTid,int kBreakLevel) 
    {
        int iItemType = kTid / 1000;
        int breakSelfIcon = 0;

        switch ((ITEM_TYPE)iItemType)
        {
            case ITEM_TYPE.CHARACTER:
                breakSelfIcon = 0;
                return breakSelfIcon;
            case ITEM_TYPE.PET:
                breakSelfIcon = TableCommon.GetNumberFromBreakLevelConfig(kBreakLevel, "ACTIVE_NUM");
                return breakSelfIcon;
        }
        return breakSelfIcon;
    }
    ///// <summary>
    ///// 判断当前位置符灵是否可以突破
    ///// </summary>
    ///// <param name="kTeamPos"></param>
    ///// <returns></returns>
    public static bool CheckPetCanBreakUp(int kTeamPos)
    {
        if (!CheckHasPetInThisPos(kTeamPos)) 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckBreak = false;
            return false;        
        }
        //1.判断当前符灵目前是否可以突破

        ConsumeItemLogicData consumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        if (consumeItemLogicData == null)
            return false;
        ConsumeItemData curConsumeItemData = consumeItemLogicData.GetDataByTid((int)ITEM_TYPE.BREAK_STONE);

        ActiveData _roleData = GetActiveDataByTeamPos(kTeamPos);
        bool bIsCan = true;
        int breakStuffNeed = TableCommon.GetNumberFromBreakLevelConfig(_roleData.breakLevel, "NEED_GEM_NUM");
        int breakNeedCoin = TableCommon.GetNumberFromBreakLevelConfig(_roleData.breakLevel, "NEED_COIN_NUM");
        int breakSelfIconNum = GetBreakNeedSelfCount(_roleData.tid, _roleData.breakLevel);

        int breakNeedLevel = TableCommon.GetNumberFromBreakLevelConfig(_roleData.breakLevel, "ACTIVE_LEVEL");

        int iCount = PetLogicData.Self.GetBreakStuffPetCount(_roleData);

        if (null == curConsumeItemData)
        {
            bIsCan = false;
        }
        else if (breakStuffNeed > curConsumeItemData.itemNum)
        {
            bIsCan = false;
        }
        else if (breakNeedCoin > RoleLogicData.Self.gold)
        {
            bIsCan = false;
        }
        else if (breakSelfIconNum > iCount)
        {
            bIsCan = false;
        }
        else if (breakNeedLevel > _roleData.level)
        {
            bIsCan = false;
        }
        if (_roleData.breakLevel >= GameCommon.GetMaxBreakLevelByTid(_roleData.tid)) 
        {
            bIsCan = false;
        }
        TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckBreak = bIsCan;
        return bIsCan;
    
    }
    /// <summary>
    /// 判断当前位置符灵是否可以升级技能
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public static bool CheckPetHasCanSkillUp(int kTeamPos)
    {
        if (kTeamPos == (int)TEAM_POS.CHARACTER) 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckSkillUp = false;
            return false;      
        }
        //1.判断当前是否到达了可以升级技能的等级
        if (!GameCommon.IsFuncCanUse(728)) 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckSkillUp = false;
            return false;        
        }
        //2.遍历当前宠物技能，是否有可以升级的技能
        //2.1升级金币是否满足，是否达到升级技能的突破条件,技能是否满级,技能秘籍是否足够
        ActiveData _roleData = GetActiveDataByTeamPos(kTeamPos);
        if (_roleData == null) 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckSkillUp = false;
            return false;        
        }
        string _skillPrefix = "PET_SKILL_";
        for (int i = 1; i <= 4; i++)
        {
            int _skillIndex = TableCommon.GetNumberFromActiveCongfig(_roleData.tid, _skillPrefix + i);
            if (_skillIndex == 0)
                continue;
            int _skillLv = _roleData.skillLevel[i-1];

            int _costBookNum = TableCommon.GetNumberFromSkillCost(_skillLv, "SKILL_BOOK_COST");
            ConsumeItemLogicData itemData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
            if (itemData == null)
                return false;
            int _hasBookNum = itemData.GetDataByTid((int)ITEM_TYPE.SKILL_BOOK).itemNum;
            int _needCoin = TableCommon.GetNumberFromSkillCost(_skillLv, "MONEY_COST");
            int _skillOpenLevel = TableCommon.GetNumberFromActiveCongfig(_roleData.tid, "SKILL_ACTIVE_LEVEL_" + i);

            if (_hasBookNum < _costBookNum || RoleLogicData.Self.gold < _needCoin || _roleData.breakLevel < _skillOpenLevel || _roleData.level <= _skillLv || RoleLogicData.Self.character.level <= _skillLv || _skillLv >= NewSKillUpgradeBean.SKILL_LEVEL_LIMIT)
            {
                continue;
            }
            else 
            {
                TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckSkillUp = true;
                return true;
            }
        }
        TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckSkillUp = false;
        return false;
    }

    /// <summary>
    /// 判断当前位置符灵是否可以升级
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public static bool CheckPetCanLevelUp(int kTeamPos)
    {
        if (!TeamNewMarkManager.mNeedCheckLevelUp)
            return false;
        List<PetData> _petDataList = GetExpPetList(kTeamPos);
        if (_petDataList == null || kTeamPos == (int)TEAM_POS.CHARACTER)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckLevelUp = false;
            return false;
        }
        PetData _petData = GetPetDataByTeamPos(kTeamPos);
        if (_petData == null)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckLevelUp = false;
            return false;
        }
        //1.判断符灵当前等级限制
        if (_petData.level >= _petData.mMaxLevelNum)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckLevelUp = false;
            return false;
        }
        if (_petData.level >= RoleLogicData.GetMainRole().level)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckLevelUp = false;
            return false;
        }
        //2判断升级所需经验和银币总量是否足够
        int _nNeedExp = _petData.GetMaxExp() - _petData.exp;
        if (_nNeedExp > RoleLogicData.Self.gold)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckLevelUp = false;
            return false;
        }
        int _nTotalExp = 0;
        bool _bTotalEnough = false;
        for (int i = 0, count = _petDataList.Count; i < count; i++)
        {
            _nTotalExp += GameCommon.GetPetExp(_petDataList[i]);
            if (_nTotalExp >= _nNeedExp)
            {
                _bTotalEnough = true;
                break;
            }
        }
        if (!_bTotalEnough)
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckLevelUp = false;
            return false;
        }
        //2.1判断是否有可以升级的组合
        if (HasEnoughLevelUpRes(_nNeedExp, _petDataList)) 
        {
            TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckLevelUp = true;
            return true;
        }
        TeamNewMarkManager.Self.GetPetNewMarkData(kTeamPos).CheckLevelUp = false;
        return false;
    }
    /// <summary>
    /// 判断是否有足够的升级资源
    /// </summary>
    /// <returns></returns>
    private static bool HasEnoughLevelUpRes(int kNeedExp,List<PetData> kPetDataList) 
    {
        if (kPetDataList == null)
            return false;
        for (int i = 0, count = kPetDataList.Count; i < count; i++) 
        {
            int _totalExp = 0;
            int _tmpTotalExp = 0;
            for (int j = i; j < count; j++) 
            {
                int _getExp = GameCommon.GetPetExp(kPetDataList[j]);
                _tmpTotalExp = _getExp + _totalExp;
                if (_tmpTotalExp < kNeedExp)
                {
                    _totalExp += _getExp;
                }
                else 
                {
                    if (RoleLogicData.Self.gold >= _tmpTotalExp) 
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 获得可供升级的符灵列表,按照能够提供的经验排好序
    /// </summary>
    /// <returns></returns>
    public static List<PetData> GetExpPetList(int kTeamPos) 
    {
        PetData _petData = GetPetDataByTeamPos(kTeamPos);
        if (_petData == null)
            return null;
        List<PetData> itemDataList = PetLogicData.Self.mDicPetData.Values.ToList();
        itemDataList.RemoveAll(x => x.teamPos >= 0 || x.itemId == _petData.itemId || (x.starLevel >= 3 && x.tid != 30293 && x.tid != 30299 && x.tid != 30305));
        itemDataList = GameCommon.SortList<PetData>(itemDataList, GameCommon.SortPetDataByStarLevel);
        GameCommon.SortList<PetData>(itemDataList,GameCommon.SortPetDataByExp);
        return itemDataList;
    }

    public static string[] RelateIDTagArr = 
    {
        "RELATE_ID_1", "RELATE_ID_2", "RELATE_ID_3", 
        "RELATE_ID_4", "RELATE_ID_5", "RELATE_ID_6",
        "RELATE_ID_7", "RELATE_ID_8", "RELATE_ID_9",
        "RELATE_ID_10","RELATE_ID_11","RELATE_ID_12",
        "RELATE_ID_13","RELATE_ID_14","RELATE_ID_15"
    };
    public static string[] NeedContentTagArr = 
    {
        "NEED_CONTENT_1",
        "NEED_CONTENT_2",
        "NEED_CONTENT_3",
        "NEED_CONTENT_4",
        "NEED_CONTENT_5",
        "NEED_CONTENT_6"
    };
    /// <summary>
    /// 判断宠物是否能够激活缘分
    /// </summary>
    /// <param name="kPetTid"></param>
    /// <returns></returns>
    private static bool CheckThePetCanActiveRelate(int kPetTid) 
    {
        //遍历阵上所有的符灵，判断当前符灵能否激活主界面阵上4个符灵的缘分
        for (int i = (int)TEAM_POS.CHARACTER; i <= (int)TEAM_POS.PET_3; i++)
        {
            ActiveData _activeData = GetActiveDataByTeamPos(i);
            if (_activeData == null)
                continue;
            int _petTid = _activeData.tid;
            if (_petTid == 0)
                continue;
            DataTable.DataRecord _petRecord = DataCenter.mActiveConfigTable.GetRecord(_petTid);
            if (_petRecord == null)
                return false;
            //遍历该符灵的所有缘分
            int _relIndex = 0;
            List<int> _contentTidList = new List<int>();
            for (int relSuffix = 1; relSuffix <= 15; relSuffix++)
            {
                _contentTidList.Clear();
                _relIndex = _petRecord.getData(RelateIDTagArr[relSuffix - 1]);
                //得到激活相关缘分所需要的内容
                DataTable.DataRecord _relRecord = DataCenter.mRelateConfig.GetRecord(_relIndex);
                if (_relRecord == null)
                    continue;
                int _needContentIndex = 0;
                for (int needContentSuffix = 1; needContentSuffix <= 6; needContentSuffix++)
                {
                    _needContentIndex = _relRecord.getData(NeedContentTagArr[needContentSuffix - 1]);
                    if (_needContentIndex != 0)
                        _contentTidList.Add(_needContentIndex);
                }
                //判断当前符灵是否在缘分相关ID中
                int _indexOfPet = _contentTidList.IndexOf(kPetTid);
                if (_indexOfPet >= 0)
                {
                    if (_contentTidList.Count == 1)
                        return true;
                    else
                    {
                        int _contentRelCount = _contentTidList.Count;
                        int _relNeedCount = 0;
                        for (int _contentRelIndex = 0; _contentRelIndex < _contentRelCount; _contentRelIndex++)
                        {
                            if (_contentTidList[_contentRelIndex] == kPetTid)
                                _relNeedCount++;
                            else
                            {
                                if (ITEM_TYPE.PET == PackageManager.GetItemTypeByTableID(_contentTidList[_contentRelIndex]) && CheckActiveDataInTeamByTid(_contentTidList[_contentRelIndex]))
                                    _relNeedCount++;
                            }
                        }
                        if (_relNeedCount == _contentRelCount)
                            return true;
                    }
                }
                else
                {
                    continue;
                }
            }
        }
        return false;
    }
    private static List<int> _repeatPetTidList = new List<int>();
    /// <summary>
    /// 判断指定位置是否需要进行缘分提示
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    private static bool CheckHasCanActiveRelPet(int kTeamPos)
    {
        TeamPosData _teamPosData = TeamManager.GetTeamPosData(kTeamPos);
        if (_teamPosData.bodyId > 0 && CheckThePetCanActiveRelate(_teamPosData.bodyId))
            return false;
        _repeatPetTidList.Clear();
        
        //1.遍历符灵背包中未上阵的符灵，判断该符灵能否与所有在阵上的符灵激活缘分
        foreach (KeyValuePair<int, PetData> pair in PetLogicData.Self.mDicPetData)
        {
            if (_repeatPetTidList.IndexOf(pair.Value.tid) >= 0)
                continue;
            _repeatPetTidList.Add(pair.Value.tid);
            //1.判断是否上阵
            if (pair.Value.IsInTeam())
                continue;
            if (CheckThePetCanActiveRelate(pair.Value.tid))
                return true;
        }
        return false;
    }

    public static void ResetData() 
    {
        mIsRelateDirty = false;
    }
    public static bool mIsRelateDirty = true;        //> 判断是否需要重新检测缘分
    public static TEAM_POS PET_RELATE_ACTIVE_INDEX;  //> 可激活缘分的缘分站位
    /// <summary>
    /// 检测缘分提示
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public static bool CheckPetRelate() 
    {
        PET_RELATE_ACTIVE_INDEX = TEAM_POS.MAX; //> 默认值
        //首先判断缘分符灵是否有开启位置
        TeamPosData _teamPosData = TeamManager.GetTeamPosData((int)TEAM_POS.RELATE_1);
        if (_teamPosData == null)
            return false;
        PetData _petData = TeamManager.GetPetDataByTeamPos((int)TEAM_POS.RELATE_1);
        RoleData _roleData = RoleLogicData.GetMainRole();
        if (_roleData == null)
            return false;
        if (_teamPosData.openLevel > _roleData.level) 
        {
            TeamNewMarkManager.Self.mTab1NewMarkData.CheckRelatePos = false;
            return false;       
        }
        if (!mIsRelateDirty)
            return TeamNewMarkManager.Self.mTab1NewMarkData.CheckRelatePos;
        //1.得到当前空闲位置，包括不能激活缘分的符灵,只针对缘分站位中的符灵进行提示，不包括队伍主界面的符灵
        for (int i = (int)TEAM_POS.RELATE_1; i <= (int)TEAM_POS.RELATE_6; i++) 
        {
            _teamPosData = TeamManager.GetTeamPosData(i);
            if (_teamPosData.openLevel > _roleData.level)
                continue;
            //判断当前位置符灵是否能够激活缘分,如果当前位置已经能够激活缘分则不检测新的缘分
            if (_petData != null && _petData.tid != 0)
            {
                if (CheckThePetCanActiveRelate(_petData.tid))
                {
                    continue;
                }
            }
            if (CheckHasCanActiveRelPet(i)) 
            {
                PET_RELATE_ACTIVE_INDEX = (TEAM_POS)i;
                TeamNewMarkManager.Self.mTab1NewMarkData.CheckRelatePos = true;
                return true;            
            }
        }
        PET_RELATE_ACTIVE_INDEX = TEAM_POS.MAX;
        TeamNewMarkManager.Self.mTab1NewMarkData.CheckRelatePos = false; 
        return false;
    }

    //---------------------------
    // 队伍界面第二个页签-符灵背包
    /// <summary>
    /// 检测符灵背包是否已经满了
    /// </summary>
    /// <returns></returns>
    public static bool CheckPetBag() 
    {
        if (PetLogicData.Self == null)
            return false;
        if (PetLogicData.Self.mDicPetData.Count >= (int)VIPHelper.GetCurrVIPValueByField(VIP_CONFIG_FIELD.BAG_MAX_PET)) 
        {
            TeamNewMarkManager.Self.mTab2NewMarkData.CheckPetBagFull = true;
            return true;
        }
        TeamNewMarkManager.Self.mTab2NewMarkData.CheckPetBagFull = false;
        return false;
    }
    //---------------------------


    //----------------------------
    //装备 / 法器 相关
    //---------------------------
    public static EquipData GetEquipData(int kTeamPos,int kEquipType,ITEM_TYPE kRealType) 
    {
        if (ITEM_TYPE.EQUIP == kRealType) 
        {
            if (RoleEquipLogicData.Self == null)
                return null;
            return RoleEquipLogicData.Self.GetEquipDataByTeamPosAndType(kTeamPos, kEquipType);
        }
        else if (ITEM_TYPE.MAGIC == kRealType) 
        {
            if (MagicLogicData.Self == null)
                return null;
            return MagicLogicData.Self.GetEquipDataByTeamPosAndType(kTeamPos,kEquipType);
        }
        DEBUG.LogError("无法获得装备数据，只能传入装备类型或者法器类型");
        return null;
    }
    private static Dictionary<int, EquipData> __GetEquipDicData(ITEM_TYPE kRealType) 
    {
        if (ITEM_TYPE.EQUIP == kRealType)
        {
            if (RoleEquipLogicData.Self == null)
                return null;
            return RoleEquipLogicData.Self.mDicEquip;        
        }
        else if (ITEM_TYPE.MAGIC == kRealType)
        {
            if (MagicLogicData.Self == null)
                return null;
            return MagicLogicData.Self.mDicEquip; 
        }
        DEBUG.LogError("无法获得装备数据，只能传入装备类型或者法器类型");
        return null;
    }
    private static int __GetEquipTypeOpenLevelByTid(int kEquipTid) 
    {
        ITEM_TYPE _itemType = PackageManager.GetItemTypeByTableID(kEquipTid);
        if (ITEM_TYPE.MAGIC == _itemType) 
        {
            DataTable.DataRecord _record = DataCenter.mPetPostionLevel.GetRecord(101 + PackageManager.GetSlotPosByTid(kEquipTid));
            if (_record != null)
                return _record.getData("OPEN_LEVEL");
        }
        else if (ITEM_TYPE.EQUIP == _itemType) 
        {
            DEBUG.Log("装备没有等级限制");
            return 0;
        }
        return 0;
    }
    private static int __GetEquipTypeOpenLevelByPosAndType(int kEquipType,ITEM_TYPE kItemType)
    {
        if (ITEM_TYPE.MAGIC == kItemType)
        {
            DataTable.DataRecord _record = DataCenter.mPetPostionLevel.GetRecord(101 + kEquipType);
            if (_record != null)
                return _record.getData("OPEN_LEVEL");
        }
        else if (ITEM_TYPE.EQUIP == kItemType)
        {
            DEBUG.Log("装备没有等级限制");
            return 0;
        }
        return 0;
    }
    /// <summary>
    /// 判断指定符灵指定位置是否可以穿戴装备，该位置当前必须没有装备
    /// </summary>
    /// <returns></returns>
    public static bool CheckHasEquipSetUp(int kTeamPos, int kEquipType, ITEM_TYPE kRealType = ITEM_TYPE.EQUIP) 
    {
        //1.0判断当前位置是否有符灵
        if (!CheckHasPetInThisPos(kTeamPos)) 
        {
            TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckEmptyEquipSetUp = false;
            return false;
        }
        EquipData _equipData = GetEquipData(kTeamPos,kEquipType,kRealType);// RoleEquipLogicData.Self.GetEquipDataByTeamPosAndType(kTeamPos, kEquipType);
        //1.1当前位置是否为空
        if (_equipData != null) 
        {
            TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckEmptyEquipSetUp = false;
            return false;
        }          
        //2.是否有可穿戴的装备
        if (ITEM_TYPE.EQUIP == kRealType)
        {
            if (RoleEquipLogicData.Self == null)
                return false;
            TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckOpenLevel = true;
            if (RoleEquipLogicData.Self.GetEquipDataNumByType(kEquipType) - TeamManager.GetEqupNumInTeamByType(kEquipType) > 0)
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckEmptyEquipSetUp = true;
                return true;
            }
            else 
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckEmptyEquipSetUp = false;
                return false;
            }
        }
        else
        {
            if (MagicLogicData.Self == null)
                return false;
            //判断是否达到等级开放条件
            RoleData _roleData = RoleLogicData.GetMainRole();
            if (_roleData == null)
                return false;
            if (__GetEquipTypeOpenLevelByPosAndType(kEquipType, kRealType) < _roleData.level)
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckOpenLevel = true;
            }
            else 
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckOpenLevel = false;
                return false;
            }
            if (MagicLogicData.Self.GetEquipDataNumByType(kEquipType) - TeamManager.GetMagicNumInTeamByType(kEquipType) > 0)
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckEmptyEquipSetUp = true;
                return true;                    
            }
            else 
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckEmptyEquipSetUp = false;
                return false;
            }
        }
    }

    /// <summary>
    /// 判断是否有更高级的装备可以装备，该位置当前必须已经穿戴了装备
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <param name="?"></param>
    /// <returns></returns>
    public static bool CheckHasHighQualityEquipSetUp(int kTeamPos, int kEquipType, ITEM_TYPE kRealType = ITEM_TYPE.EQUIP) 
    {
        //1.判断当前位置是否有装备
        EquipData _equipData = GetEquipData(kTeamPos, kEquipType, kRealType);// RoleEquipLogicData.Self.GetEquipDataByTeamPosAndType(kTeamPos, kEquipType);
        if (_equipData == null) 
        {
            TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckHighQuality = false;
            return false;        
        }
        int _equipQuality = GameCommon.GetItemQuality(_equipData.tid);
        //2.判断当前位置是否有更高级的同类型未穿戴装备可以穿戴
        Dictionary<int, EquipData> _equipDic = __GetEquipDicData(kRealType);
        if (_equipDic == null)
            return false;
        foreach (KeyValuePair<int, EquipData> pair in _equipDic)
        {
            if (pair.Value.IsInTeam())
                continue;
            if (PackageManager.GetSlotPosByTid(_equipData.tid) != PackageManager.GetSlotPosByTid(pair.Value.tid))
                continue;
            //3.如果是经验法器则不能更换
            if (kRealType == ITEM_TYPE.MAGIC && PackageManager.IsExpMagic(_equipData.tid))
                continue;
            if (GameCommon.GetItemQuality(pair.Value.tid) > _equipQuality) 
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckHighQuality = true;
                return true;
            }
              
        }
        TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckHighQuality = false;
        return false;
    }

    /// <summary>
    /// 检测角色或符灵是否与该装备有缘分
    /// </summary>
    /// <param name="kActiveObjectTid"></param>
    /// <param name="kEquipTid"></param>
    /// <returns></returns>
    public static bool CheckHasRelate(int lhsTid,int rhsTid) 
    {
        string _relateNamePrefix = "RELATE_ID_";
        string _relateNeedContentPrefix = "NEED_CONTENT_";
        for (int i = 1; i <= 15; i++) 
        {
            int _relateID = TableCommon.GetNumberFromActiveCongfig(lhsTid, _relateNamePrefix + i);
            DataTable.DataRecord _relateRecord = DataCenter.mRelateConfig.GetRecord(_relateID);
            if(_relateRecord == null)
                continue;
            for (int j = 1; j <= 6; j++) 
            {
                int _needContentID = _relateRecord.getData(_relateNeedContentPrefix + j);
                if (_needContentID == rhsTid)
                    return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 检测是否有缘分的装备可以穿戴
    /// 说明：只负责检测同品质装备
    /// 如果当前装备没有缘分则判断是否有缘分的装备需要推荐,如果当前装备有缘分则不推荐
    /// </summary>
    /// <returns></returns>
    public static bool CheckHasRelateEquipSetUp(int kTeamPos, int kEquipType, ITEM_TYPE kRealType = ITEM_TYPE.EQUIP) 
    {
        ActiveData _activeData = TeamManager.GetActiveDataByTeamPos(kTeamPos);
        if (_activeData == null) 
        {
            TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckRelate = false;
            return false;        
        }
        //1.判断当前位置是否有装备
        EquipData _equipData = GetEquipData(kTeamPos, kEquipType, kRealType);// RoleEquipLogicData.Self.GetEquipDataByTeamPosAndType(kTeamPos, kEquipType);
        if (_equipData == null) 
        {
            TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckRelate = false;
            return false;        
        }
        int _equipQuality = GameCommon.GetItemQuality(_equipData.tid);
        //2.判断当前装备是否有缘分
        if (CheckHasRelate(_activeData.tid, _equipData.tid))
        {
            //1.判断是否有更高品质的装备
            if (CheckHasHighQualityEquipSetUp(kTeamPos, kEquipType)) 
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckRelate = true;
                return true;
            }
        }
        else 
        {
            Dictionary<int, EquipData> _equipDic = __GetEquipDicData(kRealType);
            if (_equipDic == null)
                return false;
            //判断是否有未穿戴的带缘分的装备
            foreach (KeyValuePair<int, EquipData> pair in _equipDic) 
            {
                //1.判断是否在阵上
                if (pair.Value.IsInTeam())
                    continue;
                //2.判断类型
                if (PackageManager.GetSlotPosByTid(_equipData.tid) != PackageManager.GetSlotPosByTid(pair.Value.tid))
                    continue;
                //3.判断品质
                if (_equipQuality > GameCommon.GetItemQuality(pair.Value.tid))
                    continue;
                //4.判断该装备是否有缘分效果
                if (CheckHasRelate(_activeData.tid, pair.Value.tid)) 
                {
                    TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckRelate = true;
                    return true;
                }
                   
            }
        }
        TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckRelate = false;
        return false;
    }

    /// <summary>
    /// 判断指定装备能否强化
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <param name="kEquipType"></param>
    /// <param name="kRealType"></param>
    /// <returns></returns>
    public static bool CheckEquipCanStrengthen(int kTeamPos,int kEquipType,ITEM_TYPE kRealType) 
    {
        //tmp
        if (ITEM_TYPE.MAGIC == kRealType)
            return false;
        //--
        EquipData _equipData = GetEquipData(kTeamPos,kEquipType,kRealType);
        if (_equipData == null)
            return false;
        bool bIsCan = true;

        int iNeedSyntheticCoin = 0;
        string costQualityType = "COST_MONEY_" + (int)_equipData.mQualityType;
        int costMoney = TableCommon.GetNumberFromEquipStrengthCostConfig(_equipData.strengthenLevel, "COST_MONEY");
        //计算品质额外花费
        int costMoneyQualityType = TableCommon.GetNumberFromEquipStrengthCostConfig(_equipData.strengthenLevel, costQualityType);
        iNeedSyntheticCoin = costMoney + costMoneyQualityType;
        if (_equipData.strengthenLevel >= RoleLogicData.Self.character.level * 2)
        {
            bIsCan = false;
        }
        else if (iNeedSyntheticCoin > RoleLogicData.Self.gold)
        {
            bIsCan = false;
        }
        return bIsCan;
    }

    /// <summary>
    /// 检测一个装备的信息
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <param name="kEquipType"></param>
    /// <param name="kRealType"></param>
    /// <returns></returns>
    public static bool CheckOneEquipInfo_NewMark(int kTeamPos,int kEquipType,ITEM_TYPE kRealType = ITEM_TYPE.EQUIP) 
    {
        if (ITEM_TYPE.MAGIC == kRealType) 
        {
            TeamPosData _magicTeamPosData = GetTeamPosData(kTeamPos);
            if (_magicTeamPosData.openLevel > RoleLogicData.GetMainRole().level) 
            {
               // DataCenter.SetData("TEAM_WINDOW", "UPDATE_EQUIP_NEWMARK", new TeamInfoWindow.EquipVisibleData() { mTeamPos = kTeamPos, mTid = _equipData.tid, mEquipType = PackageManager.GetSlotPosByTid(_equipData.tid) });
                return false;            
            }
        }

        if (CheckHasEquipSetUp(kTeamPos, kEquipType, kRealType))
            return true;
        if (CheckHasHighQualityEquipSetUp(kTeamPos, kEquipType, kRealType))
            return true;
        if (CheckHasRelateEquipSetUp(kTeamPos, kEquipType, kRealType))
            return true;
        //if (CheckEquipCanStrengthen(kTeamPos, kEquipType, kRealType))
        //    return true;

        return false;
    }

    public static void CheckOneEquipInfoAll_NewMark(int kTeamPos, int kEquipType, ITEM_TYPE kRealType = ITEM_TYPE.EQUIP)
    {
        TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckOpenLevel = true;
        if (ITEM_TYPE.MAGIC == kRealType)
        {
            TeamPosData _magicTeamPosData = GetTeamPosData(kTeamPos);
            if (_magicTeamPosData.openLevel > RoleLogicData.GetMainRole().level)
            {
                TeamNewMarkManager.Self.GetEquipNewMarkData(kTeamPos, kEquipType, kRealType).CheckOpenLevel = false;
            }
        }
        CheckHasEquipSetUp(kTeamPos, kEquipType, kRealType);
        CheckHasHighQualityEquipSetUp(kTeamPos, kEquipType, kRealType);
        CheckHasRelateEquipSetUp(kTeamPos, kEquipType, kRealType);
    }

    /// <summary>
    /// 检测队伍按钮是否需要显示红点
    /// </summary>
    /// <returns></returns>
    public static bool CheckTeamHasNewMark() 
    {
        if (CheckTeamInfoTab_NewMark())
            return true;
        if (CheckTeamPetBagTab_NewMark())
            return true;
        if (CheckTeamPetFragTab_NewMark())
            return true;

        return false;
    }

    /// <summary>
    /// 检测队伍信息界面红点状态
    /// </summary>
    /// <returns></returns>
    public static bool CheckTeamInfoTab_NewMark() 
    {
        for (int i = (int)TEAM_POS.CHARACTER; i <= (int)TEAM_POS.PET_3; i++)
        {
            if (CheckOnePetInfo_NewMark(i))
                return true;
        }
        if (CheckPetRelate()) 
        {
            return true;        
        }
        return false;
    }

    public static bool CheckOnePetInfo_NewMark(int i) 
    {
        //bool bState = false;
        //首先判断当前位置是否可以上阵
        TeamPosData _teamPosData = GetTeamPosData(i);
        if (_teamPosData == null)
            return false;
        RoleData _roleData = RoleLogicData.GetMainRole();
        if (_roleData == null)
            return false;
        if (_teamPosData.openLevel > _roleData.level)
            return false;

        //1.是否有空的符灵可以上阵
        //bState = CheckHasPetCanSetUp(i) || bState;
        if (CheckHasPetCanSetUp(i))
            return true;
        //2.判断是否有更高等级的符灵可以上阵
        if (CheckHasHighQualityPetCanSetUp(i))
            return true;
        // 如果当前位置没有符灵则以下不用进行判断
        if (_teamPosData.bodyId <= 0 && i != 0) 
            return false;
        //3.判断符灵是否可以突破
        if (CheckPetCanBreakUp(i))
            return true;
        //4.判断当前符灵是否有可以升级的技能
        if (CheckPetHasCanSkillUp(i))
            return true;
        //5.装备相关
        for (int j = (int)EQUIP_TYPE.ARM_EQUIP; j < (int)EQUIP_TYPE.MAX; j++)
        {
            if (CheckOneEquipInfo_NewMark(i, j, ITEM_TYPE.EQUIP))
                return true;
        }
        //6.法器相关
        for (int k = (int)MAGIC_TYPE.MAGIC1; k < (int)MAGIC_TYPE.MAX; k++)
        {
            if (CheckOneEquipInfo_NewMark(i, k, ITEM_TYPE.MAGIC))
                return true;
        }
        //7.符灵升级
        if (CheckPetCanLevelUp(i))
        {
            return true;
        }
        return false;
    }

    public static void CheckOnePetInfoAll_NewMark(int kTeamPos) 
    {
        //首先判断当前位置是否可以上阵
        TeamPosData _teamPosData = GetTeamPosData(kTeamPos);
        if (_teamPosData == null)
            return ;
        RoleData _roleData = RoleLogicData.GetMainRole();
        if (_roleData == null)
            return ;
        //1.是否有空的符灵可以上阵
        CheckHasPetCanSetUp(kTeamPos);
        //2.判断是否有更高等级的符灵可以上阵
        CheckHasHighQualityPetCanSetUp(kTeamPos);
        //3.判断符灵是否可以突破
        CheckPetCanBreakUp(kTeamPos);
        //4.判断当前符灵是否有可以升级的技能
        CheckPetHasCanSkillUp(kTeamPos) ;
        //5.装备相关
        for (int j = (int)EQUIP_TYPE.ARM_EQUIP; j < (int)EQUIP_TYPE.MAX; j++)
        {
            CheckOneEquipInfoAll_NewMark(kTeamPos, j, ITEM_TYPE.EQUIP);
        }
        //6.法器相关
        for (int k = (int)MAGIC_TYPE.MAGIC1; k < (int)MAGIC_TYPE.MAX; k++)
        {
            CheckOneEquipInfoAll_NewMark(kTeamPos, k, ITEM_TYPE.MAGIC);
        }
        //7.符灵升级
        CheckPetCanLevelUp(kTeamPos);
    }

    /// <summary>
    /// 检测队伍符灵背包红点状态
    /// </summary>
    /// <returns></returns>
    public static bool CheckTeamPetBagTab_NewMark() 
    {
        return TeamManager.CheckPetBag();
    }
    /// <summary>
    /// 检测符灵碎片背包是否有可以合成的碎片
    /// </summary>
    /// <returns></returns>
    public static bool CheckTeamPetFragTab_NewMark() 
    {
        if (PetFragmentLogicData.Self == null)
            return false;
        if (PetFragmentLogicData.Self.CheckHasPetFragmentToCompose()) 
        {
            TeamNewMarkManager.Self.mTab3NewMarkData.CheckCanCompose = true;
            return true;
        }
        TeamNewMarkManager.Self.mTab3NewMarkData.CheckCanCompose = false;
        return false;
    }
    /// <summary>
    /// 设置返回主界面按钮红点状态
    /// </summary>
    public static void SetBackToHomePage_NewMark() 
    {
        // 判断是否有邮件
        if (SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_MAIL))
        {
            ShowBackToHomePage_NewMark();
            return;   
        }
        // 判断队伍是否有提示
        if (CheckTeamHasNewMark()) 
        {
            ShowBackToHomePage_NewMark();
            return;            
        }
        GlobalModule.DoCoroutine(TeamManager.DoAction_RequestNewMark());
    }

    private static IEnumerator DoAction_RequestNewMark()
    {
        #region 请求任务状态信息
        //1.请求任务状态信息
        //1.1日常任务
        yield return new GetDailyTaskDataRequester().Start();
        if (TaskState.hasDailyAwardAcceptable) 
        {
            ShowBackToHomePage_NewMark();
            yield break;
        }
        //1.2成就任务
        yield return new GetAchievementDataRequester().Start();
        if (TaskState.hasAchieveAwardAcceptable) 
        {
            ShowBackToHomePage_NewMark();
            yield break;
        }
        #endregion
    }

    private static void ShowBackToHomePage_NewMark() 
    {
        //战斗胜利界面
        tWindow _tWin = DataCenter.GetData("PVE_ACCOUNT_WIN_WINDOW") as tWindow;
        if (_tWin != null && _tWin.mGameObjUI != null && _tWin.IsOpen()) 
        {
            DataCenter.SetData("PVE_ACCOUNT_WIN_WINDOW", "SHOW_NEWMARK", null);
        }
        //战斗失败界面
        tWindow _tLose = DataCenter.GetData("PVE_ACCOUNT_LOSE_WINDOW") as tWindow;
        if (_tLose != null && _tLose.mGameObjUI != null && _tLose.IsOpen())
        {
            DataCenter.SetData("PVE_ACCOUNT_LOSE_WINDOW", "SHOW_NEWMARK", null);
        }
    }
#endregion
    /// <summary>
    /// 判断指定tid的符灵是否在队伍中
    /// </summary>
    /// <param name="kTid"></param>
    /// <returns></returns>
    public static bool CheckPetInTeamByTid(int kTid) 
    {
        for (int i = (int)TEAM_POS.PET_1; i <= (int)TEAM_POS.PET_3; i++) 
        {
            if (GetBodyTidByTeamPos(i) == kTid)
                return true;
        }
        return false;
    }
    //added by xuke
    /// <summary>
    /// 判断指定tid的角色是否在队伍中，包括符灵和角色
    /// </summary>
    /// <param name="kTid"></param>
    /// <returns></returns>
    public static bool CheckActiveDataInTeamByTid(int kTid)
    {
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++) 
        {
            if (GetBodyTidByTeamPos(i) == kTid)
                return true;
        }
        return false;
    }


    /// <summary>
    /// 判断指定tid的符灵是否在队伍中，包括缘分站位
    /// </summary>
    /// <param name="kTid"></param>
    /// <returns></returns>
    public static bool CheckPetInAllTeamByTid(int kTid)
    {
        if (CheckPetInTeamByTid(kTid))
            return true;
        for (int i = (int)TEAM_POS.RELATE_1; i <= (int)TEAM_POS.RELATE_6; i++) 
        {
            if (GetBodyTidByTeamPos(i) == kTid)
                return true;
        }
        return false;
    }
    //end
    public static PetData GetPetDataByCurTeamPos()
    {
        return GetPetDataByTeamPos((int)mCurTeamPos);
    }

    public static ActiveData GetActiveDataByTeamPos(int iTeamPos)
    {
        if(iTeamPos == (int)TEAM_POS.CHARACTER)
        {
            return RoleLogicData.Self.character;
        }

        return GetPetDataByTeamPos(iTeamPos);
    }

    public static ActiveData GetActiveDataByCurTeamPos()
    {
        return GetActiveDataByTeamPos((int)mCurTeamPos);
    }

    public static int GetBodyTidByTeamPos(int iTeamPos)
    {
        if (TEAM_POS.CHARACTER == (TEAM_POS)iTeamPos)
        {
            return RoleLogicData.Self.character.tid;
        }

        PetData petData = GetPetDataByTeamPos(iTeamPos);
        if (petData != null)
        {
            return petData.tid;
        }
        return 0;
    }

    public static int GetBodyTidByCurTeamPos()
    {
        return GetBodyTidByTeamPos((int)mCurTeamPos);
    }

    public static ActiveData GetBodyDataByTeamPos(int iTeamPos)
    {
        if (TEAM_POS.CHARACTER == (TEAM_POS)iTeamPos)
        {
            return RoleLogicData.Self.character;
        }

        return GetPetDataByTeamPos(iTeamPos);
    }

    public static ActiveData GetBodyDataByCurTeamPos()
    {
        return GetBodyDataByTeamPos((int)mCurTeamPos);
    }

    public static EquipData GetRoleEquipDataByTeamPos(int iTeamPos, int iType)
    {
        TeamPosData teamPosData;
        if (mDicTeamPosData.TryGetValue((TEAM_POS)iTeamPos, out teamPosData))
        {
            if (iType >= teamPosData.mEquipIdList.Count)
                return null;
            int itemId = teamPosData.mEquipIdList[iType];
            RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
            return logicData.GetEquipDataByItemId(itemId);
        }
        return null;
    }

    public static EquipData GetRoleEquipDataByCurTeamPos(int type)
    {
        return GetRoleEquipDataByTeamPos((int)mCurTeamPos, type);
    }

    public static EquipData GetMagicDataByTeamPos(int iTeamPos, int type)
    {
        TeamPosData teamPosData;
        if (mDicTeamPosData.TryGetValue((TEAM_POS)iTeamPos, out teamPosData))
        {
            if (type >= teamPosData.mMagicIdList.Count)
                return null;
            int itemId = teamPosData.mMagicIdList[type];
            MagicLogicData logicData = DataCenter.GetData("MAGIC_DATA") as MagicLogicData;
            return logicData.GetEquipDataByItemId(itemId);
        }
        return null;
    }
    //added by xuke
    public static int GetMagicEquipOpenLevelByType(int type) 
    {
        return TableCommon.GetNumberFromPetPostionLevel(101 + type, "OPEN_LEVEL");
    }

    //end
    public static EquipData GetMagicDataByCurTeamPos(int type)
    {
        return GetMagicDataByTeamPos((int)mCurTeamPos, type);
    }


    /// <summary>
    /// 判断指定位置的符灵是否穿戴齐了四件装备
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public static bool IsEquipFull(TEAM_POS kTeamPos) 
    {
        EquipData _equipData;
        for (int i = 0; i <= 3; i++) 
        {
            _equipData = GetRoleEquipDataByTeamPos((int)kTeamPos, i);
            if (_equipData == null)
                return false;    
        }
        return true;
    }

    /// <summary>
    /// 判断指定位置的符灵是否穿戴齐了法器
    /// </summary>
    /// <param name="kTeamPos"></param>
    /// <returns></returns>
    public static bool IsMagicEquipFull(TEAM_POS kTeamPos)
    {
        for (int i = 0; i < 2; i++)
        {
            var magic = TeamManager.GetMagicDataByTeamPos((int)kTeamPos, i);
            if (magic == null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 该符灵是否在阵上
    /// </summary>
    /// <param name="iItemId">唯一id</param>
    /// <returns></returns>
    public static bool IsPetInTeam(int iItemId)
    {
        foreach(KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            if (pair.Value.bodyId == iItemId)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 指定tid的符灵是否在阵上
    /// </summary>
    /// <param name="iTid">tid</param>
    /// <returns></returns>
    public static bool IsPetInTeamByTid(int iTid)
    {
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            if (pair.Key != TEAM_POS.CHARACTER)
            {
                PetData itemData = PetLogicData.Self.GetPetDataByItemId(pair.Value.bodyId);
                if (itemData != null && itemData.tid == iTid)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 得到指定tid在阵上的符灵
    /// </summary>
    /// <param name="iTid">tid</param>
    /// <returns></returns>
    public static PetData GetPetInTeamByTid(int iTid)
    {
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            if (pair.Key != TEAM_POS.CHARACTER)
            {
                PetData itemData = PetLogicData.Self.GetPetDataByItemId(pair.Value.bodyId);
                if (itemData != null && itemData.tid == iTid)
                {
                    return itemData;
                }
            }
        }
        return null;
    }

    public static bool IsEquipInTeam(int iItemId)
    {
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            foreach (int tempId in pair.Value.mEquipIdList)
            {
                if (tempId == iItemId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsMagicInTeam(int iItemId)
    {
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            foreach (int tempId in pair.Value.mMagicIdList)
            {
                if (tempId == iItemId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // get use pet num
    public static int GetPetNumInTeam()
    {
        int iCount = 0;
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            if (pair.Value.bodyId > 0)
                iCount++;
        }
        return iCount;
    }

    public static int GetEqupNumInTeam()
    {
        int iCount = 0;
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            foreach (int iItemId in pair.Value.mEquipIdList)
            {
                if (iItemId > 0)
                    iCount++;
            }
        }
        return iCount;
    }
    public static int GetEqupNumInTeamByType(int iType)
    {
        if(iType >= (int)EQUIP_TYPE.MAX || iType < (int)EQUIP_TYPE.ARM_EQUIP)
            return 0;

        int iCount = 0;
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            if (pair.Key >= TEAM_POS.CHARACTER && pair.Key < TEAM_POS.MAX)
            {
                if (pair.Value.mEquipIdList[iType] > 0)
                    iCount++;
            }
        }
        return iCount;
    }
    
    public static int GetMagicNumInTeam()
    {
        int iCount = 0;
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            foreach (int iItemId in pair.Value.mMagicIdList)
            {
                if (iItemId > 0)
                    iCount++;
            }
        }
        return iCount;
    }

    public static int GetMagicNumInTeamByType(int iType)
    {
        if (iType >= (int)MAGIC_TYPE.MAX || iType < (int)MAGIC_TYPE.MAGIC1)
            return 0;

        int iCount = 0;
        foreach (KeyValuePair<TEAM_POS, TeamPosData> pair in mDicTeamPosData)
        {
            if (pair.Key >= TEAM_POS.CHARACTER && pair.Key < TEAM_POS.MAX)
            {
                if (pair.Value.mMagicIdList[iType] > 0)
                    iCount++;
            }
        }
        return iCount;
    }

    //-------------------------------------------------
    // 请求上下阵
    //-------------------------------------------------
    public static void RequestChangeTeamPos(int iTeamPos, int iUpItemId, int iUpTid, int iDownItemId, int iDownTid)
    {
        switch (PackageManager.GetItemTypeByTableID(iUpTid))
        {
            case ITEM_TYPE.PET:
                RequestChangeBodyTeamPos(iTeamPos, iUpItemId, iUpTid, iDownItemId, iDownTid);
                return;
            case ITEM_TYPE.EQUIP:
                RequestChangeEquipTeamPos(iTeamPos, iUpItemId, iUpTid, iDownItemId, iDownTid);
                return;
            case ITEM_TYPE.MAGIC:
                RequestChangeMagicTeamPos(iTeamPos, iUpItemId, iUpTid, iDownItemId, iDownTid);
                return;
        }

        switch (PackageManager.GetItemTypeByTableID(iDownTid))
        {
            case ITEM_TYPE.PET:
                RequestChangeBodyTeamPos(iTeamPos, iUpItemId, iUpTid, iDownItemId, iDownTid);
                return;
            case ITEM_TYPE.EQUIP:
                RequestChangeEquipTeamPos(iTeamPos, iUpItemId, iUpTid, iDownItemId, iDownTid);
                return;
            case ITEM_TYPE.MAGIC:
                RequestChangeMagicTeamPos(iTeamPos, iUpItemId, iUpTid, iDownItemId, iDownTid);
                return;
        }
    }

    public static void RequestChangeBodyTeamPos(int iTeamPos, int iUpItemId, int iUpTid, int iDownItemId, int iDownTid)
    {
        CS_PetLineupChange teamPosChangeData = new CS_PetLineupChange();

        teamPosChangeData.teamPos = iTeamPos;
        teamPosChangeData.upItemId = iUpItemId;
        teamPosChangeData.upTid = iUpTid;
        teamPosChangeData.downItemId = iDownItemId;
        teamPosChangeData.downTid = iDownTid;
        HttpModule.Instace.SendGameServerMessage(teamPosChangeData, "CS_PetLineupChange", x => NetManager.RequestTeamPosChangSuccess(teamPosChangeData, x), NetManager.RequestTeamPosChangFail);
    }
    public static void RequestChangeEquipTeamPos(int iTeamPos, int iUpItemId, int iUpTid, int iDownItemId, int iDownTid)
    {
        CS_BodyEquipChange teamPosChangeData = new CS_BodyEquipChange();

        teamPosChangeData.teamPos = iTeamPos;
        teamPosChangeData.upItemId = iUpItemId;
        teamPosChangeData.upTid = iUpTid;
        teamPosChangeData.downItemId = iDownItemId;
        teamPosChangeData.downTid = iDownTid;
        HttpModule.Instace.SendGameServerMessage(teamPosChangeData, "CS_BodyEquipChange", x => NetManager.RequestTeamPosChangSuccess(teamPosChangeData, x), NetManager.RequestTeamPosChangFail);
    }
    public static void RequestChangeMagicTeamPos(int iTeamPos, int iUpItemId, int iUpTid, int iDownItemId, int iDownTid)
    {
        CS_BodyEquipChange teamPosChangeData = new CS_BodyEquipChange();

        teamPosChangeData.teamPos = iTeamPos;
        teamPosChangeData.upItemId = iUpItemId;
        teamPosChangeData.upTid = iUpTid;
        teamPosChangeData.downItemId = iDownItemId;
        teamPosChangeData.downTid = iDownTid;
        HttpModule.Instace.SendGameServerMessage(teamPosChangeData, "CS_BodyEquipChange", x => NetManager.RequestTeamPosChangSuccess(teamPosChangeData, x), NetManager.RequestTeamPosChangFail);
    }

    //-------------------------------------------------
    // 上下阵成功
    //-------------------------------------------------
    public static bool ChangeTeamPos(TeamPosChangeData teamPosChangeData)
    {
        if (teamPosChangeData.upTid >= 0)
        {
            switch (PackageManager.GetItemTypeByTableID(teamPosChangeData.upTid))
            {
                case ITEM_TYPE.PET:
                    ChangeBodyTeamPos(teamPosChangeData);
                    break;
                case ITEM_TYPE.EQUIP:
                    ChangeEquipTeamPos(teamPosChangeData);
                    break;
                case ITEM_TYPE.MAGIC:
                    ChangeMagicTeamPos(teamPosChangeData);
                    break;
            }

            OnChangeTeamPos.Invoke(teamPosChangeData);
        }
        else if(teamPosChangeData.downTid >= 0)
        {
            switch (PackageManager.GetItemTypeByTableID(teamPosChangeData.downTid))
            {
                case ITEM_TYPE.PET:
                    ChangeBodyTeamPos(teamPosChangeData);
                    break;
                case ITEM_TYPE.EQUIP:
                    ChangeEquipTeamPos(teamPosChangeData);
                    break;
                case ITEM_TYPE.MAGIC:
                    ChangeMagicTeamPos(teamPosChangeData);
                    break;
            }

            OnChangeTeamPos.Invoke(teamPosChangeData);
        }
        return true;
    }

    private static bool ChangeBodyTeamPos(TeamPosChangeData teamPosChangeData)
    {
        if(teamPosChangeData != null)
        {
            if (PackageManager.GetItemTypeByTableID(teamPosChangeData.upTid) == ITEM_TYPE.PET
                || PackageManager.GetItemTypeByTableID(teamPosChangeData.downTid) == ITEM_TYPE.PET)
            {
                PetLogicData.Self.ChangeTeamPos(teamPosChangeData);
            }
            TeamPosData teamPosData = GetTeamPosData(teamPosChangeData.teamPos);
            teamPosData.bodyId = teamPosChangeData.upItemId;

            Relationship.OnTeamChanged(teamPosChangeData);
            OnChangeBodyTeamPos.Invoke(teamPosChangeData);
        }
        return true;
    }

    private static bool ChangeEquipTeamPos(TeamPosChangeData teamPosChangeData)
    {
        if (teamPosChangeData != null)
        {
            if (PackageManager.GetItemTypeByTableID(teamPosChangeData.upTid) == ITEM_TYPE.EQUIP
                || PackageManager.GetItemTypeByTableID(teamPosChangeData.downTid) == ITEM_TYPE.EQUIP)
            {
                RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
                if (logicData != null)
                {
                    logicData.ChangeTeamPos(teamPosChangeData);
                }
            }

            TeamPosData teamPosData = GetTeamPosData(teamPosChangeData.teamPos);
            int iIndex = PackageManager.GetSlotPosByTid(teamPosChangeData.upTid != -1 ? teamPosChangeData.upTid : teamPosChangeData.downTid);
            if (iIndex >= (int)EQUIP_TYPE.ARM_EQUIP && iIndex < teamPosData.mEquipIdList.Count)
            {
                teamPosData.mEquipIdList[iIndex] = teamPosChangeData.upItemId;
            }

            Relationship.OnTeamChanged(teamPosChangeData);
            OnChangeEquipTeamPos.Invoke(teamPosChangeData);
        }
        return true;
    }

    private static bool ChangeMagicTeamPos(TeamPosChangeData teamPosChangeData)
    {
        if (teamPosChangeData != null)
        {
            if (PackageManager.GetItemTypeByTableID(teamPosChangeData.upTid) == ITEM_TYPE.MAGIC
                || PackageManager.GetItemTypeByTableID(teamPosChangeData.downTid) == ITEM_TYPE.MAGIC)
            {
                MagicLogicData logicData = DataCenter.GetData("MAGIC_DATA") as MagicLogicData;
                if (logicData != null)
                {
                    logicData.ChangeTeamPos(teamPosChangeData);
                }
            }

            TeamPosData teamPosData = GetTeamPosData(teamPosChangeData.teamPos);
            int iIndex = PackageManager.GetSlotPosByTid(teamPosChangeData.upTid != -1 ? teamPosChangeData.upTid : teamPosChangeData.downTid);
            if (iIndex >= (int)MAGIC_TYPE.MAGIC1 && iIndex < teamPosData.mMagicIdList.Count)
            {
                teamPosData.mMagicIdList[iIndex] = teamPosChangeData.upItemId;
            }

            Relationship.OnTeamChanged(teamPosChangeData);
            OnChangeMagicTeamPos.Invoke(teamPosChangeData);
        }
        return true;
    }

    public static IEnumerable<EquipData> AllEquipAndMagicInTeamPos(TEAM_POS teamPos)
    {
        TeamPosData teamData = mDicTeamPosData[teamPos];
        RoleEquipLogicData equipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;

        foreach (var i in teamData.mEquipIdList)
        {
            if (i > 0)
            {
                yield return equipLogicData.mDicEquip[i];
            }
        }

        MagicLogicData magicLogicData = DataCenter.GetData("MAGIC_DATA") as MagicLogicData;

        foreach (var i in teamData.mMagicIdList)
        {
            if (i > 0)
            {
                yield return magicLogicData.mDicEquip[i];
            }
        }
    }

    public static bool IsPosOpen(int iTeamPos)
    {
        TeamPosData teamPosData;
        if (mDicTeamPosData.TryGetValue((TEAM_POS)iTeamPos, out teamPosData))
        {
            return teamPosData.openLevel <= RoleLogicData.GetMainRole().level;
        }

        return false;
    }

    //public static bool ChangeEquip(int iIndex, int iUpItemId, int iDownItemId)
    //{
    //    if (iIndex < mTeamData.mEquipIdList.Count)
    //    {
    //        if(iUpItemId)
    //    }
    //    return false;
    //}

    //public static bool UpEquip(int iIndex, int iUpItemId)
    //{
    //    if (iIndex < mTeamData.mEquipIdList.Count)
    //    {
    //        RoleEquipLogicData logic = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
    //        EquipData equipData = logic.GetEquipDataByItemId(iUpItemId);
    //        int posType = PackageManager.Instace.GetPosByTid(equipData.tid);
    //        int iDownItemId = mTeamData.mEquipIdList[iIndex];
    //    }
    //    return false;
    //}

    //public void OnChangeTeamPos(int iTeamPos)
    //{

    //}

    //public void OnChangeBodyTeamPos(int iTeamPos)
    //{

    //}

    //public void OnChangeEquipTeamPos(int iTeamPos)
    //{

    //}

    //public void OnChangeMagicTeamPos(int iTeamPos)
    //{

    //}
}

public class TeamPosData
{
    public int teamPos;
    public int bodyId;
    public List<int> mEquipIdList = new List<int>();
    public List<int> mMagicIdList = new List<int>();
    public int openLevel = 1;
    public TeamPosData(int iPos)
    {
        teamPos = iPos;
        openLevel = TableCommon.GetNumberFromPetPostionLevel(teamPos, "OPEN_LEVEL");
    }
    public TeamPosData(TeamPosData teamPosData)
    {
        if (teamPosData != null)
        {
            teamPos = teamPosData.teamPos;
            bodyId = teamPosData.bodyId;
            mEquipIdList = teamPosData.mEquipIdList.ToList();
            mMagicIdList = teamPosData.mMagicIdList.ToList();
            openLevel = teamPosData.openLevel;
        }
    }




}