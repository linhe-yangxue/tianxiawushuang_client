using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 历练快捷入口是否显示
/// </summary>
public class TrialEasyJumpVisibleHelper
{
    private static TrialEasyJumpVisibleHelper msInstance = new TrialEasyJumpVisibleHelper();

    private Dictionary<TrialEasyJumpType, Func<bool>> mDicFuncVisible = new Dictionary<TrialEasyJumpType, Func<bool>>();        //跳转入口是否可见集合

    private TrialEasyJumpVisibleHelper()
    {
        __InitAllFuncVisible();
    }

    public static TrialEasyJumpVisibleHelper Instance
    {
        get { return msInstance; }
    }

    /// <summary>
    /// 注册入口是否可见的判断函数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="func"></param>
    private void __RegisterFuncVisible(TrialEasyJumpType easyJumpType, Func<bool> func)
    {
        if (mDicFuncVisible == null)
            return;

        mDicFuncVisible[easyJumpType] = func;
    }
    private void __InitAllFuncVisible()
    {
        //竞技场商店奖励页签入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.ArenaShopAward, () =>
        {
            return PVPNewMarkManager.Self.RewardVisible;
        });

        //封灵塔商店奖励页签入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.RammbockShopAward, () =>
        {
            return RammbockNewMarkManager.Self.RewardVisible;
        });
        //封灵塔免费重置入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.RammbockFreeReset, () =>
        {
            //检查是否开启封灵塔
            if (!RammbockNewMarkManager.Self.CheckOpenLevel)
                return false;

            if (!RammbockWindow.CanCheckFreeReset())
            {
                RammbockNetManager.RequestGetTowerClimbingInfo(true, () =>
                {
                    DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
                });
                return false;
            }
            return RammbockWindow.CanFreeReset();
        });

        //夺宝可以合成入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.GrabTreasureCombine, () =>
        {
            return GrabTreasureNewMarkManager.Self.GrabTreasureVisible;
        });

        //可以寻仙入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.FairylandCanExplore, () =>
        {
            //检查是否开启寻仙
            if (!FairylandNewMarkManager.Self.CheckOpenLevel)
                return false;

            if (!FairylandWindow.CanGetMyStates())
            {
                GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates("", true, () =>
                {
                    DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
                }));
                return false;
            }
            return FairylandWindow.HasMySpecifiedState(FAIRYLAND_ELEMENT_STATE.NO_EXPLORE);
        });
        //寻仙可以收获入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.FairylandCanHarvest, () =>
        {
            //检查是否开启寻仙
            if (!FairylandNewMarkManager.Self.CheckOpenLevel)
                return false;

            if (!FairylandWindow.CanGetMyStates())
            {
                GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates("", true, () =>
                {
                    DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
                }));
                return false;
            }
            return FairylandWindow.HasMySpecifiedState(FAIRYLAND_ELEMENT_STATE.TO_HARVEST);
        });

        //出现新天魔入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.BossNewAppear, () =>
        {
            //检查天魔是否开启
            if (!BossNewMarkManager.Self.CheckOpenLevel)
                return false;
            return BossRaidWindow.HasNewBossAppear;
        });
        //天魔活动1入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.BossActive1, () =>
        {
            //检查天魔是否开启
            if (!BossNewMarkManager.Self.CheckOpenLevel)
                return false;
            return BossStageInfoWindow.IsFeatEventOccured(1);
        });
        //天魔活动2入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.BossActive2, () =>
        {
            //检查天魔是否开启
            if (!BossNewMarkManager.Self.CheckOpenLevel)
                return false;
            return BossStageInfoWindow.IsFeatEventOccured(2);
        });
        //天魔功勋奖励入口是否可见
        __RegisterFuncVisible(TrialEasyJumpType.BossMeritAward, () =>
        {
            //检查天魔是否开启
            if (!BossNewMarkManager.Self.CheckOpenLevel)
                return false;

            if (!BossBattleAwardWindow.CanCheckValidMeritAward())
            {
                BossBattleNetManager.RequestGetDamageAndMerit(true, () =>
                {
                    BossBattleNetManager.RequestGetMeritAwardList(true, () =>
                    {
                        DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
                    });
                });

                return false;
            }
            return BossBattleAwardWindow.HasValidMeritAward();
        });
    }

    /// <summary>
    /// 获取快捷跳转是否可见
    /// </summary>
    /// <param name="easyJumpType"></param>
    /// <returns></returns>
    public bool IsEasyJumpVisible(TrialEasyJumpType easyJumpType)
    {
        if (mDicFuncVisible == null || mDicFuncVisible.Count <= 0)
            return false;

        Func<bool> tmpFunc = null;
        if (!mDicFuncVisible.TryGetValue(easyJumpType, out tmpFunc) ||
            tmpFunc == null)
            return false;
        return tmpFunc();
    }
}
