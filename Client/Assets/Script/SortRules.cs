using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SortPetDataByRelation<T> : IComparer<T>
{
    Dictionary<int, int> mActivatedRelations = new Dictionary<int, int>();
	private int mTeamPos = 0;

	public SortPetDataByRelation(int teamPos)
	{
		mTeamPos = teamPos;
	}

    public int Compare(T left, T right)
    {
        PetData a = left as PetData;
        PetData b = right as PetData;
        if (a == null || b == null) return 0;

        bool bIsRelationDescending = DataCenter.Get("DESCENDING_ORDER_RELATION");

        // 按可激活缘分数量排序
        int iACount = GetActivatedRelation(a.tid);
        int iBCount = GetActivatedRelation(b.tid);

        if (iACount != iBCount)
        {
            return iBCount - iACount;
        }
        else
        {
            // 已上阵宠物在前，助战宠物其次，非上阵宠物在后
            if ((int)GameCommon.GetTeamPosType(a) > (int)GameCommon.GetTeamPosType(b))
                return -1;
            else if ((int)GameCommon.GetTeamPosType(a) < (int)GameCommon.GetTeamPosType(b))
                return 1;
            else
            {
                // 经验蛋放在后
                if (!GameCommon.IsExpPet(a) && GameCommon.IsExpPet(b))
                    return -1;
                else if (GameCommon.IsExpPet(a) && !GameCommon.IsExpPet(b))
                    return 1;
                else
                {
                    // 把品质低的宠物放在最前面
                    if (a.starLevel != b.starLevel)
                        return (a.starLevel - b.starLevel) * (bIsRelationDescending ? -1 : 1);
                    else
                    {
                        // 等级高的在前
                        if (a.level != b.level)
                            return b.level - a.level;
                        else
                        {
                            // 突破等级高的在前
                            if (a.breakLevel != b.breakLevel)
                                return b.breakLevel - a.breakLevel;
                            else
                            {
                                // 按tid排序
                                return b.tid - a.tid;
                            }
                        }
                    }
                }
            }
        }
    }

    private int GetActivatedRelation(int tid)
    {
        int count = 0;
        if(mActivatedRelations.ContainsKey(tid))
            return mActivatedRelations[tid];
        count = BagPetWindow.GetRelateNewActivateCount(tid, mTeamPos);
        mActivatedRelations.Add(tid, count);
        return count;
    }
}