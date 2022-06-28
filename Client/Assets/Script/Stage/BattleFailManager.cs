using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using System;
using xk;

namespace xk
{
    class Range
    {
        public int Min { set; get; }
        public int Max { set; get; }
        public Range() { }
        public Range(int min, int max)
        {
            this.Min = min;
            this.Max = max;
        }
        public bool IsInRange(int value)
        {
            return (Min <= value && value <= Max);
        }
    }
}

public struct FunctionWeight
{
    public int mGainFuncID;
    public int mWeight;
    public float mPercent;
};

public struct FailGuidInfo
{
    public DataRecord mGuidRecord;
    public List<FunctionWeight> mFuncWeightList;
};

/// <summary>
/// 战斗失败管理
/// </summary>
public class BattleFailManager : ManagerSingleton<BattleFailManager>
{
    int mLastRoleLevel = int.MinValue;
    DataRecord mLastFailRecord = null;

    /// <summary>
    /// 根据玩家等级获得对应的记录
    /// </summary>
    /// <returns></returns>
    private DataRecord GetFailRecord()
    {
        int _curRoleLv = RoleLogicData.Self.character.level;
        if (mLastRoleLevel == _curRoleLv)
        {
            return mLastFailRecord;
        }
        using (var recordPair = DataCenter.mFailGuide.GetAllRecord().GetEnumerator())
        {
            while (recordPair.MoveNext())
            {
                DataRecord _record = recordPair.Current.Value;
                int[] _lvArr = ParseRangeString(_record.getData("LEVEL_AREA"));
                if (_lvArr == null)
                {
                    Debug.LogError("FailGuid表格数据格式不对,索引是:" + _record.getIndex() + " 字段是: LEVEL_AREA");
                    return null;
                }
                Range _range = new Range(_lvArr[0], _lvArr[1]);
                if (_range.IsInRange(_curRoleLv))
                {
                    mLastRoleLevel = _curRoleLv;
                    mLastFailRecord = _record;
                    return _record;
                }
            }
            return null;
        }
    }

    public FailGuidInfo GetFailGuideInfo()
    {
        DataRecord _failRecord = GetFailRecord();
        if (_failRecord == null)
        {
            Debug.LogError("没有对应的失败引导记录");
            return default(FailGuidInfo);
        }

        FailGuidInfo _failGuidInfo;
        _failGuidInfo.mGuidRecord = _failRecord;
        List<FunctionWeight> _funcWeightList = new List<FunctionWeight>();
        string _functionWeightStrInfo = _failRecord.getData("FUNCTION_WEIGHT");
        string[] _funcAndWeightArr = _functionWeightStrInfo.Split('|');
        for (int i = 0, count = _funcAndWeightArr.Length; i < count; ++i)
        {
            FunctionWeight _funcWeight;
            int[] _arr = ParseRangeString(_funcAndWeightArr[i]);
            if (_arr == null || _arr.Length != 2)
                continue;
            _funcWeight.mGainFuncID = _arr[0];
            _funcWeight.mWeight = _arr[1];
            _funcWeight.mPercent = GetPercentByFuncWeight(_arr[1]);
            _funcWeightList.Add(_funcWeight);
        }

        _failGuidInfo.mFuncWeightList = _funcWeightList;
        //foreach (var value in _funcWeightList)
        //{
        //    Debug.LogError("ID: " + value.mGainFuncID + "  weight: " + value.mWeight + "  percent: " + value.mPercent);
        //}
        return _failGuidInfo;
    }

    //对权重从大到小进行排序
    private int FuncWeightCompare(FunctionWeight lhs, FunctionWeight rhs)
    {
        return (lhs.mWeight - rhs.mWeight) * -1;
    }
    //对百分比从小到大进行排序
    private int FuncPercentCompare(FunctionWeight lhs, FunctionWeight rhs)
    {
        if (lhs.mPercent - rhs.mPercent < 0)
            return -1;
        if (lhs.mPercent - rhs.mPercent > 0)
            return 1;
        return 0;
    }


    string[] mFuncRateIndexArr = { "FUNCTION_RATE_1", "FUNCTION_RATE_2", "FUNCTION_RATE_3" };
    ///<summary>
    ///根据配表索引获得推荐的功能ID
    ///</summary>
    ///<param name="kIndex"></param>
    ///<returns></returns>
    public int GetRecommendGainFuncId(FailGuidInfo kFailGuidInfo)
    {
        List<FunctionWeight> _funcWeightList = kFailGuidInfo.mFuncWeightList;
        for (int i = 0, count = mFuncRateIndexArr.Length; i < count; ++i)
        {
            int _gainFuncID = -1;
            if (i != count - 1)
            {
                _gainFuncID = RecommendRule(kFailGuidInfo.mFuncWeightList, kFailGuidInfo.mGuidRecord.getData(mFuncRateIndexArr[i]));
            }
            else
            {
                _gainFuncID = RecommendRule(kFailGuidInfo.mFuncWeightList, kFailGuidInfo.mGuidRecord.getData(mFuncRateIndexArr[i]), true);
            }
            if (_gainFuncID != -1)
            {
                return _gainFuncID;
            }
        }
        return -1;
    }
    /// <summary>
    /// 推荐规则
    /// </summary>
    /// <param name="kFunctionWeightList">推荐功能ID和对应的权重列表</param>
    /// <param name="isLast">是否是最后一个,因为比较规则不一样</param>
    /// <param name="kRecommendPercentInfo">推荐比较数据</param>
    /// <returns></returns>
    private int RecommendRule(List<FunctionWeight> kFunctionWeightList, string kRecommendPercentInfo, bool isLast = false)
    {
        List<float> _recommendInfoList = new List<float>();
        string[] _recStrArr = kRecommendPercentInfo.Split('|');
        for (int i = 0, count = _recStrArr.Length; i < count; ++i)
        {
            _recommendInfoList.Add(float.Parse(_recStrArr[i].Trim()));
        }
        List<FunctionWeight> _finalFuncList = new List<FunctionWeight>();
        for (int j = 0, count = _recommendInfoList.Count; j < count; ++j)
        {
            if (kFunctionWeightList.Count <= j)
                break;
            if (kFunctionWeightList[j].mPercent < _recommendInfoList[j])
            {
                _finalFuncList.Add(kFunctionWeightList[j]);
            }
        }
        //1.如果有功能的百分比值 < 对应的配置
        if (_finalFuncList.Count != 0)
        {
            _finalFuncList.Sort(FuncWeightCompare);
            return _finalFuncList[0].mGainFuncID;
        }
        else
        {
            if (!isLast)
            {
                //标记，由外部进行下一次判断
                return -1;
            }
            else
            {
                kFunctionWeightList.Sort(FuncPercentCompare);
                if(kFunctionWeightList.Count > 0)
                    return kFunctionWeightList[0].mGainFuncID;
                return -1;
            }
        }

    }
    // 将"111#222"格式的字符转换成{111,222}数组
    private int[] ParseRangeString(string kStr)
    {
        string[] _strArr = kStr.Split('#');
        if (_strArr.Length != 2)
        {
            Debug.LogError("数据格式不对 : " + kStr);
            return null;
        }
        int _lhs = int.Parse(_strArr[0].Trim());
        int _rhs = int.Parse(_strArr[1].Trim());

        return new int[] { _lhs, _rhs };
    }
    private Dictionary<int, Func<float>> mPercentDic = new Dictionary<int, Func<float>>();
    public void Init()
    {
        mPercentDic.Clear();
        //1.符灵升级
        mPercentDic.Add(1, GetPetLevelUp_Percent);
        //2.角色突破
        mPercentDic.Add(2, GetBreakLevel_Percent);
        //3.角色天命
        mPercentDic.Add(3, GetFate_Percent);
        //4.符灵技能
        mPercentDic.Add(4, GetPetSkill_Percent);
        //5.装备强化
        mPercentDic.Add(5, GetEquipStrengthen_Percent);
        //6.装备精炼
        mPercentDic.Add(6, GetEquipRefine_Percent);
        //7.法器强化
        mPercentDic.Add(7, GetMagicEquipStrengthen_Percent);
        //8.法器精炼
        mPercentDic.Add(8, GetMagicEquipRefine_Percent);
        //9.商城
        mPercentDic.Add(9, GetShop_Percent);
        //10.装备获得
        mPercentDic.Add(10, GetEquipObtain);
        //11.装备穿戴
        mPercentDic.Add(11, GetEquipDress_Percent);
        //12.法器获得
        mPercentDic.Add(12, GetMagicObtain_Percent);
        //13.法器穿戴
        mPercentDic.Add(13, GetMagicDress_Percent);
        //14.上阵
        mPercentDic.Add(14, GetOnPos_Percent);
        //15.缘分助战
        mPercentDic.Add(15, GetRelation_Percent);
    }
    private float GetPercentByFuncWeight(int kWeight)
    {
        Func<float> _percentFunc;
        if (mPercentDic.TryGetValue(kWeight, out _percentFunc))
        {
            return _percentFunc();
        }

        Debug.LogError("mPercent中没有添加对应的FuncWeight:" + kWeight);
        return 0f;
    }

    //----------------------------------------
    // 计算对应功能的百分比
    //----------------------------------------
    /// <summary>
    /// 符灵升级
    /// 分子=当前符灵等级总和
    /// 分母=当前主角等级×3
    /// </summary>
    private float GetPetLevelUp_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        //当前符灵等级总和
        for (int i = (int)TEAM_POS.PET_1; i < (int)TEAM_POS.MAX; ++i)
        {
            ActiveData _activeData = TeamManager.GetActiveDataByTeamPos(i);
            if (_activeData == null)
                continue;
            _numerator += _activeData.level;
        }
        //主角等级x3
        _denominator = RoleLogicData.Self.character.level * 3;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 角色突破
    /// </summary>
    /// <returns></returns>
    private float GetBreakLevel_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        //当前符灵突破等级总和
        for (int i = (int)TEAM_POS.PET_1; i < (int)TEAM_POS.MAX; ++i)
        {
            ActiveData _activeData = TeamManager.GetActiveDataByTeamPos(i);
            if (_activeData == null)
                continue;
            _numerator += _activeData.breakLevel;
        }
        //当前主角最大突破等级
        int _roleLevel = RoleLogicData.Self.character.level;
        int _curMaxBreakLevel = 1;
        bool _hasSetValue = false;
        using (var recordPair = DataCenter.mBreakLevelConfig.GetAllRecord().GetEnumerator())
        {
            while (recordPair.MoveNext())
            {
                DataRecord _record = recordPair.Current.Value;
                if (_roleLevel < _record.getData("ACTIVE_LEVEL"))
                {
                    _hasSetValue = true;
                    _curMaxBreakLevel = _record.getIndex();
                }
            }
            if (!_hasSetValue)
            {
                _curMaxBreakLevel = GameCommon.GetMaxBreakLevelByTid(RoleLogicData.Self.character.tid);
            }
        }
        _denominator = _curMaxBreakLevel * 3;
        return GetPercent(_numerator, _denominator);
    }

    private float GetFate_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        //当前所有角色天命等级
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; ++i)
        {
            ActiveData _activeData = TeamManager.GetActiveDataByTeamPos(i);
            if (_activeData == null)
                continue;
            _numerator += _activeData.fateLevel;
        }

        //天命上限
        _denominator = GameCommon.GetMaxFateLevel() * 4;
        return GetPercent(_numerator, _denominator);
    }

    /// <summary>
    /// 符灵技能
    /// </summary>
    /// <returns></returns>
    private float GetPetSkill_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        //当前符灵技能等级总和
        for (int i = (int)TEAM_POS.PET_1; i < (int)TEAM_POS.MAX; ++i)
        {
            ActiveData _activeData = TeamManager.GetActiveDataByTeamPos(i);
            if (_activeData == null)
                continue;
            for (int j = 0; j < 4; ++j)
            {
                _numerator += _activeData.skillLevel[j];
            }
        }

        //当前技能上限 * 4 * 3
        _denominator = NewSKillUpgradeBean.SKILL_LEVEL_LIMIT * 4 * 3;
        return GetPercent(_numerator, _denominator);
    }

    /// <summary>
    /// 装备强化
    /// </summary>
    /// <returns></returns>
    private float GetEquipStrengthen_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _equipNum = 0;
        //当前已穿戴装备强化等级总和
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; ++i)
        {
            for (int j = (int)EQUIP_TYPE.ARM_EQUIP; j < (int)EQUIP_TYPE.MAX; ++j)
            {
                EquipData _equipData = TeamManager.GetEquipData(i, j, ITEM_TYPE.EQUIP);
                if (_equipData == null)
                    continue;
                _numerator += _equipData.strengthenLevel;
                _equipNum++;
            }
        }
        //当前已经穿戴装备数量 x 主角等级 x 2
        _denominator = _equipNum * RoleLogicData.Self.character.level * 2;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 装备精炼
    /// </summary>
    /// <returns></returns>
    private float GetEquipRefine_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _equipNum = 0;
        //当前已穿戴装备强化等级总和
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; ++i)
        {
            for (int j = (int)EQUIP_TYPE.ARM_EQUIP; j < (int)EQUIP_TYPE.MAX; ++j)
            {
                EquipData _equipData = TeamManager.GetEquipData(i, j, ITEM_TYPE.EQUIP);
                if (_equipData == null)
                    continue;
                _numerator += _equipData.refineLevel;
                _equipNum++;
            }
        }

        //装备精炼上限等级×当前已穿戴装备数量
        _denominator = TableCommon.GetNumberFromRoleEquipConfig(((int)ITEM_TYPE.EQUIP * 100 + 1), "MAX_REFINE_LEVEL") * _equipNum;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 法器强化
    /// </summary>
    /// <returns></returns>
    private float GetMagicEquipStrengthen_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _equipNum = 0;

        //分子=当前已穿戴装备精炼等级总和
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; ++i)
        {
            for (int j = (int)MAGIC_TYPE.MAGIC1; j < (int)MAGIC_TYPE.MAX; ++j)
            {
                EquipData _equipData = TeamManager.GetEquipData(i, j, ITEM_TYPE.MAGIC);
                if (_equipData == null)
                    continue;
                _numerator += _equipData.strengthenLevel;
                _equipNum++;
            }
        }
        //分母=当前已穿戴法器数量×法器强化等级上限
        _denominator = TableCommon.GetNumberFromRoleEquipConfig(((int)ITEM_TYPE.MAGIC * 100 + 1), "MAX_GROW_LEVEL") * _equipNum;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 法器精炼
    /// </summary>
    /// <returns></returns>
    private float GetMagicEquipRefine_Percent()
    {

        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _equipNum = 0;
        //分子=当前已穿戴装备精炼等级总和
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; ++i)
        {
            for (int j = (int)MAGIC_TYPE.MAGIC1; j < (int)MAGIC_TYPE.MAX; ++j)
            {
                EquipData _equipData = TeamManager.GetEquipData(i, j, ITEM_TYPE.MAGIC);
                if (_equipData == null)
                    continue;
                _numerator += _equipData.refineLevel;
                _equipNum++;
            }
        }
        //分母=当前已穿戴法器数量×法器精炼等级上限
        _denominator = TableCommon.GetNumberFromRoleEquipConfig(((int)ITEM_TYPE.MAGIC * 100 + 1), "MAX_REFINE_LEVEL") * _equipNum;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 商城
    /// </summary>
    /// <returns></returns>
    private float GetShop_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母
        //分子=当前拥有橙卡总数×3+当前紫卡总数×1
        int _hasOrangeCardNum = 0;
        int _hasPurpleCardNum = 0;
        using (var cardPair = PetLogicData.Self.mDicPetData.GetEnumerator())
        {
            while (cardPair.MoveNext())
            {
                PetData _petData = cardPair.Current.Value;
                if ((int)PET_QUALITY.ORANGE == GameCommon.GetItemQuality(_petData.tid))
                {
                    _hasOrangeCardNum++;
                }
                else if ((int)PET_QUALITY.PURPLE == GameCommon.GetItemQuality(_petData.tid))
                {
                    _hasPurpleCardNum++;
                }
            }
        }
        _numerator = _hasOrangeCardNum * 3 + _hasPurpleCardNum;
        //分母=1
        _denominator = 1;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 装备获得
    /// </summary>
    /// <returns></returns>
    private float GetEquipObtain()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _hasOrangeNum = 0;
        int _hasPurpleNum = 0;
        int _hasBlueNum = 0;
        //分子=当前拥有橙色装备数×3+当前拥有紫色装备数×2+当前拥有蓝色装备数×1
        using (var equipPair = RoleEquipLogicData.Self.mDicEquip.GetEnumerator())
        {
            while (equipPair.MoveNext())
            {
                EquipData _equipData = equipPair.Current.Value;
                if ((int)EQUIP_QUALITY_TYPE.BEST == GameCommon.GetItemQuality(_equipData.tid))
                {
                    _hasOrangeNum++;
                }
                else if ((int)EQUIP_QUALITY_TYPE.BETTER == GameCommon.GetItemQuality(_equipData.tid))
                {
                    _hasPurpleNum++;
                }
                else if ((int)EQUIP_QUALITY_TYPE.GOOD == GameCommon.GetItemQuality(_equipData.tid))
                {
                    _hasBlueNum++;


                }
            }
        }
        _numerator = _hasOrangeNum * 3 + _hasPurpleNum * 2 + _hasBlueNum;
        //分母=1
        _denominator = 1;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 装备穿戴
    /// </summary>
    /// <returns></returns>
    private float GetEquipDress_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _equipNum = 0;       //> 已经穿戴装备数量
        int _canEquipNum = 0;    //> 可以装备的数量

        int _roleLevel = RoleLogicData.Self.character.level;
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; ++i)
        {
            //分子=当前已经穿戴的装备数量
            for (int j = (int)EQUIP_TYPE.ARM_EQUIP; j < (int)EQUIP_TYPE.MAX; ++j)
            {
                EquipData _equipData = TeamManager.GetEquipData(i, j, ITEM_TYPE.EQUIP);
                if (_equipData == null)
                    continue;
                _equipNum++;
            }
            //分母=当前可穿戴的装备数量（包含主角的已开放阵位×4）
            TeamPosData _teamPosData = TeamManager.GetTeamPosData(i);
            if (_teamPosData.openLevel <= _roleLevel)
            {
                _canEquipNum += (int)EQUIP_TYPE.MAX;
            }
        }
        _numerator = _equipNum;
        _denominator = _canEquipNum;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 法器获得
    /// </summary>
    /// <returns></returns>
    private float GetMagicObtain_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _hasOrangeNum = 0;
        int _hasPurpleNum = 0;
        int _hasBlueNum = 0;
        //分子=当前拥有橙色法器数×3+当前拥有紫色法器数×2+当前拥有蓝色法器数×1
        using (var magicPair = MagicLogicData.Self.mDicEquip.GetEnumerator())
        {
            while (magicPair.MoveNext())
            {
                EquipData _equipData = magicPair.Current.Value;
                if ((int)EQUIP_QUALITY_TYPE.BEST == GameCommon.GetItemQuality(_equipData.tid))
                {
                    _hasOrangeNum++;
                }
                else if ((int)EQUIP_QUALITY_TYPE.BETTER == GameCommon.GetItemQuality(_equipData.tid))
                {
                    _hasPurpleNum++;
                }
                else if ((int)EQUIP_QUALITY_TYPE.GOOD == GameCommon.GetItemQuality(_equipData.tid))
                {
                    _hasBlueNum++;
                }
            }
        }
        _numerator = _hasOrangeNum * 3 + _hasPurpleNum * 2 + _hasBlueNum;
        //分母=1
        _denominator = 1;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 法器穿戴
    /// </summary>
    /// <returns></returns>
    private float GetMagicDress_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _equipNum = 0;       //> 已经穿戴装备数量
        int _canEquipNum = 0;    //> 可以装备的数量

        int _roleLevel = RoleLogicData.Self.character.level;
        for (int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; ++i)
        {
            TeamPosData _teamPosData = TeamManager.GetTeamPosData(i);
            for (int j = (int)MAGIC_TYPE.MAGIC1; j < (int)MAGIC_TYPE.MAX; ++j)
            {
                //分母=当前可穿戴的法器数量（包含主角的已开放阵位×2）
                if (_teamPosData.openLevel <= _roleLevel)
                {
                    if (TeamManager.GetMagicEquipOpenLevelByType(j) <= _roleLevel)
                    {
                        _canEquipNum++;
                    }
                }
                //分子=当前已经穿戴的法器数量
                EquipData _equipData = TeamManager.GetEquipData(i, j, ITEM_TYPE.MAGIC);
                if (_equipData == null)
                    continue;
                _equipNum++;
            }
        }
        _numerator = _equipNum;
        _denominator = _canEquipNum;
        return GetPercent(_numerator, _denominator);
    }
    /// <summary>
    /// 上阵
    /// </summary>
    /// <returns></returns>
    private float GetOnPos_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _hasOnPosNum = 0;        //> 已经上阵符灵数量
        int _hasOpenPosNum = 0;      //> 当前开放阵位数量
        int _roleLevel = RoleLogicData.Self.character.level;
        for (int i = (int)TEAM_POS.PET_1; i < (int)TEAM_POS.MAX; ++i)
        {
            TeamPosData _teamPosData = TeamManager.GetTeamPosData(i);
            if (_teamPosData.bodyId > 0)
            {
                _hasOnPosNum++;
            }
            if (_teamPosData.openLevel <= _roleLevel)
            {
                _hasOpenPosNum++;
            }
        }
        _numerator = _hasOnPosNum;
        _denominator = _hasOpenPosNum;
        return GetPercent(_numerator, _denominator);
    }
    private float GetRelation_Percent()
    {
        int _numerator = 0;      //> 分子
        int _denominator = 1;    //> 分母

        int _hasOnPosNum = 0;    //> 当前缘分上阵符灵 
        int _hasOpenPosNum = 0;  //> 当前开放缘分站位
        int _roleLevel = RoleLogicData.Self.character.level;

        for (int i = (int)TEAM_POS.RELATE_1; i <= (int)TEAM_POS.RELATE_6; ++i)
        {
            TeamPosData _teamPosData = TeamManager.GetTeamPosData(i);
            if (_teamPosData.openLevel <= _roleLevel)
            {
                _hasOpenPosNum++;
            }
            if (_teamPosData.bodyId > 0)
            {
                _hasOnPosNum++;
            }
        }
        //分子=已经上阵的助阵符灵
        _numerator = _hasOnPosNum;
        //分母=当前开放的助阵阵位
        _denominator = _hasOnPosNum;
        return GetPercent(_numerator, _denominator);
    }

    private float GetPercent(int kNumerator, int kDenominator)
    {
        if (kDenominator == 0)
            return 0f;
        return kNumerator * 1.0f / kDenominator;
    }


    ///----------------
    /// UI刷新
    ///-----------------
    //初始化失败引导UI
    public void InitFailGuideUI(UIGridContainer kGridContainer, Action kCloseWinAction)
    {
        if (kGridContainer == null)
            return;
        FailGuidInfo _failGuidInfo = GetFailGuideInfo();
        int _iconCount = _failGuidInfo.mFuncWeightList.Count;
        kGridContainer.MaxCount = _iconCount;
        //Vector3 _originPos = kGridContainer.transform.localPosition;
        kGridContainer.transform.localPosition = new Vector3(0 - (kGridContainer.CellWidth / 2) * _iconCount, 0, 0);
        int _recommendFuncID = GetRecommendGainFuncId(_failGuidInfo);

        for (int i = 0, count = _iconCount; i < count; ++i)
        {
            int _gainFuncID = _failGuidInfo.mFuncWeightList[i].mGainFuncID;
            UpdateCell(kGridContainer.controlList[i], _gainFuncID, _gainFuncID == _recommendFuncID, kCloseWinAction);
        }
    }

    //UI刷新每一个图标
    public void UpdateCell(GameObject kCellObj, int kGainFuncID, bool kShowRecommendIcon, Action kCloseWinAction)
    {
        //显示图标
        GameCommon.SetUIVisiable(kCellObj, "recommend_sprite", kShowRecommendIcon);
        DataRecord _record = DataCenter.mGainFunctionConfig.GetRecord(kGainFuncID);
        GameCommon.SetIcon(kCellObj, "background", _record.getData("FAILD_SPRITE_NAME"), _record.getData("FAILD_ATLAS_NAME"));
        //绑定事件
        AddButtonAction(kCellObj, () =>
        {
            Debug.LogError("funcID: " + kGainFuncID);
            if (kCloseWinAction != null)
            {
                kCloseWinAction();
            }

            DataCenter.Set("FROM_BATTLE_FAIL", kGainFuncID);
            MainProcess.QuitBattle();
            MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
        });
    }
    private void AddButtonAction(GameObject button, Action action)
    {
        var evt = button.GetComponent<UIButtonEvent>();
        if (evt == null) DEBUG.LogError("No exist button event > " + button.name);
        else evt.AddAction(action);
    }
}
