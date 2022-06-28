using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 历练快捷跳转
/// </summary>
public class TrialEasyJumpHelper
{
    private static TrialEasyJumpHelper msInstance = new TrialEasyJumpHelper();

    private Dictionary<TrialEasyJumpType, Action> mDicFuncJump = new Dictionary<TrialEasyJumpType, Action>();       //跳转方法集合

    private TrialEasyJumpHelper()
    {
        __InitAllFuncJump();
    }

    public static TrialEasyJumpHelper Instance
    {
        get { return msInstance; }
    }

    /// <summary>
    /// 注册跳转方法
    /// </summary>
    /// <param name="easyJumpType"></param>
    /// <param name="func"></param>
    private void __RegisterFuncJump(TrialEasyJumpType easyJumpType, Action func)
    {
        if (mDicFuncJump == null)
            return;

        mDicFuncJump[easyJumpType] = func;
    }
    private void __InitAllFuncJump()
    {
        //竞技场商店奖励页签跳转
        __RegisterFuncJump(TrialEasyJumpType.ArenaShopAward, () =>
        {
            MainUIScript.Self.OpenMainUI();
            DataCenter.OpenWindow(UIWindowString.arena_main_window);
            DataCenter.OpenWindow("PVP_PRESTIGE_SHOP_WINDOW", 2);
        });

        //封灵塔商店奖励页签跳转
        __RegisterFuncJump(TrialEasyJumpType.RammbockShopAward, () =>
        {
            DataCenter.OpenWindow("RAMMBOCK_WINDOW");
            DataCenter.OpenWindow("SHOP_RENOWN_WINDOW", 4);
        });
        //封灵塔免费重置跳转
        __RegisterFuncJump(TrialEasyJumpType.RammbockFreeReset, () =>
        {
            DataCenter.OpenWindow("RAMMBOCK_WINDOW");
        });

        //夺宝可以合成跳转
        __RegisterFuncJump(TrialEasyJumpType.GrabTreasureCombine, () =>
        {
            DataCenter.OpenWindow("GRABTREASURE_WINDOW");
            //延迟几帧跳转到第一个能合成的法器，防止跳转失败
            GlobalModule.DoOnNextUpdate(() =>
            {
                GlobalModule.DoLater(() =>
                {
                    GlobalModule.DoOnNextLateUpdate(() =>
                    {
                        GlobalModule.DoOnNextUpdate(() =>
                        {
                            DataCenter.SetData("GRABTREASURE_WINDOW", "CHANGE_TO_FIRST_COMBINE", null);
                        });
                    });
                }, 0.1f);
            });
        });

        //可以寻仙跳转
        __RegisterFuncJump(TrialEasyJumpType.FairylandCanExplore, () =>
        {
            GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates(""));
        });
        //寻仙可以收获跳转
        __RegisterFuncJump(TrialEasyJumpType.FairylandCanHarvest, () =>
        {
            GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates(""));
        });

        //出现新天魔跳转
        __RegisterFuncJump(TrialEasyJumpType.BossNewAppear, () =>
        {
            DataCenter.OpenWindow("BOSS_RAID_WINDOW");
        });
        //天魔活动1跳转
        __RegisterFuncJump(TrialEasyJumpType.BossActive1, () =>
        {
            DataCenter.OpenWindow("BOSS_RAID_WINDOW");
        });
        //天魔活动2跳转
        __RegisterFuncJump(TrialEasyJumpType.BossActive2, () =>
        {
            DataCenter.OpenWindow("BOSS_RAID_WINDOW");
        });
        //天魔功勋奖励跳转
        __RegisterFuncJump(TrialEasyJumpType.BossMeritAward, () =>
        {
            DataCenter.OpenWindow("BOSS_RAID_WINDOW");
            DataCenter.OpenWindow("BOSS_AWARD_WINDOW", null);
        });
    }

    public void EasyJump(TrialEasyJumpType easyJumpType)
    {
        if (mDicFuncJump == null || mDicFuncJump.Count <= 0)
            return;

        Action tmpFunc = null;
        if (!mDicFuncJump.TryGetValue(easyJumpType, out tmpFunc) ||
            tmpFunc == null)
            return;
        tmpFunc();
    }
}
