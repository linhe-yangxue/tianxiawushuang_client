using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DataTable;

public static class AffectFunc {

    public static Affect GetPetAllAffect(int teamPos) {
        if(teamPos<0||teamPos>3)
            DEBUG.LogError("不在阵上的符灵请勿调用此函数");
        var activeData=TeamManager.GetActiveDataByTeamPos(teamPos);
        //by chenliang
        //begin

//         return StrongMasterAffect.GetTotalAffect(teamPos)+
//             BreakAffect.GetTotalAffect(activeData)+
//             StrongRefineAffect.GetTotalAffect(teamPos)+
//             Relationship.GetTotalAffect(teamPos);
//-----------------
        List<Func<int, Affect>> tmpListFunc = new List<Func<int, Affect>>()
        {
            StrongMasterAffect.GetTotalAffect,
            (int tmpTeamPos) =>
            {
                return BreakAffect.GetTotalAffect(activeData);
            },
            (int tmpTeamPos) =>
            {
                return PointStarAffect.GetTotalAffect();
            },
            StrongRefineAffect.GetEquipAffect,
            StrongRefineAffect.GetMagicAffect,
            SuitAffect.Instance.GetTotalAffect,
            Relationship.GetTotalAffect,
            (int tempTeamPos) =>
            {
                return FateAffect.GetTotalAffect(activeData);
            }
        };
        Affect tmpAffectRet = null;
        Affect tmpAffect = null;

        for (int i = 0, count = tmpListFunc.Count; i < count; i++)
        {
            tmpAffect = tmpListFunc[i](teamPos);

            if (tmpAffect != null)
            {
                if (tmpAffectRet != null)
                    tmpAffectRet.Append(tmpAffect);
                else
                    tmpAffectRet = tmpAffect;
            }
        }

        return tmpAffectRet;
        //end
    }

    public static float Final(Affect affect, AFFECT_TYPE type, float value)
    {
        if (affect != null)
        {
            value = affect.Final(type, value);
        }

        return value;
    }

    public static float Final(int teamPos, AFFECT_TYPE type, float value)
    {
        Affect affect = GetPetAllAffect(teamPos);
        return Final(affect, type, value);
    }
}

public static class StrongMasterAffect {

    static int GetMinLevel(List<EquipData> equipList,string levelName, bool bIsStrengthen = true) {
        //by chenliang
        //begin

//        return equipList.Select(equip => StrongMasterContainer.GetLevel(equip.strengthenLevel,levelName).field1).Min();
//-------------------
        //防止列表为空
        if (equipList == null || equipList.Count <= 0)
            return 0;
        IEnumerable<int> tmpSel = equipList.Select(equip => StrongMasterContainer.GetLevel(bIsStrengthen ? equip.strengthenLevel : equip.refineLevel, levelName).field1);
        if(tmpSel == null)
            return 0;
        return tmpSel.Min();

        //end
    }

    static Affect GetAffect(string colName,int level) {
        int affectIndex=TableManager.GetTable("StrengMaster").GetRecord(level).getData(colName);
        return AffectBuffer.GetAffect(affectIndex);
    }

    public static Affect GetTotalAffect(int teamPos) {
        Affect totalAffect = new Affect();

        List<EquipData> equipList=new List<EquipData>();
        for(int i=0;i<4;i++) {
            var equip=TeamManager.GetRoleEquipDataByTeamPos(teamPos,i);
            if(equip!=null) equipList.Add(equip);
        }

        List<EquipData> magicList=new List<EquipData>();
        for(int i=0;i<2;i++) {
            var magic=TeamManager.GetMagicDataByTeamPos(teamPos,i);
            if(magic!=null) magicList.Add(magic);
        }

        if (equipList.Count == (int)EQUIP_TYPE.MAX)
        {
            int equipStrongLevel = GetMinLevel(equipList, "EQUIP_STERNG_LEVEL");
            for (int i = 0; i < 4; i++) totalAffect.Append(GetAffect("EQUIP_STERNG_BUFF" + (i + 1), equipStrongLevel));

            int equipRefineLevel = GetMinLevel(equipList, "EQUIP_REFINE_LEVEL", false);
            for (int i = 0; i < 2; i++) totalAffect.Append(GetAffect("EQUIP_REFINE_BUFF" + (i + 1), equipRefineLevel));
        }

        if (magicList.Count == (int)MAGIC_TYPE.MAX)
        {
            int magicStrongLevel = GetMinLevel(magicList, "MAGIC_STERNG_LEVEL");
            for (int i = 0; i < 3; i++) totalAffect.Append(GetAffect("MAGIC_STERNG_BUFF" + (i + 1), magicStrongLevel));

            int magicRefineLevel = GetMinLevel(magicList, "MAGIC_REFINE_LEVEL", false);
            for (int i = 0; i < 2; i++) totalAffect.Append(GetAffect("MAGIC_REFINE_BUFF" + (i + 1), magicRefineLevel));
        }

        return totalAffect;
    }
}
public static class BreakAffect {
    public static Affect GetTotalAffect(ActiveData activeData) {
        Affect totalAffect=new Affect();

        
        //Func<int,string,float> getField=(tid,fieldName)=>(float)DataCenter.mActiveConfigTable.GetRecord(tid).getData(fieldName);
        //Func<string,float> getBreakValue=fieldName => {
        //    var breakAttr=getField(activeData.tid,"BREAK_"+fieldName);
        //    return breakAttr*activeData.breakLevel*(activeData.level-1);
        //};

        //totalAffect.Append(new Affect(AFFECT_TYPE.ATTACK,getBreakValue("ATTACK")));
        //totalAffect.Append(new Affect(AFFECT_TYPE.HP,getBreakValue("HP")));
        //totalAffect.Append(new Affect(AFFECT_TYPE.MP,getBreakValue("MP")));
        //totalAffect.Append(new Affect(AFFECT_TYPE.PHYSICAL_DEFENCE,getBreakValue("PHYSICAL_DEFENCE")));
        //totalAffect.Append(new Affect(AFFECT_TYPE.MAGIC_DEFENCE,getBreakValue("MAGIC_DEFENCE")));

        totalAffect.Append(GetAuraBreakAffect(activeData));
        
        return totalAffect;
    }

    public static Affect GetAuraBreakAffect(ActiveData selActiveData)
    {
        Affect totalAffect=new Affect();
        ActiveData[] team=TeamManager.mDicTeamPosData.Values.
            Select(teamPosData => TeamManager.GetActiveDataByTeamPos(teamPosData.teamPos))
            .Where(value => value != null && (value.teamPos >=(int)TEAM_POS.CHARACTER && value.teamPos <= (int)TEAM_POS.PET_3)).ToArray();
        team.Foreach(activeData => {
            for(int breakLevel=1;breakLevel<=activeData.breakLevel;breakLevel++) {
                var breakBuffId=(int)DataCenter.mBreakBuffConfig.GetRecord(activeData.tid).getData("BREAK_"+breakLevel);

                // 某些宠物某突破等级对应的天赋id可能为0,故需要判空
                var record = DataCenter.mAffectBuffer.GetRecord(breakBuffId);
                if(record != null)
                {
                    var affectRange = (EXTRA_BUFF_EFFECT)((int)record.getData("EXTRA_CONDITION"));
                    if (affectRange == EXTRA_BUFF_EFFECT.TEAM_BUFF) totalAffect.Append(AffectBuffer.GetAffect(breakBuffId));
                    else if (affectRange == EXTRA_BUFF_EFFECT.NONE && (selActiveData.teamPos == activeData.teamPos)) totalAffect.Append(AffectBuffer.GetAffect(breakBuffId));
                }                
            }
        });
        return totalAffect;
    }

    public static Affect GetAddBaseAffect(ActiveData activeData) {
        Affect totalAffect=new Affect();
        Func<int,string,float> getField=(tid,fieldName) => (float)DataCenter.mActiveConfigTable.GetRecord(tid).getData(fieldName);
        Func<string,float> getBreakBaseValue=fieldName => {
            var baseBreakAttr=getField(activeData.tid,"BASE_BREAK_"+fieldName);
            return baseBreakAttr*activeData.breakLevel;
        };

        totalAffect.Append(new Affect(AFFECT_TYPE.ATTACK,getBreakBaseValue("ATTACK")));
        totalAffect.Append(new Affect(AFFECT_TYPE.HP,getBreakBaseValue("HP")));
        totalAffect.Append(new Affect(AFFECT_TYPE.MP,getBreakBaseValue("MP")));
        totalAffect.Append(new Affect(AFFECT_TYPE.PHYSICAL_DEFENCE,getBreakBaseValue("PHYSICAL_DEFENCE")));
        totalAffect.Append(new Affect(AFFECT_TYPE.MAGIC_DEFENCE,getBreakBaseValue("MAGIC_DEFENCE")));

        return totalAffect;
    }
}
public static class PointStarAffect {
    public static Affect GetTotalAffect() {
        Affect totalAffect=new Affect();
        //by chenliang 
        //begin

//         PointStarLogicData.Self.GetAllPointStarData().Foreach(value => 
//             totalAffect.Append(new Affect((AFFECT_TYPE)value.mAffectType,(float)value.mValue)));
//--------------------
        if (PointStarLogicData.Self == null)
            return null;
        for (int i = (int)POINT_STAR_TYPE.ATTACK, lastI = (int)POINT_STAR_TYPE.HP; i <= lastI; i++)
        {
            POINT_STAR_TYPE tmpStarType = (POINT_STAR_TYPE)i;
            AFFECT_TYPE tmpAffectType = PointStarLogicData.Self.GetAttributeType(tmpStarType);
            float tmpAffectValue = PointStarLogicData.Self.GetAttributeValue(tmpStarType);
            totalAffect.Append(new Affect(tmpAffectType, tmpAffectValue));
        }

        //end
        return totalAffect;
    }
}
public static class StrongRefineAffect {
    public static Affect GetEquipAffect(int teamPos) {
        Affect totalAffect=new Affect();
        for(int i=0;i<=3;i++) {
            //by chenliang
            //begin

//            var equip=TeamManager.GetRoleEquipDataByTeamPos(teamPos,i);
//-------------------
            //应该区分装备、法器
            //EquipData equip = null;
            //if(i >= 0 && i <= 3)
            //    equip = TeamManager.GetRoleEquipDataByTeamPos(teamPos, i);
            //else
            //    equip = TeamManager.GetMagicDataByTeamPos(teamPos, i - 4);

            EquipData equip = TeamManager.GetRoleEquipDataByTeamPos(teamPos, i);

            //end
            if(equip!=null) {
                var record=DataCenter.mRoleEquipConfig.GetRecord(equip.tid);
                int type_0=record.getData("ATTRIBUTE_TYPE_0");
                if(type_0!=0) {
                    totalAffect.Append(new Affect((AFFECT_TYPE)type_0,
                        ((float)record.getData("BASE_ATTRIBUTE_0")+(float)record.getData("STRENGTHEN_0")*(equip.strengthenLevel -1))/10000));
                }
                int type_1=record.getData("ATTRIBUTE_TYPE_1");
                if(type_1!=0) {
                    totalAffect.Append(new Affect((AFFECT_TYPE)type_1,
                        ((float)record.getData("BASE_ATTRIBUTE_1")+(float)record.getData("STRENGTHEN_1")*(equip.strengthenLevel -1))/10000));
                }

                int refineType_0=record.getData("REFINE_TYPE_0");
                if(refineType_0!=0) {
                    totalAffect.Append(new Affect((AFFECT_TYPE)refineType_0,
                        (float)record.getData("REFINE_VALUE_0")*equip.refineLevel/10000));
                }

                int refineType_1=record.getData("REFINE_TYPE_1");
                if(refineType_1!=0) {
                    totalAffect.Append(new Affect((AFFECT_TYPE)refineType_1,
                        (float)record.getData("REFINE_VALUE_1")*equip.refineLevel/10000));
                }
            }
        }
        return totalAffect;
    }

    public static Affect GetMagicAffect(int teamPos)
    {
        Affect totalAffect = new Affect();
        for (int i = 4; i < 6; i++)
        {
            //by chenliang
            //begin

            //            var equip=TeamManager.GetRoleEquipDataByTeamPos(teamPos,i);
            //-------------------
            //应该区分装备、法器
            //EquipData equip = null;
            //if (i >= 0 && i <= 3)
            //    equip = TeamManager.GetRoleEquipDataByTeamPos(teamPos, i);
            //else
            //    equip = TeamManager.GetMagicDataByTeamPos(teamPos, i - 4);

            EquipData equip = TeamManager.GetMagicDataByTeamPos(teamPos, i - 4);

            //end
            if (equip != null)
            {
                var record = DataCenter.mRoleEquipConfig.GetRecord(equip.tid);
                int type_0 = record.getData("ATTRIBUTE_TYPE_0");
                if (type_0 != 0)
                {
                    totalAffect.Append(new Affect((AFFECT_TYPE)type_0,
                        ((float)record.getData("BASE_ATTRIBUTE_0") + (float)record.getData("STRENGTHEN_0") * (equip.strengthenLevel - 1)) / 10000));
                }
                int type_1 = record.getData("ATTRIBUTE_TYPE_1");
                if (type_1 != 0)
                {
                    totalAffect.Append(new Affect((AFFECT_TYPE)type_1,
                        ((float)record.getData("BASE_ATTRIBUTE_1") + (float)record.getData("STRENGTHEN_1") * (equip.strengthenLevel - 1)) / 10000));
                }

                int refineType_0 = record.getData("REFINE_TYPE_0");
                if (refineType_0 != 0)
                {
                    totalAffect.Append(new Affect((AFFECT_TYPE)refineType_0,
                        (float)record.getData("REFINE_VALUE_0") * equip.refineLevel / 10000));
                }

                int refineType_1 = record.getData("REFINE_TYPE_1");
                if (refineType_1 != 0)
                {
                    totalAffect.Append(new Affect((AFFECT_TYPE)refineType_1,
                        (float)record.getData("REFINE_VALUE_1") * equip.refineLevel / 10000));
                }
            }
        }
        return totalAffect;
    }
}
//by chenliang
//begin

public class SuitEquipInfo
{
    private int m_SuitID = -1;
    private string m_SuitName = "";
    private int m_SuitEquipCount = 0;

    public int SuitID
    {
        set { m_SuitID = value; }
        get { return m_SuitID; }
    }
    public string SuitName
    {
        set { m_SuitName = value; }
        get { return m_SuitName; }
    }
    public int SuitEquipCount
    {
        set { m_SuitEquipCount = value; }
        get { return m_SuitEquipCount; }
    }
}

/// <summary>
/// 套装
/// </summary>
public class SuitAffect
{
    private static SuitAffect ms_Instance;

    private Dictionary<int, DataRecord> m_DicSuit = new Dictionary<int, DataRecord>();

    public static SuitAffect Instance
    {
        get
        {
            if (ms_Instance == null)
                ms_Instance = new SuitAffect();
            return ms_Instance;
        }
    }

    public Affect GetTotalAffect(int teamPos)
    {
        Dictionary<int, SuitEquipInfo> tmpDicSuitEquip = __GetSuitEquipInfo(teamPos);
        List<int> tmpListSuitBuff = __GetSuitBuff(tmpDicSuitEquip);

        Affect tmpTotlAffect = new Affect();
        for (int i = 0, count = tmpListSuitBuff.Count; i < count; i++)
        {
            Affect tmpBuffAffect = AffectBuffer.GetAffect(tmpListSuitBuff[i]);
            if (tmpBuffAffect == null)
                continue;
            tmpTotlAffect.Append(tmpBuffAffect);
        }

        return tmpTotlAffect;
    }

    /// <summary>
    /// 获取套装装备信息
    /// </summary>
    /// <param name="teamPos"></param>
    /// <returns></returns>
    private Dictionary<int, SuitEquipInfo> __GetSuitEquipInfo(int teamPos)
    {
        Dictionary<int, SuitEquipInfo> tmpDicSuitEquip = new Dictionary<int, SuitEquipInfo>();
        for (int i = 0; i < 4; i++)
        {
            EquipData tmpEquip = TeamManager.GetRoleEquipDataByTeamPos(teamPos, i);
            if (tmpEquip != null)
            {
                //找到装备
                DataRecord tmpSuitRecord;
                if (!DataCenter.mDicSuitEquipTid.TryGetValue(tmpEquip.tid, out tmpSuitRecord))
                {
                    DEBUG.LogError("Equip " + tmpEquip.tid + " doesn't exist in DicSuitEquip");
                    continue;
                }
                //找到套装记录
                int tmpSuitTid = (int)tmpSuitRecord.getObject("INDEX");
                SuitEquipInfo tmpSuitEquipInfo;
                if (!tmpDicSuitEquip.TryGetValue(tmpSuitTid, out tmpSuitEquipInfo))
                {
                    //新建一个套装装备信息
                    tmpSuitEquipInfo = new SuitEquipInfo()
                    {
                        SuitID = tmpSuitTid,
                        SuitName = tmpSuitRecord.get("SET_NAME"),
                        SuitEquipCount = 0
                    };
                    tmpDicSuitEquip[tmpSuitTid] = tmpSuitEquipInfo;
                }
                tmpSuitEquipInfo.SuitEquipCount += 1;
            }
        }
        return tmpDicSuitEquip;
    }

    /// <summary>
    /// 获取套装Buff
    /// </summary>
    /// <param name="dicSuitEquip"></param>
    /// <returns></returns>
    private List<int> __GetSuitBuff(Dictionary<int, SuitEquipInfo> dicSuitEquip)
    {
        if (dicSuitEquip == null)
            return null;

        List<int> tmpListBuff = new List<int>();

        foreach (KeyValuePair<int, SuitEquipInfo> tmpPair in dicSuitEquip)
        {
            DataRecord tmpSuitRecord = DataCenter.mSetEquipConfig.GetRecord(tmpPair.Key);
            int tmpEquipCount = tmpPair.Value.SuitEquipCount;
            if (tmpEquipCount <= 1)
                continue;
            for (int j = 2; j <= tmpEquipCount; j++)
            {
                for (int i = 0, count = 3; i < count; i++)
                {
                    object tmpObjSuitBuffID = tmpSuitRecord.getObject("SET" + j + "_BUFF_ID_" + (i + 1));
                    if (tmpObjSuitBuffID == null || tmpObjSuitBuffID.GetType() == typeof(bool) || (Convert.ToInt32(tmpObjSuitBuffID) == 0))
                        continue;
                    int tmpSuitBuffID = (int)tmpObjSuitBuffID;
                    tmpListBuff.Add(tmpSuitBuffID);
                }
            }
        }

        return tmpListBuff;
    }
}

//end

// 天命
public static class FateAffect
{
    public static Affect GetTotalAffect(ActiveData activeData)
    {
        Affect totalAffect = new Affect();
        if(activeData == null)
            return totalAffect;

        for (int i = 0; i < 4; ++i)
        {
            AFFECT_TYPE affectType = BreakInfoWindow.GetAttributeType((BASE_ATTRITUBE_TYPE)(i + 1));
            affectType = GameCommon.ToAffectTypeEnum(GameCommon.ToAffectTypeString(affectType) + "_RATE");
            Affect affect = new Affect(affectType, TableCommon.GetNumberFromFateConfig(activeData.fateLevel, "TOTAL_ADD_RATE")/100.0f);
            totalAffect.Append(affect);
        }
        return totalAffect;
    }
}





