using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DataTable;

public enum RelateAlteration
{
    KeepActive,
    KeepInactive,
    Active2Inactive,
    Inactive2Active,
}


public class Relationship
{
    public int tid { get; private set; }        // 缘分tid
    public bool active { get; private set; }    // 缘分是否激活

    // 委托，当缘分可能发生变化时（即TeamPos状态改变时）调用，主要用于刷新UI
    // 第一个参数表示改变的TeamPos
    // 第二个参数表示增加的激活缘分ID集合
    // 第三个参数表示减少的激活缘分ID集合
    public static DelegateList<int, HashSet<int>, HashSet<int>> onRelationChanged = new DelegateList<int, HashSet<int>, HashSet<int>>();

    // 初始化，刷新当前全部缘分并注册委托以监听TeamPos的变动
    public static void Init()
    {
        onRelationChanged.Clear();
        RefreshAll();
    }

    private static void RefreshAll()
    {
        teamRelationship.Clear();

        for (int i = 0; i <= 3; ++i)
        {
            Refresh(i);
        }

        for (int i = 11; i <= 16; ++i)
        {
            Refresh(i);
        }
    }

	//具体tid是否已激活
	public static bool IsActiveByTid(int tid, int teamPos)
	{
		bool ret = false;
		if (teamContextSet.ContainsKey (teamPos)) 
		{
			var contextSet = teamContextSet [teamPos];
			ret =  contextSet.Contains (tid);
		}
		return ret;
	}

    private static void Refresh(int teamPos)
    {
        var contextSet = GetContextTidSet(teamPos);

        if (teamContextSet.ContainsKey(teamPos))
            teamContextSet[teamPos] = contextSet;
        else
            teamContextSet.Add(teamPos, contextSet);

        ActiveData activeData = TeamManager.GetActiveDataByTeamPos(teamPos);

        if (activeData == null)
        {
            HashSet<int> old = GetCachedActiveRelateTidSet(teamPos);
            teamRelationship.Remove(teamPos);

            if (onRelationChanged != null)
                onRelationChanged.Invoke(teamPos, new HashSet<int>(), old);

            return;
        }

        List<Relationship> result = new List<Relationship>();
        DataRecord record = DataCenter.mActiveConfigTable.GetRecord(activeData.tid);

        for (int i = 1; i <= 15; ++i)
        {
            string fieldName = "RELATE_ID_" + i.ToString();
            int relTid = record[fieldName];

            if (relTid > 0)
            {
                Relationship rel = new Relationship();
                rel.tid = relTid;
                rel.active = teamPos <= 3 ? IsRelateActive(contextSet, relTid) : false;
                result.Add(rel);
            }
        }

        HashSet<int> before = GetCachedActiveRelateTidSet(teamPos);

        if (teamRelationship.ContainsKey(teamPos))
            teamRelationship[teamPos] = result;
        else
            teamRelationship.Add(teamPos, result);

        HashSet<int> after = GetCachedActiveRelateTidSet(teamPos);
        HashSet<int> added = new HashSet<int>(after);
        added.ExceptWith(before);
        HashSet<int> removed = new HashSet<int>(before);
        removed.ExceptWith(after);

        if (onRelationChanged != null)
        {
            onRelationChanged.Invoke(teamPos, added, removed);
        }
    }

    public static Affect GetTotalAffect(int teamPos)
    {
        var total = new Affect();
        var activeRelateSet = GetCachedActiveRelateTidSet(teamPos);

        using (var iter = activeRelateSet.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                int buff = DataCenter.mRelateConfig.GetData(iter.Current, "BUFF_ID");

                if (buff > 0)
                    total.Append(AffectBuffer.GetAffect(buff));
            }
        }

        return total;
    }

    public static bool mShowAllRel = false; //> 是否显示所有的缘分
    public static TeamPosChangeData mTeamPosChangeData = null;

    public static void OnTeamChanged(TeamPosChangeData teamPosChangeData)
    {
        mShowAllRel = ITEM_TYPE.PET == PackageManager.GetItemTypeByTableID(teamPosChangeData.upTid);
        mTeamPosChangeData = teamPosChangeData;
        int teamPos = teamPosChangeData.teamPos;
#region origin
        //if (teamPos >= 0 && teamPos <= 3)
        //{
        //    Refresh(teamPos);
        //}
        //else if (teamPos >= 11 && teamPos <= 16)
        //{
        //    for (int i = 0; i <= 3; ++i)
        //    {
        //        Refresh(i);
        //    }

        //    Refresh(teamPos);
        //}
#endregion

#region added by xuke 除了缘分站位中的符灵，队伍中的符灵上阵也要计算缘分效果
        if ((0 <= teamPos && teamPos <= 3) || (11 <= teamPos && teamPos <= 16)) 
        {
            for (int i = 0; i <= 3; ++i) 
            {
                Refresh(i);
            }
        }
        if ((11 <= teamPos && teamPos <= 16)) 
        {
            Refresh(teamPos);
        }

        //added by xuke 红点相关
        TeamNewMarkManager.Self.CheckPet();
        TeamNewMarkManager.Self.RefreshTeamNewMark();
        TeamNewMarkManager.Self.RefreshRelPosNewMark();
        //end
#endregion
    }

    private static Dictionary<int, List<Relationship>> teamRelationship = new Dictionary<int, List<Relationship>>();
    private static Dictionary<int, HashSet<int>> teamContextSet = new Dictionary<int, HashSet<int>>();


    /// <summary>
    /// 获取在指定阵位处缓存的缘分上下文集合
    /// </summary>
    /// <param name="teamPos"> 阵位 </param>
    /// <returns> 缓存的缘分上下文集合 </returns>
    public static HashSet<int> GetCachedContextTidSet(int teamPos)
    {
        HashSet<int> result = null;
        
        if (!teamContextSet.TryGetValue(teamPos, out result))
        {
            result = GetContextTidSet(teamPos);
        }

        return result;
    }

    /// <summary>
    /// 获取在指定阵位处缓存的缘分列表
    /// </summary>
    /// <param name="teamPos"> 阵位 </param>
    /// <returns> 缘分列表 </returns>
    public static List<Relationship> GetCachedRelationshipList(int teamPos)
    {
        List<Relationship> result;

        if (teamRelationship.TryGetValue(teamPos, out result))
        {
            return result;
        }
        
        return new List<Relationship>();
    }

    /// <summary>
    /// 获取在指定阵位处缓存的缘分Tid集合
    /// </summary>
    /// <param name="teamPos"> 阵位 </param>
    /// <returns> 缓存的缘分Tid集合 </returns>
    public static HashSet<int> GetCachedRelateTidSet(int teamPos)
    {
        HashSet<int> result = new HashSet<int>();
        List<Relationship> rels = GetCachedRelationshipList(teamPos);

        if (rels != null)
        {
            for (int i = 0; i < rels.Count; ++i)
            {
                result.Add(rels[i].tid);
            }
        }

        return result;
    }

    /// <summary>
    /// 获取在指定阵位处缓存的已激活缘分的数量
    /// </summary>
    /// <param name="teamPos"> 阵位 </param>
    /// <returns> 缓存的已激活缘分数量 </returns>
    public static int GetCachedActiveRelateCount(int teamPos)
    {
        List<Relationship> result;

        if (teamRelationship.TryGetValue(teamPos, out result))
        {
            return result.Count(x => x.active);
        }

        return 0;
    }

    /// <summary>
    /// 获取在指定阵位处缓存的已激活缘分Tid集合
    /// </summary>
    /// <param name="teamPos"> 阵位 </param>
    /// <returns> 缓存的已激活缘分Tid集合 </returns>
    public static HashSet<int> GetCachedActiveRelateTidSet(int teamPos)
    {
        HashSet<int> result = new HashSet<int>();
        List<Relationship> rels = GetCachedRelationshipList(teamPos);

        if (rels != null)
        {
            for (int i = 0; i < rels.Count; ++i)
            {
                if (rels[i].active)
                {
                    result.Add(rels[i].tid);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 获取指定符灵的所有缘分tid集合
    /// </summary>
    /// <param name="activeRecord"> 符灵配置 </param>
    /// <returns> 缘分tid集合 </returns>
    public static HashSet<int> GetRelateTidSet(DataRecord activeRecord)
    {
        HashSet<int> set = new HashSet<int>();

        if (activeRecord == null)
            return set;

        for (int relSuffix = 1; relSuffix <= 15; relSuffix++)
        {
            int relIndex = activeRecord.getData("RELATE_ID_" + relSuffix);

            if (relIndex > 0)
                set.Add(relIndex);
        }

        return set;
    }

    /// <summary>
    /// 获取指定符灵的所有缘分tid集合
    /// </summary>
    /// <param name="activeTid"> 符灵tid </param>
    /// <returns> 缘分tid集合 </returns>
    public static HashSet<int> GetRelateTidSet(int activeTid)
    {
        DataRecord activeRecord = DataCenter.mActiveConfigTable.GetRecord(activeTid);
        return GetRelateTidSet(activeRecord);
    }

    /// <summary>
    /// 获取指定缘分的激活条件的tid集合
    /// </summary>
    /// <param name="relateIndex"> 缘分配置 </param>
    /// <returns> 激活条件的tid集合 </returns>
    public static HashSet<int> GetConditionTidSet(DataRecord relateRecord)
    {
        HashSet<int> condition = new HashSet<int>();

        if (relateRecord == null)
            return condition;

        for (int contentSuffix = 1; contentSuffix <= 6; contentSuffix++)
        {
            int conditionTid = relateRecord.getData("NEED_CONTENT_" + contentSuffix);

            if (conditionTid > 0)
                condition.Add(conditionTid);
        }

        return condition;
    }

    /// <summary>
    /// 获取指定缘分的激活条件的tid集合
    /// </summary>
    /// <param name="relateIndex"> 缘分tid </param>
    /// <returns> 激活条件的tid集合 </returns>
    public static HashSet<int> GetConditionTidSet(int relateIndex)
    {
        DataRecord relRecord = DataCenter.mRelateConfig.GetRecord(relateIndex);
        return GetConditionTidSet(relRecord);
    }

    /// <summary>
    /// 获取指定阵位上所有参与缘分计算的tid的集合，包括所有阵位上的符灵（包括缘分助战符灵）和指定阵位上的所有装备法器的tid
    /// </summary>
    /// <param name="teamPos"> 阵位 </param>
    /// <returns> 指定阵位上所有参与缘分计算的tid的集合</returns>
    public static HashSet<int> GetContextTidSet(int teamPos)
    {
        HashSet<int> context = new HashSet<int>();

        for (int i = 0; i < (int)TEAM_POS.MAX; ++i)
        {
            var data = TeamManager.GetActiveDataByTeamPos(i);

            if (data != null && data.tid > 0)
            {
                context.Add(data.tid);
            }
        }

        for (int i = (int)TEAM_POS.RELATE_1; i <= (int)TEAM_POS.RELATE_6; ++i)
        {
            var data = TeamManager.GetActiveDataByTeamPos(i);

            if (data != null && data.tid > 0)
            {
                context.Add(data.tid);
            }
        }

        var equipDatas = RoleEquipLogicData.Self.GetEquipDatasByTeamPos(teamPos);

        for (int i = 0; i < equipDatas.Length; ++i)
        {
            if (equipDatas[i].tid > 0)
            {
                context.Add(equipDatas[i].tid);
            }
        }

        var magicDatas = MagicLogicData.Self.GetEquipDatasByTeamPos(teamPos);

        for (int i = 0; i < magicDatas.Length; ++i)
        {
            if (magicDatas[i].tid > 0)
            {
                context.Add(magicDatas[i].tid);
            }
        }

        return context;
    }


    /// <summary>
    /// 修改缘分上下文集合
    /// </summary>
    /// <param name="contextTidSet"> 上下文集合 </param>
    /// <param name="addTid"> 上阵的Tid </param>
    /// <param name="removeTid"> 下阵的Tid </param>
    /// <returns> 修改后的集合 </returns>
    public static HashSet<int> AlterContextTidSet(IEnumerable<int> contextTidSet, int addTid, int removeTid)
    {
        HashSet<int> context = new HashSet<int>(contextTidSet);

        if(removeTid > 0)
            context.Remove(removeTid);

        if(addTid > 0)
            context.Add(addTid);

        return context;
    }

    /// <summary>
    /// 指定缘分是否已激活
    /// </summary>
    /// <param name="contextTidSet"> 上下文环境 </param>
    /// <param name="relateTid"> 缘分ID </param>
    /// <returns> 缘分是否已激活 </returns>
    public static bool IsRelateActive(IEnumerable<int> contextTidSet, int relateTid)
    {
        var conditionSet = GetConditionTidSet(relateTid);
        return conditionSet.IsSubsetOf(contextTidSet);
    }

    /// <summary>
    /// 按照指定状态过滤缘分集合
    /// </summary>
    /// <param name="contextTidSet"> 上下文环境 </param>
    /// <param name="relateTidSet"> 指定缘分tid集合 </param>
    /// <param name="active"> 指定状态 </param>
    /// <returns> 过滤后的缘分集合 </returns>
    public static HashSet<int> FilterRelateByStatus(IEnumerable<int> contextTidSet, IEnumerable<int> relateTidSet, bool active)
    {
        HashSet<int> resultSet = new HashSet<int>();

        using (var iter = relateTidSet.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                if (IsRelateActive(contextTidSet, iter.Current) == active)
                {
                    resultSet.Add(iter.Current);
                }
            }
        }

        return resultSet;
    }

    /// <summary>
    /// 计算指定缘分在旧的上下文环境和新的上下文环境中的状态变动
    /// </summary>
    /// <param name="oldContextTidSet"> 旧的上下文环境 </param>
    /// <param name="newContextTidSet"> 新的上下文环境 </param>
    /// <param name="relateTid"> 指定缘分tid </param>
    /// <returns> 缘分状态变动 </returns>
    public static RelateAlteration GetRelateAlteration(IEnumerable<int> oldContextTidSet, IEnumerable<int> newContextTidSet, int relateTid)
    {
        var conditionSet = GetConditionTidSet(relateTid);
        bool isOldActive = conditionSet.IsSubsetOf(oldContextTidSet);
        bool isNewActive = conditionSet.IsSubsetOf(newContextTidSet);

        if (isOldActive)
            return isNewActive ? RelateAlteration.KeepActive : RelateAlteration.Active2Inactive;
        else 
            return isNewActive ? RelateAlteration.Inactive2Active : RelateAlteration.KeepInactive;
    }

    /// <summary>
    /// 在变动环境中按照指定状态变化过滤缘分集合
    /// </summary>
    /// <param name="oldContextTidSet"> 旧的上下文环境 </param>
    /// <param name="newContextTidSet"> 新的上下文环境 </param>
    /// <param name="relateTidSet"> 指定缘分tid集合 </param>
    /// <param name="relateAlteration"> 指定状态变化 </param>
    /// <returns> 过滤后的缘分集合 </returns>
    public static HashSet<int> FilterRelateByAlteration(IEnumerable<int> oldContextTidSet, IEnumerable<int> newContextTidSet, IEnumerable<int> relateTidSet, RelateAlteration relateAlteration)
    {
        HashSet<int> resultSet = new HashSet<int>();

        using (var iter = relateTidSet.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                if (GetRelateAlteration(oldContextTidSet, newContextTidSet, iter.Current) == relateAlteration)
                {
                    resultSet.Add(iter.Current);
                }
            }
        }

        return resultSet;
    }
}