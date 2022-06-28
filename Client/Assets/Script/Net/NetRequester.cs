using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;


public interface INetRequester
{
    GameServerMessage sendMsg { get; }   
    Coroutine Start(); 
    bool success { get; }
    string respText { get; }
    int respCode { get; }
    RespMessage respMsg { get; }    
}


public abstract class NetRequester<TSend, TResp> : INetRequester 
    where TSend : GameServerMessage
    where TResp : RespMessage
{
    public TSend sendMsg { get; private set; }
    public TResp respMsg { get; private set; }
    public bool success { get; private set; }
    public string respText { get; private set; }
    public int respCode { get; private set; }

    GameServerMessage INetRequester.sendMsg
    {
       get { return this.sendMsg; }
    }

    RespMessage INetRequester.respMsg
    {
        get { return this.respMsg; }
    }

    public Coroutine Start()
    {
        OnStart();
        sendMsg = GetRequest();

        if (string.IsNullOrEmpty(sendMsg.pt))
        {
            sendMsg.pt = sendMsg.GetType().Name;
        }

        return NetManager.StartWaitResp(sendMsg, _OnSuccess, _OnFail);
    }

    private void _OnSuccess(string resp)
    {
        respText = resp;   
        respMsg = JCode.Decode<TResp>(respText);
        respCode = 1;
        success = true;
        OnSuccess();
    }

    private void _OnFail(string resp)
    {
        respText = resp;
        respCode = string.IsNullOrEmpty(resp) ? 0 : int.Parse(resp);
        success = false;
        OnFail();
    }

    protected abstract TSend GetRequest();
    protected virtual void OnStart() { }
    protected virtual void OnSuccess() { }
    protected virtual void OnFail() { }
}


/// <summary>
/// 获取主线副本数据
/// </summary>
public class BattleMainMapRequester : NetRequester<CS_BattleMainMap, SC_BattleMainMap>
{
    protected override CS_BattleMainMap GetRequest()
    {
        return new CS_BattleMainMap();
    }

    protected override void OnSuccess()
    {
        var datas = respMsg.arr;

        foreach (var d in respMsg.arr)
        {
            if (d.fightCount < 0)
                d.fightCount = 0;

            if (d.successCount < 0)
                d.successCount = 0;
        }

        //if (MapLogicData.Self == null || !GameCommon.bIsLogicDataExist("MAP_DATA"))
        //{
        //    MapLogicData logicData = new MapLogicData();
        //    DataCenter.RegisterData("MAP_DATA", logicData);
        //}
        //
        MapLogicData.Instance.Update(datas);
    }
}


/// <summary>
/// 获取活动副本数据
/// </summary>
public class BattleActiveMapRequester : NetRequester<CS_BattleActiveMap, SC_BattleActiveMap>
{
    protected override CS_BattleActiveMap GetRequest()
    {
        return new CS_BattleActiveMap();
    }

    protected override void OnSuccess()
    {
        var datas = respMsg.arr;

        foreach (var d in respMsg.arr)
        {
            if (d.fightCount < 0)
                d.fightCount = 0;

            if (d.successCount < 0)
                d.successCount = 0;
        }

        //if (MapLogicData.Self == null || !GameCommon.bIsLogicDataExist("MAP_DATA"))
        //{
        //    MapLogicData logicData = new MapLogicData();
        //    DataCenter.RegisterData("MAP_DATA", logicData);
        //}
        //
        MapLogicData.Instance.Update(datas);
    }
}


public interface IBattleAccountRequester : INetRequester
{
    BattleAccountInfo accountInfo { get; }
}


/// <summary>
/// 主线副本战斗结算
/// </summary>
public class BattleMainAccountRequester : NetRequester<CS_BattleMainResult, SC_BattleMainResult>, IBattleAccountRequester
{
    public static Action<BattleAccountInfo> onAccount = null;

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
        accountInfo.staminaCost = StageProperty.GetStageStaminaCost(accountInfo.battleId);

        //modified by xuke 更改金币和经验的产出公式
        //accountInfo.gold = stage.mStageConfig["BOSS_MONEY"];
        //var mainRole = RoleLogicData.GetMainRole();
        //accountInfo.roleExp = accountInfo.staminaCost * 10;
        //主线金币公式：每次副本经验产出=(基数A+等级*系数A)*体力消耗
        var mainRole = RoleLogicData.GetMainRole();
        accountInfo.gold = GameCommon.GetMainStageGold(mainRole.level);
        accountInfo.roleExp = GameCommon.GetMainStageExp(mainRole.level);//(int)(_record_exp["BASE"] + mainRole.level * _record_exp["QUOTIETY"]) * StageProperty.GetCurrentStageStaminaCost();
        //end
        accountInfo.preRoleLevel = mainRole.level;
        accountInfo.preRoleExp = mainRole.exp;
    }

    protected override CS_BattleMainResult GetRequest()
    {
        var send = new CS_BattleMainResult();
        send.battleId = accountInfo.battleId;
        send.isWin = accountInfo.isWin ? 1 : 0;
        send.starRate = accountInfo.starRate;
        send.isAuto = MainProcess.mStage.mHasAutoFighted ? 1 : 0;
        return send;
    }

    protected override void OnSuccess()
    {
        accountInfo.dropList = respMsg.arr;

        if (onAccount != null)
        {
            onAccount(accountInfo);
        }

        // 更新地图数据
        MapLogicData.Instance.UpdateData(accountInfo.battleId, accountInfo.isWin, accountInfo.starRate);

        if (accountInfo.isWin)
        {
            // 扣除体力
            GameCommon.RoleChangeStamina(-accountInfo.staminaCost);

            // 获取掉落物品
            //PackageManager.AddItem(accountInfo.dropList);
			List<ItemDataBase> _itemList = PackageManager.UpdateItem(accountInfo.dropList);
			DataCenter.SetData("PVE_ACCOUNT_WIN_WINDOW","SET_DROP_LIST_ITEM",_itemList);

            // 增加金币
            RoleLogicData.Self.AddGold(accountInfo.gold);

            // 增加角色经验
            RoleLogicData.GetMainRole().AddExp(accountInfo.roleExp,true);
            
            //减少挑战次数
            MapLogicData.Instance.GetMapDataByStageIndex(accountInfo.battleId).todaySuccess++;
        }

        var mainRole = RoleLogicData.GetMainRole();
        accountInfo.postRoleLevel = mainRole.level;
        accountInfo.postRoleExp = mainRole.exp;
        
        //added by xuke 冒险红点
        AdventureNewMarkManager.Self.CheckCurPage_NewMark();
        //end
    }
}


/// <summary>
/// 主线副本退出
/// </summary>
public class ExitBattleMainRequester : NetRequester<CS_ExitBattleMain, SC_ExitBattleMain>
{
    protected override CS_ExitBattleMain GetRequest()
    {
        var send = new CS_ExitBattleMain();
        send.battleId = MainProcess.mStage.mConfigIndex;
        send.isAuto = MainProcess.mStage.mHasAutoFighted ? 1 : 0;
        return send;
    }
}


/// <summary>
/// 主线副本请求扫荡
/// </summary>
public class BattleMainSweepRequester : NetRequester<CS_BattleMainSweep, SC_BattleMainSweep>, IBattleAccountRequester
{
    public ItemDataBase sweepTicket { get; private set; }
    public bool canSweep { get; private set; }
    public BattleAccountInfo accountInfo { get; private set; }

    public BattleMainSweepRequester()
    {
        sweepTicket = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.SAODANG_POINT);
        canSweep = sweepTicket.itemId >= 0 && sweepTicket.itemNum > 0;
    }

    protected override void OnStart()
    {
        accountInfo = new BattleAccountInfo();
        accountInfo.battleId = DataCenter.Get("CURRENT_STAGE");
        accountInfo.isWin = true;
        accountInfo.isSweep = true;
        accountInfo.starMask = ~0;
        accountInfo.starRate = 3;
        accountInfo.battleTime = 0f;
        accountInfo.staminaCost = StageProperty.GetStageStaminaCost(accountInfo.battleId);

        var r = DataCenter.mStageTable.GetRecord(accountInfo.battleId);
        //modified by xuke begin
        var mainRole = RoleLogicData.GetMainRole();
        accountInfo.gold = GameCommon.GetMainStageGold(mainRole.level);//r["BOSS_MONEY"];     
        accountInfo.roleExp = GameCommon.GetMainStageExp(mainRole.level);//accountInfo.staminaCost * 10;
        //end
        accountInfo.preRoleLevel = mainRole.level;
        accountInfo.preRoleExp = mainRole.exp;
    }

    protected override CS_BattleMainSweep GetRequest()
    {
        var send = new CS_BattleMainSweep();
        send.battleId = accountInfo.battleId;
        send.sweepObj = sweepTicket;
        return send;
    }

    protected override void OnSuccess()
    {
        accountInfo.dropList = respMsg.arr;

        // 更新地图数据
        MapLogicData.Instance.UpdateData(accountInfo.battleId, accountInfo.isWin, accountInfo.starRate);

        if (accountInfo.isWin)
        {
            // 扣除体力
            GameCommon.RoleChangeStamina(-accountInfo.staminaCost);

            // 获取掉落物品
            //PackageManager.AddItem(accountInfo.dropList);
            List<ItemDataBase> _itemList = PackageManager.UpdateItem(accountInfo.dropList);
			for(int x=0;x<_itemList.Count ;x++){
				accountInfo.dropList[x].itemNum =_itemList[x].itemNum;
				//DEBUG.Log ("######"+num.ToString ());
			}
			DataCenter.SetData("PVE_ACCOUNT_CLEAN_WINDOW", "SET_DROP_LIST_ITEM", _itemList);

            // 增加金币
            RoleLogicData.Self.AddGold(accountInfo.gold);

            // 增加角色经验
            RoleLogicData.GetMainRole().AddExp(accountInfo.roleExp);

            // 扣除扫荡券
            //ConsumeItemLogicData.Self.RemoveItemData(sendMsg.sweepObj.tid, 1);
            
            //减少关卡挑战次数
            MapLogicData.Instance.GetMapDataByStageIndex(accountInfo.battleId).todaySuccess++;
        }
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.CLEAN_OUT);
        var mainRole = RoleLogicData.GetMainRole();
        accountInfo.postRoleLevel = mainRole.level;
        accountInfo.postRoleExp = mainRole.exp;
    }
}


/// <summary>
/// 请求活动副本刷新标志位
/// </summary>
public class BattleActiveFreshRequester : NetRequester<CS_BattleActiveFresh, SC_BattleActiveFresh>
{
    protected override CS_BattleActiveFresh GetRequest()
    {
        return new CS_BattleActiveFresh();
    }
}


/// <summary>
/// 主线副本开战请求
/// </summary>
public class BattleMainStartRequester : NetRequester<CS_BattleMainStart, SC_BattleMainStart>
{
    protected override CS_BattleMainStart GetRequest()
    {
        var req = new CS_BattleMainStart();
        req.battleId = DataCenter.Get("CURRENT_STAGE");
        return req;
    }
}


/// <summary>
/// 活动副本开战请求
/// </summary>
public class BattleActiveStartRequester : NetRequester<CS_BattleActiveStart, SC_BattleActiveStart>
{
    protected override CS_BattleActiveStart GetRequest()
    {
        var req = new CS_BattleActiveStart();
        req.battleId = DataCenter.Get("CURRENT_STAGE");
        return req;
    }
}


/// <summary>
/// 活动副本结算
/// </summary>
public class BattleActiveAccountRequester : NetRequester<CS_BattleActiveResult, SC_BattleActiveResult>, IBattleAccountRequester
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

        accountInfo.gold = stage.mStageConfig["BOSS_MONEY"];
        var mainRole = RoleLogicData.GetMainRole();
        accountInfo.roleExp = mainRole.level * 10;

        accountInfo.preRoleLevel = mainRole.level;
        accountInfo.preRoleExp = mainRole.exp;
    }

    protected override CS_BattleActiveResult GetRequest()
    {
        var send = new CS_BattleActiveResult();
        send.battleId = accountInfo.battleId;
        send.isWin = accountInfo.isWin ? 1 : 0;
        send.starRate = accountInfo.starRate;
        return send;
    }

    protected override void OnSuccess()
    {
        accountInfo.dropList = respMsg.arr;

        // 更新地图数据
        MapLogicData.Instance.UpdateData(accountInfo.battleId, accountInfo.isWin, accountInfo.starRate);

        if (accountInfo.isWin)
        {
            // 扣除体力
            GameCommon.RoleChangeStamina(-CommonParam.battleActiveStaminaCost);

            // 获取掉落物品
            PackageManager.AddItem(accountInfo.dropList);

            // 增加金币
            RoleLogicData.Self.AddGold(accountInfo.gold);

            // 增加角色经验
            RoleLogicData.GetMainRole().AddExp(accountInfo.roleExp);
        }

        var mainRole = RoleLogicData.GetMainRole();
        accountInfo.postRoleLevel = mainRole.level;
        accountInfo.postRoleExp = mainRole.exp;
    }
}


/// <summary>
/// 请求自身竞技场排名
/// </summary>
public class GetArenaRankRequester : NetRequester<CS_GetArenaRank, SC_GetArenaRank>
{
    protected override CS_GetArenaRank GetRequest()
    {
        return new CS_GetArenaRank();
    }

    protected override void OnSuccess()
    {
        PeakPvpData.curRank = respMsg.curRank;
        PeakPvpData.bestRank = respMsg.bestRank;
    }
}


/// <summary>
/// 请求竞技场挑战列表
/// </summary>
public class RefreshChallengeListRequester : NetRequester<CS_RefreshChallengeList, SC_RefreshChallengeList>
{
    protected override CS_RefreshChallengeList GetRequest()
    {
        return new CS_RefreshChallengeList();
    }

    protected override void OnSuccess()
    {
        PeakPvpData.challengeList = new List<ArenaPlayer>(respMsg.arr);
        PeakPvpData.lastResfreshTime = Time.time;
    }
}


/// <summary>
/// 竞技场开战
/// </summary>
public class ChallengeStartRequester : NetRequester<CS_ChallengeStart, SC_ChallengeStart>
{
    public bool notEnoughSpirit { get; set; }

    private int mTargetRank = -1;
    private int mTargetId = 0;

    public ChallengeStartRequester(int targetId, int targetRank)
    {
        mTargetId = targetId;
        mTargetRank = targetRank;

        notEnoughSpirit = RoleLogicData.Self.spirit < 2;
    }

    protected override CS_ChallengeStart GetRequest()
    {
        var req = new CS_ChallengeStart();
        req.targetId = mTargetId;
        req.targetRank = mTargetRank;
        return req;
    }

    protected override void OnSuccess()
    {
        if (respMsg.isFighting == 0 && respMsg.rankChanged == 0)
        {
            PeakPvpData.challengeTarget = respMsg.opponent;
            PeakPvpData.challengeRank = mTargetRank;
            PeakPvpData.challengeUid = mTargetId;
            //GameCommon.RoleChangeSpirit(-2); 改为结算时扣除精力
        }
    }
}


/// <summary>
/// 竞技场结算
/// </summary>
public class ChallengeResultRequester : NetRequester<CS_ChallengeResult, SC_ChallengeResult>
{
    public PeakAccountInfo accountInfo { get; private set; }

    protected override void OnStart()
    {
        accountInfo = new PeakAccountInfo();
        StageBattle stage = MainProcess.mStage;
        accountInfo.mIsWin = stage.mbSucceed;
        accountInfo.mChallengeName = PeakPvpData.challengeTarget.name;

        RoleData mainRole = RoleLogicData.GetMainRole();
        accountInfo.mPreRoleLevel = mainRole.level;
        accountInfo.mPreRoleExp = mainRole.exp;
        accountInfo.mPreRank = PeakPvpData.curRank;
        accountInfo.mPreBestRank = PeakPvpData.bestRank;
    }

    protected override CS_ChallengeResult GetRequest()
    {
        var send = new CS_ChallengeResult();
        send.isWin = accountInfo.mIsWin ? 1 : 0;
        return send;
    }

    protected override void OnSuccess()
    {
        DataRecord r = DataCenter.mPvpLoot.GetRecord(accountInfo.mIsWin ? 1 : 0);
        RoleData mainRole = RoleLogicData.GetMainRole();

        accountInfo.mAwardGold = r["MONEY_AWARD_TIME"] * mainRole.level;
        accountInfo.mAwardRoleExp = 10;//r["ROLE_EXP_AWARD_TIME"] * mainRole.level;
        accountInfo.mAwardReputation = r["REPUTATION_AWARD"];    
        accountInfo.mAwardGroup = r["GROUP_AWARD"];

        // 增加金币
        GameCommon.RoleChangeGold(accountInfo.mAwardGold);

        // 增加声望
        GameCommon.RoleChangeReputation(accountInfo.mAwardReputation);

        // 增加角色经验
        RoleLogicData.GetMainRole().AddExp(accountInfo.mAwardRoleExp);

        // 扣除精力
        GameCommon.RoleChangeSpirit(-2);

        if (accountInfo.mIsWin)
        {
            // 获取掉落物品
            accountInfo.mAwardItem = PackageManager.UpdateItem(respMsg.item);

            accountInfo.mPostRank = accountInfo.mPreRank > PeakPvpData.challengeRank ? PeakPvpData.challengeRank : accountInfo.mPreRank;
            PeakPvpData.curRank = accountInfo.mPostRank;

            if (PeakPvpData.curRank < PeakPvpData.bestRank)
            {
                PeakPvpData.bestRank = PeakPvpData.curRank;
                accountInfo.mPostBestRank = PeakPvpData.bestRank;

                int awardIndex = PeakPvpData.GetAwardIndexByRank(PeakPvpData.bestRank);
                
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


/// <summary>
/// 获取日常任务数据
/// </summary>
public class GetDailyTaskDataRequester : NetRequester<CS_GetDailyTaskData, SC_GetDailyTaskData>
{
    protected override CS_GetDailyTaskData GetRequest()
    {
        return new CS_GetDailyTaskData();
    }

    protected override void OnSuccess()
    {
        TaskLogicData.Instance.UpdateDailyTasks(respMsg.arr, respMsg.scoreAwards, respMsg.curScore);
        TaskState.shouldDailyRefresh = false;
        TaskState.hasDailyAwardAcceptable = TaskProperty.HasAwardAcceptable(TASK_PAGE.DAILY);
    }
}


/// <summary>
/// 获取成就数据
/// </summary>
public class GetAchievementDataRequester : NetRequester<CS_GetAchievementData, SC_GetAchievementData>
{
    protected override CS_GetAchievementData GetRequest()
    {
        return new CS_GetAchievementData();
    }

    protected override void OnSuccess()
    {
        TaskLogicData.Instance.UpdateAchievements(respMsg.arr);
        TaskState.shouldAchieveRefresh = false;
        TaskState.hasAchieveAwardAcceptable = TaskProperty.HasAwardAcceptable(TASK_PAGE.ACHIEVEMENT);
    }
}


/// <summary>
/// 获取日常任务奖励
/// </summary>
public class GetDailyTaskAwardRequester : NetRequester<CS_GetDailyTaskAward, SC_GetDailyTaskAward>
{
    public int taskId { get; private set; }
    public List<ItemDataBase> awardItems { get; private set; }

    public GetDailyTaskAwardRequester(int taskId)
    {
        this.taskId = taskId;
    }

    protected override CS_GetDailyTaskAward GetRequest()
    {
        return new CS_GetDailyTaskAward() { taskId = this.taskId };
    }

    protected override void OnSuccess()
    {
        List<ItemDataBase> temp = new List<ItemDataBase>(respMsg.arr);
        TaskProperty prop = new TaskProperty(taskId);
        List<ItemDataBase> others = prop.awardItems.FindAll(x => x.tid / 1000 == (int)ITEM_TYPE.ROLE_ATTRIBUTE);
        temp.AddRange(others);

        awardItems = TaskLogicData.Instance.AcceptDailyTaskAward(taskId, temp);
        TaskState.hasDailyAwardAcceptable = TaskProperty.HasAwardAcceptable(TASK_PAGE.DAILY);
    }
}


/// <summary>
/// 获取成就任务奖励
/// </summary>
public class GetAchievementAwardRequester : NetRequester<CS_GetAchievementAward, SC_GetAchievementAward>
{
    public int taskId { get; private set; }
    public List<ItemDataBase> awardItems { get; private set; }

    public GetAchievementAwardRequester(int taskId)
    {
        this.taskId = taskId;
    }

    protected override CS_GetAchievementAward GetRequest()
    {
        return new CS_GetAchievementAward() { taskId = this.taskId };
    }

    protected override void OnSuccess()
    {
        List<ItemDataBase> temp = new List<ItemDataBase>(respMsg.arr);
        TaskProperty prop = new TaskProperty(taskId);
        List<ItemDataBase> others = prop.awardItems.FindAll(x => x.tid / 1000 == (int)ITEM_TYPE.ROLE_ATTRIBUTE);
        temp.AddRange(others);

        awardItems = TaskLogicData.Instance.AcceptAchievementAward(taskId, temp);
        TaskState.hasAchieveAwardAcceptable = TaskProperty.HasAwardAcceptable(TASK_PAGE.ACHIEVEMENT);
    }
}


/// <summary>
/// 获取积分奖励
/// </summary>
public class GetTaskScoreAwardRequester : NetRequester<CS_GetTaskScoreAward, SC_GetTaskScoreAward>
{
    public int awardId { get; private set; }
    public List<ItemDataBase> awardItems { get; private set; }

    public GetTaskScoreAwardRequester(int awardId)
    {
        this.awardId = awardId;
    }

    protected override CS_GetTaskScoreAward GetRequest()
    {
        return new CS_GetTaskScoreAward() { awardId = this.awardId };
    }

    protected override void OnSuccess()
    {
        List<ItemDataBase> temp = new List<ItemDataBase>(respMsg.arr);
        List<ItemDataBase> configAwards = TaskProperty.GetDailyTaskScoreAwardItems(awardId);
        List<ItemDataBase> others = configAwards.FindAll(x => x.tid / 1000 == (int)ITEM_TYPE.ROLE_ATTRIBUTE);
        temp.AddRange(others);

        awardItems = TaskLogicData.Instance.AcceptTaskScoreAward(awardId, temp);
        TaskState.hasDailyAwardAcceptable = TaskProperty.HasAwardAcceptable(TASK_PAGE.DAILY);
    }
}


/// <summary>
/// 装备精炼
/// </summary>
public class RefineEquipRequester : NetRequester<CS_RefineEquip, SC_RefineEquip>
{
    public int equipItemId { get; private set; }
    public int equipTid { get; private set; }
    public ItemDataBase[] refineCosts { get; private set; }

    public int addExp 
    {
        get
        {
            int exp = 0;

            if (refineCosts != null)
            {
                foreach (ItemDataBase item in refineCosts)
                {
                    DataRecord r = DataCenter.mEquipRefineStoneConfig.GetRecord(item.tid);
                    exp += r["REFINE_STONE_EXP"] * item.itemNum;
                }
            }

            return exp;
        }
    }

    public RefineEquipRequester(int itemId, int tid, ItemDataBase[] costs)
    {
        equipItemId = itemId;
        equipTid = tid;
        refineCosts = costs;
    }

    protected override CS_RefineEquip GetRequest()
    {
        return new CS_RefineEquip() { itemId = equipItemId, tid = equipTid, arr = refineCosts };
    }

    protected override void OnSuccess()
    {
        int temp = addExp;

        // 扣除消耗品
        PackageManager.RemoveItem(refineCosts);

        // 增加精炼经验
        EquipData equip = RoleEquipLogicData.Self.GetEquipDataByItemId(equipItemId);
        equip.AddRefineExp(temp);
    }
}


/// <summary>
/// 使用消耗品
/// </summary>
public class UsePropRequester : NetRequester<CS_UseProp, SC_UseProp>
{
    public ItemDataBase prop { get; private set; }

    public UsePropRequester(ItemDataBase prop)
    {
        this.prop = prop;
    }

    protected override CS_UseProp GetRequest()
    {
        return new CS_UseProp { prop = this.prop };
    }

    protected override void OnSuccess()
    {
        PackageManager.RemoveItem(prop);
        //addded by xuke
        DataCenter.Set("USE_PROP_AWARD",PackageManager.UpdateItem(respMsg.items));
        //end
    }
}


/// <summary>
/// 使用多选一宝箱
/// </summary>
public class OpenBoxSelectRequester : NetRequester<CS_OpenBoxSelect, SC_OpenBoxSelect>
{
    public ItemDataBase prop { get; private set; }
    public ItemDataBase selectItem { get; private set; }

    public OpenBoxSelectRequester(ItemDataBase prop, ItemDataBase selectItem)
    {
        this.prop = prop;
        this.selectItem = selectItem;
    }

    protected override CS_OpenBoxSelect GetRequest()
    {
        return new CS_OpenBoxSelect { prop = this.prop, selectItem = this.selectItem };
    }

    protected override void OnSuccess()
    {
        PackageManager.RemoveItem(prop);
        PackageManager.UpdateItem(respMsg.items);
    }
}


/// <summary>
/// 使用免战令
/// </summary>
public class UseTruceTokenRequester : NetRequester<CS_GrabTreasure_UseTruceToken, SC_GrabTreasure_UseTruceToken>
{
    public ItemDataBase item { get; private set; }
    public long peaceTime { get; private set; }

    public UseTruceTokenRequester(ItemDataBase item, long peaceTime)
    {
        this.item = item;
        this.peaceTime = peaceTime;
    }

    protected override CS_GrabTreasure_UseTruceToken GetRequest()
    {
        var req = new CS_GrabTreasure_UseTruceToken();
		req.truceCard = item;
        return req;
    }

    protected override void OnSuccess()
    { 
        PackageManager.RemoveItem(item.tid, item.itemId, 1);
        DataCenter.SetData("GRABTREASURE_WINDOW", "ADD_PEACE_TIME", peaceTime);
    }
}