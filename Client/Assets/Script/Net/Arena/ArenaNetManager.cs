using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;


public class ArenaNetManager
{
    public static int iDamage = 0;
    /// <summary>
    /// ArenaChallengeList
    /// </summary>
    public static void RequestArenaChallengeList()
    {
        CS_ArenaChallengeList data = new CS_ArenaChallengeList();
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestArenaChallengeListSuccess, __OnRequestFailed);
    }
    private static void __OnRequestArenaChallengeListSuccess(string text)
    {
        SC_ArenaChallengeList retRoleData = JCode.Decode<SC_ArenaChallengeList>(text);
        if (retRoleData != null){
            ArenaBase.InitArenaData(retRoleData.arenaPrincipal, retRoleData.arenaChallengeList);
            DataCenter.SetData(UIWindowString.arena_main_window, "INIT_UI", null);
        }
            //
    }

    /// <summary>
    /// 请求自身竞技场排名
    /// </summary>
    public class ArenaChallengeListRequester : NetRequester<CS_ArenaChallengeList, SC_ArenaChallengeList>
    {
        protected override CS_ArenaChallengeList GetRequest()
        {
            return new CS_ArenaChallengeList();
        }

        protected override void OnSuccess()
        {
            ArenaBase.InitArenaData(respMsg.arenaPrincipal, respMsg.arenaChallengeList);
        }
    }

    /// <summary>
    /// ArenaBattleStart-未使用
    /// </summary>
    public static void RequestArenaBattleStart(int rivalRnak)
    {
        CS_ArenaBattleStart data = new CS_ArenaBattleStart();
        data.rivalRank = rivalRnak;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestArenaBattleStartSuccess, __OnRequestFailed);
    }
    private static void __OnRequestArenaBattleStartSuccess(string text)
    {
        SC_DailyStageBattleStart retRoleData = JCode.Decode<SC_DailyStageBattleStart>(text);
        if (retRoleData != null){
        }
            //DataCenter.SetData(UIWindowString.union_pk_prepare_window, "REQUESTBATTLE", null);
    }

    /// <summary>
    /// ArenaBattleEnd-未使用
    /// </summary>
    public static void RequestArenaBattleEnd(int guildId, int times, int damage)
    {
        CS_ArenaBattleEnd data = new CS_ArenaBattleEnd();
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestArenaBattleEndSuccess, __OnRequestFailed);
    }
    private static void __OnRequestArenaBattleEndSuccess(string text)
    {
        SC_GuildBossBattleEnd retRoleData = JCode.Decode<SC_GuildBossBattleEnd>(text);
        if (retRoleData != null) {
            //update data 

        }
    }

    /// <summary>
    /// ArenaBattleRecord
    /// </summary>
    public static void RequestArenaBattleRecord()
    {
        CS_ArenaBattleRecord data = new CS_ArenaBattleRecord();
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestArenaBattleRecordSuccess, __OnRequestFailed);
    }
    private static void __OnRequestArenaBattleRecordSuccess(string text)
    {
        SC_ArenaBattleRecord retRoleData = JCode.Decode<SC_ArenaBattleRecord>(text);
        if (retRoleData != null)
        {
            DataCenter.SetData(UIWindowString.arena_record_window, "ARENA_RECORD_INITUI", retRoleData);
        }
    }

    /// <summary>
    /// ArenaRankList
    /// </summary>
    public static void RequestArenaRankList()
    {
        CS_ArenaRankList data = new CS_ArenaRankList();
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestArenaRankListSuccess, __OnRequestFailed);
    }
    private static void __OnRequestArenaRankListSuccess(string text)
    {
        SC_ArenaRankList retRoleData = JCode.Decode<SC_ArenaRankList>(text);
        if (retRoleData != null)
        {
            DataCenter.OpenWindow(UIWindowString.arena_rank_window);
            DataCenter.SetData(UIWindowString.arena_rank_window, "ARENA_RANK_INITUI", retRoleData);
        }
    }

    /// <summary>
    /// CS_ArenaSweep
    /// </summary>
    public static void RequestArenaSweep()
    {
        CS_ArenaSweep data = new CS_ArenaSweep();
        if (RoleLogicData.Self.spirit >= ArenaBase.limitSpirit)
        {
            HttpModule.Instace.SendGameServerMessage(data, __OnRequestArenaSweepSuccess, __OnRequestFailed);
        }
        else
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
        }
    }
    private static void __OnRequestArenaSweepSuccess(string text)
    {
        SC_ArenaSweep retRoleData = JCode.Decode<SC_ArenaSweep>(text);
        if (retRoleData != null)
        {
            GameCommon.RoleChangeSpirit(-ArenaBase.limitSpirit);
            DataCenter.SetData(UIWindowString.arena_list_window, "ARENA_LIST_INITUI", retRoleData);
			DataCenter.Set("FUNC_ENTER_INDEX",FUNC_ENTER_INDEX.Challenge_Five);
        }
    }

    //统一处理失败报错 客户端能避免，尽量避免
    private static void __OnRequestFailed(string text)
    {
        //TODO
        //int ret = int.Parse(text);
        //if (ret == 2102)
        //{
        //    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_BOSS_RAIN_DEAD);
        //    return;
        //}
    }

    public class ArenaAccountInfo
    {
        public bool mIsWin = false;
        public string mChallengeName = "";

        public List<ItemDataBase> mAwardItem;
        public List<ItemDataBase> mAddItem;
        public int mAwardGold = 0;
        public int mAwardRoleExp = 0;
        public int mAwardReputation = 0;
        public int mAwardGroup = 0;

        public int mPreRoleLevel = 0;
        public int mPreRoleExp = 0;
        public int mPreRank = 0;
        public int mPreBestRank = 0;

        public int mPostRoleLevel = 0;
        public int mPostRoleExp = 0;
        public int mPostRank = 0;
        public int mPostBestRank = 0;

        public bool mHasBreak = false;
        public int mBreakAward = 0;
    }

    /// <summary>
    /// 竞技场开战请求-new
    /// </summary>
    public class ArenaBattleStartRequester : NetRequester<CS_ArenaBattleStart, SC_ArenaBattleStart>
    {
        public bool notEnoughSpirit { get; set; }

        private ArenaWarrior mPlayer = null;

        public ArenaBattleStartRequester(ArenaWarrior player)
        {
            mPlayer = player;

            notEnoughSpirit = RoleLogicData.Self.spirit < ArenaBase.limitSpirit;
        }

        protected override CS_ArenaBattleStart GetRequest()
        {
            var req = new CS_ArenaBattleStart();
            req.rivalRank = ArenaBase.getChallengeRank();
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

            ArenaBase.challengeTarget = respMsg.rival;
        }
    }

    /// <summary>
    /// 竞技场结算
    /// </summary>
    public class ArenaBattleEndRequester : NetRequester<CS_ArenaBattleEnd, SC_ArenaBattleEnd>
    {
        public ArenaAccountInfo accountInfo { get; private set; }

        protected override void OnStart()
        {
            accountInfo = new ArenaAccountInfo();
            StageBattle stage = MainProcess.mStage;
            accountInfo.mIsWin = stage.mbSucceed;
            accountInfo.mChallengeName = ArenaBase.challengeTarget.name;

            RoleData mainRole = RoleLogicData.GetMainRole();
            accountInfo.mPreRoleLevel = mainRole.level;
            accountInfo.mPreRoleExp = mainRole.exp;
            accountInfo.mPreRank = ArenaBase.arenaWarriorObject.rank;
            accountInfo.mPreBestRank = ArenaBase.arenaWarriorObject.bestRank;
        }

        protected override CS_ArenaBattleEnd GetRequest()
        {
            var send = new CS_ArenaBattleEnd();
            send.success = accountInfo.mIsWin ? 1 : 0;
            send.rivalRank = ArenaBase.getChallengeRank();
            return send;
        }

        protected override void OnSuccess()
        {
            DataRecord r = DataCenter.mPvpLoot.GetRecord(accountInfo.mIsWin ? 1 : 0);
            RoleData mainRole = RoleLogicData.GetMainRole();

            if (respMsg.addItems != null && respMsg.addItems.Length > 1)
            {
                accountInfo.mAwardGold = respMsg.addItems[0].itemNum;
                accountInfo.mAwardReputation = respMsg.addItems[1].itemNum;
            }
            
            accountInfo.mAwardGroup = r["GROUP_AWARD"];

            // 扣除精力
            GameCommon.RoleChangeSpirit(-ArenaBase.limitSpirit);

            // 获取掉落物品
            accountInfo.mAwardItem = PackageManager.UpdateItem(respMsg.rewards);
            accountInfo.mAddItem = respMsg.addItems.ToList();

            if (accountInfo.mIsWin)
            {
                if (ArenaBase.getChallengeRank() != -1)
                {
                    accountInfo.mPostRank = accountInfo.mPreRank > ArenaBase.getChallengeRank() ? ArenaBase.getChallengeRank() : accountInfo.mPreRank;
                }
                else
                {
                    accountInfo.mPostRank = accountInfo.mPreRank;
                }
                ArenaBase.arenaWarriorObject.rank = accountInfo.mPostRank;

                if (ArenaBase.arenaWarriorObject.rank < ArenaBase.arenaWarriorObject.bestRank)
                {
                    ArenaBase.arenaWarriorObject.bestRank = ArenaBase.arenaWarriorObject.rank;
                    accountInfo.mPostBestRank = ArenaBase.arenaWarriorObject.bestRank;

                    int awardIndex = ArenaBase.GetAwardIndexByRank(ArenaBase.arenaWarriorObject.bestRank);

                    if (awardIndex > 0)
                    {
                        accountInfo.mHasBreak = true;

                        int awardLoot = DataCenter.mPvpRankAwardConfig.GetData(awardIndex, "GOLD_AWARD_BY_RANK");
                        accountInfo.mBreakAward = awardLoot * (accountInfo.mPreBestRank - accountInfo.mPostBestRank);

                        PackageManager.AddItem((int)ITEM_TYPE.YUANBAO, -1, accountInfo.mBreakAward);
                    }
                }
            }

            accountInfo.mPostRoleLevel = mainRole.level;
            accountInfo.mPostRoleExp = mainRole.exp;
        }
    }
}
