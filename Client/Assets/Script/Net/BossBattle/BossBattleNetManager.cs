using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System;

/// <summary>
/// 界面结束后的行为
/// </summary>
public enum eACTIVE_AFTER_APPEAR
{
    SELECT_LEVEL,
    NEXT_LEVEL,
    AGAIN_LEVEL,
    CLEAN_LEVEL,
    QUIT_CLEAN_LEVEL,
    BACK_HOME_PAGE,
}

public class BossBattleStartData
{
	public Boss_GetDemonBossList_BossData bossData;
    //public int bossQuality = -1;
    public int leftHpAfterBattle = -1;
    public int battleDuration = -1;
// 	public int beatDemonCard;
// 	public int meritTimes;
// 	public int battleAchieve;
}

public class BossBattleNetManager
{
    /// <summary>
    /// 获取伤害输出和功勋
    /// </summary>
    public static void RequestGetDamageAndMerit(bool isJustRestore = false, Action successCallback = null)
    {
        CS_Boss_GetDamageAndMerit roleData = new CS_Boss_GetDamageAndMerit();
        HttpModule.Instace.SendGameServerMessage(
            roleData,
            (string text) =>
            {
                __OnRequestGetDamageAndMeritSuccess(text, isJustRestore);
                if (successCallback != null)
                    successCallback();
            },
            __OnRequestGetDemonBossListFailed);
    }
    private static void __OnRequestGetDamageAndMeritSuccess(string text, bool isJustRestore)
    {
        SC_Boss_GetDamageAndMerit retRoleData = JCode.Decode<SC_Boss_GetDamageAndMerit>(text);
        if (isJustRestore)
        {
            if (retRoleData != null)
                DataCenter.SetData("BOSS_RAID_WINDOW", "RESTORE_DAMAGE_MERIT", retRoleData);
        }
        else
        {
            if (retRoleData != null)
                DataCenter.SetData("BOSS_RAID_WINDOW", "REFRESH_ROLE_DATA", retRoleData);
            BossBattleNetManager.RequestGetDemonBossList();
        }
    }
    private static void __OnRequestGetDamageAndMeritFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 获取天魔列表
    /// </summary>
    public static void RequestGetDemonBossList(bool isJustRestoreBossList = false, Action successCallback = null)
    {
        CS_Boss_GetDemonBossList bossList = new CS_Boss_GetDemonBossList();
        HttpModule.Instace.SendGameServerMessage(
            bossList,
            (string text) =>
            {
                __OnRequestGetDemonBossListSuccess(text, isJustRestoreBossList);
                if (successCallback != null)
                    successCallback();
            },
            __OnRequestGetDemonBossListFailed);
    }
    private static void __OnRequestGetDemonBossListSuccess(string text, bool isJustRestoreBossList)
    {
        SC_Boss_GetDemonBossList retBossList = JCode.Decode<SC_Boss_GetDemonBossList>(text);
        if (isJustRestoreBossList)
        {
            if (retBossList != null)
            {
                if (retBossList.arr != null && retBossList.arr.Length > 0)
                {
                    DEBUG.Log("DemonBossList-BOSS_LIST_LENTH = " + retBossList.arr.Length);
                    GameCommon.SetDataByZoneUid("BOSS_LIST_LENTH", retBossList.arr.Length.ToString());
                }
                DataCenter.SetData("BOSS_RAID_WINDOW", "RESTORE_BOSS_LIST", retBossList);
            }
            return;
        }
        if (retBossList != null)
        {
            DataCenter.SetData("BOSS_RAID_WINDOW", "REFRESH_BOSS_LIST", retBossList);
            if (retBossList.arr != null && retBossList.arr.Length > 0)
            {
                DEBUG.Log("DemonBossList-BOSS_LIST_LENTH = " + retBossList.arr.Length);
                GameCommon.SetDataByZoneUid("BOSS_LIST_LENTH", retBossList.arr.Length.ToString());
                SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_BOSS, true);
            }
            else
            {
                SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_BOSS, false);
            }
        }
        else
        {
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_BOSS, false);
        }
    }
    private static void __OnRequestGetDemonBossListFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 进入天魔副本 
    /// </summary>
    public static void RequestDemonBossStart(Boss_GetDemonBossList_BossData bossData, int buttonIndex, int eventIndex)
    {
        CS_Boss_DemonBossStart bossStart = new CS_Boss_DemonBossStart();
        bossStart.finderId = bossData.finderId;
        //bossStart.bossIndex = bossData.bossIndex;
        bossStart.buttonIndex = buttonIndex;
        bossStart.eventIndex = eventIndex;
        int tmpBeatDemonCard = (buttonIndex == 1) ? 1 : 2;
        //事件1触发时逻辑
        if (eventIndex == 1)
        {
            DataRecord tmpRecord = DataCenter.mFeatEventConfig.GetRecord(1);
            if (tmpRecord != null)
                tmpBeatDemonCard = (int)tmpRecord.getObject("COST_MONSTER_TOKEN");
        }

        //检查降魔令是否不足
        if (RoleLogicData.Self.beatDemonCard < tmpBeatDemonCard)
        {
            DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_DEMONBOSS_BEATDEMONCARD_NOT_ENOUGH, "", "");
            return;
        }

        HttpModule.Instace.SendGameServerMessage(bossStart, t => __OnRequestDemonBossStartSuccess(bossData, buttonIndex, tmpBeatDemonCard, t), __OnRequestDemonBossStartFailed);
    }
    private static void __OnRequestDemonBossStartSuccess(Boss_GetDemonBossList_BossData bossData, int buttonIndex, int beatDemonCard, string text)
    {
        //进入天魔副本

        SC_Boss_DemonBossStart retBossStart = JCode.Decode<SC_Boss_DemonBossStart>(text);

        if (retBossStart.bossDead != 0)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_DEMONBOSS_BOSS_NOT_EXIST);
            DataCenter.CloseWindow("BOSS_STAGE_INFO_WINDOW");
            DataCenter.OpenWindow("BOSS_RAID_WINDOW");
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_BOSS, false);
            return;
        }
        if (retBossStart.ret == 1)
        {
            //扣降魔令
            RoleLogicData.Self.AddBeatDemonCard(-beatDemonCard);

            GlobalModule.ClearAllWindow();

            //			DataCenter.OpenWindow("MAIN_CENTER_GROUP");
            //			DataCenter.OpenWindow("BossRaidWindow");


            //            DataCenter.CloseWindow("BossRaidWindow");
            int bossID = bossData.tid;
            DataRecord config = DataCenter.mBossConfig.GetRecord(bossID);
            if (config == null)
            {
                //				Log("BOSS 配置不存在 >"+bossID.ToString());
                return;
            }
            DataCenter.Set("CURRENT_STAGE", (int)config.get("SCENE_ID"));

            BossBattleStartData startData = new BossBattleStartData();
            startData.bossData = bossData;
            //startData.bossQuality = retBossStart.quality;
            startData.leftHpAfterBattle = retBossStart.hpLeft;
            startData.battleDuration = retBossStart.duration;
            DataCenter.Set("START_BOSS_BATTLE", startData);

            if (buttonIndex == 1)
            {
                //普通攻击Buff
                DataCenter.Set("IS_BOSS_NORMAL_ATTACK", 0);
                DataCenter.Set("IS_BOSS_POWER_ATTACK", 0);
                //				MainProcess.mStage.mBossNormalAttack = 0;
                //				MainProcess.mStage.mBossPowerAttack = 0;
            }
            else if (buttonIndex == 2)
            {
                //全力一击Buff
                DataCenter.Set("IS_BOSS_NORMAL_ATTACK", 0);
                DataCenter.Set("IS_BOSS_POWER_ATTACK", 300002);
                //				MainProcess.mStage.mBossNormalAttack = 0;
                //				MainProcess.mStage.mBossPowerAttack = 300002;
            }

            MainProcess.LoadBossBattleLoadingScene();
        }
    }
    private static void __OnRequestDemonBossStartFailed(string text)
    {
        switch (text)
        {
            case "1851": DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_DEMONBOSS_BOSS_INFO_NOT_EXIST); break;
            case "1854":
                {
                    GlobalModule.DoLater(() => 
                        {
                            DataCenter.CloseWindow("BOSS_STAGE_INFO_WINDOW");
                            DataCenter.SetData("BOSS_RAID_WINDOW", "REQUEST_DATA", null);
                        }, 1.0f);
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_DEMONBOSS_BOSS_NOT_EXIST);
                }
                break;
        }
    }

    /// <summary>
    /// 退出天魔副本
    /// </summary>
    /// <param name="lostHP">Lost H.</param>
    public static void RequestDemonBossResult(int lostHP, Action<int, int, int> successCallback)
    {
        CS_Boss_DemonBossResult bossResult = new CS_Boss_DemonBossResult();
        //bossResult.damageOutput = lostHP;
        HttpModule.Instace.SendGameServerMessage(bossResult,
            (x) =>
            {
                SC_Boss_DemonBossResult retBossResult = JCode.Decode<SC_Boss_DemonBossResult>(x);
                if (successCallback != null)
                    successCallback(retBossResult.battleAchv, retBossResult.merit, retBossResult.playerTid);
                //added by xuke
                RoleLogicData.Self.AddBattleAchv(retBossResult.battleAchv);
                //end
            }, __OnRequestDemonBossResultFailed);
    }
    private static void __OnRequestDemonBossResultSuccess(string text)
    {
        SC_Boss_DemonBossResult retBossResult = JCode.Decode<SC_Boss_DemonBossResult>(text);
        //TODO
    }
    private static void __OnRequestDemonBossResultFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 领取功勋奖励
    /// </summary>
    /// <param name="awardIndex"></param>
    public static void RequestReceiveMeritAward(int awardIndex)
    {
        CS_Boss_ReceiveMeritAward receive = new CS_Boss_ReceiveMeritAward();
        receive.awardIndex = awardIndex;
        HttpModule.Instace.SendGameServerMessage(receive, x => { __OnRequestReceiveMeritAwardSuccess(awardIndex, x); }, __OnRequestReceiveMeritAwardFailed);
    }
    private static void __OnRequestReceiveMeritAwardSuccess(int awardIndex, string text)
    {
        SC_Boss_ReceiveMeritAward retReceive = JCode.Decode<SC_Boss_ReceiveMeritAward>(text);
        if (retReceive.ret == 1)
        {
            //刷新背包
            PackageManager.UpdateItem(retReceive.featAddArr);

            //不要在ui代码里面刷新背包里--更难维护
            DataCenter.SetData("BOSS_AWARD_WINDOW", "RECEIVE_MERIT_AWARD", awardIndex);
        }
        else
        {
            //TODO
        }
    }
    private static void __OnRequestReceiveMeritAwardFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 领取功勋奖励记录
    /// </summary>
    public static void RequestGetMeritAwardList(bool isJustRestore = false, Action successCallback = null)
    {
        CS_Boss_GetMeritAwardList awardList = new CS_Boss_GetMeritAwardList();
        HttpModule.Instace.SendGameServerMessage(
            awardList,
            (string text) =>
            {
                __OnRequestGetMeritAwardListSuccess(text, isJustRestore);
                if (successCallback != null)
                    successCallback();
            },
            __OnRequestGetMeritAwardListFailed);
    }
    private static void __OnRequestGetMeritAwardListSuccess(string text, bool isJustRestore)
    {
        SC_Boss_GetMeritAwardList retAwardList = JCode.Decode<SC_Boss_GetMeritAwardList>(text);
        if (isJustRestore)
            DataCenter.SetData("BOSS_AWARD_WINDOW", "RESTORE_GET_AWARD_LIST", retAwardList);
        else
            DataCenter.SetData("BOSS_AWARD_WINDOW", "REFRESH_LIST", retAwardList);
    }
    private static void __OnRequestGetMeritAwardListFailed(string text)
    {
        //TODO
    }
}

public class GetDemonBossList_Requester
    :NetRequester<CS_Boss_GetDemonBossList, SC_Boss_GetDemonBossList>
{
    protected override CS_Boss_GetDemonBossList GetRequest()
    {
        CS_Boss_GetDemonBossList tmpReq = new CS_Boss_GetDemonBossList();
        return tmpReq;
    }
}
