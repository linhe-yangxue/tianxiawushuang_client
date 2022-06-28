using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum POINT_STAR_TYPE
{
    NONE,
	ATTACK,             // 攻击
    PHYSICAL_DEFENCE,   // 物防
  
	MAGIC_DEFENCE,      // 法防
    HP,                 // 生命
    STAR_LEVEL_UP,      // 升星
    GAIN_ITEM,          // 获得物品
}

/// <summary>
/// 点星数据
/// </summary>
public class PointStarData
{
    public int mIndex = 0;
    public int mPointStarType = (int)POINT_STAR_TYPE.NONE;
    public int mAffectType = (int)AFFECT_TYPE.NONE;
    public int mValue;
}

/// <summary>
/// 点星逻辑
/// </summary>
public class PointStarLogicData : tLogicData
{
    static public PointStarLogicData Self;
    public int mCurIndex; // 即将激活索引
    private Dictionary<int, PointStarData> mDicActivePointStar;
    private int[] mActiveAttributeArray = new int[4];

    public PointStarLogicData()
    {
        Self = this;

        mDicActivePointStar = new Dictionary<int, PointStarData>();
    }

    public void Init(int iIndex)
    {
        mCurIndex = iIndex;
        int iTotalCount = mCurIndex;
        mDicActivePointStar.Clear();
        
        for(int m = 0; m < mActiveAttributeArray.Length; m++)
        {
            mActiveAttributeArray[m] = 0;
        }

        for (int i = 1; i < iTotalCount; i++)
        {
            AddItem(i);
        }
    }

    public IEnumerable<PointStarData> GetAllPointStarData() {
        return mDicActivePointStar.Values as IEnumerable<PointStarData>;
    }
    /// <summary>
    /// 激活点星
    /// </summary>
    /// <param name="iIndex">被激活的点星索引</param>
    /// <returns></returns>
    public bool AddItem(int iIndex)
    {
        if (mDicActivePointStar.Count < iIndex)
        {
            // TODO:暂时先不缓存数据
            //PointStarData pointStarData = new PointStarData();
            //pointStarData.mIndex = iIndex;
            //pointStarData.mPointStarType = TableCommon.GetNumberFromPointStarConfig(iIndex, "REWARD_TYPE");
            //pointStarData.mAffectType = TableCommon.GetNumberFromPointStarConfig(iIndex, "AFFECT_TYPE");
            //pointStarData.mValue = TableCommon.GetNumberFromPointStarConfig(iIndex, "REWARD_NUMERICAL");
            //mDicActivePointStar.Add(iIndex, pointStarData);

            int pointStarType = TableCommon.GetNumberFromPointStarConfig(iIndex, "REWARD_TYPE");
            string value = TableCommon.GetStringFromPointStarConfig(iIndex, "REWARD_NUMERICAL");

            if (pointStarType >= (int)POINT_STAR_TYPE.ATTACK && pointStarType <= (int)POINT_STAR_TYPE.HP)
            {
                mActiveAttributeArray[pointStarType - 1] += System.Convert.ToInt32(value);
            }

            mCurIndex = iIndex + 1;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获得点星某属性总值
    /// </summary>
    /// <param name="type">点星类型</param>
    /// <returns></returns>
    public int GetAttributeValue(POINT_STAR_TYPE type)
    {
        if (type >= POINT_STAR_TYPE.ATTACK && type <= POINT_STAR_TYPE.HP)
        {
            return mActiveAttributeArray[(int)type - 1];
        }
        return 0;
    }

    /// <summary>
    /// 获得点星的属性类型
    /// </summary>
    /// <param name="type">点星类型</param>
    /// <returns></returns>
    public AFFECT_TYPE GetAttributeType(POINT_STAR_TYPE type)
    {
        if (type >= POINT_STAR_TYPE.ATTACK && type <= POINT_STAR_TYPE.HP)
        {
            string name = type.ToString();
            return GameCommon.GetEnumFromString<AFFECT_TYPE>(name, AFFECT_TYPE.NONE);
        }
        return AFFECT_TYPE.NONE;
    }

}
