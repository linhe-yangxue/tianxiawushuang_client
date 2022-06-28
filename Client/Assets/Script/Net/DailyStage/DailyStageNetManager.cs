using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;


public class DailyStageNetManager
{
    public static int iDamage = 0;
    /// <summary>
    /// DailyStageInfo
    /// </summary>
    public static void RequestDailyStageInfo()
    {
        CS_DailyStageInfo data = new CS_DailyStageInfo();
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestDailyStageInfoSuccess, __OnRequestFailed);
    }
    private static void __OnRequestDailyStageInfoSuccess(string text)
    {
        SC_DailyStageInfo retRoleData = JCode.Decode<SC_DailyStageInfo>(text);
        if (retRoleData != null){
            DailyStageBase.idailyStageInfoObject = retRoleData.dailyStage;
            DataCenter.SetData(UIWindowString.daily_stage_main_window, "DAILYSTAGE_INIT", null);
        }
            //
    }

    /// <summary>
    /// GuildBossBattleStart-未使用
    /// </summary>
    public static void RequestDailyStageBattleStart(int guildId)
    {
        CS_DailyStageBattleStart data = new CS_DailyStageBattleStart();
        data.mid = guildId;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestDailyStageBattleStartSuccess, __OnRequestFailed);
    }
    private static void __OnRequestDailyStageBattleStartSuccess(string text)
    {
        SC_DailyStageBattleStart retRoleData = JCode.Decode<SC_DailyStageBattleStart>(text);
        if (retRoleData != null){
        }
            //DataCenter.SetData(UIWindowString.union_pk_prepare_window, "REQUESTBATTLE", null);
    }

    /// <summary>
    /// DailyStageBattleEnd-未使用
    /// </summary>
    public static void RequestDailyStageBattleEnd(int guildId, int times, int damage)
    {
        CS_DailyStageBattleEnd data = new CS_DailyStageBattleEnd();
        iDamage = damage;
        data.mid = guildId;
        data.damage = damage;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestDailyStageBattleEndSuccess, __OnRequestFailed);
    }
    private static void __OnRequestDailyStageBattleEndSuccess(string text)
    {
        SC_GuildBossBattleEnd retRoleData = JCode.Decode<SC_GuildBossBattleEnd>(text);
        if (retRoleData != null) {
            //update data 

        }
    }

    /// <summary>
    /// 活动副本开战请求-new
    /// </summary>
    public class DailyStageBattleStartRequester : NetRequester<CS_DailyStageBattleStart, SC_DailyStageBattleStart>
    {
        protected override CS_DailyStageBattleStart GetRequest()
        {
            var req = new CS_DailyStageBattleStart();
            req.mid = DataCenter.Get("STAGE_MAIN_SELECT_INDEX");
            return req;
        }

        protected override void OnFail()
        {
            //TODO
            int ret = respCode;
            if (ret == 2202)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.DAILY_STAGE_MAIN_OPEN_LEVEL_NOT_ENOUGH);
                return;
            }
        }

        protected override void OnSuccess()
        {
            base.OnSuccess();

            //活动副本设置
            DataCenter.Set("IS_DAILYS_STAGE_BATTLE", true);
        }
    }

    /// <summary>
    /// 活动副本结算-new
    /// </summary>
    public class DailyStageBattleEndRequester : NetRequester<CS_DailyStageBattleEnd, SC_DailyStageBattleEnd>, IBattleAccountRequester
    {
        public BattleAccountInfo accountInfo { get; private set; }

        protected override void OnStart()
        {
            accountInfo = new BattleAccountInfo();
            var stage = MainProcess.mStage;
            var pve = stage as PVEStageBattle;
            accountInfo.battleId = stage.mConfigIndex;
            accountInfo.isWin = stage.mbSucceed;
            accountInfo.isSweep = false;
            accountInfo.starMask = pve == null ? 0 : pve.mStarMask;//StageStar.GetStarMask(stage as PVEStageBattle);
            accountInfo.starRate = StageStar.GetStar(accountInfo.starMask);
            accountInfo.battleTime = stage.mBattleTime;

            //modified by xuke
            accountInfo.gold = stage.mStageConfig["BOSS_MONEY"];
            //特别的经验
            int index = DataCenter.Get("STAGE_MAIN_SELECT_INDEX");
            accountInfo.roleExp = 0;

            var mainRole = RoleLogicData.GetMainRole();
            accountInfo.preRoleLevel = mainRole.level;
            accountInfo.preRoleExp = mainRole.exp;
        }

        protected override CS_DailyStageBattleEnd GetRequest()
        {
            int index = DataCenter.Get("STAGE_MAIN_SELECT_INDEX");
            var send = new CS_DailyStageBattleEnd();
            send.mid = index;
            send.damage = accountInfo.isWin ? 1 : 0;
            return send;
        }

        protected override void OnFail()
        {
            //TODO
            int ret = respCode;
            if (ret == 2102)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_BOSS_RAIN_DEAD);
                return;
            }
        }
        protected override void OnSuccess()
        {
            accountInfo.dropList = respMsg.attr;

            if (accountInfo.isWin)
            {
                // 获取掉落物品
                List<ItemDataBase> _itemList = PackageManager.UpdateItem(accountInfo.dropList);
                DataCenter.SetData("PVE_ACCOUNT_WIN_WINDOW", "SET_DROP_LIST_ITEM", _itemList);

                // 增加金币
                RoleLogicData.Self.AddGold(accountInfo.gold);
            }

            var mainRole = RoleLogicData.GetMainRole();
            accountInfo.postRoleLevel = mainRole.level;
            accountInfo.postRoleExp = mainRole.exp;

            //成功的话要扣体力和此数呀--不论输赢
            int index = DataCenter.Get("STAGE_MAIN_SELECT_INDEX");
            int type = DailyStageBase.GetTypeByIndex(index);
            DailyStageBase.UpdateBattleTimes(type);
            DailyStageBase.UpdateTili(index);
        }
    }

    //统一处理失败报错 客户端能避免，尽量避免
    private static void __OnRequestFailed(string text)
    {
        //TODO
        int ret = int.Parse(text);
        if (ret == 2102)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_BOSS_RAIN_DEAD);
            return;
        }
        if (ret == 2202)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.DAILY_STAGE_MAIN_OPEN_LEVEL_NOT_ENOUGH);
            return;
        }
    }
}
