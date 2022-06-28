using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;


public class ManagerSingleton<T> where T : new() 
{
    private static T self = new T();

    public static T Self
    {
        get { return self; }
    }
} 

/// <summary>
/// 历练红点管理
/// </summary>
public class TrialNewMarkManager : ManagerSingleton<TrialNewMarkManager>
{
    #region 检测项
    public bool CheckTrial { set; get; }
    #endregion
    
    #region 可见性
    public bool TrialVisible 
    { 
        get 
        {
            if (PVPNewMarkManager.Self == null || GrabTreasureNewMarkManager.Self == null || RammbockNewMarkManager.Self == null) 
            {
                return false;
            }
            return PVPNewMarkManager.Self.PVPVisible || GrabTreasureNewMarkManager.Self.GrabTreasureVisible || RammbockNewMarkManager.Self.RammbockVisible;
        }
    }
    #endregion

    #region 检测逻辑
    /// <summary>
    /// 检测历练是否需要显示红点
    /// </summary>
    /// <returns></returns>
    public void CheckTrial_NewMark() 
    {
        RefreshTrialBtnNewMark();
    }
    /// <summary>
    /// 检测历练中所有的红点项
    /// </summary>
    public void CheckTrial_NewMarkAll() 
    {
        //1.检测夺宝(本地)
        GrabTreasureNewMarkManager.Self.CheckComposeTid_NewMark();
        //2.检测竞技场和封灵塔(请求服务器)
        GlobalModule.DoCoroutine(__RequestTrialNewMarkAllInfo());       
    }

    private IEnumerator __RequestTrialNewMarkAllInfo() 
    {
        //1.请求巅峰挑战信息
        yield return new PrestigeShopInfoRequester().Start();
        //2.请求封灵塔信息
        yield return new RammbockShopInfoRequester().Start();

        RefreshTrialNewMark();
        //by chenliang
        //begin

        //刷新历练快捷入口
        DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);

        //end
    }
    #endregion

    #region 刷新逻辑
    public void RefreshTrialBtnNewMark() 
    {
        DataCenter.SetData("ROLE_SEL_BOTTOM_RIGHT_GROUP", "REFRESH_NEWMARK", null);
    }

    public void RefreshTrialNewMark() 
    {
        tWindow _tWin = DataCenter.GetData("TRIAL_WINDOW") as tWindow;
        if (_tWin == null)
            return;
        DataCenter.SetData("TRIAL_WINDOW","REFRESH_NEWMARK",null);
    }
    #endregion
}

#region 同步请求
/// <summary>
/// 请求巅峰挑战
/// </summary>
public class PrestigeShopInfoRequester : NetRequester<CS_RequestPrestigeShopQuery, SC_ResponsePrestigeShopQuery> 
{
    protected override CS_RequestPrestigeShopQuery GetRequest()
    {
        return new CS_RequestPrestigeShopQuery() { pt = "CS_PrestigeShopQuery" };
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();
        PVPNewMarkManager.Self.CheckReward_NewMark(respMsg);
    }
}

public class RammbockShopInfoRequester : NetRequester<CS_RequestClothShopQuery, SC_ResponseClothShopQuery>
{
    protected override CS_RequestClothShopQuery GetRequest()
    {
        return new CS_RequestClothShopQuery() { pt = "CS_ClothShopQuery" };
    }
    protected override void OnSuccess()
    {
        base.OnSuccess();
        RammbockNewMarkManager.Self.CheckReward_NewMark(respMsg);
    }
}

#endregion

/// <summary>
/// 夺宝红点管理
/// </summary>
public class GrabTreasureNewMarkManager : ManagerSingleton<GrabTreasureNewMarkManager>
{
    #region 检测项
    private const int grabtreasure_func_index = 1403;
    private int mOpenLevel = int.MinValue;

    /// <summary>
    /// 判断是否达到开放等级
    /// </summary>
    public bool CheckOpenLevel 
    {
        get 
        {
            return GameCommon.IsFuncCanUse(grabtreasure_func_index);
        }
    }
    /// <summary>
    /// 检测是否有可以合成的法器
    /// </summary>
    public bool CheckCanCompose { set; get; }
    #endregion

    #region 可见性
    public bool GrabTreasureVisible { get { return CheckOpenLevel && CheckCanCompose; } }
    #endregion

    #region 检测逻辑
    /// <summary>
    /// 检测所有可以合成的法器Tid
    /// </summary>
    /// <returns></returns>
    public bool CheckComposeTid_NewMark()
    {
        if (MagicFragmentLogicData.Self == null)
            return false;
        List<int> _magicTidList = new List<int>();
        //1.得到所有有机会合成的法器
        foreach (var pair in MagicFragmentLogicData.Self.mDicEquipFragmentData) 
        {
            DataRecord _record = DataCenter.mFragmentAdminConfig.GetRecord(pair.Value.tid);
            if (_record == null)
                continue;
            int _magicTid = _record.getData("ROLEEQUIPID");
            DataRecord _magicRecord = DataCenter.mRoleEquipConfig.GetRecord(_magicTid);
            if (_magicRecord == null)
                continue;
            if (!_magicTidList.Contains(_magicTid)) 
            {
                _magicTidList.Add(_magicTid);            
            }
        }
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mEquipComposeConfig.GetAllRecord()) 
        {
            DataRecord tmpEquipConfig = pair.Value;
            int tmpShow = (int)tmpEquipConfig.getObject("BASE_SHOW");
            if (tmpShow == 0)
                continue;
            string tmpIndex = tmpEquipConfig.getData("INDEX");
            DataRecord tmpRoleConfig = DataCenter.mRoleEquipConfig.GetRecord(tmpIndex);
            if (tmpRoleConfig == null)
                continue;
            if(!_magicTidList.Contains(pair.Key))
            {
                _magicTidList.Add(pair.Key);            
            }          
        }
        //2.判断碎片是否能够合成
        for (int i = 0, count = _magicTidList.Count; i < count; i++) 
        {
            if (GrabTreasureWindow.CanComposeByTid(_magicTidList[i])) 
            {
                CheckCanCompose = true;
                return true;
            }
        }

        CheckCanCompose = false;
        return false;
    }
    #endregion 
}

/// <summary>
/// 封灵塔红点管理
/// </summary>
public class RammbockNewMarkManager : ManagerSingleton<RammbockNewMarkManager>
{
    private const int rammbock_func_index = 1402;
    public const int rewardIndex = 4;
    private int m_nOpenLevel = int.MinValue;

    #region 检测项
    /// <summary>
    /// 功能是否开启
    /// </summary>
    public bool CheckOpenLevel 
    {
        get 
        {
            return GameCommon.IsFuncCanUse(rammbock_func_index);
        } 
    }
    /// <summary>
    /// 检测灵核商店奖励页签红点是否可见
    /// </summary>
    public bool CheckReward { set; get; }
    #endregion

    #region 可见性
    /// <summary>
    /// 灵核商店奖励页签红点可见性
    /// </summary>
    public bool RewardVisible { get { return CheckOpenLevel && CheckReward; } }
    /// <summary>
    /// 灵核商店按钮红点可见性
    /// </summary>
    public bool RammbockShopBtnVisible { get { return RewardVisible; } }
    /// <summary>
    /// 封灵塔按钮红点可见性
    /// </summary>
    public bool RammbockVisible { get { return RammbockShopBtnVisible; } }
    #endregion

    #region 检测逻辑
    /// <summary>
    /// 检测灵核商店红点
    /// </summary>
    /// <returns></returns>
    public bool CheckReward_NewMark(SC_ResponseClothShopQuery rcsq) 
    {
        if (rcsq == null)
            return false;

        int _maxTabCount = 0;
        List<RammbockShopData> tabDataList = TableCommon.GetRammbockInfo(rewardIndex, out _maxTabCount);
        List<ClothStruct> csList = rcsq.cloth;
        foreach (var rewardData in tabDataList) 
        {
            //1.判断等级是否满足
            if (RoleLogicData.Self.character.level < rewardData.itemShowLevel)
                continue;
            //2.判断VIP等级是否满足
            if (RoleLogicData.Self.vipLevel < rewardData.vipLevelDisplay)
                continue;
            //3.星星限制
            if (rcsq.star < rewardData.openStarNum)
                continue;
            int buyNum = rewardData.buyNum;
            for (int i = 0, count = csList.Count; i < count; i++) 
            {
                if (rewardData.index == csList[i].index) 
                {
                    buyNum -= csList[i].buyNum;
                }
            }
            if (buyNum > 0) 
            {
                CheckReward = true;
                return true;
            }
        }
        CheckReward = false;
        return false;
    }

    public void RequestTowerShopInfoAndCheckReward(System.Action kRefershCallback) 
    {
        RammbockShopNet.RequestClothShopInfo(
        (text) =>
        {
            CheckReward_NewMark(JCode.Decode<SC_ResponseClothShopQuery>(text));
            if (kRefershCallback != null) 
            {
                kRefershCallback();
            }
        }
        ,
        (text) => 
        {
        
        });
        
    }
    #endregion

    #region 刷新逻辑
    public void RefreshRammbockNewMark() 
    {
        DataCenter.SetData("SHOP_RENOWN_WINDOW", "REFRESH_NEWMARK", null);
        DataCenter.SetData("RAMMBOCK_WINDOW", "REFRESH_NEWMARK", null);
    }


    #endregion
}

/// <summary>
/// 巅峰挑战红点管理
/// </summary>
public class PVPNewMarkManager : ManagerSingleton<PVPNewMarkManager>
{
    private const int pvp_func_index = 1405;    //> 巅峰挑战开放等级索引
    public const int rewardTabIndex = 2;       //> 奖励页签索引
    private int m_nOpenLevel = int.MinValue;

    #region 检测项
    /// <summary>
    /// 功能是否开启
    /// </summary>
    public bool CheckOpenLevel 
    {
        get 
        {
            return GameCommon.IsFuncCanUse(pvp_func_index);
        } 
    }
    /// <summary>
    /// 声望商店是否有奖励
    /// </summary>
    public bool CheckReward { set; get; }
    #endregion

    #region 可见性
    /// <summary>
    /// 奖励标签红点是否可见
    /// </summary>
    public bool RewardVisible { get { return CheckOpenLevel && CheckReward; } }
    /// <summary>
    /// 声望商店按钮红点是否可见
    /// </summary>
    public bool PrestigeShopBtnVisible { get { return RewardVisible; } }
    /// <summary>
    /// 巅峰挑战红点可见性
    /// </summary>
    public bool PVPVisible { get { return PrestigeShopBtnVisible; } }
    #endregion

    #region 检测方法
    /// <summary>
    /// 检测声望商店奖励页签红点是否可见
    /// </summary>
    /// <returns></returns>
    public bool CheckReward_NewMark(SC_ResponsePrestigeShopQuery rcq) 
    {
        if (rcq == null)
            return false;
       
        int _maxTabCount = 0;
        List<PrestigeShopData> psdList = TableCommon.GetPrestigeInfo(rewardTabIndex, out _maxTabCount);
        PrestigeStruce[] ps = rcq.commodity;
        foreach (var psData in psdList) 
        {
            int buyNum = psData.buyNum;
            //2.判断奖励页签物品是否有剩余购买次数
            for (int i = 0, count = ps.Length; i < count; i++) 
            {
                if (psData.index == ps[i].index) 
                {
                    buyNum -= ps[i].buyNum;
                }
            }
            //3.如果有,判断是否达到排名要求
            if (buyNum > 0) 
            {
                //4.如果达到排名要求，判断是否有足够的货币
                if(psData.openRank == 0 || rcq.rank <= psData.openRank)
                {
                    CheckReward = true;
                    return true;
                }

            }
            
        }
        CheckReward = false;
        return false;
    }

    public void RequestPrestigeShopInfoAndCheckReward(System.Action kRefreshCallback) 
    {
        PVPPrestigeShopNet.RequestPrestigeShopQuery(
        (text) => 
        {
            CheckReward_NewMark(JCode.Decode<SC_ResponsePrestigeShopQuery>(text));
            if (kRefreshCallback != null) 
            {
                kRefreshCallback();
            }
        }
        , 
        (text) => 
        {

        });
    }

   
    #endregion

    #region 刷新方法
    /// <summary>
    /// 刷新竞技场红点
    /// </summary>
    public void RefreshPrestigeNewMark() 
    {
        DataCenter.SetData("ARENA_MAIN_WINDOW", "REFRESH_NEWMARK", null);
        DataCenter.SetData("PVP_PRESTIGE_SHOP_WINDOW", "REFRESH_NEWMARK", null);
    }
    #endregion
}
//by chenliang
//begin

/// <summary>
/// 寻仙红点管理
/// </summary>
public class FairylandNewMarkManager : ManagerSingleton<FairylandNewMarkManager>
{
    private const int mFairylandFuncIndex = 1404;   //寻仙开放等级索引
    private int mOpenLevel = int.MinValue;          //开启等级

    #region 检测项
    /// <summary>
    /// 功能是否开启
    /// </summary>
    public bool CheckOpenLevel
    {
        get
        {
            return GameCommon.IsFuncCanUse(mFairylandFuncIndex);
        }
    }
    #endregion

    #region 可见性
    /// <summary>
    /// 寻仙红点可见性
    /// </summary>
    public bool FairylandVisible { get { return false; } }
    #endregion

    #region 检测方法
    #endregion

    #region 刷新方法
    #endregion
}

/// <summary>
/// 天魔红点管理
/// </summary>
public class BossNewMarkManager : ManagerSingleton<BossNewMarkManager>
{
    private const int mBossFuncIndex = 1401;   //天魔开放等级索引
    private int mOpenLevel = int.MinValue;          //开启等级

    #region 检测项
    /// <summary>
    /// 功能是否开启
    /// </summary>
    public bool CheckOpenLevel
    {
        get
        {
            return GameCommon.IsFuncCanUse(mBossFuncIndex);
        }
    }
    #endregion

    #region 可见性
    /// <summary>
    /// 天魔红点可见性
    /// </summary>
    public bool BossVisible { get { return false; } }
    #endregion

    #region 检测方法
    #endregion

    #region 刷新方法
    #endregion
}

//end
